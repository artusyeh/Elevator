using UnityEngine;

[CreateAssetMenu(menuName="Building/Power Circuit")]
public sealed class PowerCircuit : ScriptableObject
{
    public string id;
    public string label;
    public bool startsEnabled=true;
    public bool backupSupply;
}
