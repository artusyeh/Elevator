using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class AlleyVending
{
    const string ModelFolder="Assets/Alley/Models/Vending Machine/";
    const string MaterialFolder="Assets/Alley/Materials/Vending Machine/";
    const string PrefabPath="Assets/Alley/Prefabs/Vending Machine.prefab";
    static Bounds BoundsOf(GameObject g){var rs=g.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
    static Material Material(string name,string texture,float smooth,float metal=0,float emission=0,bool glass=false)
    {
        string path=MaterialFolder+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.name=name;
        var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(ModelFolder+"Textures/"+texture+".png");if(tex==null)throw new Exception("Missing original texture: "+texture);
        m.SetTexture("_BaseMap",tex);m.SetColor("_BaseColor",Color.white);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);
        if(emission>0){m.EnableKeyword("_EMISSION");m.SetTexture("_EmissionMap",tex);m.SetColor("_EmissionColor",Color.white*emission);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
        if(glass){m.SetFloat("_Surface",1);m.SetFloat("_Blend",1);m.SetFloat("_SrcBlend",(float)BlendMode.One);m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_SrcBlendAlpha",(float)BlendMode.One);m.SetFloat("_DstBlendAlpha",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.SetFloat("_Cull",0);m.SetOverrideTag("RenderType","Transparent");m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.EnableKeyword("_ALPHAPREMULTIPLY_ON");m.renderQueue=3000;}
        EditorUtility.SetDirty(m);return m;
    }
    public static string Replace()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode before saving the replacement.");
        Directory.CreateDirectory(MaterialFolder);AssetDatabase.Refresh();
        foreach(var path in Directory.GetFiles(ModelFolder+"Textures/","*.png"))
        {var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Default;t.sRGBTexture=true;t.alphaSource=TextureImporterAlphaSource.FromInput;t.maxTextureSize=4096;t.npotScale=TextureImporterNPOTScale.None;t.textureCompression=TextureImporterCompression.CompressedHQ;t.anisoLevel=4;t.SaveAndReimport();}
        var map=new Dictionary<string,Material>{
            {"Shell",Material("Cabinet","ShellCorona_Beauty",.32f,.12f)},
            {"Backbone",Material("Interior","BackboneCorona_Beauty",.32f,.08f)},
            {"Background",Material("Display Backing","BackgroundCorona_Beauty",.2f,0,.35f)},
            {"Bottles",Material("Drink Display","BottlesCorona_Beauty",.34f,.02f,.35f)},
            {"PriceTags",Material("Prices","PriceTagsCorona_Beauty",.25f,0,.25f)},
            {"Misc",Material("Payment Panel","MiscCorona_Beauty",.4f,.1f)},
            {"BoxDispencer",Material("Dispensing Tray","BoxDispencerCorona_Beauty",.28f)},
            {"BoxDispencerLid",Material("Tray Flap","BoxDispencerLidCorona_DiffuseModified",.4f)},
            {"Glass",Material("Display Glass","GlassCorona_DiffuseColor",.88f,0,0,true)}
        };
        map["Background"].SetFloat("_Cull",0); map["Bottles"].SetFloat("_Cull",0); var instances=GameObject.Find("Alley").GetComponentsInChildren<Transform>().Where(t=>t.name=="Vending Machine").Select(t=>new {t.gameObject,at=t.position,rotation=t.rotation,parent=t.parent}).ToArray();
        var root=new GameObject("Vending Machine");var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder+"Japanese Vending Machine.fbx"));model.name="Model";model.transform.SetParent(root.transform,false);model.transform.localRotation=Quaternion.Euler(0,180,0);
        var shell=model.GetComponentsInChildren<Renderer>().First(r=>r.name=="Shell");float scale=1.85f/shell.bounds.size.y;model.transform.localScale=Vector3.one*scale;
        var bounds=shell.bounds;model.transform.localPosition-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        foreach(var r in model.GetComponentsInChildren<Renderer>()){var m=map[r.name];r.sharedMaterial=m;r.name=m.name;r.lightProbeUsage=LightProbeUsage.BlendProbes;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;r.receiveShadows=true;if(m.name=="Display Glass")r.shadowCastingMode=ShadowCastingMode.Off;}
        bounds=shell.bounds;var body=root.AddComponent<BoxCollider>();body.center=bounds.center;body.size=bounds.size;
        var lamp=new GameObject("Display Light");lamp.transform.SetParent(root.transform,false);lamp.transform.localPosition=new Vector3(0,1.18f,-.55f);lamp.transform.localRotation=Quaternion.Euler(35,180,0);var light=lamp.AddComponent<Light>();light.type=LightType.Spot;light.spotAngle=112;light.innerSpotAngle=70;light.intensity=1.8f;light.range=3;light.useColorTemperature=true;light.colorTemperature=5100;light.color=Color.white;light.shadows=LightShadows.None;light.lightmapBakeType=LightmapBakeType.Realtime;lamp.AddComponent<UniversalAdditionalLightData>();
        var pad=GameObject.CreatePrimitive(PrimitiveType.Cube);pad.name="Concrete Pad";pad.transform.SetParent(root.transform,false);pad.transform.localPosition=new Vector3(0,-.08f,0);pad.transform.localScale=new Vector3(1.10f,.16f,.80f);pad.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Weathered Concrete.mat"); var glow=new GameObject("Display Spill");glow.transform.SetParent(root.transform,false);glow.transform.localPosition=new Vector3(0,1.3f,-.44f);var spill=glow.AddComponent<Light>();spill.type=LightType.Point;spill.intensity=.05f;spill.range=1.5f;spill.color=new Color(.86f,.93f,1);spill.shadows=LightShadows.None;spill.lightmapBakeType=LightmapBakeType.Realtime; var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);UnityEngine.Object.DestroyImmediate(root);
        foreach(var old in instances){UnityEngine.Object.DestroyImmediate(old.gameObject);var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.transform.SetParent(old.parent,true);g.transform.SetPositionAndRotation(new Vector3(2.65f,.16f,old.at.z),old.rotation);}
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Replaced "+instances.Length+" machines. Height 1.85 m. Original FBX geometry and texture images retained.";
    }
    public static string RoadHeight()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");
        var street=GameObject.Find("Street").transform;var road=street.Find("Road");if(road.position.y<-.18f)return "Road correction already applied.";
        foreach(Transform t in street)
        {if(new[]{"Road","Manhole Cover","Worn Edge Marking","Gully Grate"}.Contains(t.name))t.position+=Vector3.down*.08f;else if(t.name=="Precast Gutter")t.position+=Vector3.down*.06f;}
        var details=GameObject.Find("Facade Details");foreach(Transform t in details.transform)if(t.name=="Drain Grate")t.position+=Vector3.down*.06f;else if(t.name=="Damp Edge")t.position+=Vector3.down*.08f;
        // Keep the existing side passage footprint; this apron bridges its new 8 cm road lip.
        var mesh=new Mesh();mesh.name="Passage Threshold";mesh.vertices=new[]{new Vector3(2.85f,-.08f,24.2f),new Vector3(3.65f,.005f,24.2f),new Vector3(2.85f,-.08f,25.8f),new Vector3(3.65f,.005f,25.8f)};mesh.triangles=new[]{0,2,1,1,2,3};mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,"Assets/Alley/Models/Passage Threshold.asset");var ramp=new GameObject("Passage Threshold");ramp.transform.SetParent(street,false);ramp.AddComponent<MeshFilter>().sharedMesh=mesh;ramp.AddComponent<MeshRenderer>().sharedMaterial=road.GetComponent<Renderer>().sharedMaterial;ramp.AddComponent<MeshCollider>().sharedMesh=mesh;
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Road, covers and road markings lowered by 8 cm; gutters and grate fittings lowered by 6 cm. Existing curb and sidewalk positions preserved.";
    }
    public static string SettleRoad()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");
        var street=GameObject.Find("Street").transform;float level=street.Find("Road").GetComponent<Renderer>().bounds.max.y;
        foreach(Transform t in street)if(t.name=="Manhole Cover"){var b=BoundsOf(t.gameObject);t.position+=Vector3.up*(level+.001f-b.max.y);}
        // The old passage slabs extended across the main road. Trim only that overlap.
        var floor=street.Find("Side Passage Floor");float end=floor.GetComponent<Renderer>().bounds.max.x;var scale=floor.localScale;scale.x=end-3.65f;floor.localScale=scale;var pos=floor.position;pos.x=(3.65f+end)*.5f;floor.position=pos;
        var paving=GameObject.Find("Service Alley").transform.Find("Paving");end=paving.GetComponent<Renderer>().bounds.max.x;scale=paving.localScale;scale.x=end-3.65f;paving.localScale=scale;pos=paving.position;pos.x=(3.65f+end)*.5f;paving.position=pos;
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Manhole tops set 1 mm above asphalt; side-passage slab overlaps removed.";
    }
    public static void Capture(string file,Vector3 position,Vector3 angles)
    {
        var cam=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>().eyes;var p=cam.transform.position;var q=cam.transform.rotation;var target=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1600,1000,24);var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);
        try{cam.transform.SetPositionAndRotation(position,Quaternion.Euler(angles));cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes("References/Vending Machine/"+file+".png",image.EncodeToPNG());}
        finally{cam.transform.SetPositionAndRotation(p,q);cam.targetTexture=target;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
    }
    public static string SeatCovers()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");
        var street=GameObject.Find("Street").transform;var road=street.Find("Road");var bounds=road.GetComponent<Renderer>().bounds;float y=bounds.max.y;
        var covers=street.Cast<Transform>().Where(t=>t.name=="Manhole Cover").OrderBy(t=>t.position.z).ToArray();
        var vertices=new List<Vector3>();var triangles=new List<int>();
        Action<Vector3,Vector3,Vector3,Vector3> quad=(a,b,c,d)=>{int n=vertices.Count;vertices.Add(road.InverseTransformPoint(a));vertices.Add(road.InverseTransformPoint(b));vertices.Add(road.InverseTransformPoint(c));vertices.Add(road.InverseTransformPoint(d));triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});};
        Action<float,float> strip=(a,b)=>{if(b>a)quad(new Vector3(bounds.min.x,y,a),new Vector3(bounds.min.x,y,b),new Vector3(bounds.max.x,y,b),new Vector3(bounds.max.x,y,a));};
        float last=bounds.min.z;
        foreach(var cover in covers)
        {
            float z=cover.position.z;float x=cover.position.x;float radius=0;
            foreach(var filter in cover.GetComponentsInChildren<MeshFilter>())foreach(var vertex in filter.sharedMesh.vertices){var p=filter.transform.TransformPoint(vertex);radius=Mathf.Max(radius,new Vector2(p.x-x,p.z-z).magnitude);}
            float r=radius-.005f;float half=r+.035f;strip(last,z-half);
            Func<float,Vector3> circle=a=>new Vector3(x+Mathf.Cos(a)*r,y,z+Mathf.Sin(a)*r);
            Func<float,Vector3> outer=a=>{float dx=Mathf.Cos(a),dz=Mathf.Sin(a);float tx=(dx>0?bounds.max.x-x:x-bounds.min.x)/Mathf.Max(.00001f,Mathf.Abs(dx));float tz=half/Mathf.Max(.00001f,Mathf.Abs(dz));float t=Mathf.Min(tx,tz);return new Vector3(x+dx*t,y,z+dz*t);};
            var angles=Enumerable.Range(0,96).Select(i=>i*Mathf.PI*2/96).ToList();
            foreach(float edge in new[]{bounds.min.x-x,bounds.max.x-x})foreach(float side in new[]{-half,half}){float a=Mathf.Atan2(side,edge);angles.Add(a<0?a+Mathf.PI*2:a);}angles.Sort();
            for(int i=0;i<angles.Count;i++){float a=angles[i],b=i+1<angles.Count?angles[i+1]:Mathf.PI*2;quad(circle(a),circle(b),outer(b),outer(a));}
            last=z+half;
        }
        strip(last,bounds.max.z);var mesh=new Mesh();mesh.name="Alley Road";mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.uv=vertices.Select(v=>new Vector2(v.x,v.z)).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();Unwrapping.GenerateSecondaryUVSet(mesh);
        const string path="Assets/Alley/Models/Alley Road.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}road.GetComponent<MeshFilter>().sharedMesh=mesh;
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Asphalt cutouts seated around five original metal cover meshes; continuous road collider retained.";
    }
}


