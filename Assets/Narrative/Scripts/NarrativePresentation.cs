using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class NarrativePresentation : MonoBehaviour
{
    public CanvasGroup blackout,titleGroup;
    public Text monologue,nightTitle,chapterTitle,subtitle,notification;
    public CanvasGroup objectiveGroup;
    public Text objectiveText,guidanceText;
    public GameObject modal,reportEntry,completion;
    public Text documentTitle,documentBody;
    public RectTransform documentContent;
    public ScrollRect documentScroll;
    public InputField reportInput;
    public GameObject submitButton;
    public NarrativeDocument regulations;
    float subtitleUntil,notificationUntil,readStarted;
    float objectiveUntil;
    string dialogueCompletion;
    string readSignal;
    bool restorePhone;
    public bool Busy=>modal.activeSelf||blackout.alpha>.01f||completion.activeSelf;
    public void ResetView()
    {
        blackout.alpha=0;blackout.gameObject.SetActive(false);titleGroup.alpha=0;modal.SetActive(false);completion.SetActive(false);subtitle.text="";notification.text="";
        var scan=InteractionScanner.Active;if(scan!=null){scan.documentOpen=false;scan.menuOpen=false;scan.cutscenePlaying=false;}Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;
    }
    public IEnumerator Opening(NightDefinition night)
    {
        var scan=InteractionScanner.Active;if(scan!=null)scan.cutscenePlaying=true;
        blackout.gameObject.SetActive(true);blackout.alpha=1;titleGroup.alpha=0;
        foreach(string line in night.openingLines)
        {
            monologue.text=line;float elapsed=0;
            while(elapsed<night.openingLineSeconds){elapsed+=Time.unscaledDeltaTime;float fade=Mathf.Min(elapsed/.3f,(night.openingLineSeconds-elapsed)/.3f);monologue.color=new Color(.88f,.89f,.88f,Mathf.Clamp01(fade));yield return null;}
        }
        monologue.text="";titleGroup.alpha=0;
        float t=0;while(t<night.fadeSeconds){t+=Time.unscaledDeltaTime;blackout.alpha=1-Mathf.SmoothStep(0,1,t/night.fadeSeconds);yield return null;}
        blackout.alpha=0;blackout.gameObject.SetActive(false);if(scan!=null)scan.cutscenePlaying=false;
    }
    public void Say(string speaker,string text,float seconds,string completionId=null){subtitle.text=string.IsNullOrEmpty(speaker)?text:speaker+": "+text;subtitleUntil=Time.unscaledTime+seconds;dialogueCompletion=completionId;}
    public void Objective(string objective,string guidance){objectiveText.text=objective;guidanceText.text=guidance;objectiveGroup.alpha=1;objectiveUntil=Time.unscaledTime+8;}
    public void Restrict(bool value){var scan=InteractionScanner.Active;if(scan!=null)scan.SetCutscenePlaying(value);}
    public IEnumerator Fade(float target,float seconds){blackout.gameObject.SetActive(true);float initial=blackout.alpha,elapsed=0;while(elapsed<seconds){elapsed+=Time.unscaledDeltaTime;blackout.alpha=Mathf.Lerp(initial,target,Mathf.SmoothStep(0,1,elapsed/seconds));yield return null;}blackout.alpha=target;if(target<=0)blackout.gameObject.SetActive(false);}
    public void Notify(string text){notification.text=text;notificationUntil=Time.unscaledTime+4;}
    void Update(){if(Time.unscaledTime>subtitleUntil){subtitle.text="";if(!string.IsNullOrEmpty(dialogueCompletion)){string id=dialogueCompletion;dialogueCompletion=null;StoryDirector.Active.Publish(StoryEventKind.DialogueCompleted,id);}}if(Time.unscaledTime>notificationUntil)notification.text="";if(objectiveGroup!=null&&Time.unscaledTime>objectiveUntil)objectiveGroup.alpha=Mathf.MoveTowards(objectiveGroup.alpha,0,Time.unscaledDeltaTime*2);}
    void BeginModal()
    {
        var phone=Object.FindFirstObjectByType<PhoneControls>();restorePhone=phone!=null&&phone.IsOpen;if(restorePhone)phone.SetOpen(false);
        var scan=InteractionScanner.Active;if(scan!=null)scan.SetDocumentOpen(true);Cursor.lockState=CursorLockMode.None;Cursor.visible=true;modal.SetActive(true);readStarted=Time.unscaledTime;
    }
    public void Read(NarrativeDocument document)
    {
        if(document==null)return;BeginModal();reportEntry.SetActive(false);submitButton.SetActive(false);documentTitle.text=document.title;documentBody.gameObject.SetActive(true);documentBody.text=document.body;readSignal="document."+document.documentId+".read";
        Canvas.ForceUpdateCanvases();documentContent.sizeDelta=new Vector2(documentContent.sizeDelta.x,Mathf.Max(590,documentBody.preferredHeight+40));documentBody.rectTransform.sizeDelta=new Vector2(documentBody.rectTransform.sizeDelta.x,documentContent.sizeDelta.y);documentScroll.verticalNormalizedPosition=1;
    }
    public void Rules(){Read(regulations);readSignal="rules.read";}
    public void Report(){BeginModal();readSignal=null;documentTitle.text="Nightly report";documentBody.gameObject.SetActive(false);reportEntry.SetActive(true);submitButton.SetActive(true);reportInput.text="";}
    public void Submit(){if(StoryDirector.Active.TrySubmitReport(reportInput.text))Close();}
    public void Close()
    {
        if(!string.IsNullOrEmpty(readSignal)&&(documentScroll.verticalNormalizedPosition<=.03f||documentContent.rect.height<=documentScroll.viewport.rect.height+1))StoryDirector.Signal(readSignal);
        modal.SetActive(false);var scan=InteractionScanner.Active;if(scan!=null){scan.SetDocumentOpen(false);scan.IgnoreInputThisFrame();}
        var phone=Object.FindFirstObjectByType<PhoneControls>();if(restorePhone&&phone!=null&&phone.GetComponent<PhoneBattery>().CanUse)phone.SetOpen(true);else{Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
    }
    public void Complete(){completion.SetActive(true);var phone=Object.FindFirstObjectByType<PhoneControls>();if(phone!=null)phone.SetOpen(false);var scan=InteractionScanner.Active;if(scan!=null)scan.menuOpen=true;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
    public void Restart(){StoryDirector.Active.StartNight(1);}
}
