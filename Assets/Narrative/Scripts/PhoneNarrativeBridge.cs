using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(50)]
public sealed class PhoneNarrativeBridge : MonoBehaviour
{
    public RectTransform taskContent,maintenanceContent,recordsContent,messageContent;
    public PhoneStoryRow taskTemplate,requestTemplate,recordTemplate;
    public GameObject incomingTemplate,outgoingTemplate,dateTemplate;
    public GameObject messageScroll,reportForm,beginButton,reportButton;
    public InputField reportInput;
    public GameObject[] legacyMessages,unreadBadges;
    public Text[] contactNames,contactPreviews,contactTimes,unreadCounts;
    public Text dashboardNight,dashboardStatus,dashboardNoticeTitle,dashboardNoticeBody,dashboardFooter;
    PhoneApplications apps;
    PhoneManagement manager;
    int taskCount=-1,requestCount=-1,recordCount=-1,messageCount=-1;
    string currentContact="Supervisor";
    float readingTime;
    bool refreshing;
    public bool StoryActive=>StoryDirector.Active!=null&&StoryDirector.Active.state!=null;
    void Awake(){apps=GetComponent<PhoneApplications>();manager=GetComponent<PhoneManagement>();}
    void OnEnable(){StoryDirector.Changed+=Refresh;}
    void OnDisable(){StoryDirector.Changed-=Refresh;}
    void Start(){Refresh();}
    public static string Contact(int index)=>index==1?"Landlord":index==2?"Maintenance":"Supervisor";
    public void Refresh()
    {
        if(!StoryActive||apps==null)return;
        RefreshManagement();var director=StoryDirector.Active;
        if(dashboardNight!=null)dashboardNight.text="Night "+director.state.night.ToString("00")+" · "+director.definition.shiftLabel;
        if(dashboardStatus!=null)dashboardStatus.text=director.ShiftStatus;
        if(dashboardNoticeTitle!=null)dashboardNoticeTitle.text=director.ManagementNoticeTitle;
        if(dashboardNoticeBody!=null)dashboardNoticeBody.text=director.ManagementNotice;
        if(dashboardFooter!=null)dashboardFooter.text=director.definition.staffPortalName+" · Staff portal";
        for(int i=0;i<contactNames.Length;i++)
        {
            string contact=Contact(i);var messages=director.state.messages.Where(m=>m.contact==contact).ToArray();var last=messages.LastOrDefault();int unread=messages.Count(m=>!m.outgoing&&!m.read);
            contactNames[i].text=contact;contactPreviews[i].text=last==null?"No messages":last.body.Length>50?last.body.Substring(0,47)+"…":last.body;
            contactTimes[i].text=last==null?"":last.time.Substring(Math.Max(0,last.time.Length-5));unreadBadges[i].SetActive(unread>0);unreadCounts[i].text=unread.ToString();
        }
        if(apps.CurrentPage==PhoneApplications.Page.Conversation)RenderMessages();
    }
    static void Clear(Transform content){foreach(Transform child in content){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
    PhoneStoryRow Row(PhoneStoryRow template,RectTransform parent,float at,int index)
    {
        var row=Instantiate(template,parent,false);row.gameObject.SetActive(true);var rt=(RectTransform)row.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.anchoredPosition=new Vector2(0,at);var nav=row.GetComponent<PhoneNavigationButton>();if(nav!=null)nav.index=index;return row;
    }
    public void RefreshManagement()
    {
        if(!StoryActive||refreshing||manager==null)return;refreshing=true;
        try
        {
            var director=StoryDirector.Active;var data=director.definition.management;manager.data=data;
            if(taskCount!=data.assignments.Length)
            {
                Clear(taskContent);taskCount=data.assignments.Length;manager.taskStatuses=new Text[taskCount];taskContent.sizeDelta=new Vector2(720,taskCount*230+25);
                for(int i=0;i<taskCount;i++){var task=data.assignments[i];var row=Row(taskTemplate,taskContent,-110-i*230,i);row.name="Assignment · "+task.id;row.time.gameObject.SetActive(false);row.title.text=task.title;row.location.text=task.location;manager.taskStatuses[i]=row.status;}
            }
            for(int i=0;i<taskCount;i++)manager.taskStatuses[i].text=PhoneManagement.StatusLabel(manager.GetTaskStatus(data.assignments[i].id));
            var requests=data.requests.Where(r=>director.state.requests.Any(s=>s.id==r.reference)).ToArray();
            if(requestCount!=requests.Length)
            {
                Clear(maintenanceContent);requestCount=requests.Length;maintenanceContent.sizeDelta=new Vector2(720,Mathf.Max(940,requestCount*305+25));
                for(int i=0;i<requestCount;i++){var item=requests[i];var row=Row(requestTemplate,maintenanceContent,-150-i*305,i);row.name=item.reference;row.time.text=item.reference+"  ·  "+item.time;row.title.text=item.title;row.location.text=item.location;row.body.text=item.description;}
            }
            foreach(var row in maintenanceContent.GetComponentsInChildren<PhoneStoryRow>())row.status.text=StoryDirector.Value(director.state.requests,row.name,"Reported");
            if(recordCount!=director.state.records.Count)
            {
                Clear(recordsContent);recordCount=director.state.records.Count;recordsContent.sizeDelta=new Vector2(720,Mathf.Max(940,recordCount*275+25));
                for(int i=0;i<recordCount;i++){var item=director.state.records[i];var row=Row(recordTemplate,recordsContent,-135-i*275,i);row.time.text=item.time+"  ·  "+item.location;row.title.text=item.title;row.body.text=item.description.Length>140?item.description.Substring(0,137)+"…":item.description;}
            }
            beginButton.SetActive(director.CurrentId=="begin");reportButton.SetActive(director.CurrentId=="report");if(director.CurrentId!="report")reportForm.SetActive(false);
        }
        finally{refreshing=false;}
    }
    public bool OpenConversation(int index)
    {
        if(!StoryActive)return false;currentContact=Contact(index);messageCount=-1;apps.conversationTitle.text=currentContact;
        foreach(var item in legacyMessages)if(item!=null)item.SetActive(false);messageScroll.SetActive(true);apps.Open(PhoneApplications.Page.Conversation);RenderMessages();StoryDirector.Active.ReadMessages(currentContact);Refresh();return true;
    }
    public void SendMessage(string body){if(string.IsNullOrWhiteSpace(body))return;StoryDirector.Active.Send(currentContact,body.Trim(),true);apps.messageInput.text="";RenderMessages();}
    void RenderMessages()
    {
        var director=StoryDirector.Active;var messages=director.state.messages.Where(m=>m.contact==currentContact).ToArray();if(messageCount==messages.Length)return;messageCount=messages.Length;
        Clear(messageContent);float y=0;string date=null;
        foreach(var message in messages)
        {
            string day=message.time.Length>=10?message.time.Substring(0,10):"Tonight";
            if(day!=date){var separator=Instantiate(dateTemplate,messageContent,false);separator.SetActive(true);var rt=(RectTransform)separator.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.anchoredPosition=new Vector2(0,-y-30);separator.GetComponentInChildren<Text>().text=day;date=day;y+=75;}
            var bubble=Instantiate(message.outgoing?outgoingTemplate:incomingTemplate,messageContent,false);bubble.SetActive(true);var body=bubble.transform.Find("Body").GetComponent<Text>();body.text=message.body;
            float height=Mathf.Max(95,body.preferredHeight+63);var rect=(RectTransform)bubble.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.sizeDelta=new Vector2(570,height);rect.anchoredPosition=new Vector2(message.outgoing?65:-65,-y-height*.5f);
            body.rectTransform.sizeDelta=new Vector2(510,height-55);body.rectTransform.anchoredPosition=new Vector2(0,12);var time=bubble.transform.Find("Time").GetComponent<Text>();time.text=message.time.Substring(Math.Max(0,message.time.Length-5));time.rectTransform.anchoredPosition=new Vector2(0,-height*.5f+22);y+=height+24;
        }
        messageContent.sizeDelta=new Vector2(720,Mathf.Max(920,y+20));Canvas.ForceUpdateCanvases();messageContent.GetComponentInParent<ScrollRect>().verticalNormalizedPosition=0;director.ReadMessages(currentContact);
    }
    public void ShowRecord(int index)
    {
        if(!StoryActive||index<0||index>=StoryDirector.Active.state.records.Count)return;var record=StoryDirector.Active.state.records[index];var doc=ScriptableObject.CreateInstance<NarrativeDocument>();doc.title=record.title;doc.body=record.time+"\n"+record.location+"\n\n"+record.description;StoryDirector.Active.Presentation.Read(doc);Destroy(doc);
    }
    public void BeginShift(){var director=StoryDirector.Active;var zone=director.Binding("office.zone");var player=InteractionScanner.Active;if(zone!=null&&player!=null&&zone.GetComponent<BoxCollider>().bounds.Contains(player.transform.position+Vector3.up*.5f))StoryDirector.Signal("shift.begin");else director.Remind();}
    public void ShowReport(){reportInput.text="";reportForm.SetActive(true);}
    public void CancelReport(){reportForm.SetActive(false);}
    public void SubmitReport(){if(StoryActive&&StoryDirector.Active.TrySubmitReport(reportInput.text))reportForm.SetActive(false);}
    public void TickReading(float dt)
    {
        if(!StoryActive||apps.CurrentPage!=PhoneApplications.Page.Regulations){readingTime=0;return;}
        readingTime+=Mathf.Max(0,dt);var scroll=apps.GetComponentInChildren<ScrollRect>();if(scroll!=null&&scroll.verticalNormalizedPosition<=.03f)StoryDirector.Signal("rules.read");
    }
    void Update(){TickReading(Time.unscaledDeltaTime);}
}
