using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoomGateLatch : MonoBehaviour,IInteractionTarget
{
    public string requiredKeyId;
    public Transform leaf;
    public bool locked=true;
    public bool open;
    public float openingAngle=105;
    public float degreesPerSecond=95;
    public string InteractionPrompt=>locked?(BuildingKeys.Has(requiredKeyId)?"Unlock Gate":"Locked Gate"):open?"Close Gate":"Open Gate";
    public float InteractionRange=>2.2f;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled;
    public void Interact(InteractionScanner source)
    {
        if(locked){if(!BuildingKeys.Has(requiredKeyId))return;locked=false;}
        open=!open;
    }
    public void Tick(float dt)
    {
        if(leaf!=null)leaf.localRotation=Quaternion.RotateTowards(leaf.localRotation,Quaternion.Euler(0,open&&!locked?openingAngle:0,0),degreesPerSecond*Mathf.Max(0,dt));
    }
    void Update(){Tick(Time.deltaTime);}
}
