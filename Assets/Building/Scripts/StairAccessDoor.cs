using UnityEngine;

public sealed class StairAccessDoor : MonoBehaviour,IInteractionTarget
{
    public Transform leaf;
    public bool locked=true;
    [Tooltip("A matching key permits unlocking. Leave empty for a lock controlled only by SetLocked.")]
    public string requiredKeyId="Stairwell";
    public bool open;
    public float openingAngle=-95;
    public float degreesPerSecond=85;
    public string InteractionPrompt=>locked?(BuildingKeys.Has(requiredKeyId)?"Unlock":"Locked"):open?"Close":"Open";
    public float InteractionRange=>2.2f;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled;
    public void SetLocked(bool value){locked=value;if(value)open=false;}
    public void Interact(InteractionScanner source)
    {
        if(locked){if(!BuildingKeys.Has(requiredKeyId))return;locked=false;}
        open=!open;
    }
    public void Tick(float dt){if(leaf!=null)leaf.localRotation=Quaternion.RotateTowards(leaf.localRotation,Quaternion.Euler(0,open&&!locked?openingAngle:0,0),degreesPerSecond*Mathf.Max(0,dt));}
    void Update(){Tick(Time.deltaTime);}
}
