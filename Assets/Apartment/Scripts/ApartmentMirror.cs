using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public sealed class ApartmentMirror : MonoBehaviour
{
    public Camera reflectionCamera;
    public Renderer mirrorSurface;
    public RenderTexture texture;
    [Range(10,60)]public float refreshRate=30;
    public bool renderOnlyWhenVisible=true;
    [Tooltip("Limit punctual shadow sources only during the reflection render.")]
    [Range(0,8)]public int maximumShadowedLights=4;
    struct ShadowState{public Light light;public LightShadows shadows;}
    Light[] shadowSources;
    float nextShadowCache;
    readonly System.Collections.Generic.List<Light> rankedShadows=new System.Collections.Generic.List<Light>();
    readonly System.Collections.Generic.List<ShadowState> suppressedShadows=new System.Collections.Generic.List<ShadowState>();
    readonly Plane[] viewPlanes=new Plane[6];
    readonly RaycastHit[] visibilityHits=new RaycastHit[16];
    float nextFrame;
    bool rendering;
    void LateUpdate()
    {
        var source=Camera.main;if(source==null||Time.unscaledTime<nextFrame)return;
        nextFrame=Time.unscaledTime+1/Mathf.Max(1,refreshRate);
        if(renderOnlyWhenVisible){GeometryUtility.CalculateFrustumPlanes(source,viewPlanes);if(!CameraSurfaceVisibility.CanSee(source,mirrorSurface,transform,8,viewPlanes,visibilityHits))return;}
        RenderFrom(source);
    }
    public void RenderFrom(Camera source)
    {
        if(rendering||reflectionCamera==null||texture==null||mirrorSurface==null)return;
        if(Vector3.Distance(source.transform.position,transform.position)>8)return;
        rendering=true;bool visible=mirrorSurface.enabled;bool previous=GL.invertCulling;
        try
        {
            Vector3 normal=-transform.forward,position=transform.position;
            var plane=new Vector4(normal.x,normal.y,normal.z,-Vector3.Dot(normal,position));
            var reflect=Matrix4x4.identity;
            for(int a=0;a<3;a++)for(int b=0;b<4;b++)reflect[a,b]-=2*plane[a]*plane[b];
            reflectionCamera.CopyFrom(source);reflectionCamera.enabled=false;reflectionCamera.targetTexture=texture;
            reflectionCamera.transform.position=reflect.MultiplyPoint(source.transform.position);
            reflectionCamera.worldToCameraMatrix=source.worldToCameraMatrix*reflect;
            var worldToView=reflectionCamera.worldToCameraMatrix;
            var point=worldToView.MultiplyPoint(position+normal*.005f);var n=worldToView.MultiplyVector(normal).normalized;
            reflectionCamera.projectionMatrix=source.CalculateObliqueMatrix(new Vector4(n.x,n.y,n.z,-Vector3.Dot(point,n)));
            mirrorSurface.enabled=false;GL.invertCulling=!previous;
            LimitReflectionShadows();
            RenderPipeline.SubmitRenderRequest(reflectionCamera,new UniversalRenderPipeline.SingleCameraRequest{destination=texture});
            mirrorSurface.sharedMaterial.SetTexture("_BaseMap",texture);
            mirrorSurface.sharedMaterial.SetMatrix("_ReflectionVP",GL.GetGPUProjectionMatrix(reflectionCamera.projectionMatrix,true)*worldToView);
        }
        finally{foreach(var entry in suppressedShadows)if(entry.light!=null)entry.light.shadows=entry.shadows;suppressedShadows.Clear();GL.invertCulling=previous;mirrorSurface.enabled=visible;rendering=false;}
    }
    void LimitReflectionShadows()
    {
        if(shadowSources==null||Time.realtimeSinceStartup>=nextShadowCache){shadowSources=Object.FindObjectsByType<Light>(FindObjectsSortMode.None);nextShadowCache=Time.realtimeSinceStartup+5;}
        rankedShadows.Clear();var position=reflectionCamera.transform.position;
        foreach(var light in shadowSources)if(light!=null&&light.isActiveAndEnabled&&light.type!=LightType.Directional&&light.shadows!=LightShadows.None)rankedShadows.Add(light);
        rankedShadows.Sort((a,b)=>(a.transform.position-position).sqrMagnitude.CompareTo((b.transform.position-position).sqrMagnitude));
        for(int i=Mathf.Max(0,maximumShadowedLights);i<rankedShadows.Count;i++){var light=rankedShadows[i];suppressedShadows.Add(new ShadowState{light=light,shadows=light.shadows});light.shadows=LightShadows.None;}
    }
}
