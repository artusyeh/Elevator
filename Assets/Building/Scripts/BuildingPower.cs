using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-300)]
public sealed class BuildingPower : MonoBehaviour
{
    public static BuildingPower Active { get; private set; }
    public PowerCircuit[] circuits;
    public static event Action Changed;
    static readonly Dictionary<string,bool> states=new Dictionary<string,bool>();
    public static bool MainSupply { get; private set; }=true;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession(){states.Clear();MainSupply=true;Active=null;Changed=null;}
    void OnEnable(){Active=this;Changed?.Invoke();}
    void OnDisable(){if(Active==this)Active=null;}
    public static bool IsEnabled(PowerCircuit circuit){if(circuit==null)return true;return states.TryGetValue(circuit.id,out bool value)?value:circuit.startsEnabled;}
    public static bool IsPowered(PowerCircuit circuit)=>circuit==null||IsEnabled(circuit)&&(MainSupply||circuit.backupSupply);
    public static void SetCircuit(PowerCircuit circuit,bool value){if(circuit==null)return;states[circuit.id]=value;Changed?.Invoke();}
    public static void SetMain(bool value){MainSupply=value;Changed?.Invoke();}
}
