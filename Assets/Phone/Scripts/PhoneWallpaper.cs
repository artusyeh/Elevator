using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(CanvasRenderer))]
public sealed class PhoneWallpaper : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;const int w=36,h=72;
        for(int y=0;y<=h;y++)for(int x=0;x<=w;x++){float u=(float)x/w,v=(float)y/h;float wave=Mathf.Exp(-Mathf.Pow((u-.5f-Mathf.Sin(v*5)*.33f)/.23f,2));float mist=Mathf.Exp(-((u-.65f)*(u-.65f)+(v-.55f)*(v-.55f))*5);var c=Color.Lerp(new Color(.012f,.022f,.043f),new Color(.07f,.18f,.23f),wave*.5f+mist*.35f);vh.AddVert(new Vector3(r.xMin+u*r.width,r.yMin+v*r.height),c,new Vector2(u,v));if(x<w&&y<h){int i=y*(w+1)+x;vh.AddTriangle(i,i+w+1,i+1);vh.AddTriangle(i+1,i+w+1,i+w+2);}}
    }
}
