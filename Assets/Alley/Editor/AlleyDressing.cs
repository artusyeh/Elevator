using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class AlleyDressing
{
    [Serializable] public class Sources { public Model[] models; }
    [Serializable] public class Model { public string asset; public ModelMaterial[] materials; }
    [Serializable] public class ModelMaterial { public string name,color,normal,arm,unityMask,alpha; public bool doubleSided; public float[] factor; public float metallic,roughness; }
    const string Folder="Assets/Alley/";
    static readonly Dictionary<string,string> Names=new Dictionary<string,string>{
        {"blossom-wave-manhole-cover","Manhole Cover"},{"street-gully-grate","Gully Grate"},{"potted_plant_01","Potted Fern"},{"potted_plant_02","Potted Foliage"},{"potted_plant_04","Potted Succulent"},{"trashbag","Trash Bag"},{"plastic_crate_01","Red Crate"},{"plastic_crate_02","Storage Crate"},{"cardboard_box_01","Delivery Carton"},{"utility_box_01","Utility Cabinet"},{"city-bicycle","Basket Bicycle"},{"cargo-bicycle","Delivery Bicycle"},{"commuter-bicycle","Commuter Bicycle"},{"waste-bin","Waste Bin"},{"metal-bucket","Cleaning Bucket"},{"traffic-cone","Traffic Cone"},{"green-plastic-watering-can","Watering Can"},{"drink-crate-stack","Drink Crates"},{"a-frame-chalkboard-sign","Menu Stand"},{"wash-bottle","Cleaning Bottle"},{"foam-box-herb-planter","Herb Planter"},{"wall-mailbox","Wall Mailbox"},{"entrance-intercom","Entrance Intercom"},{"security-camera","Security Camera"},{"street-broom","Broom"},{"folded-umbrella","Folded Umbrella"},{"shop-noren","Noren"}};
    static Dictionary<string,GameObject> prefabs=new Dictionary<string,GameObject>();
    static Transform Group(string name,Transform p){var g=new GameObject(name);g.transform.SetParent(p,false);return g.transform;}
    static Bounds BoundsOf(GameObject g){var rs=g.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
    static string Safe(string s){return Regex.Replace(s,@"[^\w\- ]","_");}
    static Material ModelMat(Model model,ModelMaterial src)
    {
        string path=Folder+"Materials/Street/"+model.asset+" "+Safe(src.name)+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        string f=Folder+"Models/Street/"+model.asset+"/";m.name=src.name;
        m.SetColor("_BaseColor",src.factor!=null&&src.factor.Length==4?new Color(src.factor[0],src.factor[1],src.factor[2],src.factor[3]):Color.white);
        if(!string.IsNullOrEmpty(src.color))m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(f+src.color));
        if(!string.IsNullOrEmpty(src.normal)){m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(f+src.normal));m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.7f);}
        if(!string.IsNullOrEmpty(src.unityMask)){var mask=AssetDatabase.LoadAssetAtPath<Texture2D>(f+src.unityMask);m.SetTexture("_MetallicGlossMap",mask);m.SetTexture("_OcclusionMap",mask);m.SetFloat("_Metallic",1);m.SetFloat("_Smoothness",.85f);m.EnableKeyword("_METALLICSPECGLOSSMAP");m.EnableKeyword("_OCCLUSIONMAP");}
        else {m.SetFloat("_Metallic",src.metallic);m.SetFloat("_Smoothness",Mathf.Clamp(1-src.roughness,.1f,.9f));}
        m.SetFloat("_Cull",src.doubleSided?0:2);
        if(src.alpha=="MASK"){m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.4f);m.EnableKeyword("_ALPHATEST_ON");m.renderQueue=2450;}
        EditorUtility.SetDirty(m);return m;
    }
    [MenuItem("Alley/Import Street Props")]
    public static string Library()
    {
        Directory.CreateDirectory(Folder+"Materials/Street");Directory.CreateDirectory(Folder+"Prefabs/Street");AssetDatabase.Refresh();
        var records=JsonUtility.FromJson<Sources>(File.ReadAllText(Folder+"Models/Street/ModelSources.json"));
        foreach(var model in records.models)
        {
            if(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"Prefabs/Street/"+Names[model.asset]+".prefab")!=null)continue; string folder=Folder+"Models/Street/"+model.asset+"/";
            var normals=new HashSet<string>(model.materials.Where(m=>!string.IsNullOrEmpty(m.normal)).Select(m=>Path.GetFileName(m.normal)));
            var colors=new HashSet<string>(model.materials.Where(m=>!string.IsNullOrEmpty(m.color)).Select(m=>Path.GetFileName(m.color)));
            foreach(var path in Directory.GetFiles(folder,"*",SearchOption.AllDirectories))
            {
                if(!path.EndsWith(".png")&&!path.EndsWith(".jpg"))continue;var imp=AssetImporter.GetAtPath(path.Replace('\\','/')) as TextureImporter;if(imp==null)continue;
                string file=Path.GetFileName(path);imp.maxTextureSize=2048;imp.anisoLevel=4;if(normals.Contains(file))imp.textureType=TextureImporterType.NormalMap;else imp.sRGBTexture=colors.Contains(file);imp.SaveAndReimport();
            }
            string fbx=folder+model.asset+".fbx";var importer=AssetImporter.GetAtPath(fbx) as ModelImporter;if(importer==null)throw new Exception("Missing model: "+fbx);
            importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.SaveAndReimport();
            var map=new Dictionary<string,Material>();foreach(var src in model.materials)map[src.name]=ModelMat(model,src);
            var root=Group(Names[model.asset],null);var child=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(fbx));child.transform.SetParent(root,false);
            foreach(var r in child.GetComponentsInChildren<Renderer>())
            {
                var slots=r.sharedMaterials;for(int i=0;i<slots.Length;i++){Material found;if(slots[i]!=null&&map.TryGetValue(slots[i].name,out found))slots[i]=found;else slots[i]=map.Values.First();}r.sharedMaterials=slots;
                string old=r.name.ToLowerInvariant();r.name=old.Contains("wheel")?"Wheel":old.Contains("handle")?"Handlebar":model.asset.Contains("plant")?"Foliage and Pot":old.Contains("pedal")?"Pedal":old.Contains("frame")||old=="body"?"Frame":"Details";
            }
            var bounds=BoundsOf(root.gameObject);child.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);root.gameObject.isStatic=true;
            string prefabPath=Folder+"Prefabs/Street/"+Names[model.asset]+".prefab";prefabs[model.asset]=PrefabUtility.SaveAsPrefabAsset(root.gameObject,prefabPath);UnityEngine.Object.DestroyImmediate(root.gameObject);
        }
        AssetDatabase.SaveAssets();return "Imported "+records.models.Length+" street-prop prefabs.";
    }
    static GameObject Prefab(string key){GameObject p;if(!prefabs.TryGetValue(key,out p)){p=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"Prefabs/Street/"+Names[key]+".prefab");prefabs[key]=p;}return p;}
    static Transform Place(string key,Transform parent,Vector3 position,float yaw,float height)
    {
        var g=(GameObject)PrefabUtility.InstantiatePrefab(Prefab(key));g.transform.SetParent(parent,false);g.transform.position=Vector3.zero;float h=BoundsOf(g).size.y;g.transform.localScale*=height/h;g.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));return g.transform;
    }
    static Material SignMat(string name)
    {
        string path=Folder+"Materials/Street/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m!=null)return m;
        m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Textures/Notices/"+name+".png"));m.SetFloat("_Smoothness",.1f);AssetDatabase.CreateAsset(m,path);return m;
    }
    static Transform Notice(string name,Transform parent,Vector3 local,Vector2 size)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=local;g.transform.localScale=new Vector3(size.x,size.y,1);g.GetComponent<Renderer>().sharedMaterial=SignMat(name);UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;
    }
    static void Wire(Transform p,Vector3 a,Vector3 b,float sag,Material mat,float width=.012f)
    {
        var g=Group("Service Cable",p);var l=g.gameObject.AddComponent<LineRenderer>();l.useWorldSpace=true;l.positionCount=25;l.startWidth=width;l.endWidth=width;l.numCornerVertices=3;l.sharedMaterial=mat;
        for(int i=0;i<25;i++){float t=i/24f;l.SetPosition(i,Vector3.Lerp(a,b,t)-Vector3.up*(4*sag*t*(1-t)));}
    }
    static void Net(Transform parent,Vector3 at)
    {
        var texture=new Texture2D(32,32,TextureFormat.RGBA32,true);for(int y=0;y<32;y++)for(int x=0;x<32;x++)texture.SetPixel(x,y,new Color(.11f,.25f,.2f,(x%16<2||y%16<2)?1:0));texture.Apply();string tpath=Folder+"Textures/Notices/Collection Net.png";File.WriteAllBytes(tpath,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(tpath);
        var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.name="Collection Net";mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tpath));mat.mainTextureScale=new Vector2(90,150);mat.SetFloat("_AlphaClip",1);mat.SetFloat("_Cutoff",.5f);mat.EnableKeyword("_ALPHATEST_ON");mat.SetFloat("_Cull",0);mat.renderQueue=2450;AssetDatabase.CreateAsset(mat,Folder+"Materials/Street/Collection Net.mat");
        var verts=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
        for(int z=0;z<=24;z++)for(int x=0;x<=16;x++){float u=x/16f,v=z/24f;float y=.08f+.53f*Mathf.Exp(-Mathf.Pow((u-.5f)*2,2)*1.1f-Mathf.Pow((v-.5f)*2,2)*.65f);verts.Add(new Vector3((u-.5f)*.85f,y,(v-.5f)*1.7f));uv.Add(new Vector2(u,v));if(z<24&&x<16){int i=z*17+x;tris.AddRange(new[]{i,i+17,i+1,i+1,i+17,i+18});}}
        var mesh=new Mesh();mesh.name="Draped Net";mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"Models/Street/Draped Net.asset");var g=Group("Collection Net",parent);g.position=at;g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;
    }
    public static void Finish()
    {
        var street=GameObject.Find("Street").transform;
        var covers=street.Cast<Transform>().Where(t=>t.name=="Manhole Cover").Select(t=>t.position).ToArray();
        foreach(var t in street.Cast<Transform>().ToArray())if(t.name=="Manhole Cover"||t.name=="Manhole Grooves")UnityEngine.Object.DestroyImmediate(t.gameObject);
        foreach(var at in covers)Place("blossom-wave-manhole-cover",street,new Vector3(at.x,-.006f,at.z),at.z*13,.009f);
        foreach(float z in new[]{4.4f,12.2f,24.8f,37.3f})Place("street-gully-grate",street,new Vector3(z<20?-2.82f:2.82f,-.351f,z),90,.355f);
        var menu=GameObject.Find("Menu Stand").transform;
        if(menu.Find("Menu")==null){var card=Notice("Menu",menu,new Vector3(0,.47f,.137f),new Vector2(.365f,.51f));card.localRotation=Quaternion.Euler(-10,180,0);}
        var material=SignMat("Noren");material.mainTextureScale=new Vector2(-1,1);material.mainTextureOffset=new Vector2(1,0);EditorUtility.SetDirty(material);
        var sky=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Materials/Tokyo Night.mat");
        sky.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Textures/Tokyo Tower Immanuelle.jpg"));sky.SetVector("_Crop",new Vector4(.36f,.12f,.38f,.8f));sky.SetFloat("_Bearing",-3);sky.SetFloat("_Elevation",19);sky.SetVector("_AngularSize",new Vector4(16,43,0,0));sky.SetFloat("_PhotoExposure",.58f);RenderSettings.skybox=sky;EditorUtility.SetDirty(sky);
        var lightParent=GameObject.Find("Lighting").transform;
        var fill=lightParent.Find("Sky Bounce");if(fill==null)fill=Group("Sky Bounce",lightParent);
        if(fill.childCount==0)foreach(float z in new[]{7f,19f,34f,46f}){var g=Group("Indirect Fill",fill);g.position=new Vector3(0,3.8f,z);var l=g.gameObject.AddComponent<Light>();l.type=LightType.Point;l.intensity=2.2f;l.range=12;l.color=new Color(.62f,.73f,1);l.shadows=LightShadows.None;l.lightmapBakeType=LightmapBakeType.Mixed;}
        var views=GameObject.Find("Viewpoints").transform;
        if(views.Find("Waste Collection")==null){var v=Group("Waste Collection",views);v.position=new Vector3(.3f,1.65f,23.8f);v.rotation=Quaternion.Euler(6,-78,0);}
        if(views.Find("Residential Details")==null){var v=Group("Residential Details",views);v.position=new Vector3(-.45f,1.65f,7.4f);v.rotation=Quaternion.Euler(4,93,0);}
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
    public static void Polish()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");
        var sky=RenderSettings.skybox;sky.SetFloat("_Elevation",15);sky.SetVector("_AngularSize",new Vector4(15,34,0,0));sky.SetFloat("_PhotoExposure",.36f);EditorUtility.SetDirty(sky);
        var n=GameObject.Find("Collection Net");n.GetComponent<Renderer>().enabled=false;
        var mat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Materials/Street/Woven Green Cord.mat");
        if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.name="Woven Green Cord";mat.SetColor("_BaseColor",new Color(.24f,.35f,.17f));mat.SetFloat("_Smoothness",.2f);AssetDatabase.CreateAsset(mat,Folder+"Materials/Street/Woven Green Cord.mat");}
        if(n.transform.childCount==0)for(int axis=0;axis<2;axis++)for(int k=0;k<=(axis==0?16:28);k++)
        {
            var g=Group("Net Cord",n.transform);var line=g.gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=25;line.startWidth=.004f;line.endWidth=.004f;line.sharedMaterial=mat;line.shadowCastingMode=ShadowCastingMode.Off;
            for(int j=0;j<25;j++){float u=axis==0?k/16f:j/24f,v=axis==0?j/24f:k/28f;float y=.08f+.53f*Mathf.Exp(-Mathf.Pow((u-.5f)*2,2)*1.1f-Mathf.Pow((v-.5f)*2,2)*.65f);line.SetPosition(j,new Vector3((u-.5f)*.85f,y,(v-.5f)*1.7f));}
        }
        GameObject.Find("Collection Schedule").transform.position=new Vector3(-3.03f,1.55f,24.9f);
        foreach(Transform t in GameObject.Find("Residential Entrances").transform)if(t.name=="Potted Fern"&&Mathf.Abs(t.position.z-7.2f)<.2f)t.position+=Vector3.back*1.05f;
        var dressing=GameObject.Find("Set Dressing");if(dressing!=null)dressing.name="Neighborhood Details";
        foreach(var file in Directory.GetFiles(Folder+"Prefabs/Street","*.prefab"))
        {
            var contents=PrefabUtility.LoadPrefabContents(file);
            try {foreach(var t in contents.GetComponentsInChildren<Transform>())if(t!=contents.transform&&(t.name.Contains("_")||Regex.IsMatch(t.name,@"\.\d+$")))t.name=t.GetComponent<Renderer>()!=null?"Details":"Assembly";PrefabUtility.SaveAsPrefabAsset(contents,file);}
            finally {PrefabUtility.UnloadPrefabContents(contents);}
        }
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
    [MenuItem("Alley/Dress the Street")]
    public static void Dress()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");if(GameObject.Find("Set Dressing")!=null)throw new Exception("Dressing pass already applied.");
        var root=Group("Set Dressing",GameObject.Find("Alley").transform);var homes=Group("Residential Entrances",root);var shopProps=Group("Shop Supplies",root);var garbage=Group("Waste Collection",root);var utilities=Group("Street Fixtures",root);
        var props=GameObject.Find("Props").transform;
        var oldBikes=props.Cast<Transform>().Where(t=>t.name=="City Bicycle").ToArray();int count=0;
        foreach(var bike in oldBikes){Vector3 at=bike.position;float yaw=bike.eulerAngles.y-90;Place(count++%2==0?"city-bicycle":"commuter-bicycle",homes,at+new Vector3(0,.04f,.13f),yaw,1.12f);UnityEngine.Object.DestroyImmediate(bike.gameObject);}
        foreach(var t in props.Cast<Transform>().ToArray())if(new[]{"Terracotta Pot","Plant Stem","Leaf","Garbage Bag","Garbage Bin","Cardboard Delivery Carton","Packing Tape"}.Contains(t.name))UnityEngine.Object.DestroyImmediate(t.gameObject);
        Place("cargo-bicycle",shopProps,new Vector3(-2.94f,.04f,13.9f),175,1.13f);
        var plants=new[]{new Vector3(-2.94f,.1f,3.1f),new Vector3(-2.94f,.08f,3.85f),new Vector3(3.02f,.1f,7.2f),new Vector3(3.05f,.1f,7.85f),new Vector3(-3.08f,.1f,10.9f),new Vector3(3.05f,.08f,17.6f),new Vector3(3.08f,.08f,18.3f),new Vector3(-2.9f,.08f,34.1f),new Vector3(-2.85f,.08f,35.1f)};
        for(int i=0;i<plants.Length;i++)Place(i%3==0?"potted_plant_01":i%3==1?"potted_plant_04":"potted_plant_02",homes,plants[i],i*73,.38f+(i%3)*.22f);
        Place("foam-box-herb-planter",homes,new Vector3(2.98f,.12f,10),180,.38f);Place("green-plastic-watering-can",homes,new Vector3(3,.08f,8.55f),-30,.3f);
        Place("plastic_crate_01",shopProps,new Vector3(-2.96f,.09f,8.9f),14,.31f);Place("plastic_crate_02",shopProps,new Vector3(-2.96f,.42f,8.9f),-3,.25f);
        Place("drink-crate-stack",shopProps,new Vector3(-3.02f,.06f,5.9f),90,.68f);
        Place("cardboard_box_01",shopProps,new Vector3(-2.96f,.1f,9.8f),-5,.38f);Place("cardboard_box_01",shopProps,new Vector3(-3.08f,.45f,9.85f),8,.23f);
        Place("metal-bucket",shopProps,new Vector3(-2.9f,.05f,4.2f),0,.29f);Place("wash-bottle",shopProps,new Vector3(-3.05f,.06f,4.65f),-25,.22f);
        var broom=Place("street-broom",shopProps,new Vector3(-3.02f,.05f,4.85f),-90,1.17f);broom.localRotation*=Quaternion.Euler(0,0,-8);
        var umbrella=Place("folded-umbrella",homes,new Vector3(3.02f,.12f,5.6f),90,.93f);umbrella.localRotation*=Quaternion.Euler(0,0,9);
        Place("waste-bin",garbage,new Vector3(-2.95f,.05f,22.15f),95,.85f);Place("plastic_crate_02",garbage,new Vector3(-2.98f,.05f,23.05f),15,.34f);
        for(int i=0;i<7;i++)Place("trashbag",garbage,new Vector3(-2.95f+(i%2)*.34f,.045f,24+(i/2)*.39f),i*79,.34f+(i%3)*.045f);
        Net(garbage,new Vector3(-2.8f,0,24.6f));
        Place("traffic-cone",garbage,new Vector3(-2.65f,.03f,26.25f),-10,.61f);
        Place("traffic-cone",utilities,new Vector3(2.93f,.04f,15),22,.55f);
        Place("a-frame-chalkboard-sign",shopProps,new Vector3(-2.83f,.07f,7.4f),76,.84f);
        // Per-entrance hardware is placed in the existing facade coordinate system.
        var buildings=GameObject.Find("Buildings").transform;int house=0;
        foreach(Transform b in buildings)
        {
            if(!(b.name=="Townhouse"||b.name=="Apartment Building"))continue;
            var facade=b.Find("Facade Volume");float w=facade.localScale.x;float h=facade.localScale.y;float dx=-w*.34f;
            var fitting=Group("Entrance Details",b);
            var mail=Place("wall-mailbox",fitting,Vector3.zero,0,.38f);mail.localPosition=new Vector3(dx+.83f,1,-3.25f);mail.localRotation=Quaternion.identity;
            var intercom=Place("entrance-intercom",fitting,Vector3.zero,0,.21f);intercom.localPosition=new Vector3(dx+.6f,1.48f,-3.27f);intercom.localRotation=Quaternion.identity;
            Notice("Street Address",fitting,new Vector3(-w*.43f,2,-3.2f),new Vector2(.22f,.14f));
            if(house%3==0)Notice("No Parking",fitting,new Vector3(w*.2f,1.12f,-3.19f),new Vector2(.21f,.28f));
            if(house%4==1)Notice("Missing Cat",fitting,new Vector3(-w*.43f,1.35f,-3.21f),new Vector2(.25f,.34f));
            if(house%4==2)Notice("Neighborhood Notice",fitting,new Vector3(w*.35f,1.45f,-3.2f),new Vector2(.3f,.4f));
            if(house%5==0)Notice("Shoe Repair",fitting,new Vector3(w*.36f,2.12f,-3.19f),new Vector2(.2f,.28f));
            if(house%3==1){var c=Place("security-camera",fitting,Vector3.zero,0,.21f);c.localPosition=new Vector3(w*.4f,2.65f,-3.45f);c.localRotation=Quaternion.Euler(20,0,0);Notice("Security Notice",fitting,new Vector3(w*.3f,1.8f,-3.19f),new Vector2(.2f,.27f));}
            if(house%3==2){var u=Place("utility_box_01",fitting,Vector3.zero,0,.58f);u.localPosition=new Vector3(w*.4f,1,-3.28f);u.localRotation=Quaternion.identity;}
            house++;
        }
        var shop=GameObject.Find("Minato Sushi").transform;
        var noren=Place("shop-noren",shop,Vector3.zero,0,.66f);noren.localPosition=new Vector3(-.65f,1.95f,-.48f);noren.localRotation=Quaternion.identity;foreach(var r in noren.GetComponentsInChildren<Renderer>())r.sharedMaterial=SignMat("Noren");
        Notice("Business Hours",shop,new Vector3(2.42f,1.48f,-.34f),new Vector2(.24f,.33f));Notice("Menu",shop,new Vector3(-2.5f,1.5f,-.14f),new Vector2(.27f,.38f));
        var schedule=Notice("Collection Schedule",garbage,Vector3.zero,new Vector2(.37f,.5f));schedule.position=new Vector3(-3.2f,1.45f,24.65f);schedule.rotation=Quaternion.Euler(0,-90,0);
        var apartment=GameObject.Find("Aoba Apartments").transform;Notice("Apartment Nameplate",apartment,new Vector3(-1.6f,1.65f,-.65f),new Vector2(.3f,.4f));Notice("Festival",apartment,new Vector3(1.6f,1.45f,-.65f),new Vector2(.36f,.48f));
        var wireMat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Materials/Charcoal rubber.mat");
        for(int side=-1;side<=1;side+=2)for(int z=5;z<49;z+=9){Wire(utilities,new Vector3(side*3.16f,3.2f,z),new Vector3(side*3.16f,3.5f,z+7),.18f,wireMat,.01f);}
        Wire(utilities,new Vector3(-3,8.3f,8),new Vector3(3,8.1f,15),.75f,wireMat);Wire(utilities,new Vector3(-3,7.6f,33),new Vector3(3,8.7f,41),.65f,wireMat);
        // A few leaves and paper scraps gather at gutters, away from the walking corridor.
        var leafMat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Materials/Weathered Timber.mat");
        var litterMesh=new Mesh();litterMesh.name="Fallen Leaf";litterMesh.vertices=new[]{new Vector3(-.03f,0,0),new Vector3(0,.007f,.055f),new Vector3(.028f,0,0),new Vector3(0,-.003f,-.05f)};litterMesh.triangles=new[]{0,1,2,0,2,3};litterMesh.uv=new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right};litterMesh.RecalculateNormals();AssetDatabase.CreateAsset(litterMesh,Folder+"Models/Street/Fallen Leaf.asset");
        for(int i=0;i<28;i++){var g=Group("Fallen Leaf",utilities);g.position=new Vector3((i%2==0?-1:1)*(2.82f+(i%3)*.045f),.04f,3.7f+(i%7)*.13f+(i/7)*7.1f);g.rotation=Quaternion.Euler(0,i*59,0);g.gameObject.AddComponent<MeshFilter>().sharedMesh=litterMesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=leafMat;}
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}


