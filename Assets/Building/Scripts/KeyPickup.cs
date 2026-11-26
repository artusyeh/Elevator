using UnityEngine;

[DisallowMultipleComponent]
public sealed class KeyPickup : MonoBehaviour,IInteractionTarget
{
    public string keyId="Stairwell";
    public string label="";
    [Tooltip("For board keys, keep the hook interactive so a held key can be returned.")]
    public bool allowReturn;
    public GameObject visualRoot;
    [Min(.1f)] public float interactionRange=1.9f;
    public string InteractionPrompt=>(allowReturn&&BuildingKeys.Has(keyId)?"Return":"Pick Up")+(string.IsNullOrEmpty(label)?"":" "+label);
    public float InteractionRange=>interactionRange;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&(allowReturn||!BuildingKeys.Has(keyId));
    public void Interact(InteractionScanner source)
    {
        if(!CanInteract(source))return;
        if(allowReturn&&BuildingKeys.Has(keyId))BuildingKeys.Return(keyId);
        else BuildingKeys.Collect(keyId);
    }
    void OnEnable(){if(Application.isPlaying){BuildingKeys.Changed+=Refresh;Refresh();}}
    void OnDisable(){BuildingKeys.Changed-=Refresh;}
    void Refresh()
    {
        bool held=BuildingKeys.Has(keyId);
        if(visualRoot!=null&&visualRoot!=gameObject)visualRoot.SetActive(!held);
        else if(held)gameObject.SetActive(false);
    }
}
