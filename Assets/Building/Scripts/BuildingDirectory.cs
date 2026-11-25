using UnityEngine;

public sealed class BuildingDirectory : ScriptableObject
{
    [System.Serializable] public struct Unit {public int floor;public string number;}
    public Unit[] units;
}
