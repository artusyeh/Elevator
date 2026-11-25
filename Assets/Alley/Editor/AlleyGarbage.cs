using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class AlleyGarbage
{
    const string Base="Assets/Alley/";
    static Transform Group(string n,Transform p){var g=new GameObject(n);g.transform.SetParent(p,false);return g.transform;}
    static Bounds BoundsOf(Transform t){var rs=t.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
    static Material Mat(string name,Color color,float smooth,float metal=0)
    {string path=Base+"Materials/Garbage/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);EditorUtility.SetDirty(m);return m;}
    static Material Plastic(string name,float opacity)
    {
        var m=Mat(name,new Color(.86f,.89f,.86f,opacity),.34f);m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);m.SetFloat("_BlendModePreserveSpecular",1);m.SetFloat("_SrcBlend",(float)BlendMode.One);m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_SrcBlendAlpha",(float)BlendMode.One);m.SetFloat("_DstBlendAlpha",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.SetFloat("_Cull",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.EnableKeyword("_ALPHAPREMULTIPLY_ON");m.SetOverrideTag("RenderType","Transparent");m.renderQueue=3000;
        string id=AssetDatabase.FindAssets("trashbag_nor_gl_1k t:Texture2D",new[]{Base+"Models/Street/trashbag"}).FirstOrDefault();if(id!=null){m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(id)));m.SetFloat("_BumpScale",.24f);m.EnableKeyword("_NORMALMAP");}return m;
    }
    public static string Library()
    {
        Directory.CreateDirectory(Base+"Materials/Garbage");Directory.CreateDirectory(Base+"Prefabs/Garbage");AssetDatabase.Refresh();
        var paper=Mat("Paper Contents",new Color(.67f,.67f,.62f),.12f);var card=Mat("Clean Cardboard Contents",new Color(.52f,.48f,.40f),.12f);var pale=Mat("Pale Packaging",new Color(.58f,.64f,.61f),.25f);var printed=Mat("Muted Wrapper",new Color(.42f,.52f,.49f),.25f);var tie=Mat("Bag Tie",new Color(.63f,.69f,.65f),.35f);
        string[] names={"Tied Bag","Full Bag","Small Bag"};for(int i=0;i<names.Length;i++)
        {
            string name=names[i],path=Base+"Models/Garbage/"+name+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.SaveAndReimport();var film=Plastic(name+" Film",new[]{.40f,.45f,.37f}[i]);var root=Group(name,null);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));model.name="Model";model.transform.SetParent(root,false);
            foreach(var r in model.GetComponentsInChildren<Renderer>())
            {string original=r.sharedMaterial.name;r.sharedMaterial=original.Contains("Bag Plastic")?film:original.Contains("Paper")?paper:original.Contains("Cardboard")?card:original.Contains("Pale Packaging")?pale:original.Contains("Printed")?printed:tie;r.name=r.name.Split('.')[0];if(r.name=="Plastic Shell")r.shadowCastingMode=ShadowCastingMode.Off;}
            var bounds=BoundsOf(root);model.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);var c=root.gameObject.AddComponent<CapsuleCollider>();c.height=bounds.size.y*.88f;c.radius=Mathf.Min(bounds.size.x,bounds.size.z)*.44f;c.center=new Vector3(0,c.height*.5f,0);PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base+"Prefabs/Garbage/"+name+".prefab");UnityEngine.Object.DestroyImmediate(root.gameObject);
        }
        AssetDatabase.SaveAssets();return "Three clean translucent bag variants imported, with original scanned folds and contained packaging.";
    }
    static Transform PlaceBag(string name,Transform p,Vector3 at,float height,float yaw)
    {var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/Garbage/"+name+".prefab"));g.transform.SetParent(p,false);float h=BoundsOf(g.transform).size.y;g.transform.localScale=Vector3.one*(height/h);g.transform.localPosition=at;g.transform.localRotation=Quaternion.Euler(0,yaw,0);return g.transform;}
    static float Ground(Transform area,Vector3 local)
    {var world=area.TransformPoint(local);float y=area.position.y;foreach(var h in Physics.RaycastAll(new Vector3(world.x,area.position.y+.7f,world.z),Vector3.down,1.4f).OrderByDescending(h=>h.point.y)){if(h.normal.y<.8f)continue;if(h.collider.name=="Collection Pad"||!h.collider.transform.IsChildOf(area)){if(h.point.y>area.position.y+.08f)continue;y=h.point.y;break;}}return area.InverseTransformPoint(new Vector3(world.x,y,world.z)).y+.003f;}
    static void Tube(List<Vector3> vs,List<Vector3> ns,List<int> ts,IList<Vector3> points,float radius,int sides=6)
    {
        int start=vs.Count;for(int i=0;i<points.Count;i++){Vector3 tangent=points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(0,i-1)];Vector3 n=Vector3.Cross(tangent.normalized,Vector3.up).normalized;Vector3 b=Vector3.Cross(tangent.normalized,n).normalized;for(int j=0;j<sides;j++){float a=j*Mathf.PI*2/sides;Vector3 q=n*Mathf.Cos(a)+b*Mathf.Sin(a);vs.Add(points[i]+q*radius);ns.Add(q);if(i<points.Count-1){int v=start+i*sides+j,w=start+i*sides+(j+1)%sides;ts.AddRange(new[]{v,w,w+sides,v,w+sides,v+sides});}}}
    }
    static void Net(Transform area,Transform bags,string name,Vector2 size,Color color)
    {
        var colliderMesh=new Mesh();colliderMesh.indexFormat=IndexFormat.UInt32;var cv=new List<Vector3>();var ct=new List<int>();foreach(var filter in bags.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Plastic Shell")){int n=cv.Count;cv.AddRange(filter.sharedMesh.vertices.Select(v=>filter.transform.TransformPoint(v)));ct.AddRange(filter.sharedMesh.triangles.Select(t=>t+n));}colliderMesh.SetVertices(cv);colliderMesh.SetTriangles(ct,0);colliderMesh.RecalculateBounds();var test=new GameObject("Net Drape Surface");var collider=test.AddComponent<MeshCollider>();collider.sharedMesh=colliderMesh;
        int nx=Mathf.CeilToInt(size.x/.022f),nz=Mathf.CeilToInt(size.y/.022f);var h=new float[nx+1,nz+1];var support=new float[nx+1,nz+1];float dx=size.x/nx,dz=size.y/nz;
        try
        {
            for(int x=0;x<=nx;x++)for(int z=0;z<=nz;z++){var p=new Vector3(-size.x*.5f+x*dx,0,-size.y*.5f+z*dz);float y=Ground(area,p);var world=area.TransformPoint(p);RaycastHit hit;if(collider.Raycast(new Ray(new Vector3(world.x,area.position.y+1.2f,world.z),Vector3.down),out hit,2))y=Mathf.Max(y,area.InverseTransformPoint(hit.point).y+.005f);support[x,z]=h[x,z]=y;}
            // A lightweight net bridges creases while retaining clearance over every bag.
            for(int pass=0;pass<12;pass++)for(int x=0;x<=nx;x++)for(int z=0;z<=nz;z++){float y=h[x,z];if(x>0)y=Mathf.Max(y,h[x-1,z]-dx*2);if(x<nx)y=Mathf.Max(y,h[x+1,z]-dx*2);if(z>0)y=Mathf.Max(y,h[x,z-1]-dz*4);if(z<nz)y=Mathf.Max(y,h[x,z+1]-dz*4);h[x,z]=y;}
            for(int pass=0;pass<3;pass++){var copy=(float[,])h.Clone();for(int x=1;x<nx;x++)for(int z=1;z<nz;z++)h[x,z]=Mathf.Max(support[x,z],copy[x,z]*.5f+(copy[x-1,z]+copy[x+1,z]+copy[x,z-1]+copy[x,z+1])*.125f);}
            foreach(int x in new[]{0,nx})foreach(int z in new[]{0,nz})h[x,z]=Ground(area,new Vector3(-size.x*.5f+x*dx,0,-size.y*.5f+z*dz));
        }
        finally{UnityEngine.Object.DestroyImmediate(test);UnityEngine.Object.DestroyImmediate(colliderMesh);}
        var vertices=new List<Vector3>();var normals=new List<Vector3>();var tris=new List<int>();Func<int,int,Vector3> point=(x,z)=>new Vector3(-size.x*.5f+x*dx,h[x,z],-size.y*.5f+z*dz);
        for(int x=0;x<=nx;x++){var points=new List<Vector3>();for(int z=0;z<=nz;z++)points.Add(point(x,z));Tube(vertices,normals,tris,points,.0008f);}
        for(int z=0;z<=nz;z++){var points=new List<Vector3>();for(int x=0;x<=nx;x++)points.Add(point(x,z)+Vector3.up*.0005f);Tube(vertices,normals,tris,points,.0008f);}
        var hem=new List<Vector3>();for(int x=0;x<=nx;x++)hem.Add(point(x,0));for(int z=1;z<=nz;z++)hem.Add(point(nx,z));for(int x=nx-1;x>=0;x--)hem.Add(point(x,nz));for(int z=nz-1;z>=0;z--)hem.Add(point(0,z));Tube(vertices,normals,tris,hem,.0025f,8);
        var mesh=new Mesh();mesh.name=name;mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(tris,0);mesh.RecalculateBounds();string path=Base+"Models/Garbage/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}
        var root=Group(name,area);root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=root.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=Mat(name,color,.28f);renderer.shadowCastingMode=ShadowCastingMode.Off;
        var weightMat=Mat("Net Weights",new Color(.18f,.20f,.18f),.16f);foreach(int x in new[]{0,nx})foreach(int z in new[]{0,nz}){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Corner Weight";g.transform.SetParent(root,false);var at=point(x,z);at.y=Ground(area,at)+.013f;g.transform.localPosition=at;g.transform.localScale=new Vector3(.06f,.013f,.06f);g.GetComponent<Renderer>().sharedMaterial=weightMat;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());}
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base+"Prefabs/Garbage/"+name+".prefab");
    }
    static void Notice(Transform area,Vector3 start,Vector3 direction)
    {
        var hits=Physics.RaycastAll(start,direction,2).Where(h=>h.normal.y<.2f&&h.normal.y>-.2f).OrderBy(h=>h.distance).ToArray();if(hits.Length==0)throw new Exception("No wall behind garbage notice.");var hit=hits[0];var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name="Collection Schedule";g.transform.SetParent(area,true);g.transform.SetPositionAndRotation(hit.point+hit.normal*.006f,Quaternion.FromToRotation(Vector3.back,hit.normal));g.transform.localScale=new Vector3(.31f,.42f,1);g.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/Street/Collection Schedule.mat");UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
    }
    static Transform Connect(Transform area,string name)
    {
        var parent=area.parent;var p=area.position;var q=area.rotation;area.SetParent(null,true);area.SetPositionAndRotation(Vector3.zero,Quaternion.identity);var prefab=PrefabUtility.SaveAsPrefabAsset(area.gameObject,Base+"Prefabs/Garbage/"+name+".prefab");UnityEngine.Object.DestroyImmediate(area.gameObject);var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.transform.SetParent(parent,false);g.transform.SetPositionAndRotation(p,q);PrefabUtility.RecordPrefabInstancePropertyModifications(g.transform);return g.transform;
    }
    public static string Place()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");var neighborhood=GameObject.Find("Neighborhood Details").transform;if(neighborhood.Find("Garbage Collection")!=null)throw new Exception("Garbage revision already exists.");
        var old=neighborhood.Find("Waste Collection");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);var oldSign=GameObject.Find("Props").transform.Find("Garbage Collection");if(oldSign!=null)UnityEngine.Object.DestroyImmediate(oldSign.gameObject);
        var collection=Group("Garbage Collection",neighborhood);var main=Group("Service Lane Collection",collection);main.position=new Vector3(5.9f,.008f,25.46f);var bags=Group("Bags",main);
        PlaceBag("Tied Bag",bags,new Vector3(-.82f,0,-.005f),.44f,13);PlaceBag("Small Bag",bags,new Vector3(-.28f,0,.018f),.41f,-21);PlaceBag("Full Bag",bags,new Vector3(.27f,0,-.018f),.40f,8);PlaceBag("Tied Bag",bags,new Vector3(.83f,0,.01f),.46f,-17);
        var crate=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/Street/Storage Crate.prefab"));crate.name="Recycling Crate";crate.transform.SetParent(main,false);float ch=BoundsOf(crate.transform).size.y;crate.transform.localScale*=.24f/ch;crate.transform.localPosition=new Vector3(1.47f,0,0);crate.transform.localRotation=Quaternion.Euler(0,5,0);
        Physics.SyncTransforms();Notice(main,new Vector3(5.9f,1.35f,25.0f),Vector3.forward);Net(main,bags,"Blue Collection Net",new Vector2(2.35f,.58f),new Color(.08f,.36f,.54f));Connect(main,"Service Lane Collection");
        var secondary=Group("Residential Collection",collection);secondary.SetPositionAndRotation(new Vector3(-2.93f,.16f,23.05f),Quaternion.Euler(0,-90,0));var pad=GameObject.CreatePrimitive(PrimitiveType.Cube);pad.name="Collection Pad";pad.transform.SetParent(secondary,false);pad.transform.localPosition=new Vector3(0,-.13f,0);pad.transform.localScale=new Vector3(1.05f,.26f,.48f);pad.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/Weathered Concrete.mat");bags=Group("Bags",secondary);PlaceBag("Small Bag",bags,new Vector3(-.21f,.003f,0),.30f,18);PlaceBag("Full Bag",bags,new Vector3(.20f,.003f,0),.28f,-15);
        Physics.SyncTransforms();Notice(secondary,new Vector3(-2.5f,1.25f,23.05f),Vector3.left);Net(secondary,bags,"Green Collection Net",new Vector2(.98f,.42f),new Color(.12f,.42f,.25f));Connect(secondary,"Residential Collection");
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Main collection: four translucent bags, fitted blue net, four weights, recycling crate and notice. Secondary: two bags, fitted green net, weights and wall notice.";
    }
    public static string Polish()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");
        foreach(var name in new[]{"Service Lane Collection","Residential Collection"})
        {
            var root=GameObject.Find(name).transform;if(PrefabUtility.IsPartOfPrefabInstance(root))PrefabUtility.UnpackPrefabInstance(root.gameObject,PrefabUnpackMode.OutermostRoot,InteractionMode.AutomatedAction);var net=root.Cast<Transform>().First(t=>t.name.Contains("Collection Net"));UnityEngine.Object.DestroyImmediate(net.gameObject);bool main=name=="Service Lane Collection";Net(root,root.Find("Bags"),main?"Blue Collection Net":"Green Collection Net",main?new Vector2(2.35f,.58f):new Vector2(.98f,.42f),main?new Color(.08f,.36f,.54f):new Color(.12f,.42f,.25f));
        }
        var lane=GameObject.Find("Service Alley").transform;var old=lane.Find("Bulkhead Light");if(old!=null)old.gameObject.SetActive(false);
        var existing=lane.GetComponentsInChildren<Light>();foreach(var l in existing)l.enabled=false;
        var area=GameObject.Find("Service Lane Collection").transform;var oldLamp=area.Find("Collection Light");if(oldLamp!=null)UnityEngine.Object.DestroyImmediate(oldLamp.gameObject);var fixture=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/Lighting/Residential Light.prefab"));fixture.name="Collection Light";fixture.transform.SetParent(area,true);fixture.transform.SetPositionAndRotation(new Vector3(6.0f,2.25f,25.875f),Quaternion.identity);var light=fixture.GetComponentInChildren<Light>();light.type=LightType.Spot;light.transform.localPosition=new Vector3(0,-.055f,-.11f);light.transform.localRotation=Quaternion.Euler(105,0,0);light.spotAngle=120;light.innerSpotAngle=80;light.intensity=3.4f;light.range=4;light.colorTemperature=4100;light.shadows=LightShadows.Soft;
        Connect(GameObject.Find("Service Lane Collection").transform,"Service Lane Collection");Connect(GameObject.Find("Residential Collection").transform,"Residential Collection");
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Lighter fitted netting and neutral collection light saved.";
    }
    static void Capture(string file,Vector3 at,Vector3 look)
    {
        var cam=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>().eyes;var p=cam.transform.position;var q=cam.transform.rotation;var old=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1600,1000,24);var texture=new Texture2D(1600,1000,TextureFormat.RGB24,false);
        try{cam.transform.SetPositionAndRotation(at,Quaternion.LookRotation(look-at));cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1600,1000),0,0);texture.Apply();File.WriteAllBytes("References/Garbage/"+file+".png",texture.EncodeToPNG());}
        finally{cam.transform.SetPositionAndRotation(p,q);cam.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(texture);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
    }
    public static void Capture()
    {Capture("Service Lane Collection",new Vector3(3.95f,1.655f,24.5f),new Vector3(5.9f,.55f,25.46f));Capture("Collection Details",new Vector3(5.85f,1.655f,24.5f),new Vector3(5.9f,.4f,25.46f));Capture("Residential Collection",new Vector3(-.75f,1.57f,23.4f),new Vector3(-2.95f,.55f,23.05f));}
}


