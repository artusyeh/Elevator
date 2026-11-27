using UnityEngine;
using UnityEngine.Events;

public sealed class NarrativeBinding : MonoBehaviour
{
    public string id;
    public GameObject target;
    public Light lightSource;
    public StairAccessDoor door;
    public ElevatorController elevator;
    public CctvMonitor monitor;
    public NarrativeActor actor;
    public AudioSource sound;
    public ParticleSystem weather;
    public Animator animator;
    public ChairSeat chair;
    public NarrativeLightFault lightFault;
    public NarrativeTelephone telephone;
    public CctvChannel cameraFault;
    public bool soundBehindPlayer;
    [Min(.1f)]public float behindPlayerDistance=3;
    string animationState,completionSignal;
    public UnityEvent onEvent=new UnityEvent();
    public void Apply(NarrativeActionKind kind,bool value,float number,string text,AudioClip clip=null,string completionId=null)
    {
        switch(kind)
        {
            case NarrativeActionKind.DoorLock:if(door!=null)door.SetLocked(value);break;
            case NarrativeActionKind.DoorOpen:if(door!=null){door.open=value;}break;
            case NarrativeActionKind.Elevator:if(elevator!=null)elevator.RequestFloor(Mathf.RoundToInt(number));break;
            case NarrativeActionKind.CctvChannel:if(monitor!=null){int channel=Mathf.RoundToInt(number);if(channel<0&&monitor.system!=null)for(int i=0;i<monitor.system.channels.Length;i++)if(monitor.system.channels[i].location=="1F Lift Landing")channel=i;monitor.Select(Mathf.Max(0,channel));}break;
            case NarrativeActionKind.MoveActor:if(actor!=null)actor.Begin();break;
            case NarrativeActionKind.SetActive:if(target!=null)target.SetActive(value);break;
            case NarrativeActionKind.LightIntensity:if(lightSource!=null)lightSource.intensity=Mathf.Max(0,number);break;
            case NarrativeActionKind.Sound:if(sound!=null&&clip!=null){if(soundBehindPlayer&&Camera.main!=null){var view=Camera.main.transform;var forward=Vector3.ProjectOnPlane(view.forward,Vector3.up).normalized;if(forward.sqrMagnitude<.01f)forward=InteractionScanner.Active!=null?InteractionScanner.Active.transform.forward:Vector3.forward;transform.position=view.position-forward*behindPlayerDistance;}sound.PlayOneShot(clip,Mathf.Clamp01(number));StartCoroutine(AudioDone(clip.length,completionId??id+".audio"));}break;
            case NarrativeActionKind.Weather:if(weather!=null){if(value)weather.Play();else weather.Stop();}break;
            case NarrativeActionKind.InvokeBinding:onEvent.Invoke();break;
            case NarrativeActionKind.Animation:if(animator!=null){animationState=text;completionSignal=id+".animation";animator.Play(text);}break;
            case NarrativeActionKind.CameraFault:if(cameraFault!=null)cameraFault.scriptedSignalLoss=value;break;
            case NarrativeActionKind.LightFault:if(lightFault!=null)lightFault.SetFault(value);break;
            case NarrativeActionKind.RingTelephone:if(telephone!=null)telephone.Ring();break;
            case NarrativeActionKind.SeatLock:if(chair!=null)chair.standingLocked=value;break;
            case NarrativeActionKind.ElevatorDoors:if(elevator!=null){if(value)elevator.OpenDoors();else elevator.CloseDoors();}break;
            case NarrativeActionKind.CloseFloorDoors:foreach(var door in Object.FindObjectsByType<StairAccessDoor>(FindObjectsSortMode.None))if(Mathf.Abs(door.transform.position.y-number)<.3f)door.open=false;foreach(var gate in Object.FindObjectsByType<RoomGateLatch>(FindObjectsSortMode.None))if(Mathf.Abs(gate.transform.position.y-number)<.3f)gate.open=false;break;
        }
    }
    System.Collections.IEnumerator AudioDone(float seconds,string completion){yield return new WaitForSecondsRealtime(seconds);if(StoryDirector.Active!=null)StoryDirector.Active.Publish(StoryEventKind.AudioCompleted,completion);}
    void Update(){if(animator!=null&&!string.IsNullOrEmpty(animationState)){var state=animator.GetCurrentAnimatorStateInfo(0);if(state.IsName(animationState)&&state.normalizedTime>=1&&!animator.IsInTransition(0)){animationState=null;if(StoryDirector.Active!=null)StoryDirector.Active.Publish(StoryEventKind.AnimationCompleted,completionSignal);}}}
}
