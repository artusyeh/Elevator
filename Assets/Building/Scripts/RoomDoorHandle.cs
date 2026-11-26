using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoomDoorHandle : MonoBehaviour,IInteractionTarget
{
    public RoomAccessLock roomLock;
    public string InteractionPrompt=>roomLock!=null?roomLock.InteractionPrompt:"Door";
    public float InteractionRange=>2.2f;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&roomLock!=null;
    public void Interact(InteractionScanner source){if(roomLock!=null)roomLock.Interact(source);}
}
