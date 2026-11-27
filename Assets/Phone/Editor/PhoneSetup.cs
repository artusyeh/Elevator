using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string PhonePath="Assets/Phone/Resources/Phone/Phone.prefab";
    static Material PhoneMaterial(string name,Color color,float smooth=.3f,float metallic=0)
    {
        string path="Assets/Phone/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",smooth);mat.SetFloat("_Metallic",metallic);return mat;
    }
    static RectTransform PhoneRect(string name,Transform parent,Vector2 size,Vector2 at)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=at;r.gameObject.layer=5;return r;
    }
    static Image PhoneImage(string name,Transform parent,Vector2 size,Vector2 at,Color color)
    {var r=PhoneRect(name,parent,size,at);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
    static Text PhoneText(string name,Transform parent,string value,Vector2 size,Vector2 at,int fontSize,TextAnchor align=TextAnchor.MiddleCenter)
    {
        var r=PhoneRect(name,parent,size,at);var text=r.gameObject.AddComponent<Text>();text.text=value;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=fontSize;text.alignment=align;text.color=new Color(.9f,.94f,.94f);text.raycastTarget=false;text.horizontalOverflow=HorizontalWrapMode.Wrap;return text;
    }
    static Button PhoneButton(string name,Transform parent,string label,Vector2 size,Vector2 at,Color color,int fontSize=37)
    {
        var image=PhoneImage(name,parent,size,at,color);image.raycastTarget=true;var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
        var colors=button.colors;colors.highlightedColor=new Color(.8f,.9f,.95f);colors.pressedColor=new Color(.55f,.65f,.7f);button.colors=colors;
        PhoneText("Label",image.transform,label,size-Vector2.one*12,Vector2.zero,fontSize);button.navigation=new Navigation{mode=Navigation.Mode.None};return button;
    }
    static void PhonePart(string name,Transform model,Vector3 at,Vector3 size,Material material,float bevel=.002f)
    {
        var t=new GameObject(name).transform;t.SetParent(model,false);t.localPosition=at;t.gameObject.layer=5;
        t.gameObject.AddComponent<MeshFilter>().sharedMesh=Rounded(size,bevel);var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;
    }
    public static string BuildPhonePrototype()
    {
        foreach(string path in new[]{"Assets/Phone/Materials","Assets/Phone/Resources/Phone"})Directory.CreateDirectory(path);AssetDatabase.Refresh();
        var root=new GameObject("Phone");var control=root.AddComponent<PhoneControls>();var show=root.AddComponent<PhonePresentation>();var flash=root.AddComponent<PhoneFlashlight>();var apps=root.AddComponent<PhoneApplications>();var battery=root.AddComponent<PhoneBattery>();
        var visual=new GameObject("Presentation Rig").transform;visual.SetParent(root.transform,false);show.visual=visual;
        var model=new GameObject("Replaceable Phone Model").transform;model.SetParent(visual,false);
        var black=PhoneMaterial("Black Body",new Color(.025f,.028f,.030f),.45f,.35f);var glass=PhoneMaterial("Black Glass",new Color(.012f,.017f,.02f),.8f,.15f);var steel=PhoneMaterial("Camera Rim",new Color(.16f,.18f,.19f),.6f,.8f);var lens=PhoneMaterial("Camera Glass",new Color(.015f,.035f,.05f),.9f);var white=PhoneMaterial("Flash Lens",new Color(.77f,.79f,.72f),.4f);
        PhonePart("Rounded Body",model,Vector3.zero,new Vector3(.076f,.159f,.009f),black,.004f);
        PhonePart("Front Glass",model,new Vector3(0,0,-.0044f),new Vector3(.073f,.155f,.0011f),glass,.0005f);
        PhonePart("Earpiece",model,new Vector3(0,.075f,-.0051f),new Vector3(.014f,.0012f,.0006f),steel,.00025f);
        PhonePart("Power Button",model,new Vector3(.0382f,.032f,0),new Vector3(.0015f,.018f,.004f),steel,.0006f);
        PhonePart("Camera Housing",model,new Vector3(-.022f,.055f,.0054f),new Vector3(.022f,.030f,.0025f),black,.001f);
        PhonePart("Rear Camera Rim",model,new Vector3(-.022f,.060f,.007f),new Vector3(.014f,.014f,.002f),steel,.0009f);
        PhonePart("Rear Camera Lens",model,new Vector3(-.022f,.060f,.0081f),new Vector3(.010f,.010f,.0006f),lens,.00025f);
        PhonePart("Rear Flash Lens",model,new Vector3(-.022f,.041f,.007f),new Vector3(.006f,.006f,.001f),white,.0004f);
        var emitter=new GameObject("Rear Flashlight Emitter").transform;emitter.SetParent(visual,false);emitter.localPosition=new Vector3(-.022f,.041f,.0085f);flash.emitter=emitter;
        var beam=emitter.gameObject.AddComponent<Light>();beam.type=LightType.Spot;beam.spotAngle=54;beam.innerSpotAngle=24;beam.range=18;beam.intensity=24;beam.color=new Color(.92f,.96f,1);beam.shadows=LightShadows.Soft;beam.shadowCustomResolution=1024;beam.shadowBias=.025f;beam.shadowNormalBias=.08f;beam.shadowNearPlane=.04f;beam.enabled=false;beam.cullingMask=~(1<<5);flash.beam=beam;
        var spill=new GameObject("Soft Beam Edge").transform;spill.SetParent(emitter,false);var soft=spill.gameObject.AddComponent<Light>();soft.type=LightType.Spot;soft.spotAngle=78;soft.innerSpotAngle=46;soft.range=8;soft.intensity=1.4f;soft.color=beam.color;soft.cullingMask=beam.cullingMask;soft.enabled=false;flash.softBeam=soft;
        var display=PhoneRect("Physical Screen UI",visual,new Vector2(800,1600),Vector2.zero);display.localScale=Vector3.one*.000088f;display.localPosition=new Vector3(0,-.001f,-.0051f);
        var canvas=display.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.sortingOrder=20;canvas.overrideSorting=true;
        display.gameObject.AddComponent<GraphicRaycaster>();var group=display.gameObject.AddComponent<CanvasGroup>();show.screen=group;
        PhoneImage("Wallpaper",display,new Vector2(800,1600),Vector2.zero,new Color(.025f,.09f,.13f));
        PhoneImage("Wallpaper Accent",display,new Vector2(800,550),new Vector2(0,-300),new Color(.045f,.15f,.18f));
        apps.timeLabel=PhoneText("Time",display,"23:14",new Vector2(220,70),new Vector2(-235,720),37,TextAnchor.MiddleLeft);
        PhoneText("Signal",display,"||||",new Vector2(110,70),new Vector2(165,720),31);
        apps.batteryLabel=PhoneText("Battery",display,"100%",new Vector2(170,70),new Vector2(285,720),32);
        var home=PhoneRect("Home Screen",display,new Vector2(800,1360),new Vector2(0,0));apps.home=home.gameObject;
        PhoneText("Home Title",home,"HOME",new Vector2(650,80),new Vector2(0,540),34);
        PhoneText("Home Subtitle",home,"Thursday",new Vector2(650,70),new Vector2(0,440),31);
        apps.whatsAppButton=PhoneButton("WhatsApp Icon",home,"WA",new Vector2(165,165),new Vector2(-180,130),new Color(.07f,.55f,.30f),58);
        PhoneText("WhatsApp Name",home,"WhatsApp",new Vector2(300,95),new Vector2(-180,-20),32);
        apps.managementButton=PhoneButton("Management Icon",home,"AM",new Vector2(165,165),new Vector2(180,130),new Color(.24f,.36f,.47f),58);
        PhoneText("Management Name",home,"Apartment\nManagement",new Vector2(315,115),new Vector2(180,-30),29);
        var wa=PhoneRect("WhatsApp Placeholder",display,new Vector2(800,1360),Vector2.zero);apps.whatsApp=wa.gameObject;
        PhoneImage("Header",wa,new Vector2(800,145),new Vector2(0,565),new Color(.06f,.35f,.25f));PhoneText("Title",wa,"WhatsApp",new Vector2(700,100),new Vector2(0,565),47);
        PhoneText("Placeholder",wa,"No messages yet.\n\nMessaging will be added later.",new Vector2(650,350),new Vector2(0,70),35);wa.gameObject.SetActive(false);
        var am=PhoneRect("Management Placeholder",display,new Vector2(800,1360),Vector2.zero);apps.management=am.gameObject;
        PhoneImage("Header",am,new Vector2(800,145),new Vector2(0,565),new Color(.16f,.25f,.32f));PhoneText("Title",am,"Apartment Management",new Vector2(740,100),new Vector2(0,565),39);
        PhoneText("Placeholder",am,"Building services\n\nNo tasks available.\n\nTask features will be added later.",new Vector2(650,460),new Vector2(0,70),34);am.gameObject.SetActive(false);
        apps.backButton=PhoneButton("Back Button",display,"BACK",new Vector2(230,80),new Vector2(-170,-700),new Color(.06f,.13f,.17f),29);
        apps.homeButton=PhoneButton("Home Button",display,"HOME",new Vector2(230,80),new Vector2(170,-700),new Color(.06f,.13f,.17f),29);
        apps.battery=battery;control.presentation=show;control.flashlight=flash;control.applications=apps;
        foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
        visual.localPosition=show.loweredPosition;group.alpha=0;group.interactable=false;group.blocksRaycasts=false;
        PrefabUtility.SaveAsPrefabAsset(root,PhonePath);UnityEngine.Object.DestroyImmediate(root);AssetDatabase.SaveAssets();
        return "Reusable phone prefab built at "+PhonePath+"; runtime installer reuses one phone per player in Alley and Apartment.";
    }
}
