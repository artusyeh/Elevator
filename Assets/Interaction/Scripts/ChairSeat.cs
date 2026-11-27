using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ChairSeat : MonoBehaviour,IInteractionTarget
{
    public Transform eyePosition;
    public Transform standPosition;
    public float interactionRange=2.2f;
    public string InteractionPrompt=>occupant!=null?"Stand Up":"Sit";
    public float InteractionRange=>interactionRange;
    public bool IsOccupied=>occupant!=null;
    public bool standingLocked;
    public static event System.Action<ChairSeat,bool> OccupancyChanged;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetSeatEvents(){OccupancyChanged=null;}
    AlleyStudy.AlleyWalker occupant;
    CharacterController body;
    Vector3 previousEyePosition;
    Vector3 fallbackStand;
    bool bodyWasEnabled;
    int sitFrame;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled;
    public void Interact(InteractionScanner source)
    {
        if(occupant!=null){Stand();return;}
        var player=source!=null?source.GetComponent<AlleyStudy.AlleyWalker>():null;if(player==null||player.seated||eyePosition==null)return;
        occupant=player;body=player.GetComponent<CharacterController>();bodyWasEnabled=body.enabled;fallbackStand=player.transform.position;previousEyePosition=player.eyes.transform.localPosition;
        body.enabled=false;player.seated=true;player.transform.position=eyePosition.position-Vector3.up*previousEyePosition.y;player.transform.rotation=Quaternion.Euler(0,eyePosition.eulerAngles.y,0);player.eyes.transform.localRotation=Quaternion.identity;player.SyncLookPitch();sitFrame=Time.frameCount;OccupancyChanged?.Invoke(this,true);
    }
    public void Stand()
    {
        if(standingLocked){if(StoryDirector.Active!=null)StoryDirector.Active.Presentation.Say("Aki","Wait... the lift's moving.",3);return;}
        if(occupant==null)return;var player=occupant;
        player.eyes.transform.localPosition=previousEyePosition;player.transform.position=standPosition!=null?standPosition.position:fallbackStand;player.seated=false;body.enabled=bodyWasEnabled;
        var scanner=player.GetComponent<InteractionScanner>();if(scanner!=null)scanner.IgnoreInputThisFrame();occupant=null;body=null;OccupancyChanged?.Invoke(this,false);
    }
    void Update()
    {
        if(occupant==null||Time.frameCount==sitFrame||Keyboard.current==null)return;
        var scanner=occupant.GetComponent<InteractionScanner>();
        if(scanner!=null&&(scanner.phoneOpen||scanner.menuOpen||scanner.documentOpen||scanner.cutscenePlaying))return;
        if(Keyboard.current.eKey.wasPressedThisFrame)Stand();
    }
    void OnDisable(){standingLocked=false;if(occupant!=null)Stand();}
}
