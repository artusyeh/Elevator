using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    const string CabinTextures="Assets/Building/Textures/Elevator/";
    static Material CabinFinish(string name,string source,Color tint,Vector2 half,float mode,float seed=0)
    {
        string path=Base+"Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null){mat=new Material(Shader.Find("Building/Cabin Surface"));AssetDatabase.CreateAsset(mat,path);}
        mat.shader=Shader.Find("Building/Cabin Surface");
        string folder=source=="rubber_tiles"?"Assets/Alley/Textures/PBR/rubber_tiles/":CabinTextures+source+"/";
        mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Diffuse.jpg"));mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"nor_gl.jpg"));mat.SetTexture("_MaskMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Unity Mask.png"));
        mat.SetColor("_BaseColor",tint);mat.SetFloat("_ReferenceLuma",mode<.5f?.045f:.065f);mat.SetFloat("_Mode",mode);mat.SetFloat("_BumpScale",mode<.5f?.22f:.18f);mat.SetFloat("_Smoothness",mode<.5f?.72f:.53f);mat.SetFloat("_Metallic",mode<.5f?.10f:mode<1.5f?.72f:0);
        mat.SetFloat("_Wear",.83f);mat.SetVector("_PanelHalfSize",new Vector4(half.x,half.y,0,0));mat.SetVector("_Meters",new Vector4(1.7f,2.4f,0,0));mat.SetVector("_UVOffset",new Vector4(seed*.213f,seed*.317f,0,0));EditorUtility.SetDirty(mat);return mat;
    }
    static Material CabinLabel(string name,bool cutout=false)
    {
        var mat=Plain("Cabin "+name,Color.white,.17f);mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(CabinTextures+"Labels/"+name+".png"));
        if(cutout){mat.SetFloat("_AlphaClip",1);mat.SetFloat("_Cutoff",.35f);mat.EnableKeyword("_ALPHATEST_ON");mat.renderQueue=2450;}
        return mat;
    }
    static Mesh CabinRingMesh(string name,float radius,float wire)
    {
        string path=Base+"Models/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old!=null)return old;
        var mesh=new Mesh();var vs=new List<Vector3>();var ns=new List<Vector3>();var triangles=new List<int>();const int steps=64,sides=8;
        for(int i=0;i<=steps;i++)for(int j=0;j<=sides;j++)
        {float a=i*Mathf.PI*2/steps,b=j*Mathf.PI*2/sides;var n=new Vector3(Mathf.Cos(a)*Mathf.Cos(b),Mathf.Sin(a)*Mathf.Cos(b),Mathf.Sin(b));vs.Add(new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0)+n*wire);ns.Add(n);if(i<steps&&j<sides){int k=i*(sides+1)+j;triangles.AddRange(new[]{k,k+sides+1,k+1,k+1,k+sides+1,k+sides+2});}}
        mesh.SetVertices(vs);mesh.SetNormals(ns);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
    }
    static Transform CabinRing(string name,Transform parent,Vector3 at,Mesh mesh,Material material)
    {var root=G(name,parent);root.localPosition=at;root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;return root;}
    static Transform CabinScrew(Transform parent,Vector3 at,Quaternion facing)
    {
        var root=G("Screw",parent);root.localPosition=at;root.localRotation=facing;
        Rod("Screw Head",root,Vector3.zero,new Vector3(0,0,-.0035f),.0032f,"Cabin Aged Steel");
        B("Screw Slot",root,new Vector3(0,0,-.0037f),new Vector3(.0042f,.0007f,.0004f),"Black Rubber",false,.0001f);return root;
    }
    static void PrepareCabinMaps()
    {
        foreach(string source in new[]{"blue_metal_plate","metal_plate_02"})
        {
            string folder=CabinTextures+source+"/";
            foreach(string file in new[]{"Diffuse.jpg","nor_gl.jpg","arm.jpg"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(folder+file);if(importer==null)throw new Exception("Missing "+folder+file);
                importer.textureType=file=="nor_gl.jpg"?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=file=="Diffuse.jpg";importer.isReadable=file=="arm.jpg";importer.anisoLevel=8;importer.maxTextureSize=2048;importer.SaveAndReimport();
            }
            var arm=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"arm.jpg");var pixels=arm.GetPixels();for(int i=0;i<pixels.Length;i++){var p=pixels[i];pixels[i]=new Color(p.b,p.r,0,1-p.g);}
            var mask=new Texture2D(arm.width,arm.height,TextureFormat.RGBA32,false,true);mask.SetPixels(pixels);mask.Apply();File.WriteAllBytes(folder+"Unity Mask.png",mask.EncodeToPNG());UnityEngine.Object.DestroyImmediate(mask);AssetDatabase.ImportAsset(folder+"Unity Mask.png");var imp=(TextureImporter)AssetImporter.GetAtPath(folder+"Unity Mask.png");imp.sRGBTexture=false;imp.anisoLevel=8;imp.SaveAndReimport();
        }
        foreach(string path in Directory.GetFiles(CabinTextures+"Labels","*.png"))
        {AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.anisoLevel=8;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();}
        Plain("Cabin Aged Steel",new Color(.25f,.27f,.25f),.46f,.75f);
        Plain("Cabin Bakelite",new Color(.035f,.042f,.037f),.26f,.08f);
        var indicator=Plain("Cabin Button Indicator",new Color(.16f,.14f,.09f),.34f,.2f);indicator.EnableKeyword("_EMISSION");indicator.SetColor("_EmissionColor",Color.black);
        var tube=Plain("Cabin Fluorescent Tube",new Color(.65f,.70f,.66f),.18f);tube.EnableKeyword("_EMISSION");tube.SetColor("_EmissionColor",new Color(.85f,.94f,.89f));
        var cover=Plain("Cabin Prismatic Cover",new Color(.70f,.72f,.58f,.56f),.21f);cover.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/PBR/weathered_pvc_tank/Diffuse.jpg"));cover.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/PBR/weathered_pvc_tank/nor_gl.jpg"));cover.EnableKeyword("_NORMALMAP");cover.SetFloat("_BumpScale",.15f);cover.SetFloat("_Surface",1);cover.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);cover.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);cover.SetFloat("_ZWrite",0);cover.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");cover.renderQueue=3000;cover.EnableKeyword("_EMISSION");cover.SetColor("_EmissionColor",new Color(.11f,.13f,.11f));
    }
    static void CabinWallFinish(Transform cabin,Transform finishes)
    {
        string[] names={"Olive Enamel","Faded Olive Enamel","Patched Olive Enamel","Cabin Ceiling Enamel"};
        Color[] colors={new Color(.30f,.35f,.26f),new Color(.34f,.38f,.29f),new Color(.27f,.32f,.24f),new Color(.24f,.28f,.22f)};
        for(int i=0;i<names.Length;i++)CabinFinish(names[i],"blue_metal_plate",colors[i],new Vector2(.24f,1.13f),0,i+1);
        CabinFinish("Cabin Brushed Steel","metal_plate_02",new Color(.40f,.41f,.38f),new Vector2(.115f,.815f),1);
        var controls=CabinFinish("Cabin Control Steel","metal_plate_02",new Color(.40f,.41f,.38f),new Vector2(.115f,.815f),1);controls.SetFloat("_TouchWear",1);EditorUtility.SetDirty(controls);
        CabinFinish("Cabin Rubber Floor","rubber_tiles",new Color(.065f,.079f,.063f),new Vector2(.75f,.72f),2);
        CabinFinish("Cabin Door Enamel","blue_metal_plate",new Color(.29f,.33f,.25f),new Vector2(.275f,1.05f),0,5);
        CabinFinish("Cabin Entrance Enamel","blue_metal_plate",new Color(.28f,.33f,.25f),new Vector2(.395f,1.07f),0,2);
        var cabinDoors=cabin.Find("Cabin Doors");if(cabinDoors!=null)foreach(var leaf in cabinDoors.Cast<Transform>())if(leaf.GetComponent<Renderer>()!=null)leaf.GetComponent<Renderer>().sharedMaterial=M("Cabin Door Enamel");
        foreach(var part in cabin.Find("Doorway Returns").Cast<Transform>())if(part.GetComponent<Renderer>()!=null&&part.name!="Floor Apron")part.GetComponent<Renderer>().sharedMaterial=M(part.name=="Inner Door Return"?"Cabin Entrance Enamel":"Cabin Brushed Steel");
        foreach(var old in cabin.Cast<Transform>().Where(t=>t.name=="Side Panel"||t.name=="Rear Panel"))old.GetComponent<Renderer>().enabled=false;
        foreach(var old in cabin.Cast<Transform>().Where(t=>t.name=="Panel Joint"||t.name=="Rust").ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        foreach(float side in new[]{-1f,1f})for(int i=0;i<3;i++)
        {
            float z=.393f+i*.483f;string mat=(side<0?"Left ":"Right ")+new[]{"Front Enamel","Middle Enamel","Rear Enamel"}[i];
            CabinFinish(mat,"blue_metal_plate",colors[(i+(side>0?1:0))%3],new Vector2(.2385f,1.13f),0,i+1+(side>0?4:0));
            var panel=B("Wall Panel",finishes,new Vector3(side*.754f,1.14f,z),new Vector3(.008f,2.26f,.477f),mat,false,.0018f);
            foreach(float y in new[]{.13f,2.17f})foreach(float edge in new[]{-.215f,.215f})CabinScrew(finishes,new Vector3(side*.748f,y,z+edge),Quaternion.Euler(0,side>0?90:-90,0));
        }
        for(int i=0;i<3;i++)
        {string mat=new[]{"Rear Left Enamel","Rear Centre Enamel","Rear Right Enamel"}[i];CabinFinish(mat,"blue_metal_plate",colors[(i+1)%3],new Vector2(.247f,1.13f),0,9+i);B("Rear Wall Panel",finishes,new Vector3(-.50f+i*.50f,1.14f,1.604f),new Vector3(.494f,2.26f,.008f),mat,false,.0015f);foreach(float x in new[]{-.219f,.219f})foreach(float y in new[]{.14f,2.18f})CabinScrew(finishes,new Vector3(-.50f+i*.50f+x,y,1.599f),Quaternion.identity);}
        cabin.Find("Cabin Ceiling").GetComponent<Renderer>().sharedMaterial=M("Cabin Ceiling Enamel");cabin.Find("Cabin Floor").GetComponent<Renderer>().sharedMaterial=M("Cabin Rubber Floor");cabin.Find("Doorway Returns/Floor Apron").GetComponent<Renderer>().sharedMaterial=M("Cabin Rubber Floor");
        foreach(var t in cabin.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("Skirting")||t.name=="Handrail"||t.name=="Rear Handrail"||t.name=="Rail Bracket"))if(t.GetComponent<Renderer>()!=null)t.GetComponent<Renderer>().sharedMaterial=M("Cabin Brushed Steel");
        foreach(float side in new[]{-1f,1f})foreach(float z in side<0?new[]{.30f,1.35f}:new[]{.72f,1.35f})
        {
            var flange=G("Handrail Mount",finishes);flange.localPosition=new Vector3(side*.750f,.94f,z);flange.localRotation=Quaternion.Euler(0,side>0?90:-90,0);
            Rod("Mounting Flange",flange,Vector3.zero,new Vector3(0,0,-.008f),.031f,"Cabin Aged Steel");
            foreach(float x in new[]{-.021f,.021f})CabinScrew(flange,new Vector3(x,0,-.008f),Quaternion.identity);
        }
        foreach(float x in new[]{-.52f,.52f})
        {Rod("Rear Rail Support",finishes,new Vector3(x,.94f,1.60f),new Vector3(x,.94f,1.50f),.014f,"Cabin Aged Steel");Rod("Mounting Flange",finishes,new Vector3(x,.94f,1.60f),new Vector3(x,.94f,1.588f),.031f,"Cabin Aged Steel");}
    }
    static void CabinControlFinish(Transform cabin)
    {
        var panel=cabin.Find("Control Panel");panel.Find("Panel Face").GetComponent<Renderer>().sharedMaterial=M("Cabin Control Steel");
        var panelPosition=panel.localPosition;panelPosition.y=.28f;panel.localPosition=panelPosition;
        var previous=panel.Find("Button Details");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);var details=G("Button Details",panel);
        foreach(var text in panel.GetComponentsInChildren<TextMesh>())text.GetComponent<Renderer>().enabled=false;
        foreach(var control in panel.GetComponentsInChildren<ElevatorButton>())
        {
            if(control.cap==null)continue;var old=control.cap.Find("Button Marking");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            control.cap.GetComponent<Renderer>().sharedMaterial=M(control.action==ElevatorButton.Action.Alarm?"Red Enamel":"Cabin Bakelite");
            var p=control.transform.localPosition;float radius=control.action==ElevatorButton.Action.Open||control.action==ElevatorButton.Action.Close?.028f:.032f;
            CabinRing("Button Bezel",details,new Vector3(p.x,p.y,-.039f),CabinRingMesh(radius<.03f?"Door Button Bezel":"Floor Button Bezel",radius,.003f),M("Cabin Aged Steel"));
            var lamp=CabinRing("Button Indicator",details,new Vector3(p.x,p.y,-.043f),CabinRingMesh(radius<.03f?"Door Button Indicator":"Floor Button Indicator",radius-.002f,.0008f),M("Cabin Button Indicator"));control.lamp=lamp.GetComponent<Renderer>();
            string label=control.action==ElevatorButton.Action.Floor?"Floor "+control.floor:control.action.ToString();
            var marking=Q("Button Marking",panel,new Vector3(p.x,p.y,-.0415f),new Vector2(radius*1.35f,radius*1.35f),CabinLabel(label,true),Quaternion.identity);marking.SetParent(control.cap,true);
            EditorUtility.SetDirty(control);
        }
        B("Speaker Recess",details,new Vector3(0,1.62f,-.0155f),new Vector3(.15f,.13f,.003f),"Black Rubber",false,.003f);
        for(int row=0;row<7;row++)for(int col=0;col<9;col++)
        Rod("Speaker Mesh",details,new Vector3(-.058f+col*.0145f,1.571f+row*.016f,-.018f),new Vector3(-.058f+col*.0145f,1.571f+row*.016f,-.0195f),.0015f,"Cabin Aged Steel");
        foreach(float x in new[]{-.096f,.096f})foreach(float y in new[]{.32f,.66f,1.52f,1.88f})CabinScrew(details,new Vector3(x,y,-.0135f),Quaternion.identity);
    }
    static void CabinFixtureFinish(Transform cabin,Transform finishes)
    {
        foreach(var t in cabin.Find("Cabin Details").Cast<Transform>().Where(t=>t.name=="Light Housing"||t.name=="Frosted Light Cover").ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        foreach(var old in cabin.Cast<Transform>().Where(t=>t.name=="Ceiling Vent"||t.name=="Ventilation Fan"||t.name=="Fan Mounting Plate").ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        var fixture=G("Fluorescent Fixture",finishes);
        B("Pressed Steel Housing",fixture,new Vector3(0,2.245f,.70f),new Vector3(.91f,.05f,.28f),"Cabin Ceiling Enamel",false,.012f);
        foreach(float z in new[]{.645f,.755f})
        {
            Rod("Fluorescent Tube",fixture,new Vector3(-.365f,2.195f,z),new Vector3(.365f,2.195f,z),.017f,"Cabin Fluorescent Tube");
            foreach(float x in new[]{-.378f,.378f})B("Ceramic Socket",fixture,new Vector3(x,2.195f,z),new Vector3(.025f,.052f,.048f),"Aged Plastic",false,.004f);
        }
        B("Prismatic Cover",fixture,new Vector3(0,2.164f,.70f),new Vector3(.83f,.030f,.225f),"Cabin Prismatic Cover",false,.012f);
        foreach(float x in new[]{-.43f,.43f})B("Cover Clip",fixture,new Vector3(x,2.194f,.70f),new Vector3(.018f,.085f,.27f),"Cabin Aged Steel",false,.003f);
        foreach(float x in new[]{-.405f,.405f})foreach(float z in new[]{.595f,.805f})CabinScrew(fixture,new Vector3(x,2.214f,z),Quaternion.Euler(-90,0,0));
        var oldFixture=cabin.Find("Fluorescent Light");foreach(var r in oldFixture.GetComponentsInChildren<Renderer>())r.enabled=false;
        foreach(var light in oldFixture.GetComponentsInChildren<Light>())
        {
            bool spot=light.type==LightType.Spot;light.transform.position=cabin.TransformPoint(new Vector3(0,spot?2.10f:2.15f,.70f));light.transform.rotation=cabin.rotation*Quaternion.Euler(90,0,0);light.intensity=spot?2.3f:.18f;light.color=Color.white;light.useColorTemperature=true;light.colorTemperature=5100;light.range=3.1f;light.bounceIntensity=1;light.lightmapBakeType=LightmapBakeType.Realtime;light.shadows=spot?LightShadows.Soft:LightShadows.None;light.shadowBias=.006f;light.shadowNormalBias=.018f;if(spot){light.spotAngle=130;light.innerSpotAngle=95;}
        }
        foreach(var light in cabin.Find("Cabin Details").GetComponentsInChildren<Light>()){light.transform.position=cabin.TransformPoint(new Vector3(0,.92f,.88f));light.intensity=.35f;light.colorTemperature=5100;light.range=1.8f;light.shadows=LightShadows.None;}
        var emergency=cabin.Find("Emergency Light");if(emergency==null)emergency=EmergencyFixture(cabin,cabin.TransformPoint(new Vector3(-.695f,2.12f,1.14f)),AssetDatabase.LoadAssetAtPath<PowerCircuit>(BuildingBase+"Circuits/Emergency.asset"));emergency.localPosition=new Vector3(-.695f,2.12f,1.14f);emergency.localRotation=Quaternion.Euler(0,-90,0);foreach(var light in emergency.GetComponentsInChildren<Light>()){light.intensity=.09f;light.colorTemperature=5100;light.shadows=LightShadows.None;}
        var fan=G("Ventilation Fan",finishes);fan.localPosition=new Vector3(.751f,2.115f,1.30f);fan.localRotation=Quaternion.Euler(0,90,0);
        B("Fan Casing",fan,new Vector3(0,0,-.009f),new Vector3(.258f,.258f,.018f),"Patched Olive Enamel",false,.008f);
        Rod("Fan Recess",fan,new Vector3(0,0,-.019f),new Vector3(0,0,-.026f),.108f,"Black Rubber");
        for(int i=0;i<4;i++){var blade=B("Fan Blade",fan,new Vector3(0,.038f,-.031f),new Vector3(.041f,.094f,.005f),"Cabin Aged Steel",false,.009f);blade.RotateAround(fan.TransformPoint(new Vector3(0,0,-.031f)),fan.forward,31+i*90);}
        Rod("Fan Hub",fan,new Vector3(0,0,-.032f),new Vector3(0,0,-.041f),.021f,"Cabin Aged Steel");
        foreach(float radius in new[]{.035f,.064f,.095f})CabinRing("Wire Guard",fan,new Vector3(0,0,-.050f),CabinRingMesh(radius<.04f?"Fan Inner Guard":radius<.08f?"Fan Middle Guard":"Fan Outer Guard",radius,.0018f),M("Cabin Aged Steel"));
        for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Rod("Guard Spoke",fan,new Vector3(0,0,-.051f),new Vector3(Mathf.Cos(a)*.103f,Mathf.Sin(a)*.103f,-.051f),.0013f,"Cabin Aged Steel");}
        foreach(float x in new[]{-.110f,.110f})foreach(float y in new[]{-.110f,.110f})CabinScrew(fan,new Vector3(x,y,-.019f),Quaternion.identity);
        var grille=G("Ceiling Vent",finishes);B("Vent Recess",grille,new Vector3(.58f,2.266f,.90f),new Vector3(.23f,.006f,.34f),"Black Rubber",false,.004f);
        for(int i=0;i<10;i++)B("Vent Louver",grille,new Vector3(.58f,2.257f,.757f+i*.032f),new Vector3(.215f,.008f,.016f),"Cabin Aged Steel",false,.001f);
    }
    static void CabinDocuments(Transform cabin,Transform finishes)
    {
        var group=G("Certificates",finishes);
        foreach(var spec in new[]{new Vector4(.875f,1.63f,.31f,.414f),new Vector4(1.365f,1.57f,.265f,.33f)})
        {
            string name=spec.x<1?"Inspection Certificate":"Maintenance Notice";var frame=G(name,group);frame.localPosition=new Vector3(-.750f,spec.y,spec.x);frame.localRotation=Quaternion.Euler(0,-90,0);
            B("Metal Frame",frame,new Vector3(0,0,-.008f),new Vector3(spec.z+.026f,spec.w+.026f,.016f),"Cabin Aged Steel",false,.003f);
            Q("Printed Paper",frame,new Vector3(0,0,-.0165f),new Vector2(spec.z,spec.w),CabinLabel(name),Quaternion.identity);
            foreach(float x in new[]{-spec.z*.5f-.005f,spec.z*.5f+.005f})foreach(float y in new[]{-spec.w*.5f-.005f,spec.w*.5f+.005f})CabinScrew(frame,new Vector3(x,y,-.016f),Quaternion.identity);
        }
        foreach(var spec in new[]{new Vector4(.39f,1.87f,.27f,.079f),new Vector4(.39f,1.69f,.25f,.067f)})
        {
            string name=spec.y>1.8f?"Capacity Plate":"Manufacturer Plate";var plate=G(name,group);plate.localPosition=new Vector3(-.750f,spec.y,spec.x);plate.localRotation=Quaternion.Euler(0,-90,0);
            B("Metal Plate",plate,new Vector3(0,0,-.003f),new Vector3(spec.z+.012f,spec.w+.012f,.006f),"Cabin Aged Steel",false,.001f);Q("Lettering",plate,new Vector3(0,0,-.0065f),new Vector2(spec.z,spec.w),CabinLabel(name),Quaternion.identity);
        }
    }
    static void ApplyCabinVisuals(Transform cabin)
    {
        var old=cabin.Find("Cabin Finishes");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);var finishes=G("Cabin Finishes",cabin);
        CabinWallFinish(cabin,finishes);CabinControlFinish(cabin);CabinFixtureFinish(cabin,finishes);CabinDocuments(cabin,finishes);
        var mirror=cabin.Find("Mirror");var at=mirror.localPosition;at.y=1.72f;mirror.localPosition=at;var frame=cabin.Find("Mirror Frame");at=frame.localPosition;at.y=1.72f;frame.localPosition=at;frame.GetComponent<Renderer>().sharedMaterial=M("Cabin Brushed Steel");
        var material=Plain("Aged Cabin Mirror",Color.white);material.shader=Shader.Find("Building/Aged Mirror");material.SetFloat("_Age",.75f);material.SetTexture("_Dirt",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/Weathering/Discoloration.png"));mirror.GetComponent<Renderer>().sharedMaterial=material;
        foreach(float x in new[]{-.605f,.605f})foreach(float y in new[]{1.305f,2.135f})CabinScrew(finishes,new Vector3(x,y,1.572f),Quaternion.identity);
        var receiver=cabin.GetComponent<LightCircuit>();if(receiver==null){receiver=cabin.gameObject.AddComponent<LightCircuit>();receiver.circuit=AssetDatabase.LoadAssetAtPath<PowerCircuit>(BuildingBase+"Circuits/Elevator.asset");}var emergency=cabin.Find("Emergency Light");
        receiver.lights=cabin.GetComponentsInChildren<Light>(true).Where(l=>!l.transform.IsChildOf(emergency)).ToArray();receiver.emitters=cabin.GetComponentsInChildren<Renderer>(true).Where(r=>!r.transform.IsChildOf(emergency)&&r.enabled&&r.sharedMaterials.Any(m=>m!=null&&m.HasProperty("_EmissionColor")&&m.GetColor("_EmissionColor").maxColorComponent>.001f)).ToArray();
        foreach(var t in cabin.GetComponentsInChildren<Transform>(true)){GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);var r=t.GetComponent<Renderer>();if(r!=null)r.lightmapIndex=-1;}
        EditorUtility.SetDirty(receiver);
    }
    public static string RedesignCabin()
    {
        EnsureApartment();PrepareCabinMaps();var cabin=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;ApplyCabinVisuals(cabin);
        foreach(string name in new[]{"Elevator Cabin","Elevator Landing","Entrance Interior"})
        {
            string path=Base+"Prefabs/"+name+".prefab";var prefab=PrefabUtility.LoadPrefabContents(path);
            try{foreach(var c in prefab.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Elevator Cabin").ToArray())ApplyCabinVisuals(c);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        }
        SaveApartment();CaptureCabinReview("After");return "Cabin finishes, fixtures, mirror, rubber flooring and control hardware redesigned; original functional components retained.";
    }
}
