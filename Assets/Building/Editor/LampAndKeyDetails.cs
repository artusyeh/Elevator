using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string LampModels="Assets/Building/Models/Caged Fluorescent/";
    const string KeyModels="Assets/Building/Models/Door Key/";
    static Texture2D DetailMap(string path,bool normal=false,bool data=false)
    {
        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal&&!data;importer.isReadable=data;importer.anisoLevel=8;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static Texture2D DetailMask(string source,string path)
    {
        var arm=DetailMap(source,false,true);var pixels=arm.GetPixels();for(int i=0;i<pixels.Length;i++){var p=pixels[i];pixels[i]=new Color(p.b,p.r,0,1-p.g);}
        var packed=new Texture2D(arm.width,arm.height,TextureFormat.RGBA32,false,true);packed.SetPixels(pixels);packed.Apply();File.WriteAllBytes(path,packed.EncodeToPNG());UnityEngine.Object.DestroyImmediate(packed);return DetailMap(path,false,true);
    }
    static void PrepareLampAndKeyMaterials()
    {
        var lamp=Plain("Weathered Fluorescent",Color.white,.48f,1);
        lamp.SetTexture("_BaseMap",DetailMap(LampModels+"textures/caged_hanging_light_diff_2k.jpg"));lamp.SetTexture("_BumpMap",DetailMap(LampModels+"textures/caged_hanging_light_nor_gl_2k.jpg",true));lamp.SetFloat("_BumpScale",.65f);lamp.EnableKeyword("_NORMALMAP");
        var mask=DetailMask(LampModels+"textures/caged_hanging_light_arm_2k.jpg",LampModels+"textures/Unity Mask.png");lamp.SetTexture("_MetallicGlossMap",mask);lamp.SetTexture("_OcclusionMap",mask);lamp.EnableKeyword("_METALLICSPECGLOSSMAP");lamp.EnableKeyword("_OCCLUSIONMAP");lamp.SetFloat("_OcclusionStrength",.85f);
        lamp.SetTexture("_EmissionMap",DetailMap(LampModels+"textures/caged_hanging_light_emissive_2k.jpg"));lamp.SetColor("_EmissionColor",new Color(.46f,.49f,.455f));lamp.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;lamp.EnableKeyword("_EMISSION");EditorUtility.SetDirty(lamp);
        var key=Plain("Worn Brass Key",new Color(.86f,.84f,.77f),.58f,.75f);
        key.SetTexture("_BaseMap",DetailMap(KeyModels+"Textures/Image3.png"));key.SetTexture("_BumpMap",DetailMap(KeyModels+"Textures/Image1.png",true));key.SetFloat("_BumpScale",.45f);key.EnableKeyword("_NORMALMAP");
        mask=DetailMask(KeyModels+"Textures/Image2.png",KeyModels+"Textures/Unity Mask.png");key.SetTexture("_MetallicGlossMap",mask);key.SetTexture("_OcclusionMap",mask);key.EnableKeyword("_METALLICSPECGLOSSMAP");key.EnableKeyword("_OCCLUSIONMAP");EditorUtility.SetDirty(key);
        var tag=Plain("Key Tag Plastic",new Color(.45f,.43f,.35f),.24f);tag.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/PBR/weathered_pvc_tank/nor_gl.jpg"));tag.SetFloat("_BumpScale",.08f);tag.EnableKeyword("_NORMALMAP");
        string glassPath=Base+"Materials/Key Cabinet Glass.mat";var glass=AssetDatabase.LoadAssetAtPath<Material>(glassPath);if(glass==null){glass=new Material(M("Dirty Glass"));AssetDatabase.CreateAsset(glass,glassPath);}glass.SetFloat("_SpecularHighlights",0);glass.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");glass.SetFloat("_Smoothness",.20f);var glassTint=glass.GetColor("_BaseColor");glassTint.a=.065f;glass.SetColor("_BaseColor",glassTint);EditorUtility.SetDirty(glass);
        string dustPath=Base+"Materials/Fixture Dust.mat";var dust=AssetDatabase.LoadAssetAtPath<Material>(dustPath);if(dust==null){dust=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Weathering/Dirt.mat"));AssetDatabase.CreateAsset(dust,dustPath);}dust.SetFloat("_Opacity",.16f);EditorUtility.SetDirty(dust);
    }
    static void InstallCeilingFluorescent(Transform cabin)
    {
        var finishes=cabin.Find("Cabin Finishes");var prior=finishes.Find("Fluorescent Fixture");if(prior!=null)UnityEngine.Object.DestroyImmediate(prior.gameObject);
        var fixture=G("Fluorescent Fixture",finishes);fixture.localPosition=new Vector3(0,2.265f,.70f);
        var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(LampModels+"Ceiling Fluorescent.fbx"),fixture);model.name="Weathered Fixture";model.transform.localScale*=.76f;
        foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterial=M("Weathered Fluorescent");
        // Mounting feet follow the scanned housing, touching the ceiling rather than hanging in space.
        var filter=model.GetComponentInChildren<MeshFilter>();var surface=filter.gameObject.AddComponent<MeshCollider>();surface.sharedMesh=filter.sharedMesh;Physics.SyncTransforms();
        foreach(float x in new[]{-.30f,.30f})foreach(float z in new[]{-.06f,.06f})
        {
            RaycastHit hit;float low=-.055f;if(surface.Raycast(new Ray(fixture.TransformPoint(new Vector3(x,.025f,z)),fixture.TransformDirection(Vector3.down)),out hit,.20f))low=fixture.InverseTransformPoint(hit.point).y-.001f;
            B("Mounting Foot",fixture,new Vector3(x,(low+.005f)*.5f,z),new Vector3(.023f,.005f-low,.026f),"Cabin Aged Steel",false,.001f);
        }
        UnityEngine.Object.DestroyImmediate(surface);
        Q("Ceiling Dust",fixture,new Vector3(0,.0035f,0),new Vector2(1.01f,.32f),M("Fixture Dust"),Quaternion.Euler(-90,0,0));
        var old=cabin.Find("Fluorescent Light");foreach(var r in old.GetComponentsInChildren<Renderer>())r.enabled=false;
        foreach(var light in old.GetComponentsInChildren<Light>())
        {
            bool spot=light.type==LightType.Spot;light.enabled=spot;light.transform.position=cabin.TransformPoint(new Vector3(0,spot?2.070f:2.085f,.70f));light.transform.rotation=cabin.rotation*Quaternion.Euler(90,0,0);light.useColorTemperature=true;light.colorTemperature=5000;light.color=Color.white;light.intensity=spot?2.05f:0;light.range=spot?2.9f:2.1f;light.lightmapBakeType=LightmapBakeType.Realtime;light.shadows=spot?LightShadows.Soft:LightShadows.None;if(spot){light.spotAngle=145;light.innerSpotAngle=105;light.shadowNearPlane=.03f;light.shadowBias=.006f;light.shadowNormalBias=.018f;}
        }
        var emergency=cabin.Find("Emergency Light");var receiver=cabin.GetComponent<LightCircuit>();
        receiver.lights=cabin.GetComponentsInChildren<Light>(true).Where(l=>l.enabled&&!l.transform.IsChildOf(emergency)).ToArray();receiver.emitters=cabin.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&!r.transform.IsChildOf(emergency)&&r.sharedMaterials.Any(m=>m!=null&&m.HasProperty("_EmissionColor")&&m.GetColor("_EmissionColor").maxColorComponent>.001f)).ToArray();
        foreach(var t in fixture.GetComponentsInChildren<Transform>()){GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);var r=t.GetComponent<Renderer>();if(r!=null)r.lightmapIndex=-1;}EditorUtility.SetDirty(receiver);
    }
    static void MakeDoorKeyPrefab()
    {
        var root=new GameObject("Door Key");var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(KeyModels+"Door Key.fbx"),root.transform);model.name="Key and Ring";foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterial=M("Worn Brass Key");PrefabUtility.SaveAsPrefabAsset(root,BuildingBase+"Prefabs/Door Key.prefab");UnityEngine.Object.DestroyImmediate(root);
    }
    static int ReplaceCabinetKeys(Transform root)
    {
        int count=0;
        foreach(var board in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Key Cabinet").ToArray())
        {
            var prior=board.Find("Hanging Keys");if(prior!=null)UnityEngine.Object.DestroyImmediate(prior.gameObject);
            foreach(var old in board.Cast<Transform>().Where(t=>t.name=="Key"||t.name=="Key Tooth").ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
            var group=G("Hanging Keys",board);var tags=board.Cast<Transform>().Where(t=>t.name=="Key Tag").ToList();int i=0;
            foreach(var hook in board.Cast<Transform>().Where(t=>t.name=="Key Hook").OrderByDescending(t=>t.localPosition.y).ThenBy(t=>t.localPosition.x).ToArray())
            {
                var p=hook.localPosition;p.z=-.075f;hook.localPosition=p;hook.localScale=new Vector3(.0038f,.020f,.0038f);
                var key=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/Door Key.prefab"),group);key.name="Door Key";key.transform.localPosition=new Vector3(p.x,p.y+.011f,-.089f);key.transform.localRotation=Quaternion.Euler(0,70+Mathf.Sin(i*2.3f)*10,Mathf.Sin(i*1.7f)*7);key.transform.localScale=Vector3.one*(.98f+(i%3)*.015f);
                Rod("Hook Tip",group,new Vector3(p.x,p.y-.001f,-.095f),new Vector3(p.x,p.y+.005f,-.095f),.0019f,"Cabin Aged Steel");
                var tag=tags.OrderBy(t=>Vector2.Distance(new Vector2(t.localPosition.x,t.localPosition.y),new Vector2(p.x,p.y-.145f))).First();tags.Remove(tag);tag.localPosition=new Vector3(p.x+.036f,p.y-.079f,-.098f);tag.localRotation=Quaternion.Euler(0,0,Mathf.Cos(i*1.9f)*4);tag.GetComponent<Renderer>().sharedMaterial=M("Key Tag Plastic");
                var end=tag.localPosition+new Vector3(-.007f,.017f,-.004f);var joint=new Vector3(p.x+.017f,p.y-.036f,-.10f);Rod("Tag Link",group,new Vector3(p.x+.006f,p.y-.009f,-.093f),joint,.0007f,"Cabin Aged Steel");Rod("Tag Link",group,joint,end,.0007f,"Cabin Aged Steel");
                CabinRing("Tag Eyelet",group,end,CabinRingMesh("Tag Eyelet",.0026f,.0005f),M("Cabin Aged Steel"));i++;count++;
            }
            var glass=board.Find("Key Cabinet Glass");var at=glass.localPosition;at.z=-.135f;glass.localPosition=at;glass.GetComponent<Renderer>().sharedMaterial=M("Key Cabinet Glass");
            if(board.Find("Cabinet Frame")==null)
            {var frame=G("Cabinet Frame",board);foreach(float x in new[]{-.5375f,.5375f})B("Side Frame",frame,new Vector3(x,0,-.095f),new Vector3(.025f,.9f,.08f),"Old Wood",false,.003f);foreach(float y in new[]{-.4375f,.4375f})B("Top Frame",frame,new Vector3(0,y,-.095f),new Vector3(1.05f,.025f,.08f),"Old Wood",false,.003f);}
        }
        return count;
    }
    static void LampDetailCaptures(string suffix)
    {
        var c=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;
        foreach(string name in new[]{"Ceiling Light","Cabin Lighting"})
        {var at=name=="Ceiling Light"?new Vector3(-.25f,1.65f,.36f):new Vector3(0,1.65f,-1.05f);var target=name=="Ceiling Light"?new Vector3(0,2.19f,.72f):new Vector3(0,1.25f,1.3f);Capture(name+" "+suffix,c.TransformPoint(at),c.TransformPoint(target));File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png","References/Elevator Light/"+name+" "+suffix+".png",true);}
    }
    static void KeyDetailCaptures(string suffix)
    {
        var board=GameObject.Find("Ground Floor").GetComponentsInChildren<Transform>(true).First(t=>t.name=="Key Cabinet");
        foreach(string name in new[]{"Key Cabinet","Keys Close"})
        {var at=name=="Key Cabinet"?new Vector3(0,0,-1.02f):new Vector3(0,0,-.49f);var target=name=="Key Cabinet"?new Vector3(0,0,-.09f):new Vector3(0,-.06f,-.09f);Capture(name+" "+suffix,board.TransformPoint(at),board.TransformPoint(target));File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png","References/Key Cabinet/"+name+" "+suffix+".png",true);}
    }
    public static string ImproveLampAndKeys()
    {
        EnsureApartment();string backup="References/Elevator Light/Apartment.before.unity";if(!File.Exists(backup))File.Copy("Assets/Scenes/Apartment.unity",backup);if(!File.Exists("References/Elevator Light/Ceiling Light Before.png"))LampDetailCaptures("Before");if(!File.Exists("References/Key Cabinet/Key Cabinet Before.png"))KeyDetailCaptures("Before");PrepareLampAndKeyMaterials();MakeDoorKeyPrefab();
        var apartment=GameObject.Find("Apartment").transform;InstallCeilingFluorescent(UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin);int keys=ReplaceCabinetKeys(apartment);
        foreach(string name in new[]{"Elevator Cabin","Elevator Landing","Entrance Interior","Management Room"})
        {string path=Base+"Prefabs/"+name+".prefab";if(AssetDatabase.LoadAssetAtPath<GameObject>(path)==null)continue;var prefab=PrefabUtility.LoadPrefabContents(path);try{foreach(var c in prefab.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Elevator Cabin").ToArray())InstallCeilingFluorescent(c);ReplaceCabinetKeys(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}}
        var fixture=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin.Find("Cabin Finishes/Fluorescent Fixture");SaveServicePrefab(fixture,"Ceiling Fluorescent");var cabinet=apartment.GetComponentsInChildren<Transform>().First(t=>t.name=="Key Cabinet");SaveServicePrefab(cabinet,"Key Cabinet");SaveApartment();LampDetailCaptures("After");KeyDetailCaptures("After");return "Adapted CC0 fluorescent fixture fitted; "+keys+" door-key models hung with rings and connected blank tags; shared prefabs and captures saved.";
    }
}
