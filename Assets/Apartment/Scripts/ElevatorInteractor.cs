using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-200)]
public sealed class ElevatorInteractor : MonoBehaviour
{
    public Camera eyes;
    public float reach=2.3f;
    public bool Interact()
    {
        return eyes!=null&&Click(new Ray(eyes.transform.position,eyes.transform.forward));
    }
    public bool Click(Ray ray)
    {
        var scanner=GetComponent<InteractionScanner>();if(scanner!=null)return scanner.TryInteract(ray,reach,true);
        RaycastHit nearest=default;float distance=float.MaxValue;
        foreach(var hit in Physics.RaycastAll(ray,reach,~0,QueryTriggerInteraction.Ignore))
        {if(hit.collider.transform.IsChildOf(transform))continue;if(hit.distance<distance){distance=hit.distance;nearest=hit;}}
        if(nearest.collider==null)return false;
        var button=nearest.collider.GetComponent<ElevatorButton>();if(button==null)return false;button.Press();return true;
    }
    void Update()
    {
        if(GetComponent<InteractionScanner>()!=null)return;
        if(eyes==null)return;
        if(Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame){Interact();return;}
        if(Mouse.current==null||!Mouse.current.leftButton.wasPressedThisFrame)return;
        var ray=Cursor.lockState==CursorLockMode.Locked?new Ray(eyes.transform.position,eyes.transform.forward):eyes.ScreenPointToRay(Mouse.current.position.ReadValue());Click(ray);
    }
}
