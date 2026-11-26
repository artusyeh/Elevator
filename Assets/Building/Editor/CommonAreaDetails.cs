using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string CommonModels="Assets/Building/Models/Common Areas/";
    const string CommonPrefabs="Assets/Building/Prefabs/Common Areas/";
    const string CommonTextures="Assets/Building/Textures/Common Areas/";
    const string CommonReview="References/Apartment Details/";
    [Serializable] class CommonCatalog{public CommonModel[] models;}
    [Serializable] class CommonModel{public string asset;public CommonMaterial[] materials;}
    [Serializable] class CommonMaterial{public string name,color,normal,arm;public float[] factor;public float metallic,roughness;}

    static void CommonDynamic(Transform root)
    {
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
        {GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);r.lightmapIndex=-1;r.receiveGI=ReceiveGI.LightProbes;r.lightProbeUsage=LightProbeUsage.Off;}
    }
    static Material CommonPbr(string name,string maps,Color color,float metal=.25f,float smooth=.35f)
    {
        var m=Plain(name,color,smooth,metal);string folder="Assets/Alley/Textures/PBR/"+maps+"/";
        m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Diffuse.jpg"));
        var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"nor_gl.jpg");if(normal!=null){m.SetTexture("_BumpMap",normal);m.SetFloat("_BumpScale",.35f);m.EnableKeyword("_NORMALMAP");}
        var mask=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Unity Mask.png");if(mask!=null){m.SetTexture("_MetallicGlossMap",mask);m.SetTexture("_OcclusionMap",mask);m.EnableKeyword("_METALLICSPECGLOSSMAP");m.EnableKeyword("_OCCLUSIONMAP");}
        EditorUtility.SetDirty(m);return m;
    }
    static void CommonModelPart(string source,string partName,string name)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(CommonModels+source+"/"+source+".fbx");
        var original=asset.GetComponentsInChildren<MeshFilter>().First(f=>f.name==partName);
        var root=G(name,null);var part=UnityEngine.Object.Instantiate(original.gameObject,root);part.name="Original Model";
        part.transform.localPosition=Vector3.zero;part.transform.localRotation=original.transform.rotation;part.transform.localScale=original.transform.lossyScale;
        foreach(var r in part.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>M("Common "+m.name)).ToArray();
        var bounds=BoundsOf(root);part.transform.position-=bounds.center;CommonDynamic(root);
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,CommonPrefabs+name+".prefab");UnityEngine.Object.DestroyImmediate(root.gameObject);
    }
    static Transform CommonProp(string name,Transform parent,Vector3 at,float yaw=0,float height=0)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(CommonPrefabs+name+".prefab");
        var root=((GameObject)PrefabUtility.InstantiatePrefab(asset,parent)).transform;root.localRotation=Quaternion.Euler(0,yaw,0);
        if(height>0)root.localScale*=height/BoundsOf(root).size.y;
        var b=BoundsOf(root);root.position+=parent.TransformPoint(at)-new Vector3(b.center.x,b.min.y,b.center.z);CommonDynamic(root);return root;
    }
    static Material CommonPrint(string name,bool cutout=false)
    {
        var m=Plain(name,Color.white,.12f);m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(CommonTextures+name+".png"));
        if(cutout){m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.45f);m.EnableKeyword("_ALPHATEST_ON");m.SetOverrideTag("RenderType","TransparentCutout");m.renderQueue=2450;}
        EditorUtility.SetDirty(m);return m;
    }
    public static string PrepareCommonAreaDetails()
    {
        EnsureApartment();Directory.CreateDirectory(CommonPrefabs);AssetDatabase.Refresh();
        var catalog=JsonUtility.FromJson<CommonCatalog>(File.ReadAllText(CommonModels+"Sources.json"));
        foreach(var model in catalog.models)foreach(var source in model.materials)
        {
            var m=Plain("Common "+source.name,source.factor==null?Color.white:new Color(source.factor[0],source.factor[1],source.factor[2],source.factor[3]),1-source.roughness,source.metallic);
            string folder=CommonModels+model.asset+"/";m.SetTexture("_BaseMap",DetailMap(folder+source.color));
            if(!string.IsNullOrEmpty(source.normal)){m.SetTexture("_BumpMap",DetailMap(folder+source.normal,true));m.EnableKeyword("_NORMALMAP");}
            if(!string.IsNullOrEmpty(source.arm)){var mask=DetailMask(folder+source.arm,folder+"textures/"+source.name+" Unity Mask.png");m.SetTexture("_MetallicGlossMap",mask);m.SetTexture("_OcclusionMap",mask);m.EnableKeyword("_METALLICSPECGLOSSMAP");m.EnableKeyword("_OCCLUSIONMAP");}
            EditorUtility.SetDirty(m);
        }
        CommonModelPart("modular_electric_cables","cable_box_4way","Cast Junction Box");
        CommonModelPart("modular_pipes","pipe_10cm_metal","Pipe Sleeve");
        CommonModelPart("chinese_stool","chinese_stool","Household Stool");
        CommonModelPart("rubber_boots","rubber_boots_l","Rubber Boot Left");CommonModelPart("rubber_boots","rubber_boots_r","Rubber Boot Right");
        CommonPbr("Service Drain Enamel","weathered_pvc_tank",new Color(.50f,.53f,.48f),0,.22f);
        CommonPbr("Service Water Steel","rusty_metal_02",new Color(.42f,.43f,.37f),.65f,.32f);
        CommonPbr("Service Conduit Paint","painted_metal_shutter",new Color(.41f,.44f,.40f),.3f,.28f);
        CommonPbr("Household Timber","weathered_brown_planks",new Color(.47f,.40f,.32f),0,.24f);
        CommonPbr("Household Rubber","rubber_tiles",new Color(.23f,.25f,.22f),0,.12f);
        Plain("House Slipper Blue",new Color(.18f,.26f,.28f),.15f);Plain("House Slipper Ochre",new Color(.43f,.34f,.23f),.15f);Plain("House Slipper Plum",new Color(.32f,.23f,.26f),.15f);
        Plain("Old Detector Plastic",new Color(.50f,.49f,.39f),.18f);Plain("Old Alarm Enamel",new Color(.32f,.055f,.035f),.23f,.35f);
        CommonPrint("Couplet Left",true);CommonPrint("Couplet Right",true);CommonPrint("Neighborhood Paper");CommonPrint("Emergency Exit");
        var slippers=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(CommonModels+"House Slippers/House Slipper.fbx"));slippers.name="House Slipper";
        foreach(var r in slippers.GetComponentsInChildren<Renderer>())r.sharedMaterial=M(r.name.Contains("Sole")?"Household Rubber":"House Slipper Blue");
        CommonDynamic(slippers.transform);PrefabUtility.SaveAsPrefabAsset(slippers,CommonPrefabs+"House Slipper.prefab");UnityEngine.Object.DestroyImmediate(slippers);
        AssetDatabase.SaveAssets();return "Prepared free source models, PBR maps, slippers and printed surfaces.";
    }

    static Transform CommonGroup(Transform parent,string name)
    {var previous=parent.Find(name);if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);return G(name,parent);}
    static List<Vector3> CommonSmooth(Vector3[] points,float trim)
    {
        var path=new List<Vector3>{points[0]};for(int i=1;i<points.Length-1;i++)
        {var p=points[i];float length=Mathf.Min(trim,(p-points[i-1]).magnitude*.30f,(points[i+1]-p).magnitude*.30f);var a=p+(points[i-1]-p).normalized*length;var b=p+(points[i+1]-p).normalized*length;path.Add(a);for(int j=1;j<=8;j++){float t=j/8f;path.Add((1-t)*(1-t)*a+2*(1-t)*t*p+t*t*b);}}
        path.Add(points[points.Length-1]);return path;
    }
    static Transform CommonRoute(string name,Transform parent,float radius,string material,params Vector3[] points)
    {
        var path=CommonSmooth(points,Mathf.Max(radius*2.5f,.035f));const int segments=16;var vs=new List<Vector3>();var ns=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();float travelled=0;
        for(int i=0;i<path.Count;i++)
        {
            if(i>0)travelled+=Vector3.Distance(path[i-1],path[i]);var tangent=(path[Mathf.Min(i+1,path.Count-1)]-path[Mathf.Max(i-1,0)]).normalized;
            var side=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.95f?Vector3.forward:Vector3.up).normalized;var other=Vector3.Cross(tangent,side);
            for(int j=0;j<=segments;j++){float angle=j*Mathf.PI*2/segments;var normal=side*Mathf.Cos(angle)+other*Mathf.Sin(angle);vs.Add(path[i]+normal*radius);ns.Add(normal);uv.Add(new Vector2(j/(float)segments*2*Mathf.PI*radius,travelled));if(i<path.Count-1&&j<segments){int a=i*(segments+1)+j,b=a+segments+1;triangles.AddRange(new[]{a,a+1,b,a+1,b+1,b});}}
        }
        string key=name+" "+string.Join("_",points.Select(p=>p.ToString("F3")))+radius;string hash;using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key))).Replace("-","").Substring(0,14);
        string assetPath=CommonModels+"Route "+hash+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);if(mesh==null){mesh=new Mesh{name=name};mesh.SetVertices(vs);mesh.SetNormals(ns);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();mesh.RecalculateTangents();AssetDatabase.CreateAsset(mesh,assetPath);}
        var root=G(name,parent);root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=M(material);return root;
    }
    static void CommonHanger(Transform parent,Vector3 at,float radius,float ceiling)
    {
        var g=G("Pipe Saddle",parent);g.localPosition=at;
        CabinRing("Split Clamp",g,Vector3.zero,CabinRingMesh("Service Clamp "+radius.ToString("F3"),radius+.004f,.004f),M("Cabin Aged Steel"));
        float top=ceiling-at.y;Rod("Threaded Hanger",g,new Vector3(0,radius+.004f,0),new Vector3(0,top-.009f,0),.004f,"Cabin Aged Steel");
        B("Ceiling Anchor",g,new Vector3(0,top-.006f,0),new Vector3(.065f,.012f,.042f),"Cabin Aged Steel",false,.002f);
        foreach(float x in new[]{-.024f,.024f})Rod("Anchor Bolt",g,new Vector3(x,top-.012f,0),new Vector3(x,top-.016f,0),.003f,"Cabin Aged Steel");
    }
    static void CommonClip(Transform root,Vector3 at,float ceiling)
    {
        B("Cable Clip",root,at,new Vector3(.032f,.028f,.018f),"Old Detector Plastic",false,.003f);
        Rod("Clip Fixing",root,at+Vector3.up*.014f,new Vector3(at.x,ceiling,at.z),.0025f,"Cabin Aged Steel");
    }
    static void CommonDetector(Transform root,Vector3 at)
    {
        var g=G("Smoke Detector",root);g.localPosition=at;
        Rod("Mounting Base",g,Vector3.zero,Vector3.down*.012f,.060f,"Old Detector Plastic");
        Rod("Vented Body",g,Vector3.down*.012f,Vector3.down*.034f,.051f,"Old Detector Plastic");
        for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var slot=B("Intake Slot",g,new Vector3(Mathf.Sin(a)*.0508f,-.024f,Mathf.Cos(a)*.0508f),new Vector3(.015f,.008f,.0015f),"Black Rubber",false,.001f);slot.localRotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);}
        Rod("Test Button",g,Vector3.down*.034f,Vector3.down*.037f,.009f,"Old Detector Plastic");
    }
    static void CommonWallEquipment(Transform root,float floor,bool lobby=false)
    {
        var exit=G("Exit Sign",root);exit.localPosition=new Vector3(-1.998f,2.33f,11.65f);exit.localRotation=Quaternion.Euler(0,-90,0);
        B("Sign Housing",exit,new Vector3(0,0,-.022f),new Vector3(.34f,.13f,.04f),"Old Detector Plastic",false,.006f);
        Q("Printed Exit Face",exit,new Vector3(0,0,-.043f),new Vector2(.312f,.104f),M("Emergency Exit"),Quaternion.identity);
        var alarm=G("Fire Alarm",root);alarm.localPosition=new Vector3(-1.998f,1.38f,11.20f);alarm.localRotation=Quaternion.Euler(0,-90,0);
        B("Alarm Box",alarm,new Vector3(0,0,-.025f),new Vector3(.14f,.18f,.05f),"Old Alarm Enamel",false,.006f);
        B("Break Glass",alarm,new Vector3(0,-.023f,-.051f),new Vector3(.084f,.058f,.002f),"Old Detector Plastic",false,.001f);
        Rod("Alarm Bell",alarm,new Vector3(0,.35f,-.015f),new Vector3(0,.35f,-.055f),.074f,"Old Alarm Enamel");
        CommonRoute("Alarm Conduit",root,.008f,"Service Conduit Paint",new Vector3(-1.978f,2.65f,11.20f),new Vector3(-1.978f,1.51f,11.20f));
    }
    static void CommonVent(Transform root,Vector3 at,Quaternion rot)
    {
        var g=G("Ventilation Grille",root);g.localPosition=at;g.localRotation=rot;
        B("Duct Recess",g,new Vector3(0,0,.001f),new Vector3(.33f,.17f,.006f),"Black Rubber",false,.002f);
        foreach(float x in new[]{-.17f,.17f})B("Grille Frame",g,new Vector3(x,0,-.003f),new Vector3(.016f,.20f,.014f),"Service Conduit Paint",false,.002f);
        foreach(float y in new[]{-.092f,.092f})B("Grille Frame",g,new Vector3(0,y,-.003f),new Vector3(.33f,.016f,.014f),"Service Conduit Paint",false,.002f);
        for(int i=0;i<6;i++){var slat=B("Angled Louver",g,new Vector3(0,-.072f+i*.029f,-.008f),new Vector3(.32f,.012f,.018f),"Service Conduit Paint",false,.001f);slat.localRotation=Quaternion.Euler(-22,0,0);}
    }
    static void CommonCeilingWear(Transform root,float ceiling,float x,float z,int variant)
    {
        Weather(root,"Water Stain",new Vector3(x,ceiling-.0017f,z),new Vector2(.58f+variant*.08f,.82f),Quaternion.Euler(-90,0,variant*31));
        if(variant%2==0)Weather(root,"Peeling Paint",new Vector3(x+.16f,ceiling-.0027f,z+.15f),new Vector2(.40f,.55f),Quaternion.Euler(-90,0,variant*47));
    }
}
