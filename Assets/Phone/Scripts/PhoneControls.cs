using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public sealed class PhoneControls : MonoBehaviour
{
    public PhonePresentation presentation;
    public PhoneFlashlight flashlight;
    public PhoneApplications applications;
    public bool IsOpen {get;private set;}
    AlleyStudy.AlleyWalker player;
    InteractionScanner scanner;
    PhoneBattery battery;
    static bool flashlightState;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetSession(){flashlightState=false;}
    void Awake(){player=GetComponentInParent<AlleyStudy.AlleyWalker>();scanner=player.GetComponent<InteractionScanner>();battery=GetComponent<PhoneBattery>();SetFlashlight(flashlightState);}
    bool OtherInterface=>scanner!=null&&(scanner.menuOpen||scanner.cutscenePlaying||scanner.documentOpen);
    public void SetOpen(bool value)
    {
        if(value&&(OtherInterface||battery!=null&&!battery.CanUse))return;IsOpen=value;presentation.Raised=value;player.phoneInterfaceOpen=value;
        if(!value&&applications.modal!=null)applications.modal.SetActive(false);
        if(scanner!=null)scanner.phoneOpen=value;
        Cursor.lockState=value||OtherInterface?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=value||OtherInterface;
    }
    public void SetFlashlight(bool value){if(value&&battery!=null&&!battery.CanUse)value=false;flashlight.SetEnabled(value);flashlightState=flashlight.IsOn;presentation.FlashlightActive=flashlightState;presentation.Tick(0);}
    public void ToggleFlashlight(){SetFlashlight(!flashlight.IsOn);}
    void Update()
    {
        if(battery!=null&&!battery.CanUse){if(IsOpen)SetOpen(false);if(flashlight.IsOn)SetFlashlight(false);}
        if(OtherInterface){if(IsOpen)SetOpen(false);return;}
        var k=Keyboard.current;if(k==null)return;
        if(k.tabKey.wasPressedThisFrame)SetOpen(!IsOpen);
        var selected=UnityEngine.EventSystems.EventSystem.current!=null?UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject:null;
        bool typing=selected!=null&&selected.GetComponent<UnityEngine.UI.InputField>()!=null;
        if(k.fKey.wasPressedThisFrame&&!typing)ToggleFlashlight();
        if(IsOpen&&k.escapeKey.wasPressedThisFrame)SetOpen(false);
    }
    void OnDisable()
    {
        flashlightState=flashlight!=null&&flashlight.IsOn;
        if(IsOpen&&!OtherInterface){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
        IsOpen=false;if(presentation!=null)presentation.Raised=false;
        if(player!=null)player.phoneInterfaceOpen=false;if(scanner!=null)scanner.phoneOpen=false;
    }
}
