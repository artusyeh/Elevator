using System;
using System.Collections.Generic;
using UnityEngine;

// Session state survives scene transitions; a new Play session starts without keys.
public static class BuildingKeys
{
    static readonly HashSet<string> held=new HashSet<string>(StringComparer.Ordinal);
    public static event Action Changed;
    public static bool Has(string id)=>!string.IsNullOrEmpty(id)&&held.Contains(id);
    public static bool Collect(string id)
    {
        if(string.IsNullOrEmpty(id)||!held.Add(id))return false;
        Changed?.Invoke();return true;
    }
    public static bool Return(string id)
    {
        if(string.IsNullOrEmpty(id)||!held.Remove(id))return false;
        Changed?.Invoke();return true;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession(){held.Clear();Changed=null;}
}
