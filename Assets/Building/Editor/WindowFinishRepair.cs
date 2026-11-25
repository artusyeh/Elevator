using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    const string WindowFinishReview="References/Window Finish/";
    static Mesh WindowRingMesh(string name,Vector2 outer,Vector2 inner,float back,float front)
    {
        var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
        Func<Vector2,float,Vector3[]> ring=(s,z)=>new[]{new Vector3(-s.x,-s.y,z),new Vector3(s.x,-s.y,z),new Vector3(s.x,s.y,z),new Vector3(-s.x,s.y,z)};
        var of=ring(outer,front);var ob=ring(outer,back);var inf=ring(inner,front);var inb=ring(inner,back);
        Action<Vector3,Vector3,Vector3,Vector3> face=(a,b,c,d)=>{int n=vertices.Count;vertices.AddRange(new[]{a,b,c,d});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up});triangles.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});};
        for(int i=0;i<4;i++){int j=(i+1)%4;face(of[j],of[i],inf[i],inf[j]);face(ob[i],ob[j],inb[j],inb[i]);face(ob[j],ob[i],of[i],of[j]);face(inb[i],inb[j],inf[j],inf[i]);}
        var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();string path=BuildingBase+"Models/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing!=null){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);return existing;}AssetDatabase.CreateAsset(mesh,path);return mesh;
    }
    static void FitConnectedWindowFrame(Transform window,bool stair)
    {
        var names=stair?new[]{"Window Reveal","Window Frame","Outer Frame","Mullion","Window Sill"}:new[]{"Plaster Reveal","Inner Frame","Outer Frame","Window Frame","Centre Mullion","Stone Sill","Latch Plate","Window Handle"};
        foreach(var old in window.Cast<Transform>().Where(t=>names.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        if(stair){var metal=Plain("Stair Window Metal",Color.white);metal.CopyPropertiesFromMaterial(M("Stair Rail"));metal.SetColor("_BaseColor",new Color(.25f,.33f,.28f));EditorUtility.SetDirty(metal);}
        string material=stair?"Stair Window Metal":"Green Steel";var frame=G("Window Frame",window);frame.gameObject.AddComponent<MeshFilter>().sharedMesh=WindowRingMesh(stair?"Stairwell Window Frame":"Corridor Window Frame",stair?new Vector2(.7065f,.444f):new Vector2(.78f,.55f),stair?new Vector2(.6415f,.3885f):new Vector2(.7245f,.50f),stair?-.055f:-.30f,stair?.378f:.055f);var r=frame.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=M(material);r.lightmapIndex=-1;
        B(stair?"Mullion":"Centre Mullion",window,new Vector3(0,0,stair?.1615f:-.1225f),new Vector3(.037f,stair?.777f:1f,stair?.433f:.355f),material,false,.001f);
        B(stair?"Window Sill":"Stone Sill",window,new Vector3(0,stair?-.479f:-.587f,stair?.18f:-.07f),stair?new Vector3(1.48f,.065f,.66f):new Vector3(1.68f,.07f,.52f),stair?"Stair Concrete":"Concrete",true,.003f);
        if(!stair){B("Latch Plate",window,new Vector3(0,-.10f,.061f),new Vector3(.023f,.07f,.012f),"Cabin Aged Steel",false,.002f);Rod("Window Handle",window,new Vector3(0,-.10f,.073f),new Vector3(.045f,-.10f,.073f),.006f,"Cabin Aged Steel");}
        foreach(var renderer in window.GetComponentsInChildren<Renderer>()){renderer.lightmapIndex=-1;GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,0);}
    }
    static Material ObscuredGlassMaterial(int floor,bool corridor=false)
    {
        string path=Base+"Materials/"+(corridor?"Corridor Glass ":"Stairwell Glass ")+floor+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Building/Obscured Glass"));AssetDatabase.CreateAsset(material,path);}
        material.shader=Shader.Find("Building/Obscured Glass");
        material.SetTexture("_DirtMap",AssetDatabase.LoadAssetAtPath<Texture2D>(BuildingBase+"Textures/Floor Signs/Window Deposits.png"));
        material.SetFloat("_Transmission",corridor?.075f:.035f);
        material.SetFloat("_Blur",corridor?.065f:.085f);
        material.SetFloat("_Seed",floor*1.71f+(corridor?11:0));
        material.SetColor("_DirtColor",corridor?new Color(.20f,.195f,.16f):new Color(.16f,.155f,.13f));
        material.SetColor("_GlassTint",new Color(.49f,.61f,.62f));
        material.SetColor("_HazeTint",corridor?new Color(.09f,.105f,.11f):new Color(.04f,.05f,.052f));
        EditorUtility.SetDirty(material);return material;
    }
    static void ObscureCorridorWindow(Transform window,int floor)
    {
        window.Find("Window Glass").GetComponent<Renderer>().sharedMaterial=ObscuredGlassMaterial(floor,true);
        var light=window.Find("Night Light").GetComponent<Light>();light.intensity=.12f;light.range=2.1f;light.spotAngle=80;light.innerSpotAngle=50;EditorUtility.SetDirty(light);
    }
    static void ObscureStairWindow(Transform window,int floor)
    {
        FitConnectedWindowFrame(window,true);var glass=window.Find("Window Glass").GetComponent<Renderer>();glass.sharedMaterial=ObscuredGlassMaterial(floor);glass.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        var night=window.Find("Night Light").GetComponent<Light>();night.intensity=.12f;night.range=2.1f;night.spotAngle=80;night.innerSpotAngle=50;EditorUtility.SetDirty(night);
    }
    public static string CaptureWindowFinish(string suffix)
    {
        Directory.CreateDirectory(WindowFinishReview);
        Action<string,Vector3,Vector3> view=(name,at,to)=>{Capture(name+" "+suffix,at,to);File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png",WindowFinishReview+name+" "+suffix+".png",true);};
        view("Corridor Frame",new Vector3(.10f,4.63f,1.45f),new Vector3(0,4.50f,0));view("Corridor Frame Above",new Vector3(.42f,4.63f,.85f),new Vector3(0,5.02f,-.12f));view("Corridor Sill",new Vector3(-.37f,4.63f,.85f),new Vector3(0,3.98f,-.12f));
        view("Lower Stair Window",new Vector3(-3.60f,3.18f,12.88f),new Vector3(-3.8f,3.18f,14.1f));view("Upper Stair Window",new Vector3(-3.60f,14.78f,12.88f),new Vector3(-3.8f,14.78f,14.1f));view("Stair Landing",new Vector3(-4.80f,8.98f,12.5f),new Vector3(-3.8f,8.92f,13.95f));return "Six frame, sill and obscured-window views captured";
    }
    public static string FinishWindowSurfaces()
    {
        EnsureApartment();Directory.CreateDirectory(WindowFinishReview);if(!File.Exists(WindowFinishReview+"Apartment.before.unity")){File.Copy("Assets/Scenes/Apartment.unity",WindowFinishReview+"Apartment.before.unity");CaptureWindowFinish("Before");}
        foreach(var window in GameObject.Find("Apartment").GetComponentsInChildren<Transform>().Where(t=>t.name=="Corridor Window"||t.name=="Landing Window").ToArray())
        {if(window.name=="Landing Window")ObscureStairWindow(window,Mathf.RoundToInt(window.parent.position.y/2.9f)+1);else{FitConnectedWindowFrame(window,false);ObscureCorridorWindow(window,Mathf.RoundToInt((window.position.y-1.6f)/2.9f)+1);}}
        foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)){var data=camera.GetComponent<UniversalAdditionalCameraData>()??camera.gameObject.AddComponent<UniversalAdditionalCameraData>();data.requiresColorTexture=true;EditorUtility.SetDirty(data);}
        string path=Base+"Prefabs/Residential Corridor.prefab";var prefab=PrefabUtility.LoadPrefabContents(path);try{var window=prefab.transform.Find("Corridor End/Corridor Window");FitConnectedWindowFrame(window,false);ObscureCorridorWindow(window,2);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        SaveServicePrefab(GameObject.Find("Floor 2").transform.Find("Residential Corridor/Corridor End/Corridor Window"),"Corridor Window");ExportStairwellPrefabs();SaveApartment();CaptureWindowFinish("After");return "Five corridor frames now use single continuous extruded rings; five stairwell frames corrected, and five individually aged frosted glass materials obscure the existing exterior. Tiny cold light retained. Scene and reusable prefabs saved.";
    }
}
