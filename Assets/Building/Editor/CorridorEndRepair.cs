using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string CorridorEndReview="References/Corridor Ends/";
    static void CorridorEndMaterials()
    {
        Plain("Floor Sign Edge",new Color(.23f,.22f,.18f),.25f,.65f);
        for(int floor=1;floor<=6;floor++)
        {
            var mat=Plain("Floor "+floor+" Enamel",Color.white,.26f,.1f);var map=DetailMap(BuildingBase+"Textures/Floor Signs/"+floor+"F.png");mat.SetTexture("_BaseMap",map);mat.SetFloat("_AlphaClip",1);mat.SetFloat("_Cutoff",.5f);mat.EnableKeyword("_ALPHATEST_ON");mat.SetOverrideTag("RenderType","TransparentCutout");mat.renderQueue=2450;mat.SetFloat("_SpecularHighlights",0);mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");EditorUtility.SetDirty(mat);
        }
        string path=Base+"Materials/Corridor Window Glass.mat";var glass=AssetDatabase.LoadAssetAtPath<Material>(path);if(glass==null){glass=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(glass,path);}glass.shader=Shader.Find("Universal Render Pipeline/Unlit");glass.SetTexture("_BaseMap",DetailMap(BuildingBase+"Textures/Floor Signs/Window Deposits.png"));glass.SetColor("_BaseColor",new Color(.28f,.31f,.29f,.70f));glass.SetFloat("_Surface",1);glass.SetFloat("_Blend",0);glass.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);glass.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);glass.SetFloat("_ZWrite",0);glass.SetFloat("_Cull",0);glass.DisableKeyword("_ALPHAPREMULTIPLY_ON");glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.renderQueue=3000;EditorUtility.SetDirty(glass);mats["Corridor Window Glass"]=glass;
    }
    static void MountFloorPlaque(Transform parent,int number,Vector3 at,Quaternion rotation)
    {
        var sign=G("Floor Sign",parent);sign.localPosition=at;sign.localRotation=rotation;
        B("Enamel Plate",sign,Vector3.zero,new Vector3(.28f,.22f,.010f),"Floor Sign Edge",false,.003f);
        Q("Floor Marking",sign,new Vector3(0,0,-.0056f),new Vector2(.276f,.216f),M("Floor "+number+" Enamel"),Quaternion.identity);
        foreach(float x in new[]{-.120f,.120f})foreach(float y in new[]{-.090f,.090f})
        {Rod("Fixing Screw",sign,new Vector3(x,y,-.005f),new Vector3(x,y,-.007f),.0033f,"Cabin Aged Steel");B("Screw Slot",sign,new Vector3(x,y,-.0073f),new Vector3(.004f,.0007f,.0005f),"Black Rubber",false,.0001f);}
    }
    static void FitCorridorEnd(Transform room,int number)
    {
        float level=room.Find("Corridor Floor").GetComponent<Renderer>().bounds.max.y-room.position.y;
        foreach(var text in room.Cast<Transform>().Where(t=>t.GetComponent<TextMesh>()!=null&&t.GetComponent<TextMesh>().text==number.ToString()).ToArray())UnityEngine.Object.DestroyImmediate(text.gameObject);
        var old=room.Find("Corridor End");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root=G("Corridor End",room);
        var aperture=new Bounds(new Vector3(0,level+1.60f,-.09f),new Vector3(1.48f,1.02f,.60f));
        foreach(var r in room.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.enabled&&r.transform.parent==room&&new[]{"Tiled Wall","Plaster Wall"}.Contains(r.name)&&Mathf.Abs(r.bounds.center.z)<.20f).ToArray())LiftCutRenderer(r,room,new[]{aperture},"Window Wall");
        var window=G("Corridor Window",root);window.localPosition=new Vector3(0,level+1.60f,0);
        foreach(float x in new[]{-.752f,.752f})
        {
            B("Plaster Reveal",window,new Vector3(x,0,-.115f),new Vector3(.035f,1.04f,.27f),"Yellowed Plaster",true,.001f);
            B("Inner Frame",window,new Vector3(x,0,.022f),new Vector3(.055f,1.10f,.055f),"Green Steel",false,.002f);
        }
        B("Plaster Reveal",window,new Vector3(0,.53f,-.115f),new Vector3(1.54f,.040f,.27f),"Yellowed Plaster",true,.001f);
        foreach(float y in new[]{-.515f,.525f})B("Inner Frame",window,new Vector3(0,y,.022f),new Vector3(1.56f,.050f,.055f),"Green Steel",false,.002f);
        foreach(float x in new[]{-.752f,.752f})B("Outer Frame",window,new Vector3(x,0,-.267f),new Vector3(.055f,1.10f,.055f),"Green Steel",false,.002f);
        foreach(float y in new[]{-.515f,.525f})B("Outer Frame",window,new Vector3(0,y,-.267f),new Vector3(1.56f,.050f,.055f),"Green Steel",false,.002f);
        B("Centre Mullion",window,new Vector3(0,0,-.10f),new Vector3(.037f,1.02f,.29f),"Green Steel",false,.002f);
        B("Stone Sill",window,new Vector3(0,-.575f,-.07f),new Vector3(1.68f,.07f,.52f),"Concrete",true,.005f);
        var glass=Q("Window Glass",window,new Vector3(0,0,-.244f),new Vector2(1.48f,1.02f),M("Corridor Window Glass"),Quaternion.identity);glass.GetComponent<Renderer>().receiveShadows=true;
        var blocker=glass.gameObject.AddComponent<BoxCollider>();blocker.size=new Vector3(1,1,.012f);
        foreach(float x in new[]{-.645f,-.387f,-.129f,.129f,.387f,.645f})Rod("Security Bar",window,new Vector3(x,-.495f,-.335f),new Vector3(x,.495f,-.335f),.007f,"Green Steel");
        foreach(float y in new[]{-.495f,.495f})
        {
            B("Bar Rail",window,new Vector3(0,y,-.335f),new Vector3(1.36f,.026f,.024f),"Green Steel",false,.002f);
            foreach(float x in new[]{-.66f,.66f}){Rod("Bar Anchor",window,new Vector3(x,y,-.34f),new Vector3(x,y,-.266f),.011f,"Cabin Aged Steel");Rod("Anchor Bolt",window,new Vector3(x,y,-.351f),new Vector3(x,y,-.340f),.005f,"Cabin Aged Steel");}
        }
        foreach(float x in new[]{-.35f,.35f})
        {B("Latch Plate",window,new Vector3(x,-.10f,.054f),new Vector3(.023f,.07f,.012f),"Cabin Aged Steel",false,.002f);Rod("Window Handle",window,new Vector3(x,-.10f,.066f),new Vector3(x+.045f,-.10f,.066f),.006f,"Cabin Aged Steel");}
        Weather(root,"Water Stain",new Vector3(0,level+.88f,.001f),new Vector2(1.1f,.32f),Quaternion.Euler(0,180,0));
        Lamp(window,new Vector3(0,.30f,-.55f),.85f,4.2f,7800,Quaternion.LookRotation(new Vector3(0,-.45f,1)),LightType.Spot);window.Find("Light").name="Night Light";
        MountFloorPlaque(root,number,new Vector3(-.85f,level+1.92f,12.495f),Quaternion.identity);
        MountFloorPlaque(root,number,new Vector3(-1.28f,level+1.82f,.005f),Quaternion.Euler(0,180,0));
        FitConnectedWindowFrame(window,false);
        ObscureCorridorWindow(window,number);
        foreach(var r in root.GetComponentsInChildren<Renderer>()){r.lightmapIndex=-1;GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);}
    }
    static void OpenCorridorFacade(Transform exterior)
    {
        var facade=exterior.Find("Building Facade");
        for(int floor=2;floor<=6;floor++)
        {
            float level=(floor-1)*2.9f;var aperture=new Bounds(new Vector3(0,level+1.6f,-.10f),new Vector3(1.48f,1.02f,.60f));
            foreach(var r in facade.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.enabled&&r.name=="Landing Facade"&&Mathf.Abs(r.bounds.center.y-(level+1.4f))<.02f).ToArray())
            {
                // Its old inside face covered the corridor paint and could flicker against the wall.
                LiftResize(r.transform,new Vector3(0,level+1.4f,-.10f),new Vector3(4.35f,2.8f,.18f));PrefabUtility.RecordPrefabInstancePropertyModifications(r.GetComponent<MeshFilter>());PrefabUtility.RecordPrefabInstancePropertyModifications(r.transform);
                LiftCutRenderer(r,exterior,new[]{aperture},"Facade Return");PrefabUtility.RecordPrefabInstancePropertyModifications(r);foreach(var c in r.GetComponents<Collider>())PrefabUtility.RecordPrefabInstancePropertyModifications(c);
            }
            foreach(var r in facade.GetComponentsInChildren<Renderer>(true).Where(r=>new[]{"Window Recess","Window Glass","Window Grille","Window Frame","Window Sill"}.Contains(r.name)&&Mathf.Abs(r.bounds.center.y-(level+1.6f))<.70f&&Mathf.Abs(r.bounds.center.x)<.9f&&r.bounds.center.z<0&&r.bounds.center.z>-.5f))
            {r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);foreach(var c in r.GetComponents<Collider>()){c.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(c);}}
        }
    }
    public static string CaptureCorridorEnds(string suffix)
    {
        Directory.CreateDirectory(CorridorEndReview);
        Capture("Corridor Window "+suffix,new Vector3(.35f,4.55f,5.4f),new Vector3(0,4.52f,0));
        Capture("Window Detail "+suffix,new Vector3(.55f,4.55f,2.15f),new Vector3(-.15f,4.5f,-.1f));
        Capture("Floor Sign "+suffix,new Vector3(-.25f,4.55f,11.25f),new Vector3(-.85f,4.82f,12.5f));
        Capture("Upper Window "+suffix,new Vector3(.35f,16.15f,4.6f),new Vector3(0,16.10f,0));
        foreach(string name in new[]{"Corridor Window","Window Detail","Floor Sign","Upper Window"})File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png",CorridorEndReview+name+" "+suffix+".png",true);
        return "Four corridor/sign/window views captured";
    }
    public static string RepairCorridorEnds()
    {
        EnsureApartment();Directory.CreateDirectory(CorridorEndReview);if(!File.Exists(CorridorEndReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",CorridorEndReview+"Apartment.before.unity");if(!File.Exists(CorridorEndReview+"Corridor Window Before.png"))CaptureCorridorEnds("Before");CorridorEndMaterials();
        for(int floor=2;floor<=6;floor++)FitCorridorEnd(GameObject.Find("Floor "+floor).transform.Find("Residential Corridor"),floor);
        OpenCorridorFacade(GameObject.Find("Aoba Exterior").transform);
        string path=Base+"Prefabs/Residential Corridor.prefab";var prefab=PrefabUtility.LoadPrefabContents(path);try{FitCorridorEnd(prefab.transform,2);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        var corridor=GameObject.Find("Floor 2").transform.Find("Residential Corridor");SaveServicePrefab(corridor.Find("Corridor End/Corridor Window"),"Corridor Window");SaveServicePrefab(corridor.Find("Corridor End").Cast<Transform>().First(t=>t.name=="Floor Sign"),"Floor Sign");SaveApartment();CaptureCorridorEnds("After");return "Five corridor-end windows opened to the existing night neighborhood; physical worn floor signs mounted at both ends on floors 2–6; scene and reusable corridor/window/sign prefabs saved.";
    }
}
