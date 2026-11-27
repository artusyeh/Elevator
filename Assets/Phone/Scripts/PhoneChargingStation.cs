using UnityEngine;
public sealed class PhoneChargingStation : MonoBehaviour,IInteractionTarget
{
    public string stationId="security-office-desk";
    [Min(0)]public float chargingPercentPerMinute=12;
    public bool chargingEnabled=true;
    public Transform phonePlacement;
    public GameObject dockedPhone;
    public float interactionRange=2;
    PhoneBattery cachedBattery;
    void OnEnable(){if(Application.isPlaying)Refresh();}
    public string InteractionPrompt=>PhoneBattery.DockId==stationId?"Collect Phone":"Place Phone";
    public float InteractionRange=>interactionRange;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&source!=null&&(!PhoneBattery.IsDocked||PhoneBattery.DockId==stationId)&&source.GetComponentInChildren<PhoneBattery>(true)!=null;
    public void Interact(InteractionScanner source)
    {
        if(!CanInteract(source))return;var battery=source.GetComponentInChildren<PhoneBattery>(true);
        if(PhoneBattery.DockId==stationId)battery.Collect(stationId);else battery.Dock(stationId,chargingEnabled?chargingPercentPerMinute:0);
        Refresh();source.IgnoreInputThisFrame();
    }
    void LateUpdate(){Refresh();}
    void Refresh(){bool occupied=PhoneBattery.DockId==stationId;if(dockedPhone!=null)dockedPhone.SetActive(occupied);if(occupied){if(cachedBattery==null)cachedBattery=Object.FindFirstObjectByType<PhoneBattery>();if(cachedBattery!=null)cachedBattery.SetChargingRate(stationId,chargingEnabled?chargingPercentPerMinute:0);}}
}
