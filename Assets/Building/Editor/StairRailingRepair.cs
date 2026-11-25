using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    const string RailReview="References/Stair Corners/";
    const float RailHeight=.96f+(2.9f/18f)*.5f;
    const float RailRadius=.022f;
    static List<Vector3> RoundedRailPath(List<Vector3> corners)
    {
        var result=new List<Vector3>{corners[0]};
        for(int i=1;i<corners.Count-1;i++)
        {
            var a=corners[i]-corners[i-1];var b=corners[i+1]-corners[i];
            if(Vector3.Dot(a.normalized,b.normalized)>.9999f){result.Add(corners[i]);continue;}
            float trim=Mathf.Min(.075f,Mathf.Min(a.magnitude,b.magnitude)*.30f);var begin=corners[i]-a.normalized*trim;var end=corners[i]+b.normalized*trim;result.Add(begin);
            for(int j=1;j<=10;j++){float t=j/10f;result.Add(Vector3.Lerp(Vector3.Lerp(begin,corners[i],t),Vector3.Lerp(corners[i],end,t),t));}
        }
        result.Add(corners[corners.Count-1]);return result;
    }
    static List<Vector3> LandingHandrailPath(int floor)
    {
        var points=new List<Vector3>();
        if(floor==0){points.Add(new Vector3(-2.306f,RailHeight,10.18f));points.Add(new Vector3(-2.48f,RailHeight,10.18f));points.Add(new Vector3(-2.66f,RailHeight,10));}
        else points.Add(new Vector3(-3.81f,RailHeight,10));
        if(floor==5){points.Add(new Vector3(-5.314f,RailHeight,10));return RoundedRailPath(points);}
        points.AddRange(new[]{new Vector3(-4.075f,RailHeight,10),new Vector3(-4.075f,1.45f+RailHeight,12.34f),new Vector3(-4.075f,1.45f+RailHeight,12.60f),new Vector3(-3.545f,1.45f+RailHeight,12.60f),new Vector3(-3.545f,1.45f+RailHeight,12.34f),new Vector3(-3.545f,2.9f+RailHeight,10),new Vector3(-3.81f,2.9f+RailHeight,10)});
        return RoundedRailPath(points);
    }
    static void FitLandingHandrail(Transform root,int floor)
    {
        var points=LandingHandrailPath(floor);const int sides=16;var vertices=new List<Vector3>();var normals=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();float distance=0;
        for(int i=0;i<points.Count;i++)
        {
            if(i>0)distance+=Vector3.Distance(points[i],points[i-1]);var tangent=(points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(i-1,0)]).normalized;var across=Vector3.Cross(tangent,Vector3.up).normalized;var up=Vector3.Cross(tangent,across).normalized;
            for(int j=0;j<sides;j++){float angle=j*Mathf.PI*2/sides;var n=across*Mathf.Cos(angle)+up*Mathf.Sin(angle);vertices.Add(points[i]+n*RailRadius);normals.Add(n);uv.Add(new Vector2(j/(float)sides,distance));}
        }
        for(int i=0;i<points.Count-1;i++)for(int j=0;j<sides;j++){int a=i*sides+j,b=(i+1)*sides+j,an=i*sides+(j+1)%sides,bn=(i+1)*sides+(j+1)%sides;triangles.AddRange(new[]{a,an,bn,a,bn,b});}
        var mesh=new Mesh{name="Landing Handrail"};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();mesh.RecalculateTangents();string path=BuildingBase+"Models/"+(floor==0?"Ground":("Floor "+(floor+1)))+" Handrail.asset";var asset=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(asset!=null){EditorUtility.CopySerialized(mesh,asset);UnityEngine.Object.DestroyImmediate(mesh);mesh=asset;}else AssetDatabase.CreateAsset(mesh,path);
        var model=G("Handrail",root);model.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;model.gameObject.AddComponent<MeshRenderer>().sharedMaterial=M("Stair Rail");var colliders=G("Handrail Colliders",root);
        for(int i=0;i<points.Count-1;i++){var a=points[i];var b=points[i+1];var part=G("Rail Segment",colliders);part.localPosition=(a+b)*.5f;part.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);var c=part.gameObject.AddComponent<CapsuleCollider>();c.radius=RailRadius;c.height=Vector3.Distance(a,b)+RailRadius*2;}
    }
    static void AnchoredBaluster(Transform parent,Vector3 foot,float top,float radius=.008f)
    {
        var post=StairRail(parent,foot+Vector3.up*.008f,new Vector3(foot.x,top-.012f,foot.z),radius);post.name="Baluster";
        B("Post Foot",parent,foot+Vector3.up*.004f,new Vector3(.045f,.008f,.035f),"Stair Rail",false,.001f);
    }
    static void LandingBalusters(Transform parent,float left,float right,float level,float z)
    {
        int count=Mathf.CeilToInt((right-left)/.11f);
        for(int i=0;i<=count;i++){float x=Mathf.Lerp(left,right,i/(float)count);AnchoredBaluster(parent,new Vector3(x,level,z),level+RailHeight);}
    }
    static void RepairLandingRailings(Transform stairs,int floor)
    {
        foreach(var old in stairs.Cast<Transform>().Where(t=>new[]{"Railings","Landing Rail"}.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root=G("Railings",stairs);FitLandingHandrail(root,floor);
        if(floor<5)
        {
            for(int i=0;i<9;i++)
            {
                float z=10+(i+.5f)*.26f,y=(i+1)*2.9f/18f;
                AnchoredBaluster(root,new Vector3(-4.075f,y,z),RailHeight+(z-10)*1.45f/2.34f,.009f);
                z=12.34f-(i+.5f)*.26f;y=1.45f+(i+1)*2.9f/18f;
                AnchoredBaluster(root,new Vector3(-3.545f,y,z),1.45f+RailHeight+(12.34f-z)*1.45f/2.34f,.009f);
            }
            LandingBalusters(root,-3.985f,-3.635f,1.45f,12.60f);
            foreach(float x in new[]{-4.075f,-3.545f})AnchoredBaluster(root,new Vector3(x,1.45f,12.47f),1.45f+RailHeight,.012f);
        }
        if(floor==0)LandingBalusters(root,-3.985f,-2.76f,0,9.98f);
        else if(floor==5)LandingBalusters(root,-5.25f,-3.635f,0,9.98f);
        else LandingBalusters(root,-3.985f,-3.635f,0,9.98f);
        if(floor==0||floor==5)
        {
            var mount=G("Wall Mount",root);mount.localPosition=floor==0?new Vector3(-2.301f,RailHeight,10.18f):new Vector3(-5.319f,RailHeight,10);mount.localRotation=Quaternion.Euler(0,90,0);
            Rod("Mounting Flange",mount,new Vector3(0,0,-.006f),new Vector3(0,0,.006f),.043f,"Stair Rail");
            foreach(float y in new[]{-.029f,.029f})Rod("Fixing Bolt",mount,new Vector3(0,y,-.009f),new Vector3(0,y,.009f),.0038f,"Cabin Aged Steel");
        }
    }
    public static string CaptureStairCorners(string suffix)
    {
        Directory.CreateDirectory(RailReview);
        Action<string,Vector3,Vector3> view=(name,at,to)=>{Capture(name+" "+suffix,at,to);File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png",RailReview+name+" "+suffix+".png",true);};
        for(int i=1;i<6;i++){float y=i*2.9f;view("Floor "+(i+1)+" Turn",new Vector3(-4.8f,y+1.73f,9.05f),new Vector3(-3.76f,y+.62f,10.9f));}
        view("Intermediate Turn",new Vector3(-4.80f,3.18f,13.20f),new Vector3(-3.82f,2.1f,12.25f));view("Ground Guard",new Vector3(-4.72f,1.73f,9.1f),new Vector3(-3.0f,.95f,10.1f));return "Seven player-height corner views captured";
    }
    public static string RepairStairCorners()
    {
        EnsureApartment();Directory.CreateDirectory(RailReview);if(!File.Exists(RailReview+"Apartment.before.unity")){File.Copy("Assets/Scenes/Apartment.unity",RailReview+"Apartment.before.unity");CaptureStairCorners("Before");}
        for(int i=0;i<6;i++)RepairLandingRailings(GameObject.Find(i==0?"Ground Floor":"Floor "+(i+1)).transform.Find("Stairwell"),i);
        ExportStairwellPrefabs();SaveApartment();CaptureStairCorners("After");return "Six landing guards and ten flight railings now form continuous rounded handrails with matching landing heights and grounded posts. Stair geometry preserved; scene and complete Stairwell prefab saved.";
    }
}
