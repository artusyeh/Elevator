using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    const string MoodReview="References/Apartment Mood/";
    static bool PublicHall(Transform t)=>t.GetComponentsInParent<Transform>().Any(p=>p.name=="Lobby"||p.name=="Residential Corridor")&&!t.GetComponentsInParent<Transform>().Any(p=>p.name=="Elevator Cabin"||p.name=="Stairwell");
    static Material PublicLens()
    {
        string path=Base+"Materials/Public Fluorescent Cover.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(M("Residential Light Cover"));AssetDatabase.CreateAsset(material,path);}
        material.SetColor("_BaseColor",new Color(.80f,.93f,.90f));material.SetColor("_EmissionColor",new Color(.24f,.40f,.37f));material.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;material.EnableKeyword("_EMISSION");EditorUtility.SetDirty(material);return material;
    }
    static Material PublicAccentLens(string name,string source,Color emission)
    {
        string path=Base+"Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(M(source));AssetDatabase.CreateAsset(material,path);}
        material.SetColor("_BaseColor",new Color(.82f,.92f,.90f));material.SetColor("_EmissionColor",emission);material.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;material.EnableKeyword("_EMISSION");EditorUtility.SetDirty(material);return material;
    }
    static int FitPublicLighting(Transform scope)
    {
        var lens=PublicLens();int count=0;
        foreach(var fixture in scope.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Fluorescent Light"&&PublicHall(t)||t==scope&&t.name=="Corridor Light").ToArray())
        {
            bool lobby=fixture.GetComponentsInParent<Transform>().Any(t=>t.name=="Lobby");
            foreach(var light in fixture.GetComponentsInChildren<Light>(true))
            {
                light.useColorTemperature=false;light.color=new Color(.66f,.92f,.89f);light.lightmapBakeType=LightmapBakeType.Realtime;
                if(light.type==LightType.Spot){float variation=fixture.localPosition.z<4?.96f:fixture.localPosition.z<8?1f:.90f;light.intensity=(lobby?10.2f:9.4f)*variation;light.range=4.35f;light.spotAngle=148;light.innerSpotAngle=96;light.shadows=LightShadows.Soft;light.shadowBias=.008f;light.shadowNormalBias=.02f;}
                else{light.intensity=.68f;light.range=3.25f;light.shadows=LightShadows.None;}
                EditorUtility.SetDirty(light);PrefabUtility.RecordPrefabInstancePropertyModifications(light);
            }
            foreach(var r in fixture.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.name.StartsWith("Diffuser"))){r.sharedMaterial=lens;EditorUtility.SetDirty(r);PrefabUtility.RecordPrefabInstancePropertyModifications(r);}count++;
        }
        foreach(var light in scope.GetComponentsInChildren<Light>(true).Where(l=>PublicHall(l.transform)&&l.transform.parent.name=="Entrance Details"))
        {light.useColorTemperature=false;light.color=new Color(.66f,.92f,.89f);light.intensity=.48f;light.range=2.05f;light.lightmapBakeType=LightmapBakeType.Realtime;EditorUtility.SetDirty(light);PrefabUtility.RecordPrefabInstancePropertyModifications(light);}
        var accent=PublicAccentLens("Public Entrance Cover","Cabin Diffuser",new Color(.28f,.44f,.42f));
        foreach(var r in scope.GetComponentsInChildren<Renderer>(true).Where(r=>PublicHall(r.transform)&&r.name=="Entrance Light Cover"))
        {r.sharedMaterial=accent;EditorUtility.SetDirty(r);PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
        var backup=PublicAccentLens("Public Backup Cover","Backup Diffuser",new Color(.15f,.24f,.23f));
        foreach(var fixture in scope.GetComponentsInChildren<LightCircuit>(true).Where(c=>c.name=="Emergency Light"&&!c.GetComponentsInParent<Transform>().Any(t=>new[]{"Elevator Cabin","Stairwell","Electrical Room","Management Room"}.Contains(t.name))))
        {
            foreach(var light in fixture.lights){light.useColorTemperature=false;light.color=new Color(.68f,.90f,.87f);light.intensity=.10f;light.range=1.8f;light.shadows=LightShadows.None;EditorUtility.SetDirty(light);PrefabUtility.RecordPrefabInstancePropertyModifications(light);}
            foreach(var r in fixture.GetComponentsInChildren<Renderer>(true).Where(r=>r.name=="Frosted Lens")){r.sharedMaterial=backup;EditorUtility.SetDirty(r);PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
        }
        foreach(var r in scope.GetComponentsInChildren<Renderer>(true).Where(r=>PublicHall(r.transform)))
        {
            GameObjectUtility.SetStaticEditorFlags(r.gameObject,GameObjectUtility.GetStaticEditorFlags(r.gameObject)&~StaticEditorFlags.ContributeGI);
            var mesh=r as MeshRenderer;if(mesh!=null)mesh.receiveGI=ReceiveGI.LightProbes;
            r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.Off;
            EditorUtility.SetDirty(r);EditorUtility.SetDirty(r.gameObject);PrefabUtility.RecordPrefabInstancePropertyModifications(r);PrefabUtility.RecordPrefabInstancePropertyModifications(r.gameObject);
        }
        FitShadowPolicy(scope);return count;
    }
    static T MoodEffect<T>(VolumeProfile profile) where T:VolumeComponent
    {
        profile.components.RemoveAll(c=>c==null);T effect;if(!profile.TryGet<T>(out effect))effect=profile.Add<T>(true);
        string path=AssetDatabase.GetAssetPath(profile);
        if(!string.IsNullOrEmpty(path))
        {
            if(AssetDatabase.Contains(effect)&&AssetDatabase.GetAssetPath(effect)!=path){var copy=UnityEngine.Object.Instantiate(effect);copy.name=effect.name;profile.components[profile.components.IndexOf(effect)]=copy;effect=copy;}
            if(!AssetDatabase.Contains(effect))AssetDatabase.AddObjectToAsset(effect,profile);
        }
        effect.active=true;return effect;
    }
    public static string CaptureApartmentMood(string suffix)
    {
        Directory.CreateDirectory(MoodReview);
        Action<string,Vector3,Vector3> view=(name,at,to)=>{Capture(name+" "+suffix,at,to);File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png",MoodReview+name+" "+suffix+".png",true);};
        view("Lobby",new Vector3(.55f,1.73f,1.7f),new Vector3(-.10f,1.55f,10.5f));
        view("Corridor",new Vector3(.25f,4.63f,11.65f),new Vector3(0,4.51f,1.2f));
        view("Upper Corridor",new Vector3(.25f,16.23f,11.65f),new Vector3(0,16.11f,1.2f));
        view("Elevator Approach",new Vector3(.45f,1.73f,8.35f),new Vector3(2.80f,1.5f,10.8f));
        view("Management Contrast",new Vector3(.85f,1.73f,4.4f),new Vector3(-3.3f,1.3f,3.2f));
        return "Five matching lobby, corridor, upper-floor, elevator and management views captured";
    }
    public static string ApplyApartmentMood()
    {
        EnsureApartment();Directory.CreateDirectory(MoodReview);
        if(!File.Exists(MoodReview+"Apartment.before.unity"))
        {File.Copy("Assets/Scenes/Apartment.unity",MoodReview+"Apartment.before.unity");File.Copy(Base+"Materials/Apartment Post Processing.asset",MoodReview+"Apartment Post Processing.asset.before");CaptureApartmentMood("Before");}
        int count=FitPublicLighting(GameObject.Find("Apartment").transform);
        var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Base+"Materials/Apartment Post Processing.asset");
        var grade=MoodEffect<ColorAdjustments>(profile);grade.postExposure.Override(.55f);grade.contrast.Override(7);grade.saturation.Override(-8);grade.colorFilter.Override(new Color(.97f,1,1));
        var balance=MoodEffect<WhiteBalance>(profile);balance.temperature.Override(-9);balance.tint.Override(-5);
        var bloom=MoodEffect<Bloom>(profile);bloom.intensity.Override(.045f);bloom.threshold.Override(1.2f);bloom.scatter.Override(.30f);bloom.dirtIntensity.Override(0);
        var vignette=MoodEffect<Vignette>(profile);vignette.intensity.Override(.10f);vignette.smoothness.Override(.55f);
        var lift=MoodEffect<LiftGammaGain>(profile);lift.lift.Override(new Vector4(1,1,1,.0007f));
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.16f,.20f,.22f);RenderSettings.ambientEquatorColor=new Color(.11f,.145f,.16f);RenderSettings.ambientGroundColor=new Color(.055f,.075f,.08f);RenderSettings.fog=false;
        EditorUtility.SetDirty(profile);foreach(var effect in profile.components)EditorUtility.SetDirty(effect);
        foreach(string path in new[]{Base+"Prefabs/Residential Corridor.prefab",Base+"Prefabs/Entrance Interior.prefab",BuildingBase+"Prefabs/Corridor Light.prefab"})
        {var prefab=PrefabUtility.LoadPrefabContents(path);try{FitPublicLighting(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}}
        SaveApartment();CaptureApartmentMood("After");return count+" existing public fixtures retuned; cold localized pools, lower ambient, controlled fixed exposure and restrained grading saved. Architecture and management-room lamps retained.";
    }
}
