using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

// Editor-only acceptance driver. Positions the player between locations; it does not emit completion signals.
public static class NightOnePlaytest
{
    static readonly Stack<IEnumerator> routines=new Stack<IEnumerator>();
    static double waitUntil;
    public static string Result {get;private set;}="Not run";
    static bool oldBackground;
    static readonly List<string> passed=new List<string>();
    [MenuItem("Horror Game/Narrative/Run Night 1 Acceptance Test")]
    public static void Start()
    {
        if(!Application.isPlaying){Result="Enter Play Mode first.";return;}EditorApplication.isPaused=false;routines.Clear();passed.Clear();routines.Push(Run());waitUntil=0;Result="Running";oldBackground=Application.runInBackground;Application.runInBackground=true;EditorApplication.update-=Update;EditorApplication.update+=Update;
    }
    static void Update()
    {
        EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<waitUntil)return;
        try{if(routines.Count==0){Finish("PASS: "+string.Join("; ",passed));return;}var routine=routines.Peek();if(!routine.MoveNext()){routines.Pop();return;}var nested=routine.Current as IEnumerator;if(nested!=null)routines.Push(nested);else if(routine.Current is float)waitUntil=EditorApplication.timeSinceStartup+(float)routine.Current;}
        catch(Exception error){Finish("FAIL: "+error);Debug.LogError(Result);}
    }
    static void Finish(string result){Result=result;EditorApplication.update-=Update;Application.runInBackground=oldBackground;System.IO.Directory.CreateDirectory("Library/NarrativeCheckpoints");System.IO.File.WriteAllText("Library/NarrativeCheckpoints/Night1Acceptance.txt",result);}
    static StoryDirector Director=>StoryDirector.Active;
    static PhoneControls Phone=>UnityEngine.Object.FindFirstObjectByType<PhoneControls>();
    static void Check(bool condition,string description){if(!condition)throw new Exception(description+" (step "+Director.CurrentId+")");passed.Add(description);}
    static IEnumerator WaitStep(string id,float timeout=15)
    {double end=EditorApplication.timeSinceStartup+timeout;while(Director==null||Director.CurrentId!=id){if(EditorApplication.timeSinceStartup>end)throw new Exception("Expected "+id+", got "+(Director==null?"no director":Director.CurrentId));yield return null;}}
    static void Position(Vector3 at)
    {
        if(Phone!=null)Phone.SetOpen(false);StoryDirector.Warp(at,Vector3.zero,Vector3.zero);Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;Physics.SyncTransforms();var player=InteractionScanner.Active.transform;foreach(var zone in UnityEngine.Object.FindObjectsByType<NarrativeSignalZone>(FindObjectsSortMode.None))zone.Check(player);
    }
    static void Use(MonoBehaviour target,Vector3 at)
    {
        Position(at);var scan=InteractionScanner.Active;var colliders=target.GetComponentsInChildren<Collider>(true).Where(c=>c.enabled&&c.gameObject.activeInHierarchy&&!c.isTrigger).ToArray();if(colliders.Length==0)throw new Exception("No physical collider for "+target.name);
        foreach(var collider in colliders)foreach(var offset in new[]{Vector3.zero,Vector3.up*.35f,Vector3.down*.35f,Vector3.right*.35f,Vector3.left*.35f})
        {
            var point=collider.bounds.center+Vector3.Scale(collider.bounds.extents,offset);var ray=new Ray(scan.eyes.transform.position,(point-scan.eyes.transform.position).normalized);
            if(!ReferenceEquals(scan.Probe(ray),target))continue;scan.eyes.transform.rotation=Quaternion.LookRotation(ray.direction);scan.GetComponent<AlleyStudy.AlleyWalker>().SyncLookPitch();if(scan.TryInteract(ray))return;
        }
        throw new Exception("Occluded/out of range: "+target.GetType().Name+" "+target.name+" from "+at);
    }
    static IEnumerator Ride(int floor)
    {
        var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();Position(lift.cabin.TransformPoint(new Vector3(0,.05f,.85f)));
        var button=lift.cabin.GetComponentsInChildren<ElevatorButton>().First(b=>b.action==ElevatorButton.Action.Floor&&b.floor==floor);Use(button,InteractionScanner.Active.transform.position);
        double end=EditorApplication.timeSinceStartup+18;while(lift.currentFloor!=floor||lift.DoorAmount<.99f){lift.Tick(.1f);if(EditorApplication.timeSinceStartup>end)throw new Exception("Elevator failed to reach "+floor);yield return null;}
        Position(new Vector3(1.1f,(floor-1)*2.9f+.04f,10.8f));yield return .08f;
    }
    static IEnumerator Run()
    {
        Director.StartNight(1);double ready=EditorApplication.timeSinceStartup+30;while(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="Alley"||InteractionScanner.Active==null||InteractionScanner.Active.gameObject.scene.name!="Alley"){if(EditorApplication.timeSinceStartup>ready)throw new Exception("Alley did not load");yield return null;}yield return .2f;Check(Director.Opening&&Director.Presentation.blackout.alpha>.99f,"Opening starts black");Director.SkipOpening();yield return .1f;
        Check(Director.CurrentId=="arrival","Arrival objective active");var clock=Director.Clock;Director.Tick(120);Check(Director.Clock==clock,"Clock does not advance by elapsed gameplay time");int count=Director.state.messages.Count;Director.Publish(StoryEventKind.SequenceCompleted,"empty-elevator");Check(Director.CurrentId=="arrival"&&Director.state.messages.Count==count,"Out-of-order events do not advance story");
        Position(new Vector3(22,.05f,60));yield return .1f;Check(Director.state.messages.Any(m=>m.body.Contains("last guard")),"Supervisor approach messages delivered");
        UnityEngine.Object.FindObjectsByType<SceneEntrance>(FindObjectsSortMode.None).First(e=>e.destination=="Apartment").Travel();yield return WaitStep("begin");
        var utility=GameObject.Find("Electrical Room").GetComponentInChildren<MaintenanceDoor>();Use(utility,new Vector3(-1.3f,.05f,7.2f));yield return .4f;
        var key=UnityEngine.Object.FindObjectsByType<KeyPickup>(FindObjectsSortMode.None).First(k=>k.keyId=="Management");Use(key,new Vector3(-3.65f,.05f,7.35f));var officeDoor=Director.Binding("office.door").door;Use(officeDoor,new Vector3(-1.1f,.05f,5.75f));yield return .4f;
        var log=GameObject.Find("Management Room").GetComponentsInChildren<NarrativeInteractable>().First(r=>r.document!=null&&r.document.documentId=="guard-logbook");Use(log,new Vector3(-4.1f,.05f,3.0f));Check(Director.CurrentId=="begin","Optional logbook does not gate progression");Director.Presentation.documentScroll.verticalNormalizedPosition=0;Director.Presentation.Close();
        var pad=UnityEngine.Object.FindFirstObjectByType<NarrativeWorkstation>();Use(pad,new Vector3(-4.05f,.05f,3.1f));yield return WaitStep("rules");Phone.SetOpen(true);Phone.presentation.Tick(1);Phone.applications.Open(PhoneApplications.Page.Regulations);var rules=Phone.applications.GetComponentInChildren<UnityEngine.UI.ScrollRect>();rules.verticalNormalizedPosition=0;Phone.GetComponent<PhoneNarrativeBridge>().TickReading(.1f);yield return WaitStep("cctv");Phone.SetOpen(false);
        var monitor=UnityEngine.Object.FindFirstObjectByType<CctvSystem>().monitors[0];Use(monitor,new Vector3(-4.15f,.05f,3.7f));yield return WaitStep("camera.off");Check(Director.Binding("camera.fault").cameraFault.scriptedSignalLoss,"Designated camera has interference");
        var cameraPower=UnityEngine.Object.FindObjectsByType<PowerSwitch>(FindObjectsSortMode.None).First(s=>s.circuit!=null&&s.circuit.id=="CCTV Cameras"&&s.transform.position.y>0);Use(cameraPower,new Vector3(-4.4f,.05f,7.1f));yield return WaitStep("camera.on");Use(cameraPower,new Vector3(-4.4f,.05f,7.1f));yield return WaitStep("keys");Check(!Director.Binding("camera.fault").cameraFault.scriptedSignalLoss,"Restart restores CCTV feed");
        var stairKey=UnityEngine.Object.FindObjectsByType<KeyPickup>(FindObjectsSortMode.None).First(k=>k.keyId=="Stairwell");Use(stairKey,new Vector3(-4.05f,.05f,5.75f));yield return WaitStep("patrol2");yield return Ride(2);
        var door=UnityEngine.Object.FindObjectsByType<NarrativeInteractable>(FindObjectsSortMode.None).First(i=>i.signal=="inspect.door.203");Use(door,new Vector3(-.95f,2.94f,8.2f));var light2=UnityEngine.Object.FindObjectsByType<NarrativeInteractable>(FindObjectsSortMode.None).First(i=>i.signal=="inspect.light.2");Use(light2,new Vector3(0,2.94f,5.5f));yield return WaitStep("patrol3");yield return Ride(3);yield return WaitStep("patrol4");yield return Ride(4);
        Position(new Vector3(0,8.74f,7.6f));var fault=UnityEngine.Object.FindFirstObjectByType<NarrativeLightFault>();fault.Tick(.01f,InteractionScanner.Active.eyes.transform.position);yield return WaitStep("maintenance.off");Check(fault.Active,"Fourth-floor approach triggers flicker request");
        var lightPower=UnityEngine.Object.FindObjectsByType<PowerSwitch>(FindObjectsSortMode.None).First(s=>s.circuit!=null&&s.circuit.id=="Fourth Floor"&&s.transform.position.y>8);Use(lightPower,new Vector3(2.4f,8.74f,11.2f));yield return WaitStep("maintenance.on");Use(lightPower,new Vector3(2.4f,8.74f,11.2f));yield return WaitStep("verify4");Check(!fault.Active,"Circuit reset clears light fault");
        var light4=UnityEngine.Object.FindObjectsByType<NarrativeInteractable>(FindObjectsSortMode.None).First(i=>i.signal=="inspect.light.4");Use(light4,new Vector3(0,8.74f,5.5f));yield return WaitStep("return",5);Check(Director.Binding("door.slam").sound!=null,"Spatial slam completes without character spawn");yield return Ride(1);
        Position(new Vector3(-4.05f,.05f,5.75f));yield return WaitStep("returnkey");Use(stairKey,new Vector3(-4.05f,.05f,5.75f));yield return WaitStep("charge");var station=UnityEngine.Object.FindFirstObjectByType<PhoneChargingStation>();Use(station,new Vector3(-4.1f,.05f,2.59f));yield return WaitStep("telephone");Check(PhoneBattery.IsDocked,"Borrowed key returned and phone docked");var telephone=UnityEngine.Object.FindFirstObjectByType<NarrativeTelephone>();Use(telephone,new Vector3(-4.1f,.05f,2.59f));yield return WaitStep("sit",20);
        var seat=Director.Binding("security.chair").chair;Use(seat,new Vector3(-4.8f,.05f,4.3f));yield return WaitStep("empty");Check(seat.IsOccupied&&seat.standingLocked,"Seated elevator encounter locks standing only");seat.Stand();Check(seat.IsOccupied,"Standing prevented during encounter");var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();float speed=lift.metresPerSecond;lift.metresPerSecond=6;yield return WaitStep("collect",65);lift.metresPerSecond=speed;Check(lift.currentFloor==1&&lift.DoorAmount<.01f&&!seat.standingLocked,"Real elevator returns empty and closes; seat unlocks");seat.Stand();Use(station,new Vector3(-4.1f,.05f,2.59f));yield return WaitStep("report");
        Phone.SetOpen(true);Phone.presentation.Tick(1);Phone.applications.Open(PhoneApplications.Page.Records);var bridge=Phone.GetComponent<PhoneNarrativeBridge>();bridge.ShowReport();bridge.reportInput.text="Patrol completed on floors 2–4. CCTV restarted and 4F light restored. An unidentified caller disconnected.";bridge.SubmitReport();yield return WaitStep("leave");Check(Director.state.report.Length>12,"Nightly report accepted through phone");Phone.SetOpen(false);UnityEngine.Object.FindObjectsByType<SceneEntrance>(FindObjectsSortMode.None).First(e=>e.destination=="Alley").Travel();yield return WaitStep("alley.trigger");Position(new Vector3(12,.05f,58));yield return WaitStep("alley.voice");yield return WaitStep("home",15);Position(new Vector3(0,.05f,-1));yield return WaitStep("fade");yield return WaitStep("complete",6);Check(Director.state.completed&&Director.Presentation.completion.activeSelf,"Alley vocalization and final fade complete Night 1");Check(Director.state.completedSteps.Count==Director.definition.beats.Length,"Every step completed once");
    }
}
