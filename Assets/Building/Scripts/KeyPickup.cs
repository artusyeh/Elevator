using UnityEngine;

[DisallowMultipleComponent]
public sealed class KeyPickup : MonoBehaviour,IInteractionTarget
{
    public string keyId="Stairwell";
    [Min(.1f)] public float interactionRange=1.9f;
    public string InteractionPrompt=>"Pick Up";
    public float InteractionRange=>interactionRange;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&!BuildingKeys.Has(keyId);
    public void Interact(InteractionScanner source)
    {
        if(CanInteract(source))BuildingKeys.Collect(keyId);
    }
    void OnEnable(){BuildingKeys.Changed+=Refresh;Refresh();}
    void OnDisable(){BuildingKeys.Changed-=Refresh;}
    void Refresh(){if(BuildingKeys.Has(keyId))gameObject.SetActive(false);}
}
