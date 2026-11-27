using System;
using UnityEngine;
using UnityEngine.UI;
public sealed class PhoneApplications : MonoBehaviour
{
    public enum Page { Home,WhatsApp,Management,Conversation,Tasks,Shift,Directory,Maintenance,Regulations,Records,Dialer }
    [Serializable]public struct ScreenPage {public Page page;public GameObject root;}
    public ScreenPage[] pages;
    public PhoneContent content;
    public GameObject home,whatsApp,management;
    public Button whatsAppButton,managementButton,homeButton,backButton;
    public Text timeLabel,batteryLabel;
    public PhoneBattery battery;
    public Text largeTime,dateLabel,conversationTitle,incomingText,outgoingText,sentText,dialText;
    public InputField messageInput;
    public GameObject modal,glance;
    public Text modalText,glanceBattery;
    PhoneIcon[] batteryIcons;
    string digits="";
    public Page CurrentPage {get;private set;}
    float nextStatus;
    int shownBattery=-1;
    void OnEnable(){PhoneBattery.Changed+=OnChargeChanged;}
    void OnDisable(){PhoneBattery.Changed-=OnChargeChanged;}
    void OnChargeChanged(float value){if(battery!=null&&shownBattery!=battery.Percentage){UpdateStatus();shownBattery=battery.Percentage;}}
    void Awake()
    {
        batteryIcons=GetComponentsInChildren<PhoneIcon>(true);
        if(pages==null||pages.Length==0){whatsAppButton.onClick.AddListener(()=>Open(Page.WhatsApp));managementButton.onClick.AddListener(()=>Open(Page.Management));homeButton.onClick.AddListener(GoHome);backButton.onClick.AddListener(GoHome);}
        GoHome();UpdateStatus();RefreshTasks();
    }
    public void Open(Page page)
    {
        if(page==Page.Shift||page==Page.Directory)page=Page.Management;
        if(UnityEngine.EventSystems.EventSystem.current!=null)UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        CurrentPage=page;if(modal!=null)modal.SetActive(false);
        if(pages!=null&&pages.Length>0){foreach(var entry in pages)if(entry.root!=null)entry.root.SetActive(entry.page==page);}else{home.SetActive(page==Page.Home);whatsApp.SetActive(page==Page.WhatsApp);management.SetActive(page==Page.Management);}
        var manager=GetComponent<PhoneManagement>();if(manager!=null){manager.ShowList();manager.Refresh();}
        if(StoryDirector.Active!=null)StoryDirector.Active.Publish(StoryEventKind.PhonePageViewed,page.ToString());
    }
    public void GoHome(){Open(Page.Home);}
    public void Activate(PhoneNavigationButton button)
    {
        switch(button.action)
        {
            case PhoneNavigationButton.Action.Navigate:Open(button.page);break;
            case PhoneNavigationButton.Action.Conversation:
                var storyMessages=GetComponent<PhoneNarrativeBridge>();if(storyMessages!=null&&storyMessages.OpenConversation(button.index))break;
                var c=content.contacts[Mathf.Clamp(button.index,0,content.contacts.Length-1)];conversationTitle.text=c.name;incomingText.text=c.incoming;outgoingText.text=c.outgoing;sentText.text="";messageInput.text="";Open(Page.Conversation);break;
            case PhoneNavigationButton.Action.Back:if(modal!=null&&modal.activeSelf)modal.SetActive(false);else if(CurrentPage==Page.Tasks||CurrentPage==Page.Maintenance||CurrentPage==Page.Regulations||CurrentPage==Page.Records)Open(Page.Management);else Open(CurrentPage==Page.Conversation?Page.WhatsApp:Page.Home);break;
            case PhoneNavigationButton.Action.Call:modalText.text="Call unavailable\n\nUnable to connect.";modal.SetActive(true);break;
            case PhoneNavigationButton.Action.Flashlight:GetComponent<PhoneControls>().ToggleFlashlight();break;
            case PhoneNavigationButton.Action.Task:var managementUi=GetComponent<PhoneManagement>();if(managementUi!=null)managementUi.ShowTask(button.index);break;
            case PhoneNavigationButton.Action.SendMessage:var messaging=GetComponent<PhoneNarrativeBridge>();if(messaging!=null&&messaging.StoryActive)messaging.SendMessage(messageInput.text);else if(!string.IsNullOrWhiteSpace(messageInput.text)){sentText.text=messageInput.text.Trim()+"\nDraft only · not delivered";messageInput.text="";}break;
            case PhoneNavigationButton.Action.Record:GetComponent<PhoneNarrativeBridge>().ShowRecord(button.index);break;
            case PhoneNavigationButton.Action.BeginShift:GetComponent<PhoneNarrativeBridge>().BeginShift();break;
            case PhoneNavigationButton.Action.ShowReport:GetComponent<PhoneNarrativeBridge>().ShowReport();break;
            case PhoneNavigationButton.Action.CancelReport:GetComponent<PhoneNarrativeBridge>().CancelReport();break;
            case PhoneNavigationButton.Action.SubmitReport:GetComponent<PhoneNarrativeBridge>().SubmitReport();break;
            case PhoneNavigationButton.Action.Dial:if(digits.Length<16)digits+=button.argument;dialText.text=digits;break;
            case PhoneNavigationButton.Action.ClearDial:if(digits.Length>0)digits=digits.Substring(0,digits.Length-1);dialText.text=digits;break;
        }
    }
    void RefreshTasks()
    {
        var manager=GetComponent<PhoneManagement>();if(manager!=null)manager.Refresh();
    }
    void UpdateStatus(){var now=StoryDirector.Active!=null&&StoryDirector.Active.state!=null?StoryDirector.Active.Clock:DateTime.Now;if(timeLabel!=null)timeLabel.text=now.ToString("HH:mm");if(largeTime!=null)largeTime.text=now.ToString("HH:mm");if(dateLabel!=null)dateLabel.text=now.ToString("dddd, d MMMM",System.Globalization.CultureInfo.InvariantCulture);string level=battery.Percentage+"%";if(batteryLabel!=null)batteryLabel.text=level;if(glanceBattery!=null)glanceBattery.text=level+"\nBATTERY";if(batteryIcons!=null)foreach(var icon in batteryIcons)if(icon!=null&&icon.symbol==PhoneIcon.Symbol.Battery){icon.SetBattery(battery.Charge/100);icon.color=battery.Charge<=10?new Color(.83f,.43f,.35f):battery.Charge<=20?new Color(.8f,.67f,.4f):new Color(.91f,.94f,.95f);}}
    void Update(){if(Time.unscaledTime<nextStatus)return;nextStatus=Time.unscaledTime+1;UpdateStatus();shownBattery=battery.Percentage;}
    void LateUpdate(){if(glance!=null){var show=GetComponent<PhonePresentation>();glance.SetActive(show.FlashlightActive&&!show.Raised&&show.Amount<.1f);}}
}
