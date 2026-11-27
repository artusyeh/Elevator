using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(300)]
public sealed class PhonePresentation : MonoBehaviour
{
    public Transform visual;
    public CanvasGroup screen;
    public Vector3 raisedPosition=new Vector3(0,-.009f,.17f);
    public Vector3 loweredPosition=new Vector3(.19f,-.23f,.24f);
    public Vector3 flashlightPosition=new Vector3(.16f,-.145f,.24f);
    public Vector3 raisedRotation=Vector3.zero;
    public Vector3 loweredRotation=new Vector3(4,-4,-8);
    public Vector3 modelScale=Vector3.one;
    [Min(.05f)]public float animationSeconds=.32f;
    [Range(0,.003f)]public float idleMotion=.00045f;
    public bool Raised {get;set;}
    public bool FlashlightActive {get;set;}
    Renderer[] modelRenderers;
    Canvas canvas;
    Camera owner;
    readonly System.Collections.Generic.Dictionary<Camera,int> hiddenUiMasks=new System.Collections.Generic.Dictionary<Camera,int>();
    public float Amount {get;private set;}
    void Awake(){modelRenderers=visual.GetComponentsInChildren<Renderer>(true);canvas=screen.GetComponent<Canvas>();owner=GetComponentInParent<Camera>();Amount=0;Apply(0);}
    void OnEnable(){RenderPipelineManager.beginCameraRendering+=BeginView;RenderPipelineManager.endCameraRendering+=EndView;}
    void OnDisable(){RenderPipelineManager.beginCameraRendering-=BeginView;RenderPipelineManager.endCameraRendering-=EndView;foreach(var entry in hiddenUiMasks)if(entry.Key!=null)entry.Key.cullingMask=entry.Value;hiddenUiMasks.Clear();}
    void BeginView(ScriptableRenderContext context,Camera camera)
    {
        if(modelRenderers==null)return;foreach(var r in modelRenderers)if(r!=null)r.forceRenderingOff=camera!=owner;
        // Disabling the Canvas during camera rendering invalidates GraphicRaycaster depths.
        // Exclude screen UI from secondary views while keeping the interactive Canvas alive.
        if(camera!=owner&&canvas!=null&&!hiddenUiMasks.ContainsKey(camera)){hiddenUiMasks.Add(camera,camera.cullingMask);camera.cullingMask&=~(1<<canvas.gameObject.layer);}
    }
    void EndView(ScriptableRenderContext context,Camera camera)
    {if(modelRenderers==null)return;foreach(var r in modelRenderers)if(r!=null)r.forceRenderingOff=false;int mask;if(hiddenUiMasks.TryGetValue(camera,out mask)){if(camera!=null)camera.cullingMask=mask;hiddenUiMasks.Remove(camera);}}
    public void Tick(float dt)
    {
        Amount=Mathf.MoveTowards(Amount,Raised?1:0,Mathf.Max(0,dt)/animationSeconds);Apply(Amount);
    }
    void Apply(float value)
    {
        float t=value*value*(3-2*value);
        visual.localPosition=Vector3.Lerp(FlashlightActive?flashlightPosition:loweredPosition,raisedPosition,t)+new Vector3(Mathf.Sin(Time.unscaledTime*.73f),Mathf.Sin(Time.unscaledTime*1.13f),0)*idleMotion*t;
        visual.localRotation=Quaternion.Slerp(Quaternion.Euler(loweredRotation),Quaternion.Euler(raisedRotation),t);
        visual.localScale=modelScale;
        if(modelRenderers!=null)foreach(var renderer in modelRenderers)if(renderer!=null)renderer.enabled=Raised||value>.001f||FlashlightActive;
        if(screen!=null){screen.alpha=FlashlightActive?1:t;screen.interactable=Raised&&value>.95f;screen.blocksRaycasts=screen.interactable;}
    }
    void LateUpdate(){Tick(Time.unscaledDeltaTime);}
}
