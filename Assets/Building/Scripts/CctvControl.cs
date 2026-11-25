using UnityEngine;
public sealed class CctvControl : MonoBehaviour,IInteractionTarget
{
    public CctvMonitor monitor;
    public string InteractionPrompt=>"Switch Camera";
    public float InteractionRange=>2;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&monitor!=null&&monitor.Powered;
    public void Interact(InteractionScanner source){if(CanInteract(source))monitor.system.NextChannel(monitor);}
}
