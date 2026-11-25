using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string ResidentialFixtureReview="References/Residential Lights/";
    static void PrepareResidentialFixture()
    {
        const string tex=BuildingBase+"Textures/Residential Fluorescent/";
        var housing=Plain("Residential Light Housing",Color.white,.18f,.02f);housing.SetTexture("_BaseMap",DetailMap(tex+"Housing.png"));
        var lens=Plain("Residential Light Cover",Color.white,.17f);lens.SetTexture("_BaseMap",DetailMap(tex+"Diffuser.png"));lens.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tex+"Diffuser.png"));lens.SetColor("_EmissionColor",new Color(.32f,.335f,.32f));lens.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;lens.EnableKeyword("_EMISSION");lens.SetFloat("_SpecularHighlights",0);lens.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");EditorUtility.SetDirty(lens);
        var root=new GameObject("Residential Fluorescent");try
        {
            var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Models/Residential Fluorescent/Residential Fluorescent.fbx"),root.transform);model.name="Covered Fluorescent";
            foreach(var r in model.GetComponentsInChildren<Renderer>()){r.sharedMaterial=r.name.StartsWith("Diffuser")?lens:r.name.StartsWith("Cover Clip")?M("Cabin Aged Steel"):housing;r.shadowCastingMode=ShadowCastingMode.Off;r.lightmapIndex=-1;PrefabUtility.RecordPrefabInstancePropertyModifications(r);GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);}
            PrefabUtility.SaveAsPrefabAsset(root,BuildingBase+"Prefabs/Residential Fluorescent.prefab");
        }finally{UnityEngine.Object.DestroyImmediate(root);}
    }
    static int FitResidentialFixtures(Transform scope)
    {
        int count=0;var ceilings=scope.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.name.ToLower().Contains("ceiling")&&r.bounds.size.y>.025f).ToArray();
        foreach(var lamp in scope.GetComponentsInChildren<Transform>(true).Where(t=>(t.name=="Fluorescent Light"&&(t.parent.name=="Lobby"||t.parent.name=="Residential Corridor"))||(t==scope&&t.name=="Corridor Light")).ToArray())
        {
            var primary=lamp.Find("Light").GetComponent<Light>();float gap=.23f;var ceiling=ceilings.Where(r=>r.bounds.min.y>lamp.position.y&&r.bounds.min.y<lamp.position.y+.7f&&r.bounds.min.x<lamp.position.x&&r.bounds.max.x>lamp.position.x&&r.bounds.min.z<lamp.position.z&&r.bounds.max.z>lamp.position.z).OrderBy(r=>r.bounds.min.y).FirstOrDefault();if(ceiling!=null)gap=ceiling.bounds.min.y-lamp.position.y;
            var old=lamp.Find("Residential Fixture");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);old=lamp.Find("Fixture Housing");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            foreach(var r in lamp.GetComponentsInChildren<Renderer>(true))r.enabled=false;
            var fixture=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/Residential Fluorescent.prefab"),lamp);fixture.name="Residential Fixture";fixture.transform.localPosition=new Vector3(0,gap-.001f,0);PrefabUtility.RecordPrefabInstancePropertyModifications(fixture.transform);
            primary.transform.localPosition=new Vector3(0,gap-.095f,0);primary.transform.localRotation=Quaternion.Euler(90,0,0);primary.useColorTemperature=true;primary.colorTemperature=5200;primary.color=new Color(.93f,.98f,1);primary.intensity=lamp.parent!=null&&lamp.parent.name=="Lobby"?5f:4f;primary.range=4.2f;primary.spotAngle=135;primary.innerSpotAngle=95;primary.shadows=LightShadows.Soft;
            var fill=lamp.Find("Tube Spill").GetComponent<Light>();fill.transform.localPosition=new Vector3(0,gap-.17f,0);fill.useColorTemperature=true;fill.colorTemperature=5200;fill.color=new Color(.93f,.98f,1);fill.intensity=.16f;fill.range=2.35f;fill.shadows=LightShadows.Soft;
            var emitter=fixture.GetComponentsInChildren<Renderer>().First(r=>r.name.StartsWith("Diffuser"));foreach(var circuit in scope.GetComponentsInChildren<LightCircuit>(true).Where(c=>c.lights!=null&&c.lights.Contains(primary))){circuit.emitters=circuit.emitters.Where(r=>r!=null&&!r.transform.IsChildOf(lamp)).Concat(new[]{emitter}).ToArray();EditorUtility.SetDirty(circuit);}count++;
        }
        FitShadowPolicy(scope);return count;
    }
    public static string FinishResidentialLighting()
    {
        EnsureApartment();Directory.CreateDirectory(ResidentialFixtureReview);if(!File.Exists(ResidentialFixtureReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",ResidentialFixtureReview+"Apartment.before.unity");PrepareResidentialFixture();int count=FitResidentialFixtures(GameObject.Find("Apartment").transform);
        foreach(string path in new[]{Base+"Prefabs/Residential Corridor.prefab",Base+"Prefabs/Entrance Interior.prefab",BuildingBase+"Prefabs/Corridor Light.prefab"}){var prefab=PrefabUtility.LoadPrefabContents(path);try{FitResidentialFixtures(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}}
        SaveApartment();CaptureCorridorShadows("Residential");foreach(string name in new[]{"Lobby Shadows","Fixture Shadows","Corridor Shadows"})File.Copy("References/Light Shadows/"+name+" Residential.png",ResidentialFixtureReview+name+".png",true);CaptureCorridorEnds("After");return count+" flush covered residential fluorescent units installed; circuit emitters rebound and scene saved.";
    }
}
