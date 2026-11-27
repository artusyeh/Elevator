using UnityEngine;

public sealed class ElevatorFloorDisplay : MonoBehaviour
{
    public Renderer[] segments;
    public Renderer upArrow, downArrow;
    public Renderer basementPrefix;
    static readonly int[] masks={0,6,91,79,102,109,125};
    int shownFloor=-1,shownDirection=-99;
    public void Show(int floor,int direction)
    {
        if(shownFloor==floor&&shownDirection==direction)return;shownFloor=floor;shownDirection=direction;
        int mask=masks[Mathf.Clamp(floor,1,6)];
        if(basementPrefix!=null)basementPrefix.enabled=floor==0;
        for(int i=0;i<segments.Length;i++)if(segments[i]!=null)segments[i].enabled=(mask&(1<<i))!=0;
        if(upArrow!=null)upArrow.enabled=direction>0;
        if(downArrow!=null)downArrow.enabled=direction<0;
    }
}
