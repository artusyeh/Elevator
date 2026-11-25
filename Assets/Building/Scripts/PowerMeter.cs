using UnityEngine;
public sealed class PowerMeter : MonoBehaviour
{
 public PowerCircuit circuit;
 public Transform needle;
 void OnEnable(){BuildingPower.Changed+=Refresh;Refresh();}
 void OnDisable(){BuildingPower.Changed-=Refresh;}
 void Refresh(){if(needle!=null)needle.localRotation=Quaternion.Euler(0,0,BuildingPower.IsPowered(circuit)?-29.33f:55);}
}
