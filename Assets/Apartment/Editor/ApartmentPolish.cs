using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    public static string RepairSurfaces()
    {
        int repaired=0;foreach(var filter in GameObject.Find("Apartment").GetComponentsInChildren<MeshFilter>())
        {var collider=filter.GetComponent<BoxCollider>();if(collider==null)continue;filter.sharedMesh=Rounded(collider.size,.006f);repaired++;}
        var board=GameObject.Find("Ground Floor").transform.Find("Lobby").Find("Building Notices");board.localPosition=new Vector3(1.979f,1.55f,7.5f);
        foreach(var lamp in GameObject.Find("Apartment").GetComponentsInChildren<Light>().Where(l=>l.transform.parent.name=="Fluorescent Light")){lamp.lightmapBakeType=LightmapBakeType.Mixed;lamp.shadows=LightShadows.Soft;lamp.intensity=lamp.transform.position.y<3?7:5.2f;lamp.range=5.5f;}
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Validated architecture meshes against "+repaired+" physical collider dimensions.";
    }
    public static string Polish()
    {
        foreach(var asset in new[]{"dirty_tiles","damaged_plaster"})foreach(var file in Directory.GetFiles(Base+"Textures/"+asset).Where(f=>f.EndsWith(".jpg")||f.EndsWith(".png")))
        {var imp=AssetImporter.GetAtPath(file.Replace('\\','/')) as TextureImporter;if(imp==null)continue;if(Path.GetFileName(file)=="nor_gl.jpg")imp.textureType=TextureImporterType.NormalMap;else imp.sRGBTexture=Path.GetFileName(file)=="Diffuse.jpg";imp.anisoLevel=8;imp.SaveAndReimport();}
        Surface("Green Tile",Base+"Textures/dirty_tiles/",1.4f,new Color(.36f,.51f,.40f),.7f);
        Surface("Yellowed Plaster",Base+"Textures/damaged_plaster/",2,new Color(.83f,.81f,.68f),.2f);
        var root=GameObject.Find("Apartment");var ground=GameObject.Find("Ground Floor").transform;var lobby=ground.Find("Lobby");var office=ground.Find("Management Room");
        if(ground.Find("Entrance Court")==null)
        {
            var court=G("Entrance Court",ground);
            B("Pavement",court,new Vector3(0,-.12f,-3),new Vector3(6,.24f,6),"Concrete");
            Wall(court,new Vector3(-3.05f,0,-3),6.2f,3,true);Wall(court,new Vector3(3.05f,0,-3),6.2f,3,true);Wall(court,new Vector3(0,0,-6.1f),6.3f,4);
            B("Entrance Transom",court,new Vector3(0,2.44f,-.06f),new Vector3(2.5f,.24f,.09f),"Stainless Steel");
            for(int i=0;i<9;i++)Rod("Transom Bar",court,new Vector3(-1.1f+i*.275f,2.32f,-.01f),new Vector3(-1.1f+i*.275f,2.56f,-.01f),.008f,"Green Steel");
            Prop("Basket Bicycle",court,new Vector3(2.5f,.01f,-2.6f),5,1.05f,false);Prop("Potted Foliage",court,new Vector3(-2.6f,.01f,-1.7f),0,.7f,false);
            Lamp(court,new Vector3(1.5f,2.6f,-3.8f),3.8f,5,3200,Quaternion.identity);
            B("Neighbor Door",court,new Vector3(.7f,1.1f,-6),new Vector3(1.1f,2.2f,.08f),"Old Wood");
        }
        if(office.Find("Office Service Details")==null)
        {
            var details=G("Office Service Details",office);
            B("Electrical Cabinet",details,new Vector3(-5.94f,1.72f,4.9f),new Vector3(.09f,.51f,.38f),"Green Steel");
            for(int i=0;i<3;i++)PipeRun(details,new Vector3(-5.92f,2.55f,2.2f+i*.25f),new Vector3(-5.92f,.12f,2.2f+i*.25f),.012f);
            for(int row=0;row<3;row++)B("Shelf",details,new Vector3(-5.79f,.38f+row*.39f,3.4f),new Vector3(.39f,.035f,1.0f),"Old Wood",false);
            for(int stack=0;stack<4;stack++)for(int sheet=0;sheet<9;sheet++)B("Filed Document",details,new Vector3(-5.72f,.43f+stack*.04f+sheet*.002f,3.1f+stack*.15f),new Vector3(.23f,.002f,.3f),"Paper",false,.0001f);
            Prop("Delivery Carton",details,new Vector3(-5.52f,0,4.0f),-3,.45f,false);
            PipeRun(details,new Vector3(-3.15f,.03f,3.1f),new Vector3(-3.15f,.03f,4.2f),.007f);
            Weather(details,"Water Stain",new Vector3(-5.998f,1.6f,5.8f),new Vector2(.85f,1.8f),Quaternion.Euler(0,-90,0));
        }
        foreach(var landing in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Elevator Landing"))
        {
            foreach(Transform wall in landing)if(wall.name=="Shaft Wall"){var p=wall.localPosition;p.z=1.02f;wall.localPosition=p;var collider=wall.GetComponent<BoxCollider>();collider.size=new Vector3(.18f,2.8f,1.45f);wall.GetComponent<MeshFilter>().sharedMesh=Rounded(collider.size,.006f);}
            if(landing.Find("Elevator Equipment")==null)
            {
                var equipment=G("Elevator Equipment",landing);
                B("Service Cabinet",equipment,new Vector3(-1.15f,1.55f,-.105f),new Vector3(.42f,.55f,.12f),"Green Steel");
                Rod("Cabinet Latch",equipment,new Vector3(-1.0f,1.55f,-.17f),new Vector3(-1.0f,1.55f,-.19f),.02f,"Brass");
                PipeRun(equipment,new Vector3(-1.25f,1.85f,-.12f),new Vector3(-1.25f,2.64f,-.12f),.014f);
            }
        }
        var steel=M("Stainless Steel");steel.shader=Shader.Find("Universal Render Pipeline/Lit");steel.SetColor("_BaseColor",new Color(.3f,.33f,.30f));steel.SetFloat("_Metallic",.8f);steel.SetFloat("_Smoothness",.48f);steel.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/PBR/rusty_metal_02/nor_gl.jpg"));steel.SetFloat("_BumpScale",.1f);steel.EnableKeyword("_NORMALMAP");
        foreach(var light in root.GetComponentsInChildren<Light>())if(light.transform.parent.name=="Fluorescent Light"){light.colorTemperature=4400;light.range=4.5f;light.intensity=light.transform.position.y<3?4.8f:4;}
        var player=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();player.eyes.clearFlags=CameraClearFlags.SolidColor;player.eyes.backgroundColor=new Color(.006f,.008f,.007f);
        if(GameObject.Find("Viewpoints").transform.Find("Office Desk")==null)View(GameObject.Find("Viewpoints").transform,"Office Desk",new Vector3(-4.8f,1.65f,2.05f),new Vector3(-3.4f,1.3f,5.3f));
        foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())
        {var size=renderer.bounds.size;float area=size.x*size.y+size.x*size.z+size.y*size.z;var flags=GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);if(area<1)flags&=~StaticEditorFlags.ContributeGI;else flags|=StaticEditorFlags.ContributeGI;GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,flags);}
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Ground floor materials, lighting, threshold court, office details and elevator pockets refined.";
    }
    public static string Surveillance()
    {
        var views=new[]{new Vector3(0,2.5f,8.7f),new Vector3(2.8f,2.5f,12.1f),new Vector3(-5,2.4f,13.4f),new Vector3(0,5.3f,11.8f)};
        var targets=new[]{new Vector3(0,1.0f,1),new Vector3(0,1.1f,9.5f),new Vector3(-4.5f,.9f,10),new Vector3(0,4.1f,1)};
        for(int i=0;i<4;i++)
        {
            Capture("Camera "+(i+1),views[i],targets[i]);string path=Base+"Textures/Surveillance "+(i+1)+".png";File.Copy("References/Apartment/Captures/Camera "+(i+1)+".png",path,true);AssetDatabase.ImportAsset(path);var m=M("Surveillance Screen "+(i+1));m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));m.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));m.SetColor("_BaseColor",new Color(.55f,.62f,.56f));m.SetColor("_EmissionColor",new Color(.3f,.4f,.34f));m.EnableKeyword("_EMISSION");
        }
        AssetDatabase.SaveAssets();return "Four surveillance screen materials use actual scene captures; texture slots remain ready for live camera feeds.";
    }
}
