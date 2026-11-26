using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(AlleyStudy.AlleyWalker))]
[DefaultExecutionOrder(-50)]
public sealed class PlayerZoom : MonoBehaviour
{
    public Camera view;
    [Range(15,85)] public float zoomFieldOfView=40;
    [Min(.1f)] public float transitionSpeed=12;
    float normalFieldOfView;
    bool captured;
    bool focused=true;
    AlleyStudy.AlleyWalker walker;
    InteractionScanner scanner;

    void OnEnable()
    {
        walker=GetComponent<AlleyStudy.AlleyWalker>();scanner=GetComponent<InteractionScanner>();
        if(view==null&&walker!=null)view=walker.eyes;
        if(view==null)return;
        normalFieldOfView=view.fieldOfView;captured=true;focused=Application.isFocused;
    }
    void Update(){Tick(Mouse.current!=null&&Mouse.current.rightButton.isPressed,Time.unscaledDeltaTime);}
    public void Tick(bool held,float deltaTime)
    {
        if(!captured||view==null)return;
        bool gameplay=focused&&walker!=null&&walker.isActiveAndEnabled&&view.isActiveAndEnabled&&
            view==Camera.main&&Cursor.lockState==CursorLockMode.Locked&&Time.timeScale>0&&
            (scanner==null||(!scanner.menuOpen&&!scanner.cutscenePlaying&&!scanner.documentOpen));
        if(!gameplay){Restore();return;}
        float target=held?Mathf.Min(normalFieldOfView,Mathf.Clamp(zoomFieldOfView,15,85)):normalFieldOfView;
        float amount=1-Mathf.Exp(-Mathf.Max(.1f,transitionSpeed)*Mathf.Max(0,deltaTime));
        view.fieldOfView=Mathf.Lerp(view.fieldOfView,target,amount);
        if(Mathf.Abs(view.fieldOfView-target)<.01f)view.fieldOfView=target;
    }
    void Restore(){if(captured&&view!=null)view.fieldOfView=normalFieldOfView;}
    void OnApplicationFocus(bool value){focused=value;if(!value)Restore();}
    void OnApplicationPause(bool value){if(value)Restore();}
    void OnDisable(){Restore();captured=false;}
}
