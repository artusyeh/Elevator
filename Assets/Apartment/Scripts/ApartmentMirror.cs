using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public sealed class ApartmentMirror : MonoBehaviour
{
    public Camera reflectionCamera;
    public Renderer mirrorSurface;
    public RenderTexture texture;
    bool rendering;
    void LateUpdate(){if(Camera.main!=null)RenderFrom(Camera.main);}
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
            RenderPipeline.SubmitRenderRequest(reflectionCamera,new UniversalRenderPipeline.SingleCameraRequest{destination=texture});
            mirrorSurface.sharedMaterial.SetTexture("_BaseMap",texture);
            mirrorSurface.sharedMaterial.SetMatrix("_ReflectionVP",GL.GetGPUProjectionMatrix(reflectionCamera.projectionMatrix,true)*worldToView);
        }
        finally{GL.invertCulling=previous;mirrorSurface.enabled=visible;rendering=false;}
    }
}
