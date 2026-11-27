using UnityEngine;
public sealed class PhoneFlashlight : MonoBehaviour
{
    public Transform emitter;
    public Light beam;
    public Light softBeam;
    public bool IsOn {get;private set;}
    public void SetEnabled(bool value){IsOn=value;if(beam!=null)beam.enabled=value;if(softBeam!=null)softBeam.enabled=value;}
    public void Toggle(){SetEnabled(!IsOn);}
    void LateUpdate()
    {
        if(emitter==null)return;
        // The emitter stays physically attached to the rear lens.
        var cam=GetComponentInParent<Camera>();
        if(cam!=null)emitter.rotation=Quaternion.LookRotation(cam.transform.position+cam.transform.forward*12-emitter.position,cam.transform.up);
    }
}
