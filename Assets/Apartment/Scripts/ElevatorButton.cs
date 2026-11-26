using UnityEngine;

public sealed class ElevatorButton : MonoBehaviour, IInteractionTarget
{
    public enum Action { Floor, Open, Close, Call, Alarm }
    public Action action;
    public int floor=1;
    public Renderer lamp;
    public Transform cap;
    [Min(.1f)] public float interactionRange=2.3f;
    public bool Pressed { get; private set; }
    ElevatorController cachedLift;
    bool lampInitialized;
    ElevatorController Lift=>cachedLift!=null?cachedLift:(cachedLift=Object.FindFirstObjectByType<ElevatorController>());
    public string InteractionPrompt=>action==Action.Open?"Open":action==Action.Close?"Close":"Press";
    public float InteractionRange=>interactionRange;
    public bool CanInteract(InteractionScanner source)
    {
        var lift=Lift;
        return isActiveAndEnabled&&lift!=null&&(action!=Action.Floor||lift.HasPower)&&(action!=Action.Close||lift.HasPower)&&((action!=Action.Open&&action!=Action.Close)||lift.CanOperateDoors);
    }
    public void Interact(InteractionScanner source){if(CanInteract(source))Press();}
    MaterialPropertyBlock block;
    Vector3 rest;
    float flash,travel;
    void Awake(){cachedLift=Object.FindFirstObjectByType<ElevatorController>();if(cap!=null)rest=cap.localPosition;}
    public void Press()
    {
        flash=.35f;travel=.15f;
        var lift=Lift;
        if(lift!=null){if(action==Action.Floor||action==Action.Call)lift.RequestFloor(floor);else if(action==Action.Open)lift.OpenDoors();else if(action==Action.Close)lift.CloseDoors();}
        SetLamp(true);
    }
    void Update()
    {
        flash=Mathf.Max(0,flash-Time.deltaTime);travel=Mathf.Max(0,travel-Time.deltaTime);
        var lift=Lift;
        bool selected=lift!=null&&(action==Action.Floor||action==Action.Call)&&lift.IsRequested(floor);
        SetLamp(selected||flash>0);
        if(cap!=null){var position=rest+(travel>0?Vector3.forward*.003f:Vector3.zero);if(cap.localPosition!=position)cap.localPosition=position;}
    }
    public void SetLamp(bool on)
    {
        if(lampInitialized&&Pressed==on)return;Pressed=on;if(lamp==null)return;lampInitialized=true;if(block==null)block=new MaterialPropertyBlock();lamp.GetPropertyBlock(block);
        block.SetColor("_BaseColor",on?new Color(.72f,.39f,.10f):new Color(.35f,.32f,.22f));
        block.SetColor("_EmissionColor",on?new Color(1.25f,.48f,.07f):Color.black);lamp.SetPropertyBlock(block);
    }
}
