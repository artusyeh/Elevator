using System;
using UnityEngine;
using UnityEngine.UI;
public sealed class PhoneApplications : MonoBehaviour
{
    public enum Page { Home,WhatsApp,Management,Conversation,Tasks,Shift,Directory,Maintenance,Regulations,Reports,Dialer }
    [Serializable]public struct ScreenPage {public Page page;public GameObject root;}
    public ScreenPage[] pages;
    public PhoneContent content;
    public GameObject home,whatsApp,management;
    public Button whatsAppButton,managementButton,homeButton,backButton;
    public Text timeLabel,batteryLabel;
    public PhoneBattery battery;
    public Text largeTime,dateLabel,conversationTitle,incomingText,outgoingText,sentText,progressText,dialText,reportFeedback;
    public Text[] taskLabels;
    public Image progressFill;
    public PhoneIcon[] taskChecks;
    public InputField messageInput,reportInput;
    public GameObject modal,glance;
    public Text modalText,glanceBattery;
    bool[] done;
    string digits="";
    public Page CurrentPage {get;private set;}
    float nextStatus;
    void Awake()
    {
        done=new bool[content!=null&&content.tasks!=null?content.tasks.Length:0];
        if(pages==null||pages.Length==0){whatsAppButton.onClick.AddListener(()=>Open(Page.WhatsApp));managementButton.onClick.AddListener(()=>Open(Page.Management));homeButton.onClick.AddListener(GoHome);backButton.onClick.AddListener(GoHome);}
        GoHome();UpdateStatus();RefreshTasks();
    }
    public void Open(Page page)
    {
        if(UnityEngine.EventSystems.EventSystem.current!=null)UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        CurrentPage=page;if(modal!=null)modal.SetActive(false);
        if(pages!=null&&pages.Length>0){foreach(var entry in pages)if(entry.root!=null)entry.root.SetActive(entry.page==page);}else{home.SetActive(page==Page.Home);whatsApp.SetActive(page==Page.WhatsApp);management.SetActive(page==Page.Management);}
        if(page==Page.Shift||page==Page.Tasks)RefreshTasks();
    }
    public void GoHome(){Open(Page.Home);}
    public void Activate(PhoneNavigationButton button)
    {
        switch(button.action)
        {
            case PhoneNavigationButton.Action.Navigate:Open(button.page);break;
            case PhoneNavigationButton.Action.Conversation:
                var c=content.contacts[Mathf.Clamp(button.index,0,content.contacts.Length-1)];conversationTitle.text=c.name;incomingText.text=c.incoming;outgoingText.text=c.outgoing;sentText.text="";messageInput.text="";Open(Page.Conversation);break;
            case PhoneNavigationButton.Action.Back:if(modal!=null&&modal.activeSelf)modal.SetActive(false);else Open(CurrentPage==Page.Conversation?Page.WhatsApp:CurrentPage==Page.WhatsApp||CurrentPage==Page.Management||CurrentPage==Page.Dialer?Page.Home:Page.Management);break;
            case PhoneNavigationButton.Action.Call:modalText.text="Call unavailable\n\nUnable to connect.";modal.SetActive(true);break;
            case PhoneNavigationButton.Action.Flashlight:GetComponent<PhoneControls>().ToggleFlashlight();break;
            case PhoneNavigationButton.Action.Task:if(button.index>=0&&button.index<done.Length){done[button.index]=!done[button.index];RefreshTasks();}break;
            case PhoneNavigationButton.Action.SendMessage:if(!string.IsNullOrWhiteSpace(messageInput.text)){sentText.text=messageInput.text.Trim()+"\nDraft only · not delivered";messageInput.text="";}break;
            case PhoneNavigationButton.Action.SubmitReport:reportFeedback.text=string.IsNullOrWhiteSpace(reportInput.text)?"Enter a note first.":"Draft saved for this session.";break;
            case PhoneNavigationButton.Action.Dial:if(digits.Length<16)digits+=button.argument;dialText.text=digits;break;
            case PhoneNavigationButton.Action.ClearDial:if(digits.Length>0)digits=digits.Substring(0,digits.Length-1);dialText.text=digits;break;
        }
    }
    void RefreshTasks()
    {
        if(done==null)return;int complete=0;for(int i=0;i<done.Length;i++){if(done[i])complete++;if(taskLabels!=null&&i<taskLabels.Length)taskLabels[i].text=done[i]?"Done":"To do";if(taskChecks!=null&&i<taskChecks.Length)taskChecks[i].enabled=done[i];}
        if(progressText!=null)progressText.text=complete+" of "+done.Length+" checks completed";if(progressFill!=null)progressFill.fillAmount=done.Length==0?0:(float)complete/done.Length;
    }
    void UpdateStatus(){var now=DateTime.Now;if(timeLabel!=null)timeLabel.text=now.ToString("HH:mm");if(largeTime!=null)largeTime.text=now.ToString("HH:mm");if(dateLabel!=null)dateLabel.text=now.ToString("dddd, d MMMM",System.Globalization.CultureInfo.InvariantCulture);string level=battery.Percentage+"%";if(batteryLabel!=null)batteryLabel.text=level;if(glanceBattery!=null)glanceBattery.text=level+"\nBATTERY";}
    void Update(){if(Time.unscaledTime<nextStatus)return;nextStatus=Time.unscaledTime+1;UpdateStatus();}
    void LateUpdate(){if(glance!=null){var show=GetComponent<PhonePresentation>();glance.SetActive(show.FlashlightActive&&!show.Raised&&show.Amount<.1f);}}
}
