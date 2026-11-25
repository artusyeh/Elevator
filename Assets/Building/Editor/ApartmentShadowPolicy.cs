using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    static void FitShadowPolicy(Transform root)
    {
        foreach(var light in root.GetComponentsInChildren<Light>(true))
        {
            if(light.type==LightType.Directional)continue;bool fill=light.type==LightType.Point&&(new[]{"Tube Spill","Soft Fill","Lamp Spill"}.Contains(light.name)||light.transform.parent.name=="Emergency Light"||light.transform.parent.GetComponentsInChildren<Light>(true).Any(l=>l!=light&&l.type==LightType.Spot));
            if(fill){light.shadows=LightShadows.None;if(light.name=="Soft Fill")light.range=2.5f;EditorUtility.SetDirty(light);continue;}
            if(light.shadows==LightShadows.None)continue;bool indoor=light.type==LightType.Spot&&light.name!="Night Light"&&light.GetComponentsInParent<Transform>().Any(t=>new[]{"Apartment","Stairwell","Residential Corridor","Lobby","Elevator Cabin","Electrical Room","Management Room","Corridor Light"}.Contains(t.name));
            var data=light.GetComponent<UniversalAdditionalLightData>()??light.gameObject.AddComponent<UniversalAdditionalLightData>();var so=new SerializedObject(data);so.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=indoor?1:0;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(data);
        }
    }
    public static string ConfigureApartmentShadows()
    {
        EnsureApartment();foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())FitShadowPolicy(root.transform);
        foreach(string path in new[]{Base+"Prefabs/Residential Corridor.prefab",Base+"Prefabs/Entrance Interior.prefab",BuildingBase+"Prefabs/Corridor Light.prefab",Base+"Prefabs/Side Streets.prefab",Base+"Prefabs/Aoba Exterior.prefab"})
        {if(AssetDatabase.LoadAssetAtPath<GameObject>(path)==null)continue;var prefab=PrefabUtility.LoadPrefabContents(path);try{FitShadowPolicy(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}}
        var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.isActiveAndEnabled&&l.type!=LightType.Directional&&l.shadows!=LightShadows.None).ToArray();long area=0;foreach(var light in lights){var so=new SerializedObject(light.GetComponent<UniversalAdditionalLightData>());int tier=so.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue;long size=tier==1?512:256;area+=size*size*(light.type==LightType.Point?6:1);}
        string report="Shadowed punctual lights="+lights.Length+"; maximum face maps="+lights.Sum(l=>l.type==LightType.Point?6:1)+"; configured texel demand="+area+" / "+(4096L*4096L)+"; atlas unchanged at 4096.";Directory.CreateDirectory("References/Shadow Budget");File.WriteAllText("References/Shadow Budget/Checks.txt",report);SaveApartment();return report;
    }
}
