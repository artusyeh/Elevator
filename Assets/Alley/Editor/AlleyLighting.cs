using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class AlleyLighting
{
    static string Folder="Assets/Alley/Materials/";
    static Material Load(string name){return AssetDatabase.LoadAssetAtPath<Material>(Folder+name+".mat");}
    static Material Emission(string name,Color baseColor,Color emission)
    {
        var m=Load(name);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,Folder+name+".mat");}
        m.SetColor("_BaseColor",baseColor);m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);m.SetFloat("_Smoothness",.25f);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;return m;
    }
    static void SaveComponent(VolumeProfile p,VolumeComponent c){if(!AssetDatabase.Contains(c))AssetDatabase.AddObjectToAsset(c,p);EditorUtility.SetDirty(c);}
    static T Component<T>(VolumeProfile p)where T:VolumeComponent {T c;if(!p.TryGet(out c))c=p.Add<T>(true);SaveComponent(p,c);return c;}
    [MenuItem("Alley/Improve Lighting")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");
        var root=GameObject.Find("Alley");if(GameObject.Find("Window Lighting")!=null)throw new Exception("Lighting pass already applied.");
        var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach(var l in lights)
        {
            l.bounceIntensity=.8f;l.shadowBias=.025f;l.shadowNormalBias=.12f;l.shadowNearPlane=.1f;
            if(l.type==LightType.Directional){l.intensity=.045f;l.color=new Color(.78f,.84f,1);l.lightmapBakeType=LightmapBakeType.Mixed;continue;}
            var parent=l.transform.parent;
            if(parent.name=="Utilities")
            {
                l.name="Streetlight";l.type=LightType.Spot;l.transform.rotation=Quaternion.Euler(90,0,0);l.spotAngle=108;l.innerSpotAngle=67;
                l.intensity=9.5f;l.range=9;l.useColorTemperature=true;l.color=Color.white;l.colorTemperature=((int)l.transform.position.z%3==0)?3050:3850;
                l.shadows=LightShadows.Soft;l.shadowResolution=LightShadowResolution.High;l.lightmapBakeType=LightmapBakeType.Mixed;
            }
            else if(parent.name=="Vending Machine")
            {
                l.name="Display Light";l.type=LightType.Spot;l.transform.rotation=Quaternion.Euler(32,-90,0);l.spotAngle=115;l.innerSpotAngle=80;l.intensity=.85f;l.range=3.4f;l.color=Color.white;l.useColorTemperature=true;l.colorTemperature=5100;l.shadows=LightShadows.Soft;l.lightmapBakeType=LightmapBakeType.Mixed;
            }
            else
            {
                l.useColorTemperature=true;l.color=Color.white;l.colorTemperature=parent.name=="Minato Sushi"?2750:3100;l.range=Mathf.Min(l.range,4.5f);l.intensity*=.65f;l.shadows=LightShadows.Soft;l.lightmapBakeType=LightmapBakeType.Mixed;
            }
        }
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.035f,.047f,.072f);RenderSettings.ambientEquatorColor=new Color(.018f,.022f,.031f);RenderSettings.ambientGroundColor=new Color(.008f,.009f,.012f);RenderSettings.ambientIntensity=1;
        RenderSettings.fogDensity=.0025f;RenderSettings.fogColor=new Color(.015f,.025f,.046f);RenderSettings.reflectionIntensity=.65f;
        // Save every volume component as a sub-asset, so exposure and tone mapping survive reload.
        var volume=GameObject.Find("Night Lighting").GetComponent<Volume>();var profile=volume.sharedProfile;
        var exposure=Component<ColorAdjustments>(profile);exposure.postExposure.Override(.8f);exposure.contrast.Override(4);exposure.saturation.Override(-3);exposure.colorFilter.Override(Color.white);
        var tone=Component<Tonemapping>(profile);tone.mode.Override(TonemappingMode.ACES);
        var bloom=Component<Bloom>(profile);bloom.intensity.Override(.08f);bloom.threshold.Override(1.15f);bloom.scatter.Override(.35f);
        var balance=Component<WhiteBalance>(profile);balance.temperature.Override(2);balance.tint.Override(0);
        EditorUtility.SetDirty(profile);
        var vending=Emission("Vending light emission",new Color(.68f,.72f,.7f),new Color(.75f,.83f,.8f));
        var warm=Emission("Warm practical emission",new Color(.62f,.34f,.15f),new Color(1.3f,.53f,.15f));
        // Give street fixtures their own restrained diffuser instead of sharing the vending display.
        var fixture=Emission("Streetlamp Glass",new Color(.6f,.54f,.42f),new Color(1.1f,.85f,.56f));
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.name=="Streetlamp Diffuser")r.sharedMaterial=fixture;
        var unlit=Load("Unlit window");
        var wm=new[]{Emission("Curtain Amber",new Color(.32f,.23f,.13f),new Color(.44f,.24f,.085f)),Emission("Curtain Ivory",new Color(.33f,.3f,.23f),new Color(.35f,.28f,.17f)),Emission("Curtain Soft",new Color(.24f,.2f,.14f),new Color(.25f,.15f,.06f))};
        var wg=new GameObject("Window Lighting");wg.transform.SetParent(GameObject.Find("Lighting").transform,false);
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>())
        {
            if(r.name!="Window Glass")continue;
            bool shop=r.transform.parent.name=="Minato Sushi";float seed=Mathf.Abs(Mathf.Sin(r.transform.position.x*23.1f+r.transform.position.z*3.73f+r.transform.position.y*9.7f));
            bool lit=shop || seed>.67f;
            r.sharedMaterial=lit?wm[Mathf.FloorToInt(seed*10)%3]:unlit;
            if(!lit)continue;
            Vector3 towardStreet=r.transform.parent.name=="Aoba Apartments"?Vector3.back:new Vector3(-Mathf.Sign(r.transform.position.x),0,0);
            var g=new GameObject("Window Glow");g.transform.SetParent(wg.transform);g.transform.position=r.transform.position+towardStreet*.35f;
            var l=g.AddComponent<Light>();l.type=LightType.Point;l.intensity=shop?.5f:.3f;l.range=shop?2.6f:2.1f;l.color=Color.white;l.useColorTemperature=true;l.colorTemperature=2750+seed*500;l.shadows=LightShadows.Soft;l.shadowBias=.02f;l.shadowNormalBias=.08f;l.lightmapBakeType=LightmapBakeType.Mixed;
        }
        // World-mapped PBR: grain stays at real scale while larger wet masks vary along the road.
        var roughImporter=AssetImporter.GetAtPath("Assets/Alley/Textures/asphalt_02_rough_1k.jpg") as TextureImporter;roughImporter.sRGBTexture=false;roughImporter.SaveAndReimport();
        var asphalt=new Material(Shader.Find("Alley/Wet Asphalt"));asphalt.name="Rain Damp Asphalt";
        asphalt.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/asphalt_02_diff_1k.jpg"));asphalt.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/asphalt_02_nor_gl_1k.jpg"));asphalt.SetTexture("_RoughMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/asphalt_02_rough_1k.jpg"));asphalt.SetFloat("_Wetness",.38f);AssetDatabase.CreateAsset(asphalt,Folder+"Rain Damp Asphalt.mat");
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.sharedMaterial!=null && r.sharedMaterial.name.StartsWith("Damp asphalt",StringComparison.OrdinalIgnoreCase))r.sharedMaterial=asphalt;
        var pipeline=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;pipeline.maxAdditionalLightsCount=8;pipeline.shadowDistance=65;pipeline.additionalLightsShadowmapResolution=4096;pipeline.shadowDepthBias=.08f;pipeline.shadowNormalBias=.15f;
        var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset");
        foreach(var feature in data.rendererFeatures)if(feature.GetType().Name=="ScreenSpaceAmbientOcclusion")
        {
            var so=new SerializedObject(feature);so.FindProperty("m_Settings.Intensity").floatValue=.75f;so.FindProperty("m_Settings.Radius").floatValue=.24f;so.FindProperty("m_Settings.DirectLightingStrength").floatValue=.15f;so.ApplyModifiedPropertiesWithoutUndo();
        }
        data.SetDirty();EditorUtility.SetDirty(pipeline);
        var cam=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>().eyes;var cd=cam.GetUniversalAdditionalCameraData();cd.renderPostProcessing=true;cd.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;cd.antialiasingQuality=AntialiasingQuality.High;cam.allowHDR=true;
        PrepareBake(root);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
    static void PrepareBake(GameObject root)
    {
        // Bake large receivers; tiny fittings use probes plus realtime direct light and SSAO.
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>())
        {
            var b=r.bounds.size;float area=b.x*b.y+b.x*b.z+b.y*b.z;
            bool bake=area>6 && r.name!="Ground";
            var flags=GameObjectUtility.GetStaticEditorFlags(r.gameObject);flags=bake?flags|StaticEditorFlags.ContributeGI:flags&~StaticEditorFlags.ContributeGI;GameObjectUtility.SetStaticEditorFlags(r.gameObject,flags);
            if(bake){var so=new SerializedObject(r);var scale=so.FindProperty("m_ScaleInLightmap");if(scale!=null)scale.floatValue=.6f;so.ApplyModifiedPropertiesWithoutUndo();}
        }
        var settings=new LightingSettings();settings.name="Alley Lighting";settings.bakedGI=true;settings.realtimeGI=false;settings.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;settings.lightmapResolution=2;settings.lightmapMaxSize=1024;settings.indirectResolution=1;settings.directSampleCount=32;settings.indirectSampleCount=96;settings.environmentSampleCount=64;settings.maxBounces=3;settings.mixedBakeMode=MixedLightingMode.IndirectOnly;
        AssetDatabase.CreateAsset(settings,"Assets/Alley/Materials/Alley Lighting.lighting");Lightmapping.lightingSettings=settings;
        var pg=new GameObject("Light Probes");pg.transform.SetParent(GameObject.Find("Lighting").transform);var probes=pg.AddComponent<LightProbeGroup>();var points=new System.Collections.Generic.List<Vector3>();
        for(int z=2;z<=52;z+=4)foreach(float x in new[]{-2.2f,0,2.2f})foreach(float y in new[]{.3f,1.7f,3.5f})points.Add(new Vector3(x,y,z));probes.probePositions=points.ToArray();
    }
    public static string Bake(){return Lightmapping.BakeAsync()?"GI bake started":"GI bake could not start";}
    public static void Polish()
    {
        var root=GameObject.Find("Alley");
        RenderSettings.ambientSkyColor=new Color(.12f,.14f,.18f);RenderSettings.ambientEquatorColor=new Color(.08f,.085f,.1f);RenderSettings.ambientGroundColor=new Color(.045f,.045f,.05f);
        foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if(l.type==LightType.Directional)l.intensity=.14f;
            else if(l.name=="Streetlight"){l.intensity=38;l.range=10;}
            else if(l.name=="Display Light"){l.intensity=3;l.shadows=LightShadows.None;}
            else if(l.transform.parent.name!="Minato Sushi")l.shadows=LightShadows.None;
            l.shadowCustomResolution=l.type==LightType.Spot?512:256;
            var data=l.GetComponent<UniversalAdditionalLightData>();if(data==null)data=l.gameObject.AddComponent<UniversalAdditionalLightData>();
            var so=new SerializedObject(data);var tier=so.FindProperty("m_AdditionalLightsShadowResolutionTier");if(tier!=null)tier.intValue=0;so.ApplyModifiedPropertiesWithoutUndo();
        }
        var profile=GameObject.Find("Night Lighting").GetComponent<Volume>().sharedProfile;
        var color=Component<ColorAdjustments>(profile);color.postExposure.Override(.9f);color.contrast.Override(0);
        var lift=Component<LiftGammaGain>(profile);lift.lift.Override(new Vector4(1,1,1,.0018f));
        var fold=new Texture2D(128,128,TextureFormat.RGBA32,true,true);
        for(int y=0;y<128;y++)for(int x=0;x<128;x++){float v=.65f+.2f*Mathf.Sin(x*.37f)+.07f*Mathf.Sin(x*.91f+y*.025f);fold.SetPixel(x,y,new Color(v,v,v,1));}fold.Apply();
        string path="Assets/Alley/Textures/Curtain Folds.png";File.WriteAllBytes(path,fold.EncodeToPNG());UnityEngine.Object.DestroyImmediate(fold);AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.sRGBTexture=false;importer.SaveAndReimport();var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        foreach(string name in new[]{"Curtain Amber","Curtain Ivory","Curtain Soft"}){var m=Load(name);m.SetTexture("_BaseMap",texture);m.SetTexture("_EmissionMap",texture);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;m.EnableKeyword("_EMISSION");EditorUtility.SetDirty(m);}
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.name=="Drink Can") {var m=r.sharedMaterial;m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",m.GetColor("_BaseColor")*.6f);EditorUtility.SetDirty(m);}
        Load("Rain Damp Asphalt").SetFloat("_Wetness",.22f);
        EditorUtility.SetDirty(profile);foreach(var c in profile.components)EditorUtility.SetDirty(c);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
    public static void Reflections()
    {
        var parent=GameObject.Find("Lighting").transform;
        var first=GameObject.Find("Street Reflections").GetComponent<ReflectionProbe>();first.size=new Vector3(22,16,35);first.center=Vector3.zero;first.resolution=256;first.intensity=.8f;Lightmapping.BakeReflectionProbe(first,"Assets/Alley/Materials/StreetReflection.exr");
        for(int j=0;j<2;j++)
        {string name=j==0?"Midstreet Reflections":"Apartment Reflections";var g=GameObject.Find(name);if(g==null){g=new GameObject(name);g.transform.SetParent(parent);}g.transform.position=new Vector3(0,2,30+j*18);var p=g.GetComponent<ReflectionProbe>();if(p==null)p=g.AddComponent<ReflectionProbe>();p.mode=ReflectionProbeMode.Baked;p.resolution=256;p.size=new Vector3(22,16,31);p.boxProjection=true;p.blendDistance=5;p.intensity=.8f;Lightmapping.BakeReflectionProbe(p,"Assets/Alley/Materials/"+name+".exr");}
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
    public static void Capture(string stage)
    {
        AlleyBoundaries.Capture();string folder="References/Captures/Lighting/"+stage;Directory.CreateDirectory(folder);
        foreach(string f in Directory.GetFiles("References/Captures/Revision","*.png"))File.Copy(f,folder+"/"+Path.GetFileName(f),true);
    }
}
