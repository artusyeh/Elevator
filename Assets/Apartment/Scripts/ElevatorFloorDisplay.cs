using UnityEngine;

public sealed class ElevatorFloorDisplay : MonoBehaviour
{
    public Renderer[] segments;
    public Renderer upArrow, downArrow;
    static readonly int[] masks={0,6,91,79,102,109,125};
    public void Show(int floor,int direction)
    {
        int mask=masks[Mathf.Clamp(floor,1,6)];
        for(int i=0;i<segments.Length;i++)if(segments[i]!=null)segments[i].enabled=(mask&(1<<i))!=0;
        if(upArrow!=null)upArrow.enabled=direction>0;
        if(downArrow!=null)downArrow.enabled=direction<0;
    }
}
