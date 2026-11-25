using UnityEngine;

public sealed class PowerSwitch : MonoBehaviour,IInteractionTarget
{
    public PowerCircuit circuit;
    public bool mainBreaker;
    public Transform lever;
    public Renderer lamp;
    public string InteractionPrompt=>Setting?"Switch Off":"Switch On";
    public float InteractionRange=>2;
    bool Setting=>mainBreaker?BuildingPower.MainSupply:BuildingPower.IsEnabled(circuit);
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled;
    public void Interact(InteractionScanner source){if(mainBreaker)BuildingPower.SetMain(!Setting);else BuildingPower.SetCircuit(circuit,!Setting);}
    void OnEnable(){BuildingPower.Changed+=Refresh;Refresh();}
    void OnDisable(){BuildingPower.Changed-=Refresh;}
    void Refresh()
    {
        if(lever!=null)lever.localRotation=Quaternion.Euler(Setting?-28:28,0,0);
        if(lamp!=null){var b=new MaterialPropertyBlock();lamp.GetPropertyBlock(b);bool powered=mainBreaker?BuildingPower.MainSupply:BuildingPower.IsPowered(circuit);b.SetColor("_EmissionColor",powered?new Color(.05f,.34f,.12f):Color.black);lamp.SetPropertyBlock(b);}
    }
}
