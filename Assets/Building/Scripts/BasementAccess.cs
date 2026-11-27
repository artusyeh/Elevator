using UnityEngine;

// Narrative code can change these settings later; no story events are implemented here.
public sealed class BasementAccess : MonoBehaviour
{
    public bool available=true;
    public bool requiresMaintenanceKey=true;
    public string keyId="Maintenance";
    public bool CanEnter=>BuildingKeys.PlaytestAccess||available&&(!requiresMaintenanceKey||BuildingKeys.Has(keyId));
}
