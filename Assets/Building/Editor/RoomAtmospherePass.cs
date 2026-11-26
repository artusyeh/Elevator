using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string DeepAgeRoomFurniture()
    {
        EnsureApartment();var catalog=JsonUtility.FromJson<CommonCatalog>(File.ReadAllText(InteriorModels+"Sources.json"));int count=0;
        foreach(var record in catalog.models.Where(r=>File.Exists(InteriorModels+r.asset+"/"+r.asset+".glb")))foreach(var source in record.materials.Where(m=>!string.IsNullOrEmpty(m.color)))
        {
            string folder=InteriorModels+record.asset+"/";var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);tex.LoadImage(File.ReadAllBytes(folder+source.color));var pixels=tex.GetPixels32();int width=tex.width,height=tex.height;
            var arm=new Texture2D(2,2,TextureFormat.RGBA32,false);Color32[] occlusion=null;if(!string.IsNullOrEmpty(source.arm)){arm.LoadImage(File.ReadAllBytes(folder+source.arm));occlusion=arm.GetPixels32();}
            bool enamel=record.asset=="washing-machine"||record.asset=="refrigerator"||record.asset=="kitchen-base-unit-sink";float seed=count*13.7f;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                int index=y*width+x;Color c=pixels[index];float u=x/(float)width,v=y/(float)height;
                float damp=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.53f,.80f,Mathf.PerlinNoise(u*6.6f+seed,v*8.1f+4)));
                float dirt=occlusion!=null?1-occlusion[Mathf.Min(arm.height-1,y*arm.height/height)*arm.width+Mathf.Min(arm.width-1,x*arm.width/width)].r/255f:0;
                c.r*=1-damp*.43f-dirt*.36f;c.g*=.96f-damp*.49f-dirt*.39f;c.b*=.86f-damp*.51f-dirt*.40f;pixels[index]=c;
            }
            var random=new System.Random(928+count);
            for(int k=0;k<(enamel?360:160);k++)
            {
                int sx=random.Next(width),sy=random.Next(height),length=random.Next(4,Mathf.Max(5,width/28));float slope=(float)random.NextDouble()*.20f-.10f;
                for(int q=0;q<length;q++)
                {int x=sx+q,y=sy+(int)(q*slope);if(x>=width||y<1||y>=height-1)break;int index=y*width+x;Color c=pixels[index];if(c.maxColorComponent<.16f)continue;pixels[index]=Color.Lerp(c,enamel?new Color(.24f,.17f,.10f):new Color(.35f,.29f,.20f),.55f);if(q%4!=0)pixels[index+width]=Color.Lerp(pixels[index+width],new Color(.68f,.64f,.53f),.30f);}
            }
            tex.SetPixels32(pixels);tex.Apply();File.WriteAllBytes(folder+"textures/Aged Finish.png",tex.EncodeToPNG());AssetDatabase.ImportAsset(folder+"textures/Aged Finish.png",ImportAssetOptions.ForceUpdate);
            UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(arm);count++;
        }
        AssetDatabase.SaveAssets();return "Eight furniture texture sets visibly aged with AO-guided grime, irregular worn finish and localized enamel scratches; original textures retained.";
    }
    static void HideRoomParts(Transform room,params string[] names)
    {foreach(var t in room.Cast<Transform>().Where(t=>names.Contains(t.name)))t.gameObject.SetActive(false);}
    static Transform EntranceFor(RoomSpace s)
    {return GameObject.Find("Floor "+s.floor).transform.Find("Residential Corridor").Cast<Transform>().First(t=>t.GetComponent<RoomAccessLock>()!=null&&t.GetComponent<RoomAccessLock>().roomNumber==s.id);}
    static void SeparateRoomEntrance(RoomSpace s)
    {
        var door=EntranceFor(s);var roomLock=door.GetComponent<RoomAccessLock>();if(roomLock.gate!=null)return;
        var oldGate=door.Find("Security Gate");var bars=oldGate.Find("Folding Leaf");bars.localScale=Vector3.one;
        var hinge=G("Outward Gate Hinge",door);hinge.localPosition=new Vector3(-.424f,0,-.115f);
        bars.SetParent(hinge,true);bars.name="Security Gate Leaf";
        // Remove sliding guide shoes: this entrance now has a hinged grille.
        var guides=bars.Find("Gate Guides");if(guides!=null)guides.gameObject.SetActive(false);
        foreach(var child in bars.GetComponentsInChildren<Transform>().Where(t=>t.name=="Gate Lock")){var p=child.localPosition;p.x=1.00f;child.localPosition=p;}
        var proxy=hinge.gameObject.AddComponent<BoxCollider>();proxy.center=new Vector3(.424f,1.08f,0);proxy.size=new Vector3(.83f,2.04f,.055f);
        var latch=hinge.gameObject.AddComponent<RoomGateLatch>();latch.leaf=hinge;latch.requiredKeyId=roomLock.requiredKeyId;latch.openingAngle=105;latch.locked=true;latch.open=false;
        foreach(float y in new[]{.24f,1.08f,1.94f})Rod("Gate Hinge Barrel",door,new Vector3(-.429f,y,-.115f),new Vector3(-.429f,y+.075f,-.115f),.012f,"Cabin Aged Steel");
        var channel=door.Find("Gate Mounting Frame/Bottom Guide Channel");if(channel!=null)channel.gameObject.SetActive(false);
        roomLock.gate=latch;roomLock.foldingGate=null;roomLock.locked=true;roomLock.entrance.SetLocked(true);roomLock.entrance.open=false;roomLock.entrance.openingAngle=-95;roomLock.entrance.leaf.localRotation=Quaternion.identity;
        var leaf=roomLock.entrance.leaf.Find("Door Leaf");var handle=leaf.gameObject.GetComponent<RoomDoorHandle>()??leaf.gameObject.AddComponent<RoomDoorHandle>();handle.roomLock=roomLock;
        EditorUtility.SetDirty(roomLock);CommonDynamic(hinge);
    }
    static void RoomWallSwitch(Transform room,RoomSpace s)
    {
        var receiver=room.GetComponent<LightCircuit>();var supply=AssetDatabase.LoadAssetAtPath<PowerCircuit>(BuildingBase+"Circuits/"+CircuitNames[s.floor]+".asset");
        string path=BuildingBase+"Circuits/Room "+s.id+" Lights.asset";var circuit=AssetDatabase.LoadAssetAtPath<PowerCircuit>(path);
        if(circuit==null){circuit=ScriptableObject.CreateInstance<PowerCircuit>();circuit.id="Room "+s.id+" Lights";circuit.label="Room "+s.id+" Lights";circuit.startsEnabled=true;AssetDatabase.CreateAsset(circuit,path);}
        receiver.circuit=circuit;receiver.supplyCircuit=supply;EditorUtility.SetDirty(receiver);
        if(room.Find("Room Light Switch")!=null)return;
        var door=EntranceFor(s);var root=G("Room Light Switch",room);root.localPosition=new Vector3(s.side*2.189f,1.28f,door.localPosition.z-.68f);root.localRotation=Quaternion.Euler(0,s.side<0?90:-90,0);
        B("Aged Bakelite Plate",root,Vector3.zero,new Vector3(.073f,.112f,.018f),"Interior Switch Bakelite",false,.009f);
        B("Toggle Recess",root,new Vector3(0,0,-.010f),new Vector3(.032f,.054f,.005f),"Black Rubber",false,.003f);
        var rocker=G("Rocker Pivot",root);rocker.localPosition=new Vector3(0,0,-.015f);B("Yellowed Rocker",rocker,Vector3.zero,new Vector3(.025f,.044f,.010f),"Interior Switch Bakelite",false,.003f);
        foreach(float y in new[]{-.043f,.043f})Rod("Plate Screw",root,new Vector3(0,y,-.0095f),new Vector3(0,y,-.011f),.0025f,"Cabin Aged Steel");
        var box=root.gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,0,-.006f);box.size=new Vector3(.095f,.14f,.030f);
        var control=root.gameObject.AddComponent<PowerSwitch>();control.circuit=circuit;control.lever=rocker;CommonDynamic(root);
    }
    static void MoveFurnitureAssembly(Transform room,string[] names,Vector3 delta)
    {foreach(var t in room.Cast<Transform>().Where(t=>names.Contains(t.name)))t.localPosition+=delta;}
    static void DifferentiateFamilyRoom(Transform room)
    {
        if(room.Find("Child Bunk Frame")!=null)return;
        var s=RoomPlan("503");
        HideRoomParts(room,"chinese_tea_table","Household Stool","Tea Cup","Tea Surface","Tea Tray","Child Writing Desk","Desk Leg","Schoolbook","Page Edges","Old Pillow","Bed Linen");
        // Move the galley to the front wall, with a clear entrance and bedroom route.
        SeatInterior(room.Find("kitchen-base-unit-sink"),room,new Vector3(-3.31f,0,3.88f),0);
        foreach(var t in room.Cast<Transform>().Where(t=>new[]{"Cooker Cabinet","Gas Hob","Burner","cream-dome-kettle"}.Contains(t.name)))
        {var p=t.localPosition;p.x+=1.03f;p.z=3.86f;t.localPosition=p;}
        var splash=room.Find("Tile Splashback");splash.localPosition=new Vector3(-3.17f,1.22f,3.558f);
        foreach(var t in room.Cast<Transform>().Where(t=>t.name=="Wall Cupboard"||t.name=="Cupboard Pull"))
        {var p=t.localPosition;p.z=t.name=="Wall Cupboard"?3.71f:3.865f;t.localPosition=p;t.localRotation=Quaternion.Euler(0,180,0);}
        SeatInterior(room.Find("refrigerator"),room,new Vector3(-2.55f,0,6.21f),180);
        var bed=room.Find("wooden-bed");SeatInterior(bed,room,new Vector3(-5.45f,0,5.50f),bed.localEulerAngles.y);
        var frame=G("Child Bunk Frame",room);
        foreach(float x in new[]{-5.92f,-4.98f})foreach(float z in new[]{4.55f,6.45f})B("Worn Bunk Post",frame,new Vector3(x,.97f,z),new Vector3(.055f,1.94f,.055f),"Interior Family Painted Timber",true,.006f);
        B("Upper Bunk Base",frame,new Vector3(-5.45f,1.36f,5.50f),new Vector3(.97f,.09f,1.94f),"Interior Family Painted Timber",true,.008f);
        B("Upper Cotton Mattress",frame,new Vector3(-5.45f,1.46f,5.50f),new Vector3(.90f,.12f,1.83f),"Interior Linen Beige",false,.035f);
        InteriorCloth("Bed Linen",frame,new Vector3(-5.45f,1.523f,5.50f),new Vector3(.77f,1,.85f));
        foreach(float x in new[]{-5.92f,-4.98f})B("Upper Guard Rail",frame,new Vector3(x,1.77f,5.50f),new Vector3(.034f,.075f,1.85f),"Interior Family Painted Timber",true,.004f);
        foreach(float z in new[]{4.65f,6.35f})foreach(float x in new[]{-5.92f,-4.98f})B("Guard Rail Support",frame,new Vector3(x,1.61f,z),new Vector3(.030f,.31f,.030f),"Interior Family Painted Timber",false,.003f);
        foreach(float y in new[]{.18f,.43f,.68f,.93f,1.18f})B("Bunk Ladder Rung",frame,new Vector3(-5.45f,y,4.515f),new Vector3(.47f,.025f,.028f),"Interior Family Painted Timber",false,.003f);
        foreach(float x in new[]{-5.70f,-5.20f})B("Bunk Ladder Upright",frame,new Vector3(x,.67f,4.515f),new Vector3(.032f,1.34f,.032f),"Interior Family Painted Timber",false,.003f);
        MoveFurnitureAssembly(room,new[]{"Bedside Table","Desk Lamp"},new Vector3(-.80f,0,-1.0f));
        var study=G("Family Study Corner",room);
        B("Scarred Desk Top",study,new Vector3(-3.65f,.70f,6.25f),new Vector3(.89f,.040f,.42f),"Interior Family Painted Timber",false,.007f);
        foreach(float x in new[]{-4.04f,-3.26f})foreach(float z in new[]{6.09f,6.41f})B("Study Desk Leg",study,new Vector3(x,.34f,z),new Vector3(.034f,.68f,.034f),"Interior Family Painted Timber",false,.004f);
        InteriorProp("painted_wooden_chair_02",room,new Vector3(-3.65f,0,5.66f),180,.78f);HouseholdBooks(study,new Vector3(-3.80f,.723f,6.25f),4);
        B("Child Drawing Pad",study,new Vector3(-3.48f,.724f,6.24f),new Vector3(.24f,.008f,.19f),"Paper",false,.001f);
        foreach(float x in new[]{-3.54f,-3.47f,-3.40f})Rod("Worn Pencil",study,new Vector3(x,.732f,6.20f),new Vector3(x+.018f,.732f,6.30f),.002f,"Interior Notebook Cover");
        var toys=room.Find("Wooden Toy Train");toys.localPosition=new Vector3(-3.90f,.005f,5.15f);
        CommonDynamic(frame);CommonDynamic(study);
    }
    static void MountedFurnitureWear(Transform prop,string kind,Vector3 at,Vector2 size,Vector3 normal)
    {
        var temporary=prop.GetComponentsInChildren<MeshFilter>().Where(f=>f.gameObject.activeInHierarchy).Select(f=>{var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;return c;}).ToArray();
        var ray=new Ray(at+normal*.40f,-normal);float best=float.PositiveInfinity;RaycastHit selected=default;
        foreach(var c in temporary){RaycastHit hit;if(c.Raycast(ray,out hit,1f)&&hit.distance<best){best=hit.distance;selected=hit;}}
        foreach(var c in temporary)UnityEngine.Object.DestroyImmediate(c);
        if(float.IsInfinity(best))return;
        var quad=Q("Surface "+kind,prop,Vector3.zero,size,AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Weathering/"+kind+".mat"),Quaternion.identity);
        quad.SetPositionAndRotation(selected.point+normal*.0008f,Quaternion.LookRotation(-normal,Vector3.up));var scale=prop.lossyScale;quad.localScale=new Vector3(size.x/Mathf.Abs(scale.x),size.y/Mathf.Abs(scale.y),1);
    }
    static void RoomWearAndMood(Transform room,RoomSpace s)
    {
        if(room.Find("Applied Room Wear")!=null)return;G("Applied Room Wear",room);
        // Retain the scanned furniture maps; add material-specific wear on touched surfaces.
        var roots=room.Cast<Transform>().Where(t=>new[]{"old_bed_frame","wooden-bed","refrigerator","wardrobe","chinese_tea_table","Shelf_01","kitchen-base-unit-sink","washing-machine","painted_wooden_chair_02","Television Cabinet","Cooker Cabinet","Wall Cupboard"}.Contains(t.name)&&t.gameObject.activeSelf).ToArray();
        foreach(var prop in roots)
        {
            foreach(var renderer in prop.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(original=>
            {
                if(original==null||original.name.Contains("glass"))return original;
                string name="Room "+s.id+" "+original.name,path=Base+"Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(original);AssetDatabase.CreateAsset(mat,path);}
                if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",original.GetColor("_BaseColor")*new Color(.75f,.76f,.69f,1));
                if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",.14f);if(mat.HasProperty("_OcclusionStrength"))mat.SetFloat("_OcclusionStrength",1);EditorUtility.SetDirty(mat);return mat;
            }).ToArray();
        }
        foreach(var light in room.GetComponentsInChildren<Light>())
        {
            if(light.type==LightType.Spot){light.intensity=s.id=="306"?1.15f:s.id=="602"?.85f:s.id=="404"?.75f:s.id=="503"?2.0f:2.25f;light.range=s.id=="306"?3.25f:4;light.spotAngle=s.id=="306"?110:125;light.innerSpotAngle=65;light.colorTemperature=s.id=="306"?6300:s.id=="602"?6000:5000;light.color=s.id=="306"?new Color(.79f,.94f,1):Color.white;}
            else if(light.name=="Tube Bounce"){light.intensity=s.id=="306"?.045f:.065f;light.range=2.2f;}
            else if(light.transform.parent.name=="Corner Bulkhead"){light.intensity=s.id=="306"?.16f:.11f;light.colorTemperature=6000;}
        }
        foreach(var renderer in room.GetComponentsInChildren<Renderer>().Where(r=>r.sharedMaterials.Any(m=>m!=null&&m.HasProperty("_EmissionColor")&&m.GetColor("_EmissionColor").maxColorComponent>.01f)))
        {renderer.sharedMaterials=renderer.sharedMaterials.Select(original=>{if(original==null||!original.HasProperty("_EmissionColor")||original.GetColor("_EmissionColor").maxColorComponent<.01f)return original;string path=Base+"Materials/Room "+s.id+" Dim "+original.name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(original);AssetDatabase.CreateAsset(mat,path);}mat.SetColor("_EmissionColor",original.GetColor("_EmissionColor")*.35f);EditorUtility.SetDirty(mat);return mat;}).ToArray();}
        if(s.id=="306")
        {
            var lamps=room.Find("Room Fluorescent");lamps.localPosition=new Vector3(4.15f,2.68f,7.30f);
            foreach(var item in room.Cast<Transform>().Where(t=>new[]{"Drying Rail","Rail Hanger","Hanging Shirt","Clothes Hanger"}.Contains(t.name)))item.localPosition+=Vector3.right*.40f;
            var machines=room.Cast<Transform>().Where(t=>t.name=="washing-machine").OrderBy(t=>t.localPosition.z).ToArray();
            for(int i=0;i<machines.Length;i++)
            {
                var machine=machines[i];var bound=BoundsOf(machine);
                MountedFurnitureWear(machine,"Rust",new Vector3(bound.min.x,room.position.y+.12f,bound.center.z+.12f),new Vector2(.24f,.11f),Vector3.left);
                MountedFurnitureWear(machine,"Dirt",new Vector3(bound.min.x,room.position.y+.68f,bound.center.z-.13f),new Vector2(.15f,.13f),Vector3.left);
                foreach(var r in machine.GetComponentsInChildren<Renderer>().Where(r=>r.sharedMaterial.name.Contains("washing_machine")))
                {var m=new Material(r.sharedMaterial);m.name="Old Washer "+(i+1);m.SetColor("_BaseColor",i==0?new Color(.67f,.63f,.48f):new Color(.46f,.56f,.50f));string path=Base+"Materials/"+m.name+".mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing==null){AssetDatabase.CreateAsset(m,path);existing=m;}else UnityEngine.Object.DestroyImmediate(m);r.sharedMaterial=existing;}
            }
            SeatInterior(room.Find("Cleaning Bucket"),room,new Vector3(3.70f,0,6.18f),20);
            var floorBasket=room.Cast<Transform>().Where(t=>t.name=="laundry-basket").OrderBy(t=>t.position.y).First();SeatInterior(floorBasket,room,new Vector3(4.78f,0,6.47f),15);
            Weather(room,"Water Stain",new Vector3(5.109f,1.37f,8.48f),new Vector2(.39f,.98f),Quaternion.Euler(0,90,0));
        }
        if(s.id=="602")
        {
            var wardrobe=room.Find("wardrobe");if(wardrobe!=null)wardrobe.gameObject.SetActive(false);
            SeatInterior(room.Find("Desk Fan"),room,new Vector3(4.66f,.463f,2.76f),-25);
            SeatInterior(room.Find("painted_wooden_chair_02"),room,new Vector3(2.70f,0,3.05f),65);
            Weather(room,"Water Stain",new Vector3(2.166f,1.50f,2.95f),new Vector2(.50f,1.03f),Quaternion.Euler(0,-90,0));
        }
        if(s.id=="201")
        {
            foreach(var t in room.Cast<Transform>().Where(t=>new[]{"old_bed_frame","Cotton Mattress","Bed Linen","Old Pillow"}.Contains(t.name)))t.localPosition+=Vector3.left*.13f;
            var wardrobe=room.Find("wardrobe");MountedFurnitureWear(wardrobe,"Dirt",room.TransformPoint(new Vector3(-5.38f,.25f,2.79f)),new Vector2(.26f,.19f),Vector3.back);
        }
        CommonDynamic(room);
    }
    public static string ReviseRoomEntrancesAndMood()
    {
        EnsureApartment();CommonPbr("Interior Switch Bakelite","weathered_pvc_tank",new Color(.55f,.52f,.39f),0,.12f);CommonPbr("Interior Family Painted Timber","weathered_brown_planks",new Color(.27f,.34f,.30f),0,.10f);
        DifferentiateFamilyRoom(GameObject.Find("Room 503").transform);
        foreach(string id in AccessibleUnits){var s=RoomPlan(id);var room=GameObject.Find("Room "+id).transform;SeparateRoomEntrance(s);RoomWearAndMood(room,s);RoomWallSwitch(room,s);FinishInterior(room,s);}
        AssetDatabase.SaveAssets();SaveApartment();return "Five separate keyed outward gates and inward doors; five independent switches with floor-breaker supply; older finishes, darker laundry/storage, distinct elderly and child/family layouts, storage wardrobe removed.";
    }
    public static string FinishRoomAtmospherePass()
    {
        EnsureApartment();var elderly=GameObject.Find("Room 201").transform;
        foreach(var stool in elderly.Cast<Transform>().Where(t=>t.name=="Household Stool"&&t.localPosition.x> -3&&t.localPosition.z<1.10f))stool.localPosition+=Vector3.forward*.10f;
        SeatInterior(elderly.Find("Potted Succulent"),elderly,new Vector3(-5.45f,1.623f,3.06f),18);
        var slippers=elderly.Find("House Slippers");slippers.localPosition=new Vector3(-3.98f,0,.52f);
        var family=GameObject.Find("Room 503").transform;foreach(var light in family.Find("Desk Lamp").GetComponentsInChildren<Light>()){light.intensity=.72f;light.range=3.1f;}
        var sink=family.Find("kitchen-base-unit-sink");sink.localScale=new Vector3(sink.localScale.x*.76f/BoundsOf(sink).size.x,sink.localScale.y,sink.localScale.z);SeatInterior(sink,family,new Vector3(-3.20f,0,3.88f),0);
        foreach(var part in family.Cast<Transform>().Where(t=>t.name=="Wall Cupboard"||t.name=="Cupboard Pull")){if(part.localPosition.y<2.0f)part.localPosition+=Vector3.up*.26f;}
        if(family.Find("Old Bunk Reading Light")==null)
        {
            var fixture=G("Old Bunk Reading Light",family);fixture.localPosition=new Vector3(-5.45f,2.14f,3.582f);fixture.localRotation=Quaternion.Euler(0,180,0);
            Rod("Metal Bulkhead",fixture,new Vector3(0,0,.01f),new Vector3(0,0,-.022f),.081f,"Cabin Aged Steel");
            Rod("Dusty Glass Lens",fixture,new Vector3(0,0,-.023f),new Vector3(0,0,-.038f),.062f,"Backup Diffuser");
            foreach(float y in new[]{-.035f,.035f})Rod("Lens Guard",fixture,new Vector3(-.06f,y,-.044f),new Vector3(.06f,y,-.044f),.003f,"Cabin Aged Steel");
            var source=G("Reading Light",fixture);source.localPosition=new Vector3(0,-.01f,-.08f);source.rotation=Quaternion.LookRotation(new Vector3(0,-.32f,1));var light=source.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.intensity=1.1f;light.range=3.2f;light.spotAngle=95;light.innerSpotAngle=70;light.useColorTemperature=true;light.colorTemperature=4300;light.shadows=LightShadows.Soft;light.shadowBias=.008f;light.shadowNormalBias=.02f;light.lightmapBakeType=LightmapBakeType.Realtime;
            var data=source.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();var settings=new SerializedObject(data);settings.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;settings.ApplyModifiedPropertiesWithoutUndo();CommonDynamic(fixture);
        }
        foreach(string id in AccessibleUnits)FinishInterior(GameObject.Find("Room "+id).transform,RoomPlan(id));
        AssetDatabase.SaveAssets();SaveApartment();return "Room and entrance prefabs saved; elderly-room stool/fridge and plant/fridge overlaps cleared; family bedroom remains readable.";
    }
}
