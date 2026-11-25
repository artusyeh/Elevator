using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string CorridorLightReview="References/Corridor Lights/";
    static void CorridorHousingMaterial()
    {
        var m=Plain("Fluorescent Housing",new Color(.95f,.95f,.88f),.23f,.35f);
        const string path="Assets/Building/Textures/Elevator/metal_plate_02/";
        m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path+"Diffuse.jpg"));m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path+"nor_gl.jpg"));foreach(string property in new[]{"_BaseMap","_BumpMap"}){m.SetTextureScale(property,new Vector2(.18f,.18f));m.SetTextureOffset(property,new Vector2(.15f,.10f));}m.SetFloat("_BumpScale",.16f);m.EnableKeyword("_NORMALMAP");EditorUtility.SetDirty(m);
    }
    static int FitCorridorLights(Transform scope)
    {
        var ceilings=scope.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.name.ToLower().Contains("ceiling")&&r.bounds.size.y>.025f).ToArray();int count=0;
        foreach(var lamp in scope.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Fluorescent Light"&&(t.parent.name=="Lobby"||t.parent.name=="Residential Corridor")).ToArray())
        {
            if(lamp.Find("Residential Fixture")!=null)continue;
            var support=ceilings.Where(r=>r.bounds.min.y>lamp.position.y&&r.bounds.min.y<lamp.position.y+.7f&&r.bounds.min.x<lamp.position.x&&r.bounds.max.x>lamp.position.x&&r.bounds.min.z<lamp.position.z&&r.bounds.max.z>lamp.position.z).OrderBy(r=>r.bounds.min.y).FirstOrDefault();
            if(support==null)throw new Exception("No ceiling above "+lamp.parent.name+" light at "+lamp.position);
            float gap=support.bounds.min.y-lamp.position.y;float offset=Mathf.Min(0,gap-.045f);
            var original=lamp.Find("mounted_fluorescent_lights");if(original==null)throw new Exception("Original tube model missing");
            var p=original.localPosition;p.y=-.0001f+offset;original.localPosition=p;
            var prior=lamp.Find("Fixture Housing");if(prior!=null)UnityEngine.Object.DestroyImmediate(prior.gameObject);
            foreach(var stud in lamp.Cast<Transform>().Where(t=>t.name=="Mounting Stud").ToArray())UnityEngine.Object.DestroyImmediate(stud.gameObject);
            var housing=G("Fixture Housing",lamp);housing.localPosition=Vector3.up*offset;
            B("Ballast Tray",housing,new Vector3(0,.031f,0),new Vector3(.977f,.018f,.235f),"Fluorescent Housing",false,.004f);
            foreach(float z in new[]{-.113f,.113f})B("Folded Rim",housing,new Vector3(0,.010f,z),new Vector3(.977f,.041f,.008f),"Fluorescent Housing",false,.002f);
            B("Ballast Cover",housing,new Vector3(0,.003f,0),new Vector3(.77f,.030f,.042f),"Fluorescent Housing",false,.004f);
            foreach(float x in new[]{-.445f,.445f})
            {
                B("Socket Bridge",housing,new Vector3(x,.006f,0),new Vector3(.040f,.033f,.205f),"Fluorescent Housing",false,.003f);
                foreach(float z in new[]{-.065f,.065f})B("Socket Mount",housing,new Vector3(x,-.010f,z),new Vector3(.028f,.026f,.035f),"Aged Plastic",false,.003f);
            }
            foreach(float x in new[]{-.32f,.32f})
            {
                float upper=gap-offset;
                B("Ceiling Anchor",housing,new Vector3(x,upper-.003f,0),new Vector3(.085f,.006f,.075f),"Fluorescent Housing",false,.002f);
                B("Mounting Bracket",housing,new Vector3(x,(.040f+upper-.005f)*.5f,0),new Vector3(.030f,upper-.005f-.040f,.050f),"Fluorescent Housing",false,.001f);
                foreach(float z in new[]{-.024f,.024f})Rod("Anchor Screw",housing,new Vector3(x,upper-.006f,z),new Vector3(x,upper-.008f,z),.0035f,"Cabin Aged Steel");
                Rod("Cover Screw",housing,new Vector3(x,-.012f,0),new Vector3(x,-.014f,0),.0035f,"Cabin Aged Steel");
            }
            // New housing is non-emissive; existing tube renderers, light components and circuit bindings stay intact.
            foreach(var r in housing.GetComponentsInChildren<Renderer>()){r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.BlendProbes;GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);}count++;
        }
        SoftenCorridorFixtures(scope);return count;
    }
    public static string CaptureCorridorLights(string suffix)
    {
        Capture("Corridor Fixture "+suffix,new Vector3(.32f,4.55f,6.06f),new Vector3(0,5.47f,6.50f));
        Capture("Corridor Lighting "+suffix,new Vector3(.35f,4.55f,9.9f),new Vector3(0,4.65f,3.5f));
        foreach(string name in new[]{"Corridor Fixture","Corridor Lighting"})File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png",CorridorLightReview+name+" "+suffix+".png",true);
        return "Corridor light views captured";
    }
    public static string RepairCorridorLights()
    {
        EnsureApartment();Directory.CreateDirectory(CorridorLightReview);if(!File.Exists(CorridorLightReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",CorridorLightReview+"Apartment.before.unity");
        if(!File.Exists(CorridorLightReview+"Corridor Fixture Before.png"))CaptureCorridorLights("Before");CorridorHousingMaterial();
        int count=FitCorridorLights(GameObject.Find("Apartment").transform);
        foreach(string path in new[]{Base+"Prefabs/Residential Corridor.prefab",Base+"Prefabs/Entrance Interior.prefab"})
        {var prefab=PrefabUtility.LoadPrefabContents(path);try{FitCorridorLights(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}}
        var first=GameObject.Find("Ground Floor").transform.Find("Lobby").Cast<Transform>().First(t=>t.name=="Fluorescent Light");SaveServicePrefab(first,"Corridor Light");SaveApartment();CaptureCorridorLights("After");return count+" lobby/corridor lights fitted across six floors; original lamps and power references retained; scene and shared prefabs saved.";
    }
}
