using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(200)]
[DisallowMultipleComponent]
public sealed class InteractionScanner : MonoBehaviour
{
    public static InteractionScanner Active { get; private set; }
    public static event System.Action<IInteractionTarget> Interacted;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetEvents(){Interacted=null;}
    public Camera eyes;
    [Min(.1f)] public float interactionDistance=2.7f;
    public LayerMask detectionMask=~0;
    [Header("Visibility and input gates")]
    public bool menuOpen;
    public bool cutscenePlaying;
    public bool documentOpen;
    public bool phoneOpen;
    int ignoreInputFrame=-1;
    public void IgnoreInputThisFrame(){ignoreInputFrame=Time.frameCount;}
    public IInteractionTarget FocusedTarget { get; private set; }
    public bool UiVisible=>isActiveAndEnabled&&eyes!=null&&eyes.isActiveAndEnabled&&eyes==Camera.main&&
        (walker==null||walker.isActiveAndEnabled)&&Cursor.lockState==CursorLockMode.Locked&&Time.timeScale>0&&
        !menuOpen&&!cutscenePlaying&&!documentOpen&&!phoneOpen;
    readonly RaycastHit[] buffer=new RaycastHit[32];
    AlleyStudy.AlleyWalker walker;
    void Awake(){walker=GetComponent<AlleyStudy.AlleyWalker>();}
    void OnEnable(){Active=this;}
    void OnDisable(){if(Active==this)Active=null;FocusedTarget=null;}
    public void SetMenuOpen(bool value){menuOpen=value;if(value)FocusedTarget=null;}
    public void SetCutscenePlaying(bool value){cutscenePlaying=value;if(value)FocusedTarget=null;}
    public void SetDocumentOpen(bool value){documentOpen=value;if(value)FocusedTarget=null;}
    public IInteractionTarget Probe(Ray ray,float maximumDistance=-1)
    {
        float limit=maximumDistance>0?Mathf.Min(maximumDistance,interactionDistance):interactionDistance;
        int count=Physics.RaycastNonAlloc(ray,buffer,limit,detectionMask,QueryTriggerInteraction.Ignore);
        // NonAlloc can return an arbitrary subset when full, so never trust a saturated buffer for occlusion.
        var hits=count==buffer.Length?Physics.RaycastAll(ray,limit,detectionMask,QueryTriggerInteraction.Ignore):buffer;
        if(hits!=buffer)count=hits.Length;
        RaycastHit closest=default;float distance=float.MaxValue;
        for(int i=0;i<count;i++)
        {
            var hit=hits[i];if(hit.collider==null||hit.collider.transform.IsChildOf(transform))continue;
            if(hit.distance<distance){distance=hit.distance;closest=hit;}
        }
        if(closest.collider==null)return null;
        foreach(var component in closest.collider.GetComponentsInParent<MonoBehaviour>())
        {
            if(!component.isActiveAndEnabled||!(component is IInteractionTarget target))continue;
            if(distance<=Mathf.Min(limit,target.InteractionRange)&&target.CanInteract(this))return target;
        }
        return null;
    }
    public void RefreshFocus()
    {
        FocusedTarget=UiVisible?Probe(new Ray(eyes.transform.position,eyes.transform.forward)):null;
    }
    public bool TryInteract(Ray ray,float maximumDistance=-1,bool elevatorOnly=false)
    {
        if(!UiVisible)return false;
        var target=Probe(ray,maximumDistance);if(target==null||(elevatorOnly&&!(target is ElevatorButton)))return false;
        target.Interact(this);Interacted?.Invoke(target);return true;
    }
    public bool InteractFocused()
    {
        RefreshFocus();if(FocusedTarget==null)return false;
        var target=FocusedTarget;target.Interact(this);Interacted?.Invoke(target);return true;
    }
    void LateUpdate()
    {
        if(ignoreInputFrame==Time.frameCount)return;
        RefreshFocus();if(FocusedTarget==null)return;
        bool use=Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame;
        bool click=FocusedTarget is ElevatorButton&&Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame;
        if(use||click)InteractFocused();
    }
}
