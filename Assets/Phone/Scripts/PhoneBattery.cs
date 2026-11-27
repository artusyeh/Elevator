using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-200)]
public sealed class PhoneBattery : MonoBehaviour
{
    [FormerlySerializedAs("percentage"),Range(0,100)] public float startingPercentage=100;
    [Header("Percentage per real gameplay minute (exclusive modes)")]
    [Min(0)] public float idleDrain=.1f,screenDrain=.4f,flashlightDrain=1.5f,screenAndFlashlightDrain=1.8f;
    [Range(.5f,1)] public float lowChargeBrightness=.78f;
    public UnityEvent<float> chargeChanged=new UnityEvent<float>();
    public UnityEvent<int> lowBattery=new UnityEvent<int>();
    public UnityEvent depleted=new UnityEvent();
    static bool initialized,notified20,notified10,shutdown;
    static float charge,drainMultiplier=1,chargingRate;
    static string dockId;
    static readonly Queue<int> notifications=new Queue<int>();
    public static event Action<float> Changed;
    public static event Action<bool> DockChanged;
    PhoneControls controls;
    public float Charge=>initialized?charge:startingPercentage;
    public int Percentage=>Charge<=0?0:Mathf.Clamp(Mathf.CeilToInt(Charge),1,100);
    public bool CanUse=>Charge>0&&!shutdown&&!IsDocked;
    public float ScreenBrightness=>!CanUse?0:Charge<=5?lowChargeBrightness:1;
    public static bool IsDocked=>!string.IsNullOrEmpty(dockId);
    public static string DockId=>dockId;
    public bool IsShutdown=>shutdown;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession(){initialized=false;charge=100;dockId=null;shutdown=false;notified20=notified10=false;drainMultiplier=1;chargingRate=0;notifications.Clear();Changed=null;DockChanged=null;}
    void Awake(){controls=GetComponent<PhoneControls>();if(!initialized){charge=Mathf.Clamp(startingPercentage,0,100);initialized=true;}}
    void Update(){var scan=InteractionScanner.Active;if(Time.timeScale>0&&(scan==null||!scan.menuOpen))Tick(Time.unscaledDeltaTime);}
    public void Tick(float gameplaySeconds)
    {
        float minutes=Mathf.Max(0,gameplaySeconds)/60;
        if(IsDocked){SetCharge(charge+Mathf.Max(0,chargingRate)*minutes);return;}
        bool screen=controls!=null&&controls.IsOpen;
        bool light=controls!=null&&controls.flashlight!=null&&controls.flashlight.IsOn;
        float rate=screen?(light?screenAndFlashlightDrain:screenDrain):(light?flashlightDrain:idleDrain);
        SetCharge(charge-Mathf.Max(0,rate)*Mathf.Max(0,drainMultiplier)*minutes);
    }
    public void SetCharge(float value)
    {
        float previous=charge;charge=Mathf.Clamp(value,0,100);initialized=true;
        if(previous>20&&charge<=20&&!notified20){notified20=true;notifications.Enqueue(20);lowBattery.Invoke(20);}
        if(previous>10&&charge<=10&&!notified10){notified10=true;notifications.Enqueue(10);lowBattery.Invoke(10);}
        if(!Mathf.Approximately(previous,charge)){chargeChanged.Invoke(charge);Changed?.Invoke(charge);}
        if(previous>0&&charge<=0){depleted.Invoke();DisableUse();}
    }
    public void BeginNight(float percentage){notified20=notified10=false;notifications.Clear();shutdown=false;drainMultiplier=1;SetCharge(percentage);}
    public void ResetForNight(float percentage){dockId=null;chargingRate=0;BeginNight(percentage);DockChanged?.Invoke(false);}
    public void RestoreStoryState(float percentage,string dock){dockId=dock;chargingRate=string.IsNullOrEmpty(dock)?0:12;shutdown=false;SetCharge(percentage);DockChanged?.Invoke(IsDocked);}
    public void SetDrainMultiplier(float multiplier){drainMultiplier=Mathf.Max(0,multiplier);}
    public void SetShutdown(bool value){shutdown=value;if(value)DisableUse();}
    public bool Dock(string stationId,float percentPerMinute)
    {
        if(IsDocked||string.IsNullOrEmpty(stationId))return false;
        dockId=stationId;chargingRate=Mathf.Max(0,percentPerMinute);DisableUse();DockChanged?.Invoke(true);return true;
    }
    public bool Collect(string stationId){if(dockId!=stationId)return false;dockId=null;chargingRate=0;DockChanged?.Invoke(false);return true;}
    public void SetChargingRate(string stationId,float rate){if(dockId==stationId)chargingRate=Mathf.Max(0,rate);}
    void DisableUse(){if(controls==null)controls=GetComponent<PhoneControls>();if(controls!=null){if(controls.IsOpen)controls.SetOpen(false);controls.SetFlashlight(false);}}
    public static bool TakeNotification(out int threshold){threshold=0;if(notifications.Count==0)return false;threshold=notifications.Dequeue();return true;}
}
