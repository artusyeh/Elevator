using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(CanvasRenderer))]
public sealed class PhoneIcon : MaskableGraphic
{
    public enum Symbol { RoundedRect,Chat,Building,Phone,Flashlight,Back,Signal,Wifi,Battery,Person,Tasks,Progress,Directory,Wrench,Rules,Report,Send,Plus,Check,TaskList,ServiceTool,Rulebook,Archive }
    public Symbol symbol;
    [Range(0,.5f)]public float cornerRadius=.22f;
    [Range(0,1)]public float batteryFill=1;
    public void SetBattery(float amount){amount=Mathf.Clamp01(amount);if(!Mathf.Approximately(batteryFill,amount)){batteryFill=amount;SetVerticesDirty();}}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var rect=GetPixelAdjustedRect();
        System.Func<Vector2,Vector3> p=v=>new Vector3(rect.xMin+v.x*rect.width,rect.yMin+v.y*rect.height,0);
        System.Action<Vector2,Vector2,float> line=(a,b,w)=>{var delta=b-a;var n=new Vector2(-delta.y,delta.x).normalized*w*.5f;int start=vh.currentVertCount;foreach(var v in new[]{a+n,b+n,b-n,a-n})vh.AddVert(p(v),color,Vector2.zero);vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);};
        System.Action<Vector2,float,bool> circle=(at,r,fill)=>{for(int i=0;i<40;i++){float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;var va=at+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;var vb=at+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r;if(fill){int s=vh.currentVertCount;vh.AddVert(p(at),color,Vector2.zero);vh.AddVert(p(va),color,Vector2.zero);vh.AddVert(p(vb),color,Vector2.zero);vh.AddTriangle(s,s+1,s+2);}else line(va,vb,.045f);}};
        System.Action<float,float,float,float> box=(x,y,w,h)=>{line(new Vector2(x,y),new Vector2(x+w,y),.045f);line(new Vector2(x+w,y),new Vector2(x+w,y+h),.045f);line(new Vector2(x+w,y+h),new Vector2(x,y+h),.045f);line(new Vector2(x,y+h),new Vector2(x,y),.045f);};
        if(symbol==Symbol.RoundedRect){float radius=Mathf.Min(rect.width,rect.height)*cornerRadius;var points=new System.Collections.Generic.List<Vector3>();foreach(var c in new[]{new Vector3(rect.xMax-radius,rect.yMax-radius,0),new Vector3(rect.xMin+radius,rect.yMax-radius,90),new Vector3(rect.xMin+radius,rect.yMin+radius,180),new Vector3(rect.xMax-radius,rect.yMin+radius,270)})for(int i=0;i<=10;i++){float a=(c.z+i*9)*Mathf.Deg2Rad;points.Add(new Vector3(c.x+Mathf.Cos(a)*radius,c.y+Mathf.Sin(a)*radius));}vh.AddVert(rect.center,color,Vector2.zero);foreach(var pt in points)vh.AddVert(pt,color,Vector2.zero);for(int i=0;i<points.Count;i++)vh.AddTriangle(0,i+1,(i+1)%points.Count+1);return;}
        switch(symbol)
        {
            case Symbol.TaskList:box(.24f,.14f,.53f,.68f);box(.37f,.76f,.27f,.14f);for(int i=0;i<3;i++){line(new Vector2(.34f,.31f+i*.15f),new Vector2(.38f,.31f+i*.15f),.045f);line(new Vector2(.46f,.31f+i*.15f),new Vector2(.66f,.31f+i*.15f),.04f);}break;
            case Symbol.ServiceTool:line(new Vector2(.22f,.22f),new Vector2(.62f,.62f),.12f);line(new Vector2(.62f,.62f),new Vector2(.80f,.64f),.07f);line(new Vector2(.80f,.64f),new Vector2(.85f,.83f),.07f);line(new Vector2(.62f,.62f),new Vector2(.56f,.80f),.07f);line(new Vector2(.56f,.80f),new Vector2(.66f,.91f),.07f);circle(new Vector2(.22f,.22f),.09f,false);break;
            case Symbol.Rulebook:line(new Vector2(.5f,.18f),new Vector2(.5f,.78f),.045f);foreach(float side in new[]{-1f,1f}){var bottom=new Vector2(.5f+side*.32f,.25f);var top=new Vector2(.5f+side*.32f,.85f);line(new Vector2(.5f,.18f),bottom,.045f);line(bottom,top,.045f);line(top,new Vector2(.5f,.78f),.045f);for(int i=0;i<3;i++)line(new Vector2(.5f+side*.08f,.39f+i*.13f),new Vector2(.5f+side*.24f,.42f+i*.13f),.035f);}break;
            case Symbol.Archive:box(.17f,.18f,.66f,.53f);box(.13f,.71f,.74f,.14f);box(.37f,.45f,.26f,.11f);line(new Vector2(.24f,.31f),new Vector2(.76f,.31f),.035f);break;
            case Symbol.Chat:circle(new Vector2(.50f,.54f),.31f,false);line(new Vector2(.26f,.32f),new Vector2(.18f,.12f),.05f);line(new Vector2(.18f,.12f),new Vector2(.41f,.22f),.05f);line(new Vector2(.39f,.66f),new Vector2(.43f,.45f),.075f);line(new Vector2(.43f,.45f),new Vector2(.61f,.38f),.075f);break;
            case Symbol.Building:case Symbol.Directory:box(.23f,.16f,.54f,.69f);box(.44f,.16f,.12f,.20f);for(int y=0;y<3;y++)for(int x=0;x<2;x++)line(new Vector2(.35f+x*.24f,.47f+y*.13f),new Vector2(.41f+x*.24f,.47f+y*.13f),.055f);break;
            case Symbol.Phone:line(new Vector2(.29f,.79f),new Vector2(.33f,.40f),.12f);line(new Vector2(.33f,.40f),new Vector2(.69f,.22f),.12f);line(new Vector2(.23f,.79f),new Vector2(.40f,.75f),.13f);line(new Vector2(.67f,.18f),new Vector2(.77f,.35f),.13f);break;
            case Symbol.Flashlight:box(.36f,.17f,.28f,.40f);line(new Vector2(.36f,.57f),new Vector2(.24f,.72f),.05f);line(new Vector2(.64f,.57f),new Vector2(.76f,.72f),.05f);line(new Vector2(.24f,.72f),new Vector2(.76f,.72f),.06f);for(int i=0;i<3;i++)line(new Vector2(.30f+i*.20f,.84f),new Vector2(.23f+i*.27f,.96f),.035f);break;
            case Symbol.Back:line(new Vector2(.65f,.80f),new Vector2(.32f,.50f),.07f);line(new Vector2(.32f,.50f),new Vector2(.65f,.20f),.07f);break;
            case Symbol.Signal:for(int i=0;i<4;i++)line(new Vector2(.15f+i*.22f,.15f),new Vector2(.15f+i*.22f,.30f+i*.18f),.12f);break;
            case Symbol.Wifi:for(int j=0;j<3;j++)for(int i=0;i<20;i++){float a=(45+i*4.5f)*Mathf.Deg2Rad,b=(45+(i+1)*4.5f)*Mathf.Deg2Rad,r=.18f+j*.20f;line(new Vector2(.5f,.15f)+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,new Vector2(.5f,.15f)+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,.065f);}circle(new Vector2(.5f,.15f),.06f,true);break;
            case Symbol.Battery:box(.08f,.28f,.76f,.44f);line(new Vector2(.93f,.40f),new Vector2(.93f,.60f),.10f);for(int i=0;i<5;i++)if(batteryFill>i*.2f)line(new Vector2(.19f+i*.12f,.38f),new Vector2(.19f+i*.12f,.38f+.24f*Mathf.Clamp01((batteryFill-i*.2f)*5)),.075f);break;
            case Symbol.Person:circle(new Vector2(.50f,.66f),.17f,true);for(int i=0;i<8;i++)line(new Vector2(.27f+i*.065f,.20f),new Vector2(.27f+i*.065f,.34f+Mathf.Sin(i/7f*Mathf.PI)*.12f),.075f);break;
            case Symbol.Tasks:case Symbol.Rules:case Symbol.Report:box(.23f,.15f,.54f,.70f);for(int i=0;i<3;i++)line(new Vector2(.34f,.33f+i*.16f),new Vector2(.66f,.33f+i*.16f),.04f);break;
            case Symbol.Progress:circle(new Vector2(.5f,.5f),.33f,false);line(new Vector2(.5f,.5f),new Vector2(.5f,.74f),.05f);line(new Vector2(.5f,.5f),new Vector2(.67f,.41f),.05f);break;
            case Symbol.Wrench:circle(new Vector2(.64f,.72f),.16f,false);line(new Vector2(.54f,.60f),new Vector2(.25f,.24f),.13f);break;
            case Symbol.Send:line(new Vector2(.19f,.20f),new Vector2(.81f,.50f),.05f);line(new Vector2(.81f,.50f),new Vector2(.19f,.80f),.05f);line(new Vector2(.19f,.80f),new Vector2(.31f,.50f),.05f);line(new Vector2(.31f,.50f),new Vector2(.19f,.20f),.05f);line(new Vector2(.31f,.50f),new Vector2(.81f,.50f),.05f);break;
            case Symbol.Plus:line(new Vector2(.5f,.2f),new Vector2(.5f,.8f),.07f);line(new Vector2(.2f,.5f),new Vector2(.8f,.5f),.07f);break;
            case Symbol.Check:line(new Vector2(.22f,.48f),new Vector2(.43f,.26f),.07f);line(new Vector2(.43f,.26f),new Vector2(.80f,.76f),.07f);break;
        }
    }
}
