using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-400)]
public sealed class StoryDirector : MonoBehaviour
{
    public static StoryDirector Active {get;private set;}
    public static event Action Changed;
    public NightDefinition definition;
    [NonSerialized]public NightState state;
    public NarrativePresentation Presentation {get;private set;}
    public bool Opening {get;private set;}
    public bool Running=>state!=null&&!state.completed;
    public string CurrentId=>Running&&state.index<definition.beats.Length?definition.beats[state.index].id:"complete";
    public DateTime Clock=>DateTime.ParseExact(definition.date,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture).AddSeconds(state.seconds);
    public string ClockText=>Clock.ToString("HH:mm");
    public bool AllowLeave=>state==null||state.completed||Has("leave.allowed");
    bool advancing,restoring;
    public UnityEngine.InputSystem.Key recallObjectiveKey=UnityEngine.InputSystem.Key.G;
    [Min(10)]public float idleReminderSeconds=60;
    float inactivity,reminderCooldown;
    Vector3 lastPlayerPosition;
    long eventCounter;
    struct RecentEvent{public long serial;public StoryEventKind kind;public string target,value;}
    readonly List<RecentEvent> recentEvents=new List<RecentEvent>();
    readonly Dictionary<string,NarrativeBinding> bindings=new Dictionary<string,NarrativeBinding>();
    string resumeScene;
    bool skipOpeningRequested;
    readonly HashSet<string> runningSequences=new HashSet<string>();
    float nextUiClock;
    public static string CheckpointPath {
        get {
#if UNITY_EDITOR
            return Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/NarrativeCheckpoints/Night.json"));
#else
            return Path.Combine(Application.persistentDataPath,"Night.json");
#endif
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetStatics(){Active=null;Changed=null;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot()
    {
        var scene=SceneManager.GetActiveScene().name;if(scene!="Alley"&&scene!="Apartment"||Active!=null)return;
        var prefab=Resources.Load<GameObject>("Narrative/Narrative Runtime");if(prefab!=null)Instantiate(prefab);
    }
    void Awake(){if(Active!=null&&Active!=this){Destroy(gameObject);return;}Active=this;DontDestroyOnLoad(gameObject);Presentation=GetComponentInChildren<NarrativePresentation>(true);}
    void OnEnable(){SceneManager.sceneLoaded+=Loaded;InteractionScanner.Interacted+=Interacted;BuildingKeys.Changed+=KeysChanged;ChairSeat.OccupancyChanged+=SeatChanged;ElevatorController.Arrived+=LiftArrived;PhoneBattery.DockChanged+=PhoneDockChanged;}
    void OnDisable(){SceneManager.sceneLoaded-=Loaded;InteractionScanner.Interacted-=Interacted;BuildingKeys.Changed-=KeysChanged;ChairSeat.OccupancyChanged-=SeatChanged;ElevatorController.Arrived-=LiftArrived;PhoneBattery.DockChanged-=PhoneDockChanged;if(Active==this)Active=null;}
    void Start(){if(state==null||state.signals==null)StartNight(1,false);}
    public void StartNight(int number,bool loadArrival=true)
    {
        var requested=number>=1&&number<=7?Resources.Load<NightDefinition>("Narrative/Night "+number):null;if(requested==null){Debug.LogWarning("Night "+number+" has no authored definition. Only Night 1 is currently implemented.");return;}
        StopAllCoroutines();runningSequences.Clear();skipOpeningRequested=false;Opening=false;restoring=true;
        definition=requested;
        state=new NightState{night=number,seconds=definition.startSeconds,battery=definition.startingBattery};
        foreach(var message in definition.history)state.messages.Add(new PhoneStoryMessage{id=message.id,contact=message.contact,time=message.time,body=message.body,outgoing=message.outgoing,read=message.read});
        BuildingKeys.Restore(new string[0]);BuildingPower.RestoreState(new NarrativeValue[0],true);CctvSystem.RestoreSelections(new NarrativeValue[0]);
        var phone=FindFirstObjectByType<PhoneBattery>();if(phone!=null)phone.ResetForNight(definition.startingBattery);
        Presentation.ResetView();restoring=false;Rebind();
        if(loadArrival){Opening=true;Presentation.blackout.gameObject.SetActive(true);Presentation.blackout.alpha=1;resumeScene="start";SceneManager.LoadScene("Alley");}
        else{Opening=SceneManager.GetActiveScene().name=="Alley";if(Opening){Presentation.blackout.gameObject.SetActive(true);Presentation.blackout.alpha=1;var scanner=InteractionScanner.Active;if(scanner!=null)scanner.cutscenePlaying=true;}StartCoroutine(BeginOpening());}
    }
    IEnumerator BeginOpening()
    {
        yield return null;Opening=SceneManager.GetActiveScene().name=="Alley";
        if(Opening&&!skipOpeningRequested)yield return Presentation.Opening(definition);skipOpeningRequested=false;Presentation.blackout.alpha=0;Presentation.blackout.gameObject.SetActive(false);var scanner=InteractionScanner.Active;if(scanner!=null)scanner.cutscenePlaying=false;
        Opening=false;EnterBeat();Signal("scene."+SceneManager.GetActiveScene().name);
    }
    void Loaded(Scene scene,LoadSceneMode mode)
    {
        if(state==null)return;PhoneInstaller.Install();Rebind();
        if(resumeScene=="start"){resumeScene=null;var scanner=InteractionScanner.Active;if(scanner!=null)scanner.cutscenePlaying=true;StartCoroutine(BeginOpening());return;}
        ApplyWorld();
        if(resumeScene=="checkpoint"||resumeScene=="jump")
        {
            bool jumping=resumeScene=="jump";if(jumping){var spawn=Binding(definition.beats[state.index].spawnBinding);if(spawn!=null){state.playerPosition=spawn.transform.position;state.playerRotation=spawn.transform.eulerAngles;state.eyeRotation=Vector3.zero;}state.playerInElevator=false;}
            restoring=true;BuildingKeys.Restore(state.keys);BuildingPower.RestoreState(state.circuits,state.mainPower);CctvSystem.RestoreSelections(state.cctv);
            var phone=FindFirstObjectByType<PhoneBattery>();if(phone!=null)phone.RestoreStoryState(state.battery,state.dockId);
            var lift=FindFirstObjectByType<ElevatorController>();if(lift!=null)lift.RestoreAtFloor(state.elevatorFloor);
            Warp(state.playerInElevator&&lift!=null?lift.cabin.TransformPoint(state.playerCabinLocal):state.playerPosition,state.playerRotation,state.eyeRotation);restoring=false;resumeScene=null;
            if(CurrentId=="telephone"){var telephone=FindFirstObjectByType<NarrativeTelephone>();if(telephone!=null)telephone.Ring();}
            if(state.completed)Presentation.Complete();else if(jumping){EnterBeat();Evaluate();}else{RecallObjective();Run(definition.beats[state.index].enter.Where(a=>a.kind==NarrativeActionKind.Sound||a.kind==NarrativeActionKind.Fade).ToArray());}
        }
        StartCoroutine(AfterArrival(scene.name));
    }
    IEnumerator AfterArrival(string scene){yield return null;Signal("scene."+scene);Publish(StoryEventKind.AreaEntered,scene);KeysChanged();Changed?.Invoke();}
    public void Rebind(){bindings.Clear();foreach(var item in FindObjectsByType<NarrativeBinding>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(!string.IsNullOrEmpty(item.id))bindings[item.id]=item;}
    public NarrativeBinding Binding(string id){NarrativeBinding item;return !string.IsNullOrEmpty(id)&&bindings.TryGetValue(id,out item)?item:null;}
    void Update(){Tick(Time.unscaledDeltaTime);}
    public void Tick(float seconds)
    {
        if(!Running||Opening||restoring||Time.timeScale<=0)return;var scan=InteractionScanner.Active;if(scan!=null&&(scan.menuOpen||scan.cutscenePlaying||scan.documentOpen))return;
        state.beatSeconds+=Mathf.Max(0,seconds);inactivity+=seconds;reminderCooldown=Mathf.Max(0,reminderCooldown-seconds);
        var keyboard=UnityEngine.InputSystem.Keyboard.current;var mouse=UnityEngine.InputSystem.Mouse.current;
        if(scan!=null){if(Vector3.Distance(scan.transform.position,lastPlayerPosition)>.05f)inactivity=0;lastPlayerPosition=scan.transform.position;}
        if(mouse!=null&&(mouse.leftButton.wasPressedThisFrame||mouse.rightButton.wasPressedThisFrame)||keyboard!=null&&keyboard.anyKey.wasPressedThisFrame)inactivity=0;
        var selected=UnityEngine.EventSystems.EventSystem.current!=null?UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject:null;
        bool typing=selected!=null&&selected.GetComponent<UnityEngine.UI.InputField>()!=null;
        if(keyboard!=null&&!typing&&keyboard[recallObjectiveKey].wasPressedThisFrame)RecallObjective();
        if(inactivity>=idleReminderSeconds)Remind();
        if(Time.unscaledTime>=nextUiClock){nextUiClock=Time.unscaledTime+1;Changed?.Invoke();}
    }
    public bool Has(string signal)=>state!=null&&state.signals.Contains(signal);
    public static void Signal(string id){if(Active!=null)Active.Emit(id);}
    public void Emit(string id)
    {
        if(restoring||state==null||Opening||string.IsNullOrEmpty(id))return;
        bool added=!state.signals.Contains(id);if(added)state.signals.Add(id);
        if(id==definition.approachSignal&&CurrentId=="arrival"&&!Has("supervisor.approach"))
        {
            state.signals.Add("supervisor.approach");
            foreach(var message in definition.approachMessages)Send(message.contact,message.body,message.outgoing,message.id);
        }
        Publish(StoryEventKind.Signal,id);if(added)Changed?.Invoke();
    }
    public void Publish(StoryEventKind kind,string target,string value="")
    {
        if(!Running||Opening||restoring)return;eventCounter++;recentEvents.Add(new RecentEvent{serial=eventCounter,kind=kind,target=target,value=value});if(recentEvents.Count>128)recentEvents.RemoveAt(0);
        var beat=definition.beats[state.index];for(int i=0;i<beat.conditions.Length;i++)if(beat.conditions[i].Matches(kind,target,value)&&!state.stepSignals.Contains("condition."+i)){state.stepSignals.Add("condition."+i);inactivity=0;}
        if(kind==StoryEventKind.Signal&&beat.waitForSignals.Contains(target)&&!state.stepSignals.Contains(target)){state.stepSignals.Add(target);inactivity=0;}
        Evaluate();
    }
    public void RecallObjective(){if(Running){var beat=definition.beats[state.index];Presentation.Objective(beat.objective,beat.guidance);}}
    public void Remind(){if(!Running||reminderCooldown>0)return;reminderCooldown=20;inactivity=0;Presentation.Say("Aki",definition.beats[state.index].guidance,5);RecallObjective();}
    void Evaluate()
    {
        if(advancing||!Running||Opening||restoring)return;advancing=true;
        try
        {
            int guard=definition.beats.Length+1;
            while(Running&&guard-->0)
            {
                var beat=definition.beats[state.index];if(beat.waitForSignals.Any(s=>!state.stepSignals.Contains(s))||Enumerable.Range(0,beat.conditions.Length).Any(i=>!state.stepSignals.Contains("condition."+i)))break;
                if(state.completedSteps.Contains(beat.id))break;state.completedSteps.Add(beat.id);Run(beat.exit);
                int next=string.IsNullOrEmpty(beat.nextStepId)?state.index+1:Array.FindIndex(definition.beats,b=>b.id==beat.nextStepId);
                if(next<=state.index){Debug.LogError("Story steps must lead forward in the authored linear sequence: "+beat.id);break;}state.index=next;state.beatSeconds=0;
                if(state.index>=definition.beats.Length){state.completed=true;state.seconds=Mathf.Max(state.seconds,30*3600);Presentation.Complete();SaveCheckpoint();Changed?.Invoke();break;}
                EnterBeat();if(beat.checkpointOnComplete)SaveCheckpoint();
            }
        }
        finally{advancing=false;}
    }
    void EnterBeat()
    {
        state.stepSignals.Clear();var beat=definition.beats[state.index];inactivity=0;
        Run(beat.enter);if(!string.IsNullOrEmpty(beat.supervisorMessage))Send("Supervisor",beat.supervisorMessage);if(!string.IsNullOrEmpty(beat.protagonistDialogue))Presentation.Say("Aki",beat.protagonistDialogue,5);
        if(beat.scriptedSequence!=null)StartCoroutine(PlaySequence(beat.scriptedSequence));RecallObjective();
        for(int i=0;i<beat.conditions.Length;i++){var condition=beat.conditions[i];if(condition.checkCurrentStateOnEnter&&StateMatches(condition))state.stepSignals.Add("condition."+i);}
        if(beat.checkpointOnBegin)SaveCheckpoint();Changed?.Invoke();
    }
    public void Run(NarrativeAction[] actions){if(actions!=null)foreach(var action in actions)Execute(action);}
    public void Execute(NarrativeAction action)
    {
        switch(action.kind)
        {
            case NarrativeActionKind.Message:Send(string.IsNullOrEmpty(action.target)?"Supervisor":action.target,action.text,false,action.completionId);break;
            case NarrativeActionKind.Subtitle:Presentation.Say(action.target,action.text,action.number>0?action.number:4,action.completionId);break;
            case NarrativeActionKind.SetTime:state.seconds=Mathf.Max(state.seconds,action.number);break;
            case NarrativeActionKind.AdvanceTime:state.seconds+=Mathf.Max(0,action.number)*60;break;
            case NarrativeActionKind.TaskStatus:SetValue(state.tasks,action.target,action.text);break;
            case NarrativeActionKind.MaintenanceStatus:SetValue(state.requests,action.target,action.text);break;
            case NarrativeActionKind.Record:AddRecord(action.target,action.text);break;
            case NarrativeActionKind.SetFlag:if(action.value){if(!Has(action.target))state.signals.Add(action.target);}else state.signals.Remove(action.target);break;
            case NarrativeActionKind.SetCircuit:BuildingPower.SetCircuit(action.circuit,action.value);break;
            case NarrativeActionKind.LightingMood:foreach(var mood in FindObjectsByType<LightingMoodController>(FindObjectsSortMode.None))if(mood.CurrentMood!=(action.value?LightingMoodController.Mood.Horror:LightingMoodController.Mood.Normal))mood.SetHorror(action.value);break;
            case NarrativeActionKind.Sequence:if(action.sequence!=null)StartCoroutine(PlaySequence(action.sequence));break;
            case NarrativeActionKind.Battery:var phone=FindFirstObjectByType<PhoneBattery>();if(phone!=null)phone.SetCharge(action.number);break;
            case NarrativeActionKind.RestrictPlayer:Presentation.Restrict(action.value);break;
            case NarrativeActionKind.Scene:SaveCheckpoint();SceneManager.LoadScene(action.text);break;
            case NarrativeActionKind.Fade:StartCoroutine(FadeAndSignal(action));break;
            case NarrativeActionKind.PhoneCall:Presentation.Say(action.target,action.text,action.number>0?action.number:5,action.completionId);Presentation.Notify("Call · "+action.target);break;
            default:
                var binding=Binding(action.target);if(binding!=null)binding.Apply(action.kind,action.value,action.number,action.text,action.audio,action.completionId);
                if(action.kind==NarrativeActionKind.DoorLock||action.kind==NarrativeActionKind.DoorOpen||action.kind==NarrativeActionKind.SetActive||action.kind==NarrativeActionKind.LightIntensity||action.kind==NarrativeActionKind.Weather||action.kind==NarrativeActionKind.LightFault||action.kind==NarrativeActionKind.CameraFault)
                {state.world.RemoveAll(w=>w.binding==action.target&&w.kind==action.kind);state.world.Add(new NarrativeWorldValue{binding=action.target,kind=action.kind,text=action.text,number=action.number,value=action.value});}
                break;
        }
    }
    IEnumerator PlaySequence(NarrativeSequence sequence)
    {
        if(!runningSequences.Add(sequence.sequenceId))yield break;
        try{foreach(var cue in sequence.cues){if(cue.delay>0)yield return new WaitForSeconds(cue.delay);long before=eventCounter;Run(cue.actions);if(cue.waitForCompletion){float waiting=0;while(!(cue.completion.checkCurrentStateOnEnter&&StateMatches(cue.completion))&&!recentEvents.Any(e=>e.serial>before&&cue.completion.Matches(e.kind,e.target,e.value))){waiting+=Time.unscaledDeltaTime;if(waiting>cue.timeout){Debug.LogWarning("Sequence timed out: "+sequence.sequenceId);yield break;}yield return null;}}}Publish(StoryEventKind.SequenceCompleted,sequence.sequenceId);}
        finally{runningSequences.Remove(sequence.sequenceId);Presentation.Restrict(false);foreach(var seat in FindObjectsByType<ChairSeat>(FindObjectsSortMode.None))seat.standingLocked=false;}
    }
    IEnumerator FadeAndSignal(NarrativeAction action){yield return Presentation.Fade(action.value?1:0,Mathf.Max(.1f,action.number));if(!string.IsNullOrEmpty(action.completionId))Publish(StoryEventKind.Signal,action.completionId);}
    bool StateMatches(StoryCondition condition)
    {
        if(condition.kind==StoryEventKind.ItemCollected)return BuildingKeys.IsCollected(condition.target);
        if(condition.kind==StoryEventKind.ItemReturned)return !BuildingKeys.IsCollected(condition.target)&&Has("returned."+condition.target);
        if(condition.kind==StoryEventKind.CircuitChanged){var circuit=BuildingPower.Active!=null?BuildingPower.Active.circuits.FirstOrDefault(c=>c!=null&&c.id==condition.target):null;return circuit!=null&&BuildingPower.IsEnabled(circuit)==(condition.value=="on");}
        if(condition.kind==StoryEventKind.Signal&&condition.target=="phone.docked")return PhoneBattery.IsDocked;
        if(condition.kind==StoryEventKind.Signal&&condition.target=="phone.collected")return !PhoneBattery.IsDocked&&Has("phone.was.docked");
        var lift=FindFirstObjectByType<ElevatorController>();
        if(lift!=null){if(condition.kind==StoryEventKind.ElevatorArrived)return !lift.Moving&&lift.currentFloor.ToString()==condition.target;if(condition.kind==StoryEventKind.Signal&&condition.target=="elevator.opened")return !lift.Moving&&lift.DoorAmount>.999f;if(condition.kind==StoryEventKind.Signal&&condition.target=="elevator.closed")return lift.DoorAmount<.001f;}
        return false;
    }
    void SeatChanged(ChairSeat seat,bool occupied){Publish(occupied?StoryEventKind.Sitting:StoryEventKind.Standing,"security.chair");}
    void LiftArrived(int floor){Publish(StoryEventKind.ElevatorArrived,floor.ToString());}
    void PhoneDockChanged(bool docked){if(state==null||restoring)return;if(docked&&!Has("phone.was.docked"))state.signals.Add("phone.was.docked");Publish(StoryEventKind.Signal,docked?"phone.docked":"phone.collected");}
    public void TestSequence(NarrativeSequence sequence){if(sequence!=null)StartCoroutine(PlaySequence(sequence));}
    public static void SetValue(List<NarrativeValue> values,string id,string value){var item=values.Find(v=>v.id==id);if(item==null)values.Add(new NarrativeValue{id=id,value=value});else item.value=value;}
    public static string Value(List<NarrativeValue> values,string id,string fallback){var item=values.Find(v=>v.id==id);return item==null?fallback:item.value;}
    public void SetTask(string id,PhoneManagementData.TaskStatus status){SetValue(state.tasks,id,status.ToString());Changed?.Invoke();}
    public void SetMaintenance(string id,string status){SetValue(state.requests,id,status);Changed?.Invoke();}
    public void AddRecord(string title,string description){state.records.Add(new PhoneManagementData.Record{time=ClockText,title=title,location="Security office",description=description});}
    public void Send(string contact,string body,bool outgoing=false,string messageId=null)
    {
        state.messages.Add(new PhoneStoryMessage{id=messageId??"message."+state.messages.Count,contact=contact,time=Clock.ToString("yyyy-MM-dd HH:mm"),body=body,outgoing=outgoing,read=outgoing});
        if(!outgoing)Presentation.Notify("Message from "+contact);Changed?.Invoke();
    }
    public void ReadMessages(string contact){foreach(var message in state.messages.ToArray())if(message.contact==contact&&!message.read){message.read=true;Publish(StoryEventKind.MessageRead,message.id??contact);}Publish(StoryEventKind.MessageRead,contact);}
    public bool TrySubmitReport(string body)
    {
        if(CurrentId!="report"){Presentation.Notify("The shift is not ready to close yet.");return false;}
        var scan=InteractionScanner.Active;var office=Binding("office.zone");
        if(scan==null||office==null||!office.GetComponent<BoxCollider>().bounds.Contains(scan.transform.position+Vector3.up*.5f)){Presentation.Notify("Submit the report from the security office.");return false;}
        if(string.IsNullOrWhiteSpace(body)||body.Trim().Length<12){Presentation.Notify("Please write a short account of your shift.");return false;}
        state.report=body.Trim();AddRecord("Nightly report submitted",state.report);Signal("report.submitted");SaveCheckpoint();return true;
    }
    public bool CanTravel(string destination)
    {
        if(destination=="Alley"&&!AllowLeave){Presentation.Say("Aki","I should finish the shift before I leave.",3);return false;}
        if(destination=="Alley"&&PhoneBattery.IsDocked){Presentation.Notify("My phone is still on the desk charger.");return false;}return true;
    }
    void KeysChanged(){if(state==null||restoring)return;}
    void Interacted(IInteractionTarget target)
    {
        if(target is CctvMonitor||target is CctvControl)Emit("cctv.inspected");
        if(target is CctvMonitor||target is CctvControl)Publish(StoryEventKind.CctvViewed,"security.monitors");
        var viewed=target as CctvMonitor;var control=target as CctvControl;if(viewed==null&&control!=null)viewed=control.monitor;if(viewed!=null&&viewed.system!=null){var channel=viewed.system.channels[Mathf.Clamp(viewed.ChannelIndex,0,viewed.system.channels.Length-1)];Publish(StoryEventKind.CctvViewed,"camera."+channel.cameraNumber);}
        var key=target as KeyPickup;if(key!=null){bool collected=BuildingKeys.IsCollected(key.keyId);if(!collected&&!Has("returned."+key.keyId))state.signals.Add("returned."+key.keyId);Publish(collected?StoryEventKind.ItemCollected:StoryEventKind.ItemReturned,key.keyId);}
        var door=target as StairAccessDoor;if(door!=null&&!door.locked){var binding=door.GetComponent<NarrativeBinding>();Publish(StoryEventKind.DoorUnlocked,binding!=null?binding.id:door.requiredKeyId);}
        var room=target as RoomAccessLock;var handle=target as RoomDoorHandle;if(room==null&&handle!=null)room=handle.roomLock;if(room!=null&&!room.locked)Publish(StoryEventKind.DoorUnlocked,room.roomNumber);
        var gate=target as RoomGateLatch;if(gate!=null&&!gate.locked)Publish(StoryEventKind.DoorUnlocked,gate.requiredKeyId+".gate");
        var power=target as PowerSwitch;if(power!=null&&power.circuit!=null){Publish(StoryEventKind.CircuitChanged,power.circuit.id,BuildingPower.IsPowered(power.circuit)?"on":"off");if(CurrentId.StartsWith("maintenance.")&&power.circuit.id!="Fourth Floor"||CurrentId.StartsWith("camera.")&&power.circuit.id!="CCTV Cameras")Remind();}
        var component=target as Component;if(component!=null){var binding=component.GetComponent<NarrativeBinding>();Publish(StoryEventKind.Interaction,binding!=null?binding.id:component.name);}
    }
    void ApplyWorld()
    {
        foreach(var value in state.world){var binding=Binding(value.binding);if(binding!=null)binding.Apply(value.kind,value.value,value.number,value.text);}
        foreach(var value in state.doors){var binding=Binding(value.binding);if(binding!=null&&binding.door!=null){binding.door.locked=value.locked;binding.door.open=value.open;binding.door.Tick(5);}}
        foreach(var value in state.actors){var binding=Binding(value.binding);if(binding!=null&&binding.actor!=null)binding.actor.Restore(value.position,value.waypoint,value.walking);}
    }
    public void SnapshotWorld()
    {
        if(state==null)return;state.scene=SceneManager.GetActiveScene().name;
        var player=FindFirstObjectByType<AlleyStudy.AlleyWalker>();if(player!=null){state.playerPosition=player.transform.position;state.playerRotation=player.transform.eulerAngles;state.eyeRotation=player.eyes.transform.localEulerAngles;}
        if(player!=null&&player.seated){var seat=FindObjectsByType<ChairSeat>(FindObjectsSortMode.None).FirstOrDefault(s=>s.IsOccupied);if(seat!=null&&seat.standPosition!=null)state.playerPosition=seat.standPosition.position;}
        var lift=FindFirstObjectByType<ElevatorController>();if(lift!=null){state.elevatorFloor=lift.Moving?lift.TargetFloor:lift.currentFloor;var local=player!=null?lift.cabin.InverseTransformPoint(player.transform.position):Vector3.one*100;state.playerInElevator=Mathf.Abs(local.x)<.73f&&local.z>.10f&&local.z<1.6f&&local.y>-.2f&&local.y<2.25f;state.playerCabinLocal=local;}
        state.keys=BuildingKeys.Collected.ToList();state.circuits=BuildingPower.CaptureState().ToList();state.mainPower=BuildingPower.MainSupply;state.cctv=CctvSystem.CaptureSelections().ToList();
        var phone=FindFirstObjectByType<PhoneBattery>();if(phone!=null){state.battery=phone.Charge;state.dockId=PhoneBattery.DockId;}
        foreach(var binding in bindings.Values)
        {
            if(binding==null)continue;
            if(binding.door!=null){state.doors.RemoveAll(v=>v.binding==binding.id);state.doors.Add(new NarrativeDoorState{binding=binding.id,locked=binding.door.locked,open=binding.door.open});}
            if(binding.actor!=null){state.actors.RemoveAll(v=>v.binding==binding.id);state.actors.Add(new NarrativeActorState{binding=binding.id,position=binding.actor.transform.position,waypoint=binding.actor.Waypoint,walking=binding.actor.Walking});}
        }
    }
    public void SaveCheckpoint(){if(state==null||Opening||restoring)return;SnapshotWorld();Directory.CreateDirectory(Path.GetDirectoryName(CheckpointPath));File.WriteAllText(CheckpointPath,JsonUtility.ToJson(state,true));}
    public bool LoadCheckpoint()
    {
        if(!File.Exists(CheckpointPath))return false;var loaded=JsonUtility.FromJson<NightState>(File.ReadAllText(CheckpointPath));
        if(loaded==null||loaded.version!=1||loaded.night<1||loaded.night>7)return false;var savedDefinition=Resources.Load<NightDefinition>("Narrative/Night "+loaded.night);if(savedDefinition==null||loaded.index<0||loaded.index>=savedDefinition.beats.Length&&!loaded.completed)return false;definition=savedDefinition;
        if(!loaded.completed&&definition.beats[loaded.index].id=="empty"){int sit=Array.FindIndex(definition.beats,b=>b.id=="sit");loaded.index=sit;loaded.completedSteps.Remove("sit");loaded.stepSignals.Clear();loaded.playerInElevator=false;loaded.playerPosition=new Vector3(-4.3f,.05f,4.8f);}
        StopAllCoroutines();runningSequences.Clear();Opening=false;Presentation.ResetView();state=loaded;resumeScene="checkpoint";SceneManager.LoadScene(state.scene);return true;
    }
    public void JumpToBeat(int index)
    {
        if(index<0||index>=definition.beats.Length)return;StopAllCoroutines();runningSequences.Clear();Opening=false;Presentation.ResetView();restoring=true;state=new NightState{seconds=definition.startSeconds,night=definition.nightNumber,battery=definition.startingBattery};
        foreach(var message in definition.history)state.messages.Add(new PhoneStoryMessage{id=message.id,contact=message.contact,time=message.time,body=message.body,outgoing=message.outgoing,read=message.read});
        BuildingKeys.Restore(new string[0]);BuildingPower.RestoreState(new NarrativeValue[0],true);CctvSystem.RestoreSelections(new NarrativeValue[0]);
        for(int i=0;i<index;i++){ReplayPersistent(definition.beats[i].enter);ReplayPersistent(definition.beats[i].exit);state.completedSteps.Add(definition.beats[i].id);}
        int keys=Array.FindIndex(definition.beats,b=>b.id=="keys"),returned=Array.FindIndex(definition.beats,b=>b.id=="returnkey"),charge=Array.FindIndex(definition.beats,b=>b.id=="charge"),collect=Array.FindIndex(definition.beats,b=>b.id=="collect");
        if(index>1)state.keys.Add("Management");if(index>keys&&index<=returned)state.keys.Add("Stairwell");if(index>returned)state.signals.Add("returned.Stairwell");
        if(index>charge){state.signals.Add("phone.was.docked");if(index<=collect){var station=FindFirstObjectByType<PhoneChargingStation>();if(station!=null)state.dockId=station.stationId;}}
        state.circuits=BuildingPower.CaptureState().ToList();state.mainPower=BuildingPower.MainSupply;
        state.index=index;state.scene=definition.beats[index].scene;state.playerPosition=state.scene=="Alley"?new Vector3(0,.1f,2):new Vector3(0,.1f,5);state.playerRotation=Vector3.zero;state.eyeRotation=Vector3.zero;restoring=false;
        resumeScene="jump";SceneManager.LoadScene(state.scene);
    }
    void ReplayPersistent(NarrativeAction[] actions){foreach(var action in actions)if(action.kind!=NarrativeActionKind.Sequence&&action.kind!=NarrativeActionKind.Sound&&action.kind!=NarrativeActionKind.Fade&&action.kind!=NarrativeActionKind.Subtitle&&action.kind!=NarrativeActionKind.PhoneCall&&action.kind!=NarrativeActionKind.Elevator&&action.kind!=NarrativeActionKind.ElevatorDoors&&action.kind!=NarrativeActionKind.RingTelephone&&action.kind!=NarrativeActionKind.SeatLock&&action.kind!=NarrativeActionKind.RestrictPlayer&&action.kind!=NarrativeActionKind.MoveActor&&action.kind!=NarrativeActionKind.Animation&&action.kind!=NarrativeActionKind.Scene&&action.kind!=NarrativeActionKind.InvokeBinding)Execute(action);}
    public void AdvanceClock(float minutes){state.seconds+=Mathf.Max(0,minutes)*60;Changed?.Invoke();}
    public void SkipOpening(){if(!Opening)return;if(resumeScene=="start"){skipOpeningRequested=true;return;}StopAllCoroutines();Opening=false;Presentation.ResetView();EnterBeat();Publish(StoryEventKind.AreaEntered,SceneManager.GetActiveScene().name);}
    public static void Warp(Vector3 position,Vector3 rotation,Vector3 eyeRotation)
    {
        var player=FindFirstObjectByType<AlleyStudy.AlleyWalker>();if(player==null)return;var body=player.GetComponent<CharacterController>();body.enabled=false;player.transform.SetPositionAndRotation(position,Quaternion.Euler(rotation));player.eyes.transform.localRotation=Quaternion.Euler(eyeRotation);player.SyncLookPitch();body.enabled=true;
    }
}
