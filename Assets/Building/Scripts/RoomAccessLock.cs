using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoomAccessLock : MonoBehaviour,IInteractionTarget
{
    public string roomNumber;
    public string requiredKeyId;
    public bool locked=true;
    [Tooltip("Assign the operable entrance when this room's interior is built.")]
    public StairAccessDoor entrance;
    public Transform foldingGate;
    public RoomGateLatch gate;
    float gateAmount;
    public string InteractionPrompt=>gate!=null&&!gate.open&&(entrance==null||!entrance.open)?"Open Gate First":locked?(BuildingKeys.Has(requiredKeyId)?"Unlock Door ":"Locked Door ")+roomNumber:entrance!=null?entrance.InteractionPrompt+" Door":"Unlocked "+roomNumber;
    public float InteractionRange=>2.2f;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&(locked||entrance!=null);
    public void Interact(InteractionScanner source)
    {
        if(gate!=null&&!gate.open&&(entrance==null||!entrance.open))return;
        if(locked){if(!BuildingKeys.Has(requiredKeyId))return;locked=false;if(entrance!=null)entrance.SetLocked(false);}
        if(entrance!=null)entrance.Interact(source);
    }
    void Update()
    {
        if(gate!=null||foldingGate==null||entrance==null)return;
        gateAmount=Mathf.MoveTowards(gateAmount,entrance.open&&!entrance.locked?1:0,Time.deltaTime*1.5f);
        foldingGate.localScale=new Vector3(Mathf.Lerp(1,.10f,gateAmount),1,1);
    }
}
