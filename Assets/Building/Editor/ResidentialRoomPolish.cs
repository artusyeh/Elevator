using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string AgeInteriorFurniture()
    {
        EnsureApartment();var catalog=JsonUtility.FromJson<CommonCatalog>(System.IO.File.ReadAllText(InteriorModels+"Sources.json"));int changed=0;
        foreach(var record in catalog.models.Where(r=>System.IO.File.Exists(InteriorModels+r.asset+"/"+r.asset+".glb")))foreach(var source in record.materials.Where(m=>!string.IsNullOrEmpty(m.color)))
        {
            string folder=InteriorModels+record.asset+"/";var image=new Texture2D(2,2,TextureFormat.RGBA32,false);image.LoadImage(System.IO.File.ReadAllBytes(folder+source.color));var pixels=image.GetPixels32();int w=image.width,h=image.height;float seed=changed*9.71f;
            Color tint=record.asset=="wardrobe"?new Color(.74f,.70f,.57f):record.asset=="refrigerator"?new Color(.88f,.86f,.76f):new Color(.93f,.91f,.83f);
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                int index=y*w+x;Color c=pixels[index];float u=x/(float)w,v=y/(float)h;
                float patches=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.59f,.79f,Mathf.PerlinNoise(u*7.1f+seed,v*6.3f+3)));
                float streak=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.70f,.88f,Mathf.PerlinNoise(u*19.1f+seed,v*.8f+11)))*.11f;
                c.r*=tint.r*(1-patches*.35f-streak);c.g*=tint.g*(1-patches*.39f-streak);c.b*=tint.b*(1-patches*.44f-streak);pixels[index]=c;
            }
            var rng=new System.Random(418+changed);for(int k=0;k<52;k++)
            {
                int sx=rng.Next(w),sy=rng.Next(h),length=rng.Next(3,Mathf.Max(4,w/70));for(int q=0;q<length&&sx+q<w&&sy+q/8<h;q++){int index=(sy+q/8)*w+sx+q;Color c=pixels[index];if(c.maxColorComponent>.12f)pixels[index]=Color.Lerp(c,new Color(.40f,.34f,.25f,c.a),.24f);}
            }
            image.SetPixels32(pixels);image.Apply();string path=folder+"textures/Aged Finish.png";System.IO.File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
            var mat=M("Interior "+source.name);mat.SetTexture("_BaseMap",DetailMap(path));mat.SetFloat("_Smoothness",.40f);EditorUtility.SetDirty(mat);changed++;
        }
        AssetDatabase.SaveAssets();SaveApartment();return "Aged original furniture finishes: "+changed+" original texture sets retained, with patchy discoloration, fine scuffs and reduced sheen.";
    }
    public static string RestoreInteriorTextures()
    {
        EnsureApartment();AssetDatabase.Refresh();var catalog=JsonUtility.FromJson<CommonCatalog>(System.IO.File.ReadAllText(InteriorModels+"Sources.json"));
        foreach(var record in catalog.models)foreach(var source in record.materials)
        {
            var m=M("Interior "+source.name);var folder=InteriorModels+record.asset+"/";
            if(!string.IsNullOrEmpty(source.color)){m.SetTexture("_BaseMap",DetailMap(folder+source.color));m.SetColor("_BaseColor",Color.white);}
            if(!string.IsNullOrEmpty(source.normal)){m.SetTexture("_BumpMap",DetailMap(folder+source.normal,true));m.SetFloat("_BumpScale",.65f);m.EnableKeyword("_NORMALMAP");}
            if(!string.IsNullOrEmpty(source.arm)){var map=DetailMask(folder+source.arm,folder+"textures/Unity Mask.png");m.SetTexture("_MetallicGlossMap",map);m.SetTexture("_OcclusionMap",map);m.SetFloat("_Smoothness",.65f);m.SetFloat("_OcclusionStrength",.85f);m.EnableKeyword("_METALLICSPECGLOSSMAP");m.EnableKeyword("_OCCLUSIONMAP");}
            EditorUtility.SetDirty(m);
        }
        var laundry=GameObject.Find("Room 306").transform;var basket=laundry.Cast<Transform>().Where(t=>t.name=="laundry-basket").OrderByDescending(t=>t.position.y).First();SeatInterior(basket,laundry,RoomPlan("306").At(2.58f,.833f,.60f),-8);
        var bottle=laundry.Find("Cleaning Bottle");SeatInterior(bottle,laundry,new Vector3(4.53f,.615f,5.84f),0);
        var storage=GameObject.Find("Room 602").transform;var cover=storage.Find("Furniture Dust Sheet");cover.localPosition=new Vector3(3.88f,.356f,.66f);
        foreach(string id in AccessibleUnits)FinishInterior(GameObject.Find("Room "+id).transform,RoomPlan(id));AssetDatabase.SaveAssets();return "Original embedded PBR textures restored for eight downloaded models; basket and detergent seated on supporting surfaces.";
    }
    static void SeatInterior(Transform item,Transform room,Vector3 at,float yaw)
    {
        item.localRotation=Quaternion.Euler(0,yaw,0);var b=BoundsOf(item);item.position+=room.TransformPoint(at)-new Vector3(b.center.x,b.min.y,b.center.z);
    }
    static void ExtendUtilityCorner(Transform room,RoomSpace s,bool laundry)
    {
        if(room.Find("Utility Corner")!=null)return;
        float join=laundry?s.minZ:s.maxZ,far=laundry?5.45f:4.80f;
        float start=Mathf.Min(join,far),end=Mathf.Max(join,far),inner=3.36f,outer=5.18f;
        var corner=G("Utility Corner",room);
        var old=room.Cast<Transform>().First(t=>t.name=="Room Wall"&&Mathf.Abs(t.localPosition.z-join)<.01f);
        var cut=new Bounds(new Vector3((inner+outer)*.5f,1.08f,join),new Vector3(outer-inner,2.16f,.30f));
        StairWall(corner,"Corner Opening",LiftLocalBounds(old.GetComponent<Renderer>(),room),new[]{cut},"Interior Faded Wall");old.gameObject.SetActive(false);
        foreach(var skirt in room.Cast<Transform>().Where(t=>t.name=="Skirting"&&Mathf.Abs(t.localPosition.z-join)<.08f).ToArray())
        {StairWall(corner,"Skirting Return",LiftLocalBounds(skirt.GetComponent<Renderer>(),room),new[]{new Bounds(new Vector3((inner+outer)*.5f,.1f,join),new Vector3(outer-inner,.4f,.3f))},"Household Timber");skirt.gameObject.SetActive(false);}
        float floorStart=laundry?start:join+.05f,floorEnd=laundry?join-.05f:end;
        B("Corner Floor",corner,new Vector3((inner+outer)*.5f,-.07f,(floorStart+floorEnd)*.5f),new Vector3(outer-inner,.14f,floorEnd-floorStart),laundry?"Interior Damp Floor":"Interior Old Floor",true,.001f);
        B("Corner Ceiling",corner,new Vector3((inner+outer)*.5f,2.74f,(floorStart+floorEnd)*.5f),new Vector3(outer-inner,.12f,floorEnd-floorStart),"Concrete",true,.001f);
        foreach(float x in new[]{inner-.05f,outer+.05f})B("Corner Side Wall",corner,new Vector3(x,1.34f,(start+end)*.5f),new Vector3(.10f,2.68f,end-start),"Interior Faded Wall",true,.002f);
        B("Corner End Wall",corner,new Vector3((inner+outer)*.5f,1.34f,far),new Vector3(outer-inner+.10f,2.68f,.10f),"Interior Faded Wall",true,.002f);
        B("Corner Skirting",corner,new Vector3((inner+outer)*.5f,.055f,far+(laundry?.058f:-.058f)),new Vector3(outer-inner,.11f,.014f),"Household Timber",false,.001f);
        B("Opening Header",corner,new Vector3((inner+outer)*.5f,2.19f,join),new Vector3(outer-inner,.08f,.13f),"Household Timber",false,.003f);
        if(laundry)
        {
            var sink=room.Cast<Transform>().First(t=>t.name=="kitchen-base-unit-sink");SeatInterior(sink,room,new Vector3(4.25f,0,far+.39f),0);
            var bottle=room.Cast<Transform>().First(t=>t.name=="Cleaning Bottle");SeatInterior(bottle,room,new Vector3(4.53f,.81f,far+.39f),0);
            B("Wash Alcove Splashback",corner,new Vector3(4.25f,1.22f,far+.057f),new Vector3(1.26f,.78f,.012f),"Green Tile",false,.001f);
            CommonRoute("Sink Feed",corner,.014f,"Service Water Steel",new Vector3(4.90f,.55f,far+.18f),new Vector3(4.90f,2.38f,far+.18f),new Vector3(4.90f,2.38f,7.0f),new Vector3(4.95f,2.40f,7.0f));
            B("Soap Shelf",corner,new Vector3(4.25f,1.70f,far+.19f),new Vector3(.77f,.035f,.24f),"Household Timber",false,.003f);
            foreach(float x in new[]{3.98f,4.52f})B("Shelf Wall Bracket",corner,new Vector3(x,1.62f,far+.12f),new Vector3(.025f,.16f,.11f),"Cabin Aged Steel",false,.002f);
            CommonVent(corner,new Vector3(4.25f,2.30f,far+.058f),Quaternion.Euler(0,180,0));
            Weather(corner,"Water Stain",new Vector3(4.82f,1.43f,far+.059f),new Vector2(.30f,.76f),Quaternion.Euler(0,180,0));
        }
        else
        {
            var travel=room.Find("Old Travel Case");if(travel!=null)travel.localPosition=new Vector3(4.66f,.23f,2.76f);
            var shelf=room.Cast<Transform>().Where(t=>t.name=="Shelf_01").OrderByDescending(t=>t.localPosition.z).First();SeatInterior(shelf,room,new Vector3(4.30f,0,far-.29f),180);
            var cartons=room.Cast<Transform>().Where(t=>t.name=="Delivery Carton").OrderByDescending(t=>t.localPosition.z).Take(3).ToArray();
            for(int i=0;i<cartons.Length;i++)SeatInterior(cartons[i],room,new Vector3(4.25f,.32f+i*.45f,far-.30f),0);
            CommonVent(corner,new Vector3(4.25f,2.29f,far-.058f),Quaternion.identity);
            B("Maintenance Board",corner,new Vector3(inner+.012f,1.57f,(join+far)*.5f),new Vector3(.018f,.46f,.64f),"Household Timber",false,.004f);
            foreach(float y in new[]{1.42f,1.66f})Rod("Spare Cable Loop",corner,new Vector3(inner+.045f,y,(join+far)*.5f-.19f),new Vector3(inner+.045f,y,(join+far)*.5f+.19f),.013f,"Black Rubber");
            var broom=room.Cast<Transform>().First(t=>t.name=="Broom");SeatInterior(broom,room,new Vector3(3.50f,0,far-.20f),12);
        }
        // A small practical bulkhead lights the turn, rather than a broad ambient fill.
        var fixture=G("Corner Bulkhead",corner);fixture.localPosition=new Vector3(4.24f,2.52f,far+(laundry?.09f:-.09f));fixture.localRotation=Quaternion.Euler(0,laundry?180:0,0);
        B("Housing",fixture,Vector3.zero,new Vector3(.24f,.10f,.065f),"Cabin Aged Steel",false,.012f);
        B("Frosted Cover",fixture,new Vector3(0,0,-.039f),new Vector3(.20f,.068f,.020f),"Backup Diffuser",false,.009f);
        var source=G("Light",fixture);source.localPosition=new Vector3(0,-.07f,-.10f);var light=source.gameObject.AddComponent<Light>();light.type=LightType.Point;light.intensity=.30f;light.range=2.0f;light.useColorTemperature=true;light.colorTemperature=5300;light.shadows=LightShadows.None;
    }
    static void SeatStorageCartons(Transform room)
    {
        Physics.SyncTransforms();var shelves=room.Cast<Transform>().Where(t=>t.name=="Shelf_01").ToArray();
        foreach(var shelf in shelves)
        {
            var meshes=shelf.GetComponentsInChildren<MeshFilter>().Select(f=>{var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;return c;}).ToArray();
            var cartons=room.Cast<Transform>().Where(t=>t.name=="Delivery Carton").OrderBy(t=>Vector2.Distance(new Vector2(t.position.x,t.position.z),new Vector2(shelf.position.x,shelf.position.z))).Take(3).OrderBy(t=>t.position.y).ToArray();
            for(int i=0;i<cartons.Length;i++)
            {
                var b=BoundsOf(cartons[i]);var ray=new Ray(new Vector3(b.center.x,room.position.y+.82f+i*.50f,b.center.z),Vector3.down);float best=float.NegativeInfinity;
                foreach(var mesh in meshes){RaycastHit hit;if(mesh.Raycast(ray,out hit,.68f)&&hit.normal.y>.65f)best=Mathf.Max(best,hit.point.y);}
                if(best>float.NegativeInfinity)cartons[i].position+=Vector3.up*(best+.002f-b.min.y);
            }
            foreach(var mesh in meshes)UnityEngine.Object.DestroyImmediate(mesh);
        }
    }
    public static string PolishAndExpandResidentialRooms()
    {
        EnsureApartment();var elderly=GameObject.Find("Room 201").transform;var es=RoomPlan("201");
        SeatInterior(elderly.Find("refrigerator"),elderly,es.At(.43f,0,.40f),0);
        var curtain=elderly.Find("Curtain");curtain.localRotation=Quaternion.Euler(0,-90,0);curtain.localScale=new Vector3(.30f,1,1.2f);
        foreach(var part in elderly.Cast<Transform>().Where(t=>new[]{"Cooker Cabinet","Gas Hob","Burner","cream-dome-kettle"}.Contains(t.name)))part.localPosition+=Vector3.right*.15f;
        var wardrobe=elderly.Find("wardrobe");wardrobe.localScale=new Vector3(wardrobe.localScale.x*.75f/BoundsOf(wardrobe).size.x,wardrobe.localScale.y,wardrobe.localScale.z);SeatInterior(wardrobe,elderly,es.At(3.35f,0,2.8f),180);
        foreach(var carton in elderly.Cast<Transform>().Where(t=>t.name=="Medicine Box"))carton.localPosition-=Vector3.up*.007f;
        foreach(var tea in elderly.Cast<Transform>().Where(t=>new[]{"Tea Cup","Tea Surface","Tea Tray"}.Contains(t.name)))tea.localPosition+=Vector3.up*.0055f;
        if(elderly.Find("Bedside Table")==null)BedroomLamp(elderly,es,2.55f,.45f);
        var bed=elderly.Find("old_bed_frame");var b=BoundsOf(bed);if(b.size.x>b.size.z)bed.localRotation=Quaternion.Euler(0,90,0);b=BoundsOf(bed);bed.localScale=new Vector3(bed.localScale.x/b.size.x,bed.localScale.y,bed.localScale.z*1.93f/b.size.z);SeatInterior(bed,elderly,es.At(3.2f,0,1.1f),bed.localEulerAngles.y);
        var laundry=GameObject.Find("Room 306").transform;
        foreach(var item in laundry.Cast<Transform>().Where(t=>new[]{"Drying Rail","Hanging Shirt","Clothes Hanger"}.Contains(t.name)))item.localPosition+=Vector3.up*.24f;
        foreach(var hanger in laundry.Cast<Transform>().Where(t=>t.name=="Rail Hanger")){hanger.localPosition+=Vector3.up*.12f;hanger.localScale=new Vector3(hanger.localScale.x,hanger.localScale.y*.195f/.435f,hanger.localScale.z);}
        SeatInterior(laundry.Find("Cleaning Bucket"),laundry,RoomPlan("306").At(.30f,0,RoomPlan("306").length-.36f),20);
        foreach(string name in new[]{"Interior door_glass_0","Interior shelf_glass_0"}){var m=M(name);if(m==null)continue;m.SetFloat("_Surface",1);m.SetColor("_BaseColor",new Color(.23f,.28f,.27f,.12f));m.SetFloat("_Metallic",0);m.SetFloat("_Smoothness",.30f);UnityEditor.BaseShaderGUI.SetupMaterialBlendMode(m);m.SetShaderPassEnabled("ShadowCaster",false);EditorUtility.SetDirty(m);}
        ExtendUtilityCorner(laundry,RoomPlan("306"),true);var storage=GameObject.Find("Room 602").transform;ExtendUtilityCorner(storage,RoomPlan("602"),false);SeatStorageCartons(storage);
        foreach(string id in AccessibleUnits){var room=GameObject.Find("Room "+id).transform;foreach(var prop in room.Cast<Transform>().Where(t=>new[]{"old_bed_frame","wooden-bed","refrigerator","wardrobe","chinese_tea_table","Shelf_01"}.Contains(t.name)))if(prop.GetComponent<BoxCollider>()==null){var rs=prop.GetComponentsInChildren<Renderer>();var bounds=LiftLocalBounds(rs[0],prop);foreach(var r in rs.Skip(1))bounds.Encapsulate(LiftLocalBounds(r,prop));var box=prop.gameObject.AddComponent<BoxCollider>();box.center=bounds.center;box.size=bounds.size;}FinishInterior(room,RoomPlan(id));}
        AssetDatabase.SaveAssets();return "All five interiors saved. Laundry and storage expanded into L-shaped utility corners; sink plumbing, shelving and corner lighting fitted; furniture, cloth and appliance glass polished.";
    }
}
