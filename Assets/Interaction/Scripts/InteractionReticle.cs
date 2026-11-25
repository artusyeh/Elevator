using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(250)]
public sealed class InteractionReticle : MonoBehaviour
{
    public InteractionScanner scanner;
    public ReticleGraphic mark;
    public Text prompt;
    public CanvasGroup visibility;
    [Header("Appearance at 1080p")]
    public bool showDefaultDot=true;
    [Range(0,1)] public float idleOpacity=.4f;
    [Range(0,1)] public float interactionOpacity=.9f;
    [Min(.01f)] public float transitionSeconds=.14f;
    float state;
    float promptAlpha;
    string previousPrompt="";
    void Awake(){if(scanner==null)scanner=GetComponentInParent<InteractionScanner>();}
    void LateUpdate(){Tick(Time.unscaledDeltaTime);}
    public void Tick(float unscaledDeltaTime)
    {
        if(scanner==null||!scanner.UiVisible)
        {visibility.alpha=0;state=0;promptAlpha=0;prompt.text="";previousPrompt="";return;}
        visibility.alpha=1;
        bool focused=scanner.FocusedTarget!=null;
        string next=focused?scanner.FocusedTarget.InteractionPrompt:"";
        // Drop stale wording immediately when targets change; fade the new prompt in.
        if(next!=previousPrompt){promptAlpha=0;previousPrompt=next;prompt.text=string.IsNullOrEmpty(next)?"":"[E] "+next;}
        float blend=1-Mathf.Exp(-Mathf.Max(0,unscaledDeltaTime)*4/transitionSeconds);
        state=Mathf.Lerp(state,focused?1:0,blend);
        promptAlpha=Mathf.Lerp(promptAlpha,focused&&!string.IsNullOrEmpty(next)?interactionOpacity:0,blend);
        mark.SetState(state,Mathf.Lerp(showDefaultDot?idleOpacity:0,interactionOpacity,state));
        prompt.color=new Color(1,1,1,promptAlpha);
    }
}
