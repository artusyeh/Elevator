using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string CorridorShadowReview="References/Light Shadows/";
    static int SoftenCorridorFixtures(Transform scope)
    {
        int count=0;
        foreach(var lamp in scope.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Fluorescent Light"&&t.Find("Fixture Housing")!=null).ToArray())
        {
            // A tiny point source projecting the near housing gave ceiling-sized hard silhouettes.
            // Keep shadows from room geometry, but exclude the fixture's small self-shadow casters.
            foreach(var r in lamp.Find("Fixture Housing").GetComponentsInChildren<Renderer>()){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=true;EditorUtility.SetDirty(r);}
            var original=lamp.Find("Tube Spill").GetComponent<Light>();var second=lamp.Find("Ceiling Wash");
            if(second!=null)
            {
                var extra=second.GetComponent<Light>();foreach(var circuit in scope.GetComponentsInChildren<LightCircuit>(true).Where(c=>c.lights!=null&&c.lights.Contains(extra)))
                {circuit.lights=circuit.lights.Where(l=>l!=extra).ToArray();EditorUtility.SetDirty(circuit);}UnityEngine.Object.DestroyImmediate(second.gameObject);
            }
            // Leave room geometry as a real shadow caster so spill cannot pass through the slabs.
            // Lower the existing fill slightly to spread the wash and reduce its near-surface hotspot.
            original.type=LightType.Point;original.transform.localPosition=new Vector3(0,-.13f,0);original.transform.localRotation=Quaternion.identity;original.intensity=.65f;original.range=4;original.shadows=LightShadows.Soft;original.shadowStrength=1;original.shadowNearPlane=.06f;original.shadowBias=.008f;original.shadowNormalBias=.02f;original.lightmapBakeType=LightmapBakeType.Realtime;EditorUtility.SetDirty(original);
            count++;
        }
        return count;
    }
    public static string CaptureCorridorShadows(string suffix)
    {
        Directory.CreateDirectory(CorridorShadowReview);
        Capture("Lobby Shadows "+suffix,new Vector3(.15f,1.65f,2.0f),new Vector3(.15f,2.15f,8.0f));
        Capture("Fixture Shadows "+suffix,new Vector3(.32f,4.55f,6.06f),new Vector3(0,5.47f,6.50f));
        Capture("Corridor Shadows "+suffix,new Vector3(.35f,4.55f,9.9f),new Vector3(0,4.65f,3.5f));
        foreach(string name in new[]{"Lobby Shadows","Fixture Shadows","Corridor Shadows"})File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png",CorridorShadowReview+name+" "+suffix+".png",true);
        return "Ceiling/fixture and player-height corridor views captured";
    }
    public static string RepairCorridorShadows()
    {
        EnsureApartment();Directory.CreateDirectory(CorridorShadowReview);if(!File.Exists(CorridorShadowReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",CorridorShadowReview+"Apartment.before.unity");
        if(!File.Exists(CorridorShadowReview+"Lobby Shadows Before.png"))CaptureCorridorShadows("Before");int count=SoftenCorridorFixtures(GameObject.Find("Apartment").transform);
        foreach(string path in new[]{Base+"Prefabs/Residential Corridor.prefab",Base+"Prefabs/Entrance Interior.prefab",BuildingBase+"Prefabs/Corridor Light.prefab"})
        {var prefab=PrefabUtility.LoadPrefabContents(path);try{SoftenCorridorFixtures(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}}
        SaveApartment();CaptureCorridorShadows("After");return count+" fixtures: oversized housing shadows removed, spill redistributed below the fixture, existing downlights and circuit references retained; shared prefabs updated.";
    }
}
