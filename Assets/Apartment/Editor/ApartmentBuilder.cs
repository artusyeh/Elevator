using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    const string Base="Assets/Apartment/";
    static Dictionary<string,Material> mats=new Dictionary<string,Material>();
    static Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
    static System.Random random=new System.Random(1978);
    [Serializable]public class Sources{public Model[] models;}
    [Serializable]public class Model{public string asset;public ModelMaterial[] materials;}
    [Serializable]public class ModelMaterial{public string name,color,normal,arm,unityMask,emission,alpha;public float[] factor;public float metallic,roughness;public bool doubleSided;}
    static readonly Dictionary<string,string> Names=new Dictionary<string,string>{{"mounted_fluorescent_lights","Fluorescent Light"},{"Television_01","CRT Monitor"},{"metal_office_desk","Management Desk"},{"desk_lamp_arm_01","Desk Lamp"},{"wall_clock","Wall Clock"},{"binder_notebook","Notebook"},{"book_encyclopedia_set_01","Books"},{"office-chair","Office Chair"},{"fire-extinguisher","Fire Extinguisher"}};
    static Transform G(string name,Transform p){var g=new GameObject(name);g.transform.SetParent(p,false);return g.transform;}
    static Material M(string name){if(!mats.ContainsKey(name))mats[name]=AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/"+name+".mat");return mats[name];}
    static Material Plain(string name,Color color,float smooth=.3f,float metal=0)
    {var path=Base+"Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);mats[name]=m;return m;}
    static Material Surface(string name,string folder,float meters,Color color,float smooth=.65f,float metal=0)
    {var m=Plain(name,color);m.shader=Shader.Find("Alley/Architectural Surface");m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Diffuse.jpg"));m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"nor_gl.jpg"));m.SetTexture("_MaskMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Unity Mask.png"));m.SetVector("_Meters",new Vector4(meters,meters,0,0));m.SetFloat("_BumpScale",.45f);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);return m;}
    static Mesh Rounded(Vector3 size,float radius)
    {
        string key=size.ToString("F3")+radius.ToString("F3");Mesh mesh;if(meshes.TryGetValue(key,out mesh))return mesh;
        mesh=new Mesh();mesh.name="Rounded Surface";var vs=new List<Vector3>();var ns=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();var h=size*.5f;radius=Mathf.Min(radius,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.45f);var core=h-Vector3.one*radius;
        Vector3[] dirs={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
        foreach(var n in dirs)
        {
            var u=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,n);var v=Vector3.Cross(n,u);
            float hu=Vector3.Dot(new Vector3(Mathf.Abs(u.x),Mathf.Abs(u.y),Mathf.Abs(u.z)),h),hv=Vector3.Dot(new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z)),h),hn=Vector3.Dot(new Vector3(Mathf.Abs(n.x),Mathf.Abs(n.y),Mathf.Abs(n.z)),h);
            float[] xs={-hu,-hu+radius,0,hu-radius,hu},ys={-hv,-hv+radius,0,hv-radius,hv};int start=vs.Count;
            for(int j=0;j<5;j++)for(int i=0;i<5;i++){var p=n*hn+u*xs[i]+v*ys[j];var c=new Vector3(Mathf.Clamp(p.x,-core.x,core.x),Mathf.Clamp(p.y,-core.y,core.y),Mathf.Clamp(p.z,-core.z,core.z));var d=(p-c).normalized;vs.Add(c+d*radius);ns.Add(d);uv.Add(new Vector2(xs[i],ys[j]));if(i<4&&j<4){int a=start+j*5+i;tri.AddRange(new[]{a,a+1,a+6,a,a+6,a+5});}}
        }
        mesh.SetVertices(vs);mesh.SetNormals(ns);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.RecalculateBounds();mesh.RecalculateTangents();Unwrapping.GenerateSecondaryUVSet(mesh);
        string hash;using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key))).Replace("-","").Substring(0,16);
        string path=Base+"Models/Rounded Surface "+hash+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing!=null){UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);meshes[key]=mesh;return mesh;
    }
    static Transform B(string name,Transform p,Vector3 at,Vector3 size,string material,bool collider=true,float bevel=.006f)
    {var g=G(name,p);g.localPosition=at;g.gameObject.AddComponent<MeshFilter>().sharedMesh=Rounded(size,bevel);g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=M(material);if(collider)g.gameObject.AddComponent<BoxCollider>().size=size;GameObjectUtility.SetStaticEditorFlags(g.gameObject,StaticEditorFlags.ContributeGI|StaticEditorFlags.ReflectionProbeStatic);return g;}
    static Transform Q(string name,Transform p,Vector3 at,Vector2 size,Material mat,Quaternion rotation)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=at;g.transform.localRotation=rotation;g.transform.localScale=new Vector3(size.x,size.y,1);UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;g.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;return g.transform;}
    static Transform Rod(string name,Transform p,Vector3 a,Vector3 b,float radius,string mat)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=(a+b)*.5f;g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);g.transform.localScale=new Vector3(radius*2,(b-a).magnitude*.5f,radius*2);UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=M(mat);return g.transform;}
    static void Text(string content,Transform p,Vector3 at,float height,Quaternion rotation,Color color)
    {var t=G(content,p);t.localPosition=at;t.localRotation=rotation;var mesh=t.gameObject.AddComponent<TextMesh>();mesh.text=content;mesh.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");mesh.fontSize=64;mesh.characterSize=height/6;mesh.anchor=TextAnchor.MiddleCenter;mesh.color=color;mesh.GetComponent<Renderer>().sharedMaterial=mesh.font.material;}
    static Bounds BoundsOf(Transform t){var rs=t.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
    static Transform Prop(string name,Transform parent,Vector3 at,float yaw=0,float height=0,bool imported=true)
    {
        string path=(imported?Base+"Prefabs/":"Assets/Alley/Prefabs/Street/")+name+".prefab";var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab==null)throw new Exception("Missing prefab "+path);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.transform.SetParent(parent,false);g.transform.localPosition=at;g.transform.localRotation=Quaternion.Euler(0,yaw,0);
        if(height>0){var b=BoundsOf(g.transform);g.transform.localScale*=height/b.size.y;}PrefabUtility.RecordPrefabInstancePropertyModifications(g.transform);return g.transform;
    }
    static void Lamp(Transform p,Vector3 at,float intensity,float range,float kelvin,Quaternion rotation,LightType type=LightType.Point)
    {var g=G("Light",p);g.localPosition=at;g.localRotation=rotation;var l=g.gameObject.AddComponent<Light>();l.type=type;l.intensity=intensity;l.range=range;l.useColorTemperature=true;l.colorTemperature=kelvin;l.shadows=LightShadows.Soft;l.shadowBias=.012f;l.shadowNormalBias=.04f;l.lightmapBakeType=LightmapBakeType.Mixed;if(type==LightType.Spot){l.spotAngle=120;l.innerSpotAngle=75;}var d=g.gameObject.AddComponent<UniversalAdditionalLightData>();var so=new SerializedObject(d);so.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;so.ApplyModifiedPropertiesWithoutUndo();}
    static void Fluorescent(Transform room,Vector3 at,float intensity=6,float kelvin=4200)
    {var g=Prop("Fluorescent Light",room,at);Lamp(g,new Vector3(0,-.11f,0),intensity,5.5f,kelvin,Quaternion.Euler(90,0,0),LightType.Point);}
    static void Weather(Transform p,string kind,Vector3 at,Vector2 size,Quaternion rotation)
    {var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Weathering/"+kind+".mat");if(m!=null)Q(kind,p,at,size,m,rotation);}
    static Material NoticeMaterial(string name)
    {var m=Plain(name,Color.white,.12f);m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"Textures/"+name+".png"));return m;}
    static void Notice(string name,Transform p,Vector3 at,Vector2 size,Quaternion rotation)
    {Q(name,p,at,size,NoticeMaterial(name),rotation);}
    public static string Library()
    {
        var source=JsonUtility.FromJson<Sources>(File.ReadAllText(Base+"Models/Sources.json"));
        foreach(var model in source.models)
        {
            string folder=Base+"Models/"+model.asset+"/";var normals=new HashSet<string>(model.materials.Where(m=>!string.IsNullOrEmpty(m.normal)).Select(m=>Path.GetFileName(m.normal)));var colors=new HashSet<string>(model.materials.Where(m=>!string.IsNullOrEmpty(m.color)).Select(m=>Path.GetFileName(m.color)));
            foreach(var file in Directory.GetFiles(folder,"*",SearchOption.AllDirectories).Where(f=>f.EndsWith(".png")||f.EndsWith(".jpg")))
            {var imp=AssetImporter.GetAtPath(file.Replace('\\','/')) as TextureImporter;if(imp==null)continue;imp.maxTextureSize=2048;imp.anisoLevel=4;if(normals.Contains(Path.GetFileName(file)))imp.textureType=TextureImporterType.NormalMap;else imp.sRGBTexture=colors.Contains(Path.GetFileName(file));imp.SaveAndReimport();}
            var map=new Dictionary<string,Material>();foreach(var src in model.materials)
            {
                string safe=System.Text.RegularExpressions.Regex.Replace(src.name,@"[^\w\- ]"," ");var m=Plain(model.asset+" "+safe,src.factor!=null?new Color(src.factor[0],src.factor[1],src.factor[2],src.factor[3]):Color.white,1-src.roughness,src.metallic);
                if(!string.IsNullOrEmpty(src.color))m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+src.color));if(!string.IsNullOrEmpty(src.normal)){m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+src.normal));m.EnableKeyword("_NORMALMAP");}
                if(!string.IsNullOrEmpty(src.unityMask)){var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+src.unityMask);m.SetTexture("_MetallicGlossMap",tex);m.SetTexture("_OcclusionMap",tex);m.EnableKeyword("_METALLICSPECGLOSSMAP");m.EnableKeyword("_OCCLUSIONMAP");m.SetFloat("_Metallic",1);m.SetFloat("_Smoothness",.8f);}
                if(!string.IsNullOrEmpty(src.emission)){m.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+src.emission));m.SetColor("_EmissionColor",Color.white*1.25f);m.EnableKeyword("_EMISSION");m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
                m.SetFloat("_Cull",src.doubleSided?0:2);map[src.name]=m;
            }
            var impModel=(ModelImporter)AssetImporter.GetAtPath(folder+model.asset+".fbx");impModel.importCameras=false;impModel.importLights=false;impModel.importAnimation=false;impModel.generateSecondaryUV=true;impModel.SaveAndReimport();
            var root=G(Names[model.asset],null);var child=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(folder+model.asset+".fbx"));child.transform.SetParent(root,false);
            foreach(var r in child.GetComponentsInChildren<Renderer>()){var slots=r.sharedMaterials;for(int i=0;i<slots.Length;i++){Material m;slots[i]=slots[i]!=null&&map.TryGetValue(slots[i].name,out m)?m:map.Values.First();}r.sharedMaterials=slots;}
            var bounds=BoundsOf(root);child.transform.position-=new Vector3(bounds.center.x,model.asset=="mounted_fluorescent_lights"?bounds.max.y:bounds.min.y,bounds.center.z);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base+"Prefabs/"+Names[model.asset]+".prefab");UnityEngine.Object.DestroyImmediate(root.gameObject);
        }
        foreach(var file in Directory.GetFiles(Base+"Textures/Terrazzo").Where(f=>f.EndsWith(".png")||f.EndsWith(".jpg"))){var imp=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));if(Path.GetFileName(file)=="nor_gl.jpg")imp.textureType=TextureImporterType.NormalMap;else imp.sRGBTexture=Path.GetFileName(file)=="Diffuse.jpg";imp.anisoLevel=8;imp.SaveAndReimport();}
        string pbr="Assets/Alley/Textures/PBR/";
        Surface("Green Tile",pbr+"square_tiled_wall/",1.2f,new Color(.29f,.46f,.35f),.76f);
        Surface("Yellowed Plaster",pbr+"painted_plaster_wall/",2,new Color(.72f,.66f,.49f),.16f);
        Surface("Concrete",pbr+"concrete_wall_006/",2,new Color(.61f,.60f,.52f),.25f);
        Surface("Terrazzo",Base+"Textures/Terrazzo/",2,new Color(.64f,.65f,.53f),.62f);
        Surface("Green Steel",pbr+"rusty_metal_02/",1.3f,new Color(.22f,.36f,.27f),.45f,.7f);
        Surface("Pale Green Panels",pbr+"painted_plaster_wall/",1.6f,new Color(.58f,.65f,.48f),.45f,.15f);
        Surface("Old Wood",pbr+"weathered_brown_planks/",1.3f,new Color(.53f,.43f,.28f),.3f);
        Surface("Stainless Steel",pbr+"rusty_metal_02/",2,new Color(.56f,.57f,.52f),.65f,.85f);
        Plain("Black Rubber",new Color(.024f,.027f,.023f),.16f);Plain("Paper",new Color(.66f,.61f,.44f),.1f);Plain("Aged Plastic",new Color(.50f,.46f,.33f),.35f);Plain("Red Enamel",new Color(.35f,.035f,.02f),.4f,.2f);
        Plain("Brass",new Color(.4f,.32f,.13f),.48f,.8f);Plain("Green Floor",new Color(.035f,.09f,.065f),.4f,.1f);
        var glass=Plain("Dirty Glass",new Color(.36f,.4f,.32f,.11f),.8f,.1f);glass.SetFloat("_Surface",1);glass.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);glass.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);glass.SetFloat("_ZWrite",0);glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.renderQueue=3000;
        var glow=Plain("Red Display",new Color(.5f,.01f,.006f));glow.SetColor("_EmissionColor",new Color(1,.015f,.004f));glow.EnableKeyword("_EMISSION");
        AssetDatabase.SaveAssets();return "Nine free model prefabs and apartment PBR materials prepared.";
    }
}
