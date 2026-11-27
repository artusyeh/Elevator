using UnityEngine;
using UnityEngine.UI;
public sealed class PhoneBatteryAlerts : MonoBehaviour
{
    public GameObject banner;
    public Text label;
    public float duration=5;
    float remaining;
    void Update()
    {
        var scan=InteractionScanner.Active;
        if(Time.timeScale<=0||(scan!=null&&(scan.menuOpen||scan.cutscenePlaying))){banner.SetActive(false);return;}
        if(remaining<=0){int level;if(PhoneBattery.TakeNotification(out level)){label.text="Phone battery low · "+level+"%";remaining=duration;}}
        banner.SetActive(remaining>0);remaining-=Time.unscaledDeltaTime;
    }
}
