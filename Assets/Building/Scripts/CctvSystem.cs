using System.Collections.Generic;
using UnityEngine;

public sealed class CctvSystem : MonoBehaviour
{
    public CctvChannel[] channels;
    public CctvMonitor[] monitors;
    [Range(1,12)] public float refreshRate=6;
    public bool renderOnlyVisibleMonitors=true;
    [Min(1)]public float viewerDistance=12;
    public int LastRenderedChannels { get; private set; }
    public float LastRenderMilliseconds { get; private set; }
    readonly Dictionary<int,float> lastUpdate=new Dictionary<int,float>();
    readonly Plane[] viewPlanes=new Plane[6];
    readonly RaycastHit[] visibilityHits=new RaycastHit[16];
    bool[] visible;
    float nextVisibilityCheck;
    static readonly Dictionary<string,int> selections=new Dictionary<string,int>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetSession(){selections.Clear();}
    public static int ReadChannel(string id,int fallback)=>selections.TryGetValue(id,out int value)?value:fallback;
    public static void RememberChannel(string id,int value){selections[id]=value;}
    public void NextChannel(CctvMonitor monitor)
    {
        int next=monitor.ChannelIndex;for(int i=0;i<channels.Length;i++){next=(next+1)%channels.Length;bool used=false;foreach(var other in monitors)used|=other!=monitor&&other.ChannelIndex==next;if(!used)break;}
        monitor.Select(next);
    }
    void LateUpdate()
    {
        if(channels==null||channels.Length==0||monitors==null)return;
        if(visible==null||visible.Length!=monitors.Length)visible=new bool[monitors.Length];
        if(Time.unscaledTime>=nextVisibilityCheck)
        {
            nextVisibilityCheck=Time.unscaledTime+.15f;var viewer=Camera.main;
            if(viewer!=null)GeometryUtility.CalculateFrustumPlanes(viewer,viewPlanes);
            for(int i=0;i<monitors.Length;i++)visible[i]=!renderOnlyVisibleMonitors||monitors[i]!=null&&CameraSurfaceVisibility.CanSee(viewer,monitors[i].screen,monitors[i].transform,viewerDistance,viewPlanes,visibilityHits,monitors[i].FaceNormal);
        }
        // Stagger cameras: at most one low-resolution scene render per player frame.
        int oldest=-1;float stamp=float.MaxValue;
        for(int slot=0;slot<monitors.Length;slot++)if(visible[slot]&&monitors[slot]!=null&&monitors[slot].Powered)
        {
            var monitor=monitors[slot];
            int index=Mathf.Clamp(monitor.ChannelIndex,0,channels.Length-1);if(!channels[index].Powered)continue;
            float updated=lastUpdate.TryGetValue(index,out float value)?value:-100;
            if(Time.unscaledTime-updated>=1/refreshRate&&updated<stamp){oldest=index;stamp=updated;}
        }
        if(oldest>=0){channels[oldest].RenderFeed();lastUpdate[oldest]=Time.unscaledTime;}
        foreach(var monitor in monitors)if(monitor!=null)monitor.Refresh();
    }
    public void RefreshFeeds()
    {
        var timer=System.Diagnostics.Stopwatch.StartNew();var active=new HashSet<int>();
        foreach(var monitor in monitors)if(monitor!=null&&monitor.Powered)active.Add(Mathf.Clamp(monitor.ChannelIndex,0,channels.Length-1));
        LastRenderedChannels=0;foreach(int index in active)if(channels[index].Powered){channels[index].RenderFeed();LastRenderedChannels++;}
        foreach(var monitor in monitors)if(monitor!=null)monitor.Refresh();timer.Stop();LastRenderMilliseconds=(float)timer.Elapsed.TotalMilliseconds;
    }
}
