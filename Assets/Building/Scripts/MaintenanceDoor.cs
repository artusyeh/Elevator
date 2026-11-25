using UnityEngine;

public sealed class MaintenanceDoor : MonoBehaviour,IInteractionTarget
{
    public Transform leaf;
    public bool open;
    public string InteractionPrompt=>open?"Close":"Open";
    public float InteractionRange=>2.2f;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled;
    public void Interact(InteractionScanner source){open=!open;}
    void Update(){leaf.localRotation=Quaternion.RotateTowards(leaf.localRotation,Quaternion.Euler(0,open?-90:0,0),90*Time.deltaTime);}
}
