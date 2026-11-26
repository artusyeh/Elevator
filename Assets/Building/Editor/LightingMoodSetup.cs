using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    const string MoodToggleReview="References/Lighting Toggle/";
    const string MoodAssets=BuildingBase+"Lighting/";
    static string MoodBindingKey(Component component)
    {
        var path=new List<string>();for(var t=component.transform;t!=null;t=t.parent)path.Add(t.name+"["+t.GetSiblingIndex()+"]");path.Reverse();
        return string.Join("/",path.ToArray())+":"+component.GetType().Name+":"+Array.IndexOf(component.GetComponents(component.GetType()),component);
    }
    static bool MoodLamp(Light light)=>PublicHall(light.transform)||(light.transform.parent.name=="Emergency Light"&&!light.GetComponentsInParent<Transform>().Any(t=>new[]{"Elevator Cabin","Stairwell","Electrical Room","Management Room"}.Contains(t.name)));
    static VolumeProfile CopyMoodProfile(VolumeProfile source,string name,bool horror)
    {
        string path=MoodAssets+name+" Post Processing.asset";var existing=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);if(existing!=null)return existing;
        var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.name=name+" Post Processing";AssetDatabase.CreateAsset(profile,path);
        foreach(var component in source.components.Where(c=>c!=null))
        {var copy=UnityEngine.Object.Instantiate(component);copy.name=component.name;AssetDatabase.AddObjectToAsset(copy,profile);profile.components.Add(copy);}
        Action<VolumeComponent> own=component=>{if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,profile);EditorUtility.SetDirty(component);};
        var grade=MoodEffect<ColorAdjustments>(profile);grade.postExposure.Override(horror?.55f:.9f);grade.contrast.Override(horror?7:0);grade.saturation.Override(horror?-8:-3);grade.colorFilter.Override(horror?new Color(.97f,1,1):Color.white);own(grade);
        var balance=MoodEffect<WhiteBalance>(profile);balance.temperature.Override(horror?-9:-4);balance.tint.Override(horror?-5:0);own(balance);
        var bloom=MoodEffect<Bloom>(profile);bloom.intensity.Override(horror?.045f:.08f);bloom.threshold.Override(horror?1.2f:1.15f);bloom.scatter.Override(horror?.30f:.35f);bloom.dirtIntensity.Override(0);own(bloom);
        var lift=MoodEffect<LiftGammaGain>(profile);lift.lift.Override(new Vector4(1,1,1,horror?.0007f:.0018f));own(lift);
        var vignette=MoodEffect<Vignette>(profile);vignette.intensity.Override(horror?.1f:0);vignette.smoothness.Override(.55f);own(vignette);
        EditorUtility.SetDirty(profile);return profile;
    }
    static LightingMoodPreset NewMoodPreset(string name,VolumeProfile profile)
    {
        string path=MoodAssets+name+" Lighting.asset";var asset=AssetDatabase.LoadAssetAtPath<LightingMoodPreset>(path);if(asset==null){asset=ScriptableObject.CreateInstance<LightingMoodPreset>();AssetDatabase.CreateAsset(asset,path);}asset.name=name+" Lighting";asset.postProcessing=profile;return asset;
    }
    public static string InstallLightingToggle()
    {
        EnsureApartment();Directory.CreateDirectory(MoodToggleReview);if(!AssetDatabase.IsValidFolder(BuildingBase+"Lighting"))AssetDatabase.CreateFolder("Assets/Building","Lighting");
        var existing=UnityEngine.Object.FindFirstObjectByType<LightingMoodController>();if(existing!=null&&existing.normal!=null&&existing.horror!=null)return "Lighting toggle already installed; edit the two independent preset assets.";
        if(!File.Exists(MoodToggleReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",MoodToggleReview+"Apartment.before.unity");
        var apartment=GameObject.Find("Apartment");var volume=apartment.GetComponentsInChildren<Volume>().First(v=>v.isGlobal);
        var horror=NewMoodPreset("Horror",CopyMoodProfile(volume.sharedProfile,"Horror",true));var normal=NewMoodPreset("Normal",CopyMoodProfile(volume.sharedProfile,"Normal",false));
        var lamps=apartment.GetComponentsInChildren<Light>(true).Where(MoodLamp).OrderBy(MoodBindingKey).ToArray();
        var covers=apartment.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterials.Any(m=>m!=null&&new[]{"Public Fluorescent Cover","Public Entrance Cover","Public Backup Cover"}.Contains(m.name))).OrderBy(MoodBindingKey).ToArray();
        var surfaces=apartment.GetComponentsInChildren<MeshRenderer>(true).Where(r=>PublicHall(r.transform)).OrderBy(MoodBindingKey).ToArray();
        horror.lamps=lamps.Select(LightingMoodPreset.Lamp.Capture).ToArray();horror.covers=covers.Select(r=>new LightingMoodPreset.Cover{materials=r.sharedMaterials}).ToArray();horror.surfaces=surfaces.Select(LightingMoodPreset.Surface.Capture).ToArray();
        horror.ambientMode=RenderSettings.ambientMode;horror.ambientSky=RenderSettings.ambientSkyColor;horror.ambientEquator=RenderSettings.ambientEquatorColor;horror.ambientGround=RenderSettings.ambientGroundColor;horror.ambientIntensity=RenderSettings.ambientIntensity;horror.reflectionIntensity=RenderSettings.reflectionIntensity;
        var preview=EditorSceneManager.OpenPreviewScene("References/Apartment Mood/Apartment.before.unity");
        try
        {
            var originalRoot=preview.GetRootGameObjects().First(g=>g.name=="Apartment");var oldLights=originalRoot.GetComponentsInChildren<Light>(true).ToDictionary(MoodBindingKey);var oldRenderers=originalRoot.GetComponentsInChildren<Renderer>(true).ToDictionary(MoodBindingKey);
            normal.lamps=lamps.Select(l=>LightingMoodPreset.Lamp.Capture(oldLights[MoodBindingKey(l)])).ToArray();
            normal.covers=covers.Select(r=>new LightingMoodPreset.Cover{materials=oldRenderers[MoodBindingKey(r)].sharedMaterials.Select(m=>m!=null&&m.name=="Public Fluorescent Cover"?M("Residential Light Cover"):m!=null&&m.name=="Public Entrance Cover"?M("Cabin Diffuser"):m!=null&&m.name=="Public Backup Cover"?M("Backup Diffuser"):m).ToArray()}).ToArray();
            normal.surfaces=surfaces.Select(r=>LightingMoodPreset.Surface.Capture((MeshRenderer)oldRenderers[MoodBindingKey(r)])).ToArray();
            for(int i=0;i<surfaces.Length;i++){var original=oldRenderers[MoodBindingKey(surfaces[i])];GameObjectUtility.SetStaticEditorFlags(surfaces[i].gameObject,GameObjectUtility.GetStaticEditorFlags(original.gameObject));PrefabUtility.RecordPrefabInstancePropertyModifications(surfaces[i].gameObject);}
        }
        finally{EditorSceneManager.ClosePreviewScene(preview);}
        normal.ambientMode=AmbientMode.Trilight;normal.ambientSky=new Color(.19f,.235f,.31f);normal.ambientEquator=new Color(.145f,.165f,.205f);normal.ambientGround=new Color(.08f,.09f,.11f);normal.ambientIntensity=1;normal.reflectionIntensity=.65f;
        var controller=volume.GetComponent<LightingMoodController>()??volume.gameObject.AddComponent<LightingMoodController>();controller.normal=normal;controller.horror=horror;controller.interiorVolume=volume;controller.lamps=lamps;controller.covers=covers;controller.surfaces=surfaces;controller.circuits=apartment.GetComponentsInChildren<LightCircuit>(true);controller.startingMood=LightingMoodController.Mood.Normal;controller.toggleKey=UnityEngine.InputSystem.Key.L;
        controller.SetMood(LightingMoodController.Mood.Normal);
        foreach(var l in lamps){EditorUtility.SetDirty(l);PrefabUtility.RecordPrefabInstancePropertyModifications(l);}foreach(var r in covers){EditorUtility.SetDirty(r);PrefabUtility.RecordPrefabInstancePropertyModifications(r);}foreach(var r in surfaces){EditorUtility.SetDirty(r);PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
        EditorUtility.SetDirty(normal);EditorUtility.SetDirty(horror);EditorUtility.SetDirty(controller);EditorUtility.SetDirty(volume);SaveApartment();
        return "L toggle installed with independent original/horror presets, "+lamps.Length+" existing lamps, "+covers.Length+" fixture covers and "+surfaces.Length+" surface-lighting bindings. Normal is the starting mode; reopen/finalize to capture original baked renderer bindings.";
    }
    public static string FinalizeLightingToggle()
    {
        EnsureApartment();var controller=UnityEngine.Object.FindFirstObjectByType<LightingMoodController>();
        if(controller.interiorVolume.sharedProfile!=controller.normal.postProcessing)throw new InvalidOperationException("Finalize the original lightmap bindings from a reloaded Normal scene.");
        controller.normal.surfaces=controller.surfaces.Select(LightingMoodPreset.Surface.Capture).ToArray();EditorUtility.SetDirty(controller.normal);SaveApartment();
        var report="Normal baked surface bindings="+controller.normal.surfaces.Count(s=>s.lightmapIndex>=0&&s.lightmapIndex<65534)+"; independent profile components="+controller.normal.postProcessing.components.All(c=>c!=null&&AssetDatabase.GetAssetPath(c)==AssetDatabase.GetAssetPath(controller.normal.postProcessing))+" / "+controller.horror.postProcessing.components.All(c=>c!=null&&AssetDatabase.GetAssetPath(c)==AssetDatabase.GetAssetPath(controller.horror.postProcessing));File.WriteAllText(MoodToggleReview+"Saved Checks.txt",report);return report;
    }
}
