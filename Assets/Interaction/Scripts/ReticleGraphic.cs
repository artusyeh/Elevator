using UnityEngine;
using UnityEngine.UI;

// One editable vector graphic morphs from a filled disk to a hollow ring.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ReticleGraphic : MaskableGraphic
{
    [Range(0,1)] public float interaction;
    [Min(1)] public float dotDiameter=3.5f;
    [Min(4)] public float circleDiameter=12;
    [Range(.5f,2)] public float circleThickness=1;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();const int segments=64;
        float outer=Mathf.Lerp(dotDiameter,circleDiameter,interaction)*.5f;
        float inner=Mathf.Lerp(0,circleDiameter*.5f-circleThickness,interaction);
        // A subpixel transparent fringe is antialiasing, not a visible outline or glow.
        float edge=Mathf.Min(.55f,(outer-inner)*.45f);var center=rectTransform.rect.center;
        float[] radii={Mathf.Max(0,inner-edge*.5f),inner+edge*.5f,outer-edge*.5f,outer+edge*.5f};
        for(int ring=0;ring<4;ring++)for(int i=0;i<=segments;i++)
        {
            float angle=i*Mathf.PI*2/segments;var tint=color;
            if(ring==3||(ring==0&&inner>.001f))tint.a=0;
            vh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radii[ring],tint,Vector2.zero);
        }
        for(int ring=0;ring<3;ring++)for(int i=0;i<segments;i++)
        {int a=ring*(segments+1)+i,b=a+segments+1;vh.AddTriangle(a,b,b+1);vh.AddTriangle(a,b+1,a+1);}
    }
    public void SetState(float value,float alpha)
    {interaction=Mathf.Clamp01(value);color=new Color(1,1,1,alpha);SetVerticesDirty();}
}
