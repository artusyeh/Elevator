using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string InteriorModels="Assets/Building/Models/Interiors/";
    const string InteriorPrefabs="Assets/Building/Prefabs/Interiors/";
    class RoomSpace
    {
        public string id;public int floor;public float side,minZ,maxZ,depth;
        public float length=>maxZ-minZ;
        public Vector3 At(float d,float y,float v)=>new Vector3(side*(2.10f+d),y,minZ+v);
    }
    static RoomSpace RoomPlan(string id)
    {
        bool left=id=="201"||id=="503";int bay=(int.Parse(id.Substring(1))-1)/2;
        return new RoomSpace{id=id,floor=id[0]-'0',side=left?-1:1,depth=left?3.98f:3.08f,minZ=bay==0?.26f:bay==1?3.50f:6.64f,maxZ=bay==0?3.46f:bay==1?6.60f:8.99f};
    }
    static Material InteriorMaterial(string name,Color color,float smooth=.2f)
    {return Plain("Interior "+name,color,smooth);}
    public static string PrepareInteriorLibrary()
    {
        EnsureApartment();Directory.CreateDirectory(InteriorPrefabs);AssetDatabase.Refresh();
        var catalog=JsonUtility.FromJson<CommonCatalog>(File.ReadAllText(InteriorModels+"Sources.json"));
        foreach(var record in catalog.models)
        {
            foreach(var source in record.materials)
            {
                var m=Plain("Interior "+source.name,source.factor==null?Color.white:new Color(source.factor[0],source.factor[1],source.factor[2],source.factor[3]),1-source.roughness,source.metallic);
                var folder=InteriorModels+record.asset+"/";
                if(!string.IsNullOrEmpty(source.color))m.SetTexture("_BaseMap",DetailMap(folder+source.color));
                if(!string.IsNullOrEmpty(source.normal)){m.SetTexture("_BumpMap",DetailMap(folder+source.normal,true));m.EnableKeyword("_NORMALMAP");}
                if(!string.IsNullOrEmpty(source.arm)){var mask=DetailMask(folder+source.arm,folder+"textures/Unity Mask.png");m.SetTexture("_MetallicGlossMap",mask);m.SetTexture("_OcclusionMap",mask);m.EnableKeyword("_METALLICSPECGLOSSMAP");m.EnableKeyword("_OCCLUSIONMAP");}
                if(record.asset=="washing-machine"||record.asset=="refrigerator")
                {m.SetFloat("_Smoothness",.28f);m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/PBR/weathered_pvc_tank/nor_gl.jpg"));m.SetFloat("_BumpScale",.055f);m.EnableKeyword("_NORMALMAP");}
                EditorUtility.SetDirty(m);
            }
            var root=G(record.asset,null);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(InteriorModels+record.asset+"/"+record.asset+".fbx"),root);
            foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>M("Interior "+m.name)).ToArray();
            var b=BoundsOf(root);model.transform.position-=new Vector3(b.center.x,b.min.y,b.center.z);CommonDynamic(root);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,InteriorPrefabs+record.asset+".prefab");UnityEngine.Object.DestroyImmediate(root.gameObject);
        }
        var texture=new Texture2D(128,128,TextureFormat.RGB24,true);var pixels=new Color[128*128];var rng=new System.Random(21);
        for(int y=0;y<128;y++)for(int x=0;x<128;x++){float w=.72f+(x%4==0||y%4==0?-.08f:.05f)+(float)rng.NextDouble()*.05f;pixels[y*128+x]=new Color(w,w*.98f,w*.91f);}
        texture.SetPixels(pixels);texture.Apply();string texturePath=InteriorModels+"Linen Weave.asset";var existing=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);if(existing==null)AssetDatabase.CreateAsset(texture,texturePath);else{UnityEngine.Object.DestroyImmediate(texture);texture=existing;}
        foreach(var pair in new[]{("Linen Beige",new Color(.62f,.57f,.43f)),("Linen Blue",new Color(.26f,.34f,.35f)),("Dust Sheet",new Color(.49f,.48f,.40f)),("Laundry Cotton",new Color(.52f,.53f,.48f))})
        {var m=InteriorMaterial(pair.Item1,pair.Item2,.10f);m.SetTexture("_BaseMap",texture);m.SetTextureScale("_BaseMap",Vector2.one*5);m.SetFloat("_Cull",0);EditorUtility.SetDirty(m);}
        CommonPbr("Interior Old Floor","weathered_brown_planks",new Color(.63f,.57f,.45f),0,.20f);
        CommonPbr("Interior Damp Floor","square_tiled_wall",new Color(.45f,.50f,.45f),0,.38f);
        var wall=InteriorMaterial("Faded Wall",new Color(.63f,.61f,.51f),.12f);wall.CopyPropertiesFromMaterial(M("Stair Plaster"));wall.SetColor("_BaseColor",new Color(.63f,.61f,.51f));
        var wallpaper=InteriorMaterial("Old Wallpaper",new Color(.55f,.50f,.41f),.10f);wallpaper.CopyPropertiesFromMaterial(wall);wallpaper.SetColor("_BaseColor",new Color(.56f,.51f,.43f));
        var photo=InteriorMaterial("Ancestral Photograph",new Color(.74f,.69f,.56f),.10f);photo.SetTexture("_BaseMap",DetailMap("Assets/Building/Textures/Interiors/Family Photograph.jpg"));photo.SetTextureScale("_BaseMap",new Vector2(.44f,.76f));photo.SetTextureOffset("_BaseMap",new Vector2(.06f,.13f));
        InteriorMaterial("Pale Enamel",new Color(.55f,.56f,.49f),.24f);InteriorMaterial("Notebook Cover",new Color(.20f,.27f,.27f),.12f);InteriorMaterial("Medicine Carton",new Color(.68f,.66f,.54f),.13f);
        foreach(string name in new[]{"Curtain","Bed Linen","Furniture Dust Sheet","Hanging Shirt"})
        {
            var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(InteriorModels+"Linens/"+name+".fbx"));model.name=name;foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterial=M("Interior "+(name=="Curtain"?"Linen Beige":name=="Furniture Dust Sheet"?"Dust Sheet":name=="Hanging Shirt"?"Laundry Cotton":"Linen Blue"));CommonDynamic(model.transform);PrefabUtility.SaveAsPrefabAsset(model,InteriorPrefabs+name+".prefab");UnityEngine.Object.DestroyImmediate(model);
        }
        AssetDatabase.SaveAssets();return "Prepared 12 free furniture/appliance models and reusable cloth, floor and wall materials.";
    }
    static Transform InteriorProp(string name,Transform parent,Vector3 at,float yaw=0,float height=0)
    {
        var root=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(InteriorPrefabs+name+".prefab"),parent)).transform;
        root.localRotation=Quaternion.Euler(0,yaw,0);if(height>0)root.localScale*=height/BoundsOf(root).size.y;
        var b=BoundsOf(root);root.position+=parent.TransformPoint(at)-new Vector3(b.center.x,b.min.y,b.center.z);
        if(root.GetComponentsInChildren<Collider>().Length==0&&BoundsOf(root).size.y>.30f)
        {var renderers=root.GetComponentsInChildren<Renderer>();var local=LiftLocalBounds(renderers[0],root);foreach(var r in renderers.Skip(1))local.Encapsulate(LiftLocalBounds(r,root));var box=root.gameObject.AddComponent<BoxCollider>();box.center=local.center;box.size=local.size;}
        return root;
    }
    static Transform InteriorCloth(string name,Transform parent,Vector3 at,Vector3 scale,float yaw=0)
    {
        var root=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(InteriorPrefabs+name+".prefab"),parent)).transform;root.localPosition=at;root.localRotation=Quaternion.Euler(0,yaw,0);root.localScale=scale;return root;
    }
    static void RoomOpening(Transform corridor,Transform door)
    {
        var cut=new Bounds(new Vector3(Mathf.Sign(door.localPosition.x)*2.09f,1.09f,door.localPosition.z),new Vector3(.40f,2.18f,.90f));
        var group=G("Opening "+door.GetComponentInChildren<TextMesh>(true).text,corridor);
        foreach(var t in corridor.Cast<Transform>().Where(t=>new[]{"Tiled Wall","Plaster Wall","Skirting"}.Contains(t.name)&&t.gameObject.activeSelf).ToArray())
        {
            var b=LiftLocalBounds(t.GetComponent<Renderer>(),corridor);if(!b.Intersects(cut))continue;
            StairWall(group,t.name,b,new[]{cut},t.GetComponent<Renderer>().sharedMaterial.name);t.gameObject.SetActive(false);
        }
        CommonDynamic(group);
    }
    static void OperableRoomDoor(Transform door)
    {
        if(door.Find("Room Door Hinge")!=null)return;
        var leaf=door.Find("Door Leaf");foreach(string name in new[]{"Door Handle","Letter Slot"}){var part=door.Find(name);if(part!=null)part.SetParent(leaf,true);}
        var hinge=G("Room Door Hinge",door);hinge.localPosition=new Vector3(-.43f,0,.05f);leaf.SetParent(hinge,true);
        var gate=door.Find("Security Gate");var fold=G("Folding Leaf",gate);fold.localPosition=new Vector3(-.52f,0,0);
        foreach(var t in gate.Cast<Transform>().Where(t=>new[]{"Gate Stile","Folding Brace","Gate Lock","Gate Guides"}.Contains(t.name)).ToArray())t.SetParent(fold,true);
        foreach(float y in new[]{.23f,1.05f,1.88f})Rod("Door Hinge",door,new Vector3(-.43f,y,.05f),new Vector3(-.43f,y+.08f,.05f),.010f,"Cabin Aged Steel");
        var motion=door.gameObject.AddComponent<StairAccessDoor>();motion.leaf=hinge;motion.requiredKeyId=door.GetComponent<RoomAccessLock>().requiredKeyId;motion.openingAngle=-95;motion.degreesPerSecond=100;motion.locked=true;
        var roomLock=door.GetComponent<RoomAccessLock>();roomLock.entrance=motion;roomLock.foldingGate=fold;EditorUtility.SetDirty(roomLock);
    }
    static Transform InteriorShell(RoomSpace s)
    {
        var floor=GameObject.Find("Floor "+s.floor).transform;var room=G("Room "+s.id,floor);room.localPosition=Vector3.up*((s.floor-1)*2.9f);
        string wall=s.id=="404"?"Interior Old Wallpaper":"Interior Faded Wall";
        float width=s.depth+.06f;B("Room Floor",room,s.At(width*.5f,-.07f,s.length*.5f),new Vector3(width,.14f,s.length+.10f),s.id=="306"?"Interior Damp Floor":"Interior Old Floor",true,.001f);
        B("Room Ceiling",room,s.At(width*.5f,2.74f,s.length*.5f),new Vector3(width,.12f,s.length+.10f),"Concrete",true,.002f);
        foreach(float z in new[]{0,s.length})B("Room Wall",room,s.At(width*.5f,1.34f,z),new Vector3(width,2.68f,.10f),wall,true,.002f);
        B("Outer Wall",room,s.At(s.depth,1.34f,s.length*.5f),new Vector3(.12f,2.68f,s.length),wall,true,.002f);
        foreach(float z in new[]{.057f,s.length-.057f})B("Skirting",room,s.At(width*.5f,.055f,z),new Vector3(width,.11f,.014f),"Household Timber",false,.001f);
        B("Skirting",room,s.At(s.depth-.068f,.055f,s.length*.5f),new Vector3(.014f,.11f,s.length),"Household Timber",false,.001f);
        var corridor=floor.Find("Residential Corridor");var door=corridor.Cast<Transform>().First(t=>t.name=="Apartment Door"&&t.GetComponentsInChildren<TextMesh>(true).Any(x=>x.text==s.id));RoomOpening(corridor,door);OperableRoomDoor(door);
        return room;
    }
    static void InteriorLight(Transform room,RoomSpace s,Vector3 at,bool warm,float intensity)
    {
        var fixture=G("Room Fluorescent",room);fixture.localPosition=at;
        var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/Residential Fluorescent.prefab"),fixture);
        var source=G("Light",fixture);source.localPosition=Vector3.down*.17f;source.localRotation=Quaternion.Euler(90,0,0);var l=source.gameObject.AddComponent<Light>();l.type=LightType.Spot;l.intensity=intensity;l.range=4.8f;l.useColorTemperature=true;l.colorTemperature=warm?3900:5700;l.spotAngle=145;l.innerSpotAngle=100;l.shadows=LightShadows.Soft;l.shadowBias=.008f;l.shadowNormalBias=.022f;l.shadowNearPlane=.03f;l.lightmapBakeType=LightmapBakeType.Realtime;
        var data=source.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();var settings=new SerializedObject(data);settings.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;settings.ApplyModifiedPropertiesWithoutUndo();
        var spill=G("Tube Bounce",fixture);spill.localPosition=Vector3.down*.13f;var fill=spill.gameObject.AddComponent<Light>();fill.type=LightType.Point;fill.intensity=.20f;fill.range=3.0f;fill.useColorTemperature=true;fill.colorTemperature=warm?3900:5700;fill.shadows=LightShadows.None;fill.lightmapBakeType=LightmapBakeType.Realtime;
        CommonDetector(room,s.At(.42f,2.677f,s.length-.35f));
    }
    static void InteriorWindow(Transform room,RoomSpace s,float v)
    {
        var window=G("Clouded Window",room);window.localPosition=s.At(s.depth-.063f,1.65f,v);window.localRotation=Quaternion.Euler(0,s.side<0?-90:90,0);
        B("Window Recess",window,new Vector3(0,0,-.001f),new Vector3(1.22f,.99f,.008f),"Black Rubber",false,.003f);
        foreach(float x in new[]{-.61f,.61f})B("Metal Frame",window,new Vector3(x,0,-.014f),new Vector3(.045f,1.01f,.035f),"Stair Rail",false,.002f);
        foreach(float y in new[]{-.48f,.48f})B("Metal Frame",window,new Vector3(0,y,-.014f),new Vector3(1.20f,.045f,.035f),"Stair Rail",false,.002f);
        Q("Dirty Glass",window,new Vector3(0,0,-.020f),new Vector2(1.16f,.92f),M("Corridor Window Glass"),Quaternion.identity);
        B("Window Sill",window,new Vector3(0,-.525f,-.065f),new Vector3(1.30f,.045f,.15f),"Household Timber",false,.003f);
        Rod("Curtain Rail",window,new Vector3(-.70f,.66f,-.10f),new Vector3(.70f,.66f,-.10f),.008f,"Cabin Aged Steel");
        foreach(float side in new[]{-1f,1f})
        {var curtain=InteriorCloth("Curtain",window,new Vector3(side<0?-.69f:.14f,.63f,-.11f),new Vector3(.8f,1,1));}
    }
    static void InteriorPhoto(Transform room,Vector3 at,Quaternion rotation,bool empty=false)
    {
        var frame=G(empty?"Empty Photograph Frame":"Family Photograph",room);frame.localPosition=at;frame.localRotation=rotation;
        B("Picture Frame",frame,Vector3.zero,new Vector3(.27f,.22f,.014f),"Household Timber",false,.003f);
        Q("Photograph",frame,new Vector3(0,0,-.0076f),new Vector2(.23f,.18f),M(empty?"Interior Old Wallpaper":"Interior Ancestral Photograph"),Quaternion.identity);
    }
    static void InteriorBed(Transform room,RoomSpace s,float d,float v,bool family)
    {
        var bed=InteriorProp(family?"wooden-bed":"old_bed_frame",room,s.At(d,0,v),0);var b=BoundsOf(bed);if(b.size.x>b.size.z){bed.localRotation=Quaternion.Euler(0,90,0);b=BoundsOf(bed);}float width=1.00f,length=1.93f;
        bed.localScale=new Vector3(bed.localScale.x*width/b.size.x,bed.localScale.y,bed.localScale.z*length/b.size.z);
        var normalized=BoundsOf(bed);if(normalized.size.y>.95f)bed.localScale=new Vector3(bed.localScale.x,bed.localScale.y*.90f/normalized.size.y,bed.localScale.z);
        if(!family){B("Cotton Mattress",room,s.At(d,.40f,v),new Vector3(.91f,.14f,1.83f),"Interior Linen Beige",false,.055f);}
        InteriorCloth("Bed Linen",room,s.At(d,family?.52f:.48f,v-.06f),new Vector3(.83f,1,.87f));
        B("Old Pillow",room,s.At(d,.55f,v-.67f),new Vector3(.61f,.115f,.38f),"Interior Linen Beige",false,.05f);
        var box=bed.GetComponent<BoxCollider>()??bed.gameObject.AddComponent<BoxCollider>();var bound=BoundsOf(bed);box.center=bed.InverseTransformPoint(new Vector3(bound.center.x,room.position.y+.21f,bound.center.z));box.size=new Vector3(bound.size.x/Mathf.Abs(bed.lossyScale.x),.42f/Mathf.Abs(bed.lossyScale.y),bound.size.z/Mathf.Abs(bed.lossyScale.z));
    }
    static void InteriorKitchen(Transform room,RoomSpace s,float v)
    {
        var sink=InteriorProp("kitchen-base-unit-sink",room,s.At(.73f,0,v),180,.84f);var b=BoundsOf(sink);if(b.size.x>1.1f)sink.localScale=new Vector3(sink.localScale.x*1.1f/b.size.x,sink.localScale.y,sink.localScale.z);
        B("Cooker Cabinet",room,s.At(1.45f,.40f,v),new Vector3(.57f,.80f,.56f),"Household Timber",true,.008f);
        B("Gas Hob",room,s.At(1.45f,.812f,v),new Vector3(.57f,.024f,.56f),"Interior Pale Enamel",false,.004f);
        foreach(float d in new[]{1.32f,1.59f})CabinRing("Burner",room,s.At(d,.829f,v),CabinRingMesh("Kitchen Burner",.072f,.005f),M("Cabin Aged Steel")).localRotation=Quaternion.Euler(90,0,0);
        InteriorProp("cream-dome-kettle",room,s.At(1.59f,.84f,v),0,.18f);
        InteriorProp("refrigerator",room,s.At(.43f,0,.40f),0,1.35f);
        B("Tile Splashback",room,s.At(1.05f,1.25f,s.length-.058f),new Vector3(1.86f,.76f,.012f),"Green Tile",false,.001f);
        foreach(float d in new[]{.65f,1.39f}){B("Wall Cupboard",room,s.At(d,1.91f,s.length-.21f),new Vector3(.68f,.46f,.29f),"Household Timber",false,.004f);Rod("Cupboard Pull",room,s.At(d+.19f,1.79f,s.length-.36f),s.At(d+.19f,1.86f,s.length-.36f),.006f,"Cabin Aged Steel");}
    }
    static void BedroomLamp(Transform room,RoomSpace s,float d,float v)
    {
        B("Bedside Table",room,s.At(d,.22f,v),new Vector3(.30f,.44f,.33f),"Household Timber",true,.005f);
        var lamp=Prop("Desk Lamp",room,s.At(d,.443f,v),s.side<0?45:-45,.34f);
        var source=G("Bedside Light",lamp);source.position=room.TransformPoint(s.At(d,.70f,v));var light=source.gameObject.AddComponent<Light>();light.type=LightType.Point;light.intensity=.32f;light.range=1.9f;light.useColorTemperature=true;light.colorTemperature=3500;light.shadows=LightShadows.Soft;light.shadowBias=.008f;light.shadowNormalBias=.02f;light.lightmapBakeType=LightmapBakeType.Realtime;
        var data=source.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();var so=new SerializedObject(data);so.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;so.ApplyModifiedPropertiesWithoutUndo();
    }
    static void FinishInterior(Transform room,RoomSpace s)
    {
        CommonDynamic(room);var receiver=room.GetComponent<LightCircuit>();if(receiver==null)BindCircuit(room,AssetDatabase.LoadAssetAtPath<PowerCircuit>(BuildingBase+"Circuits/"+CircuitNames[s.floor]+".asset"));else{receiver.lights=room.GetComponentsInChildren<Light>(true);receiver.emitters=room.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterials.Any(m=>m!=null&&m.HasProperty("_EmissionColor")&&m.GetColor("_EmissionColor").maxColorComponent>.001f)).ToArray();EditorUtility.SetDirty(receiver);}SaveCommonModule(room,InteriorPrefabs+"Room "+s.id+".prefab");
        var door=GameObject.Find("Floor "+s.floor).transform.Find("Residential Corridor").Cast<Transform>().First(t=>t.name=="Apartment Door"&&t.GetComponentsInChildren<TextMesh>(true).Any(x=>x.text==s.id));SaveCommonModule(door,InteriorPrefabs+"Keyed Entrance "+s.id+".prefab");SaveApartment();
    }
}
