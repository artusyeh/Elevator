using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string PanelLightReview="References/Elevator Panel Light/";

    static void FitPanelIllumination(Transform cabin)
    {
        var panel=cabin.Find("Control Panel");
        var previous=panel.Find("Display Lamp");
        if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var lamp=G("Display Lamp",panel);
        lamp.localPosition=new Vector3(0,1.946f,-.018f);
        // A shallow wall-mounted hood, with the light source hidden underneath.
        B("Mounting Plate",lamp,new Vector3(0,-.010f,0),new Vector3(.204f,.046f,.008f),"Cabin Aged Steel",false,.002f);
        B("Steel Hood",lamp,new Vector3(0,.015f,-.038f),new Vector3(.220f,.008f,.084f),"Cabin Control Steel",false,.0025f);
        B("Front Lip",lamp,new Vector3(0,.003f,-.078f),new Vector3(.220f,.030f,.006f),"Cabin Aged Steel",false,.0015f);
        foreach(float x in new[]{-.107f,.107f})
        {
            B("End Cap",lamp,new Vector3(x,.003f,-.037f),new Vector3(.006f,.030f,.078f),"Cabin Aged Steel",false,.002f);
            CabinScrew(lamp,new Vector3(x*.87f,-.013f,-.0045f),Quaternion.identity);
        }
        var lens=B("Recessed Diffuser",lamp,new Vector3(0,-.009f,-.042f),new Vector3(.187f,.005f,.047f),"Panel Lamp Diffuser",false,.002f).GetComponent<Renderer>();
        var source=G("Panel Wash",lamp);
        source.localPosition=new Vector3(0,-.018f,-.063f);
        source.localRotation=Quaternion.LookRotation(new Vector3(0,-.29f,.059f));
        var light=source.gameObject.AddComponent<Light>();
        light.type=LightType.Spot;light.lightmapBakeType=LightmapBakeType.Realtime;
        light.color=Color.white;light.useColorTemperature=true;light.colorTemperature=4900;
        light.intensity=.08f;light.range=1.05f;light.spotAngle=100;light.innerSpotAngle=65;
        light.shadows=LightShadows.Soft;light.shadowResolution=LightShadowResolution.Low;
        light.shadowNearPlane=.01f;light.shadowBias=.001f;light.shadowNormalBias=.003f;
        var data=source.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
        var settings=new SerializedObject(data);
        settings.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;
        settings.ApplyModifiedPropertiesWithoutUndo();
        foreach(var r in lamp.GetComponentsInChildren<MeshRenderer>())
        {
            GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);
            r.lightmapIndex=-1;r.receiveGI=ReceiveGI.LightProbes;
            r.shadowCastingMode=ShadowCastingMode.Off;
        }
        var receiver=cabin.GetComponent<LightCircuit>();
        receiver.lights=receiver.lights.Where(l=>l!=null).Concat(new[]{light}).Distinct().ToArray();
        receiver.emitters=receiver.emitters.Where(r=>r!=null).Concat(new[]{lens}).Distinct().ToArray();
        EditorUtility.SetDirty(receiver);
    }

    public static string InstallPanelIllumination()
    {
        EnsureApartment();Directory.CreateDirectory(PanelLightReview);
        if(!File.Exists(PanelLightReview+"Apartment.before.unity"))
            File.Copy("Assets/Scenes/Apartment.unity",PanelLightReview+"Apartment.before.unity");
        var lens=Plain("Panel Lamp Diffuser",new Color(.48f,.47f,.39f),.24f);
        lens.SetColor("_EmissionColor",new Color(.13f,.145f,.125f));
        lens.EnableKeyword("_EMISSION");lens.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;
        EditorUtility.SetDirty(lens);
        FitPanelIllumination(UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin);
        int count=0;
        foreach(string path in new[]{"Assets/Apartment/Prefabs/Elevator Cabin.prefab","Assets/Apartment/Prefabs/Elevator Landing.prefab","Assets/Apartment/Prefabs/Entrance Interior.prefab"})
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach(var cabin in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Elevator Cabin"))
                {FitPanelIllumination(cabin);count++;}
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        SaveApartment();return "Installed a shielded display lamp in the current cabin and "+count+" source-prefab cabins. Existing display materials, digits and controls preserved.";
    }

    public static string CapturePanelIllumination()
    {
        var c=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;
        foreach(var view in new[]{"Panel","Cabin"})
        {
            var at=view=="Panel"?new Vector3(-.40f,1.73f,.90f):new Vector3(-.28f,1.73f,1.30f);
            var target=view=="Panel"?new Vector3(.72f,1.98f,.37f):new Vector3(.48f,1.82f,.35f);
            string name="Display lighting "+view;
            Capture(name,c.TransformPoint(at),c.TransformPoint(target));
            File.Copy("References/Apartment/Captures/"+name+".png",PanelLightReview+view+".png",true);
        }
        return "Captured panel and cabin from player eye height.";
    }
}
