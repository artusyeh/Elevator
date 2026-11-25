using UnityEngine;
using UnityEngine.InputSystem;
public sealed class SlidingDoors : MonoBehaviour
{
    public Transform left,right;
    public float closedOffset=.275f,openOffset=.96f;
    public bool open;
    bool near;
    static int usedFrame=-1;
    void Update()
    {
        var cam=Camera.main;if(cam==null)return;
        near=Vector3.Distance(cam.transform.position,transform.position+Vector3.up)<2.6f;
        if(InteractionScanner.Active==null&&near&&Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame&&usedFrame!=Time.frameCount){open=!open;usedFrame=Time.frameCount;}
        float offset=open?openOffset:closedOffset;
        var a=left.localPosition;var b=right.localPosition;
        a.x=Mathf.MoveTowards(a.x,-offset,Time.deltaTime*.55f);b.x=Mathf.MoveTowards(b.x,offset,Time.deltaTime*.55f);
        left.localPosition=a;right.localPosition=b;
    }
}
