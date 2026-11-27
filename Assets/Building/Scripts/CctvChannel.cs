using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class CctvChannel : MonoBehaviour
{
    public string location;
    public int cameraNumber;
    public Camera cameraSource;
    public Texture2D label;
    public PowerCircuit circuit;
    public bool feedEnabled=true;
    public bool locationAvailable=true;
    public bool scriptedSignalLoss;
    public int width=384,height=256;
    public RenderTexture Texture { get; private set; }
    public int RenderCount { get; private set; }
    public bool Powered=>isActiveAndEnabled&&locationAvailable&&feedEnabled&&cameraSource!=null&&BuildingPower.IsPowered(circuit);
    public void RenderFeed()
    {
        if(!Powered)return;
        if(Texture==null){Texture=new RenderTexture(width,height,16,RenderTextureFormat.ARGB32);Texture.name=location+" Feed";Texture.filterMode=FilterMode.Bilinear;Texture.Create();}
        cameraSource.enabled=false;
        cameraSource.aspect=(float)width/height;
        RenderPipeline.SubmitRenderRequest(cameraSource,new UniversalRenderPipeline.SingleCameraRequest{destination=Texture});RenderCount++;
    }
    void OnDestroy(){if(Texture!=null){Texture.Release();Destroy(Texture);}}
}
