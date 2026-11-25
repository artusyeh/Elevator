using UnityEngine;
using UnityEngine.Events;

// Optional adapter for future authored interactions; no action is installed by default.
public sealed class InteractionTarget : MonoBehaviour, IInteractionTarget
{
    public enum Verb { Open, Close, Inspect, Read, Press, Sit, PickUp }
    public Verb verb=Verb.Inspect;
    [Min(.1f)] public float interactionRange=2;
    public bool available;
    public bool showPrompt=true;
    public UnityEvent onInteract=new UnityEvent();
    public string InteractionPrompt=>!showPrompt?"":verb==Verb.PickUp?"Pick Up":verb.ToString();
    public float InteractionRange=>interactionRange;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&available;
    public void Interact(InteractionScanner source){if(CanInteract(source))onInteract.Invoke();}
}
