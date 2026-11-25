using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class AlleySurfaces
{
    const string Base="Assets/Alley/";
    static Dictionary<string,Material> materials=new Dictionary<string,Material>();
    static Dictionary<string,Mesh> bevels=new Dictionary<string,Mesh>();
    static Material Surface(string name,string asset,float meters,Color tint,float metal=1,float smooth=1)
    {
        string folder=Base+"Textures/PBR/"+asset+"/";
        var m=new Material(Shader.Find("Alley/Architectural Surface"));m.name=name;
        m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Diffuse.jpg"));m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"nor_gl.jpg"));m.SetTexture("_MaskMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Unity Mask.png"));
        m.SetVector("_Meters",new Vector4(meters,meters,0,0));m.SetColor("_BaseColor",tint);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);m.SetFloat("_BumpScale",.42f);
        AssetDatabase.CreateAsset(m,Base+"Materials/"+name+".mat");materials[name]=m;return m;
    }
    static Material Plain(string name,Color color,float smooth,float metal=0)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);AssetDatabase.CreateAsset(m,Base+"Materials/"+name+".mat");materials[name]=m;return m;
    }
    static string Key(Vector3 v){return v.x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+"_"+v.y.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+"_"+v.z.ToString("F3",System.Globalization.CultureInfo.InvariantCulture);}
    static Transform Group(string name,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    static GameObject Box(string name,Transform p,Vector3 at,Vector3 size,Material m)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=at;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;g.isStatic=true;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static GameObject Pin(string name,Transform p,Vector3 at,float radius,float depth,Material m)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=at;g.transform.localRotation=Quaternion.Euler(90,0,0);g.transform.localScale=new Vector3(radius*2,depth*.5f,radius*2);g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;
    }
    static Transform Quad(string name,Transform p,Vector3 at,Vector2 size,Material mat)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=at;g.transform.localScale=new Vector3(size.x,size.y,1);g.GetComponent<Renderer>().sharedMaterial=mat;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g.transform;
    }
    static Mesh RingMesh()
    {
        var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
        for(int a=0;a<=40;a++)for(int b=0;b<=8;b++){float u=a*Mathf.PI/20,v=b*Mathf.PI/4;Vector3 n=new Vector3(Mathf.Cos(u)*Mathf.Cos(v),Mathf.Sin(u)*Mathf.Cos(v),Mathf.Sin(v));vertices.Add(new Vector3(Mathf.Cos(u)*(.15f+.008f*Mathf.Cos(v)),Mathf.Sin(u)*(.15f+.008f*Mathf.Cos(v)),.008f*Mathf.Sin(v)));normals.Add(n);uv.Add(new Vector2(a/40f,b/8f));if(a<40&&b<8){int i=a*9+b;tris.AddRange(new[]{i,i+9,i+1,i+1,i+9,i+10});}}
        var mesh=new Mesh();mesh.name="Fan Guard";mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Base+"Models/Fan Guard.asset");return mesh;
    }
    static void Weather(Transform parent,string kind,Vector3 local,Vector2 size,float opacity=.5f)
    {
        var mat=AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/Weathering/"+kind+".mat");
        string variant=Base+"Materials/Weathering/"+kind+" "+opacity.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+".mat";
        var placed=AssetDatabase.LoadAssetAtPath<Material>(variant);if(placed==null){placed=new Material(mat);placed.name=kind;placed.SetFloat("_Opacity",opacity);AssetDatabase.CreateAsset(placed,variant);}
        Quad(kind,parent,local,size,placed);
    }
    [MenuItem("Alley/Improve Shop Section Surfaces")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");
        if(GameObject.Find("Facade Details")!=null)throw new Exception("Surface pass is already installed.");
        Directory.CreateDirectory(Base+"Materials/Weathering");Directory.CreateDirectory(Base+"Prefabs/Weathering");
        AssetDatabase.Refresh();
        foreach(string path in Directory.GetFiles(Base+"Textures/PBR","*",SearchOption.AllDirectories))
        {
            if(!(path.EndsWith(".jpg")||path.EndsWith(".png")))continue;
            string file=Path.GetFileName(path);var importer=AssetImporter.GetAtPath(path.Replace('\\','/')) as TextureImporter;if(importer==null)continue;
            if(file=="nor_gl.jpg"||file.Contains("NormalGL"))importer.textureType=TextureImporterType.NormalMap;
            else importer.sRGBTexture=file=="Diffuse.jpg"||file.Contains("_Color");
            importer.maxTextureSize=2048;importer.anisoLevel=8;importer.SaveAndReimport();
        }
        Surface("Weathered Concrete","concrete_wall_006",2,new Color(.9f,.92f,.9f));
        Surface("Ivory Plaster","painted_plaster_wall",2,new Color(.85f,.87f,.82f));
        Surface("Ceramic Facade","rounded_square_tiled_wall",2,new Color(.78f,.82f,.8f),0,.7f);
        Surface("Decorative Tile","square_tiled_wall",2,new Color(.85f,.86f,.83f));
        Surface("Block Wall","concrete_block_wall",2,new Color(.72f,.74f,.7f));
        Surface("Galvanized Sheet","corrugated_iron",2,new Color(.7f,.73f,.72f),1,.65f);
        Surface("Old Steel","rusty_metal_02",2,new Color(.6f,.59f,.54f),1,.65f);
        Surface("Painted Shutter","painted_metal_shutter",2,new Color(.72f,.76f,.7f),.8f,.7f);
        Surface("Weathered Timber","weathered_brown_planks",1.8f,new Color(.52f,.44f,.34f),0,.65f);
        Surface("Service Plastic","weathered_pvc_tank",2,new Color(.7f,.73f,.69f),0,.75f);
        Surface("Rubber Surface","rubber_tiles",1,new Color(.19f,.2f,.19f),0,.2f);
        var card=Surface("Cardboard","cardboard",1,Color.white,0,.1f);card.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"Textures/PBR/cardboard/Cardboard002_2K-JPG_Color.jpg"));card.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"Textures/PBR/cardboard/Cardboard002_2K-JPG_NormalGL.jpg"));
        Plain("Brushed Aluminium",new Color(.31f,.33f,.34f),.48f,.85f);Plain("Window Rubber",new Color(.025f,.027f,.025f),.18f);Plain("Glazed Door",new Color(.12f,.19f,.2f),.84f,.12f);
        var glass=Plain("Window Reflection",new Color(.25f,.3f,.32f,.13f),.88f,.05f);glass.SetFloat("_Surface",1);glass.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);glass.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);glass.SetFloat("_ZWrite",0);glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.renderQueue=3000;
        var rim=RingMesh();
        foreach(string kind in new[]{"Water Stain","Dirt","Rust","Moss","Mold","Concrete Crack","Peeling Paint","Discoloration","Ground Stain"})
        {
            var mat=new Material(Shader.Find("Alley/Surface Weathering"));mat.name=kind;mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"Textures/Weathering/"+kind+".png"));mat.SetFloat("_Opacity",.5f);AssetDatabase.CreateAsset(mat,Base+"Materials/Weathering/"+kind+".mat");
            var q=Quad(kind,null,Vector3.zero,Vector2.one,mat);PrefabUtility.SaveAsPrefabAsset(q.gameObject,Base+"Prefabs/Weathering/"+kind+".prefab");UnityEngine.Object.DestroyImmediate(q.gameObject);
        }
        var modelImporter=(ModelImporter)AssetImporter.GetAtPath(Base+"Models/Architectural Edges.fbx");modelImporter.generateSecondaryUV=true;modelImporter.importCameras=false;modelImporter.importLights=false;modelImporter.SaveAndReimport();
        foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(Base+"Models/Architectural Edges.fbx"))if(asset is Mesh)bevels[asset.name]=(Mesh)asset;
        var root=GameObject.Find("Alley");var original=root.GetComponentsInChildren<MeshRenderer>();int beveled=0;
        foreach(var r in original)
        {
            float z=r.transform.position.z;if(z<0||z>16||Mathf.Abs(r.transform.position.x)>11)continue;
            string name=r.name;
            if(r.sharedMaterial!=null)
            {
                string old=r.sharedMaterial.name;
                if(old.StartsWith("Dark stained timber",StringComparison.OrdinalIgnoreCase))r.sharedMaterial=materials["Weathered Timber"];
                else if(old=="Oxidized steel")r.sharedMaterial=materials["Old Steel"];
                else if(old=="Aged ivory enamel")r.sharedMaterial=materials["Service Plastic"];
            }
            if(name=="Closed Aluminium Shutter"||name=="Shutter Slat")r.sharedMaterial=materials["Painted Shutter"];
            if(name=="Rubber Tyre")r.sharedMaterial=materials["Rubber Surface"];
            if(name=="Awning Standing Seam"||name=="Deep Metal Awning"||name=="Rain Eave")r.sharedMaterial=materials["Galvanized Sheet"];
            if(name=="Cardboard Delivery Carton")r.sharedMaterial=materials["Cardboard"];
            if(name=="Door Step"||name=="Concrete Plinth")r.sharedMaterial=materials["Weathered Concrete"];
            if(name.Contains("Mullion")||name=="Window Sill")r.sharedMaterial=materials["Brushed Aluminium"];
            var filter=r.GetComponent<MeshFilter>();Mesh mesh;
            if(filter!=null&&filter.sharedMesh.name=="Cube"&&bevels.TryGetValue(Key(r.transform.localScale),out mesh)){filter.sharedMesh=mesh;beveled++;}
        }
        var detailRoot=Group("Facade Details",root.transform);
        foreach(Transform building in GameObject.Find("Buildings").transform)
        {
            if(building.position.z>16||building.position.z<0||!(building.name=="Townhouse"||building.name=="Apartment Building"))continue;
            bool left=building.position.x<0;var facade=building.Find("Facade Volume");float height=facade.localScale.y;float width=facade.localScale.x;
            string mat=left?(building.position.z<7?"Ivory Plaster":"Weathered Concrete"):(building.position.z<8?"Ceramic Facade":"Ivory Plaster");facade.GetComponent<Renderer>().sharedMaterial=materials[mat];
            var group=Group("Facade Fittings",building);
            Box("Roof Flashing",group,new Vector3(0,height+.28f,-3.04f),new Vector3(width+.2f,.065f,.24f),materials["Brushed Aluminium"]);
            Box("Rain Gutter",group,new Vector3(0,height-.04f,-3.2f),new Vector3(width,.12f,.16f),materials["Old Steel"]);
            foreach(var window in building.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Window Glass").ToArray())
            {
                Vector3 pos=window.transform.localPosition;Vector3 size=window.transform.localScale;
                Quad("Window Pane",group,pos+Vector3.back*.047f,new Vector2(size.x,size.y),glass);
                foreach(int side in new[]{-1,1})
                {Box("Window Reveal",group,pos+new Vector3(side*(size.x*.5f+.035f),0,-.025f),new Vector3(.08f,size.y+.18f,.14f),materials["Brushed Aluminium"]);Box("Glazing Seal",group,pos+new Vector3(side*(size.x*.5f-.015f),0,-.065f),new Vector3(.024f,size.y,.024f),materials["Window Rubber"]);}
                Box("Window Lintel",group,pos+new Vector3(0,size.y*.5f+.045f,-.02f),new Vector3(size.x+.16f,.075f,.16f),materials["Brushed Aluminium"]);
                Weather(group,"Water Stain",pos+new Vector3(0,-size.y*.5f-.35f,.04f),new Vector2(size.x*.8f,.7f),.35f);
            }
            foreach(var ac in building.GetComponentsInChildren<Transform>().Where(t=>t.name=="Outdoor AC Condenser").ToArray())
            {
                var fan=Group("Fan Guard",ac);fan.localPosition=new Vector3(-.17f,0,-.19f);fan.gameObject.AddComponent<MeshFilter>().sharedMesh=rim;fan.gameObject.AddComponent<MeshRenderer>().sharedMaterial=materials["Brushed Aluminium"];
                Pin("Fan Recess",ac,new Vector3(-.17f,0,-.174f),.145f,.01f,materials["Window Rubber"]);
                Pin("Fan Hub",ac,new Vector3(-.17f,0,-.2f),.035f,.025f,materials["Old Steel"]);
                for(int j=0;j<8;j++){var blade=Box("Fan Blade",ac,new Vector3(-.17f,0,-.18f),new Vector3(.024f,.26f,.012f),materials["Old Steel"]);blade.transform.localRotation=Quaternion.Euler(0,0,j*22.5f);}
                foreach(int side in new[]{-1,1}){Box("Mounting Plate",ac,new Vector3(side*.28f,-.2f,.19f),new Vector3(.11f,.28f,.028f),materials["Old Steel"]);Pin("Mounting Bolt",ac,new Vector3(side*.28f,-.18f,-.22f),.018f,.018f,materials["Brushed Aluminium"]);}
            }
            Vector3 door=new Vector3(-width*.34f,1.08f,-3.23f);
            foreach(int side in new[]{-1,1})Box("Door Jamb",group,door+new Vector3(side*.51f,0,0),new Vector3(.085f,2.2f,.12f),materials[left?"Weathered Timber":"Brushed Aluminium"]);
            Box("Door Header",group,door+Vector3.up*1.13f,new Vector3(1.1f,.09f,.15f),materials["Brushed Aluminium"]);
            Box("Mail Slot",group,door+new Vector3(0,-.65f,-.08f),new Vector3(.26f,.045f,.035f),materials["Old Steel"]);
            Box("Porch Skirting",group,new Vector3(0,.18f,-3.17f),new Vector3(width,.36f,.08f),materials["Decorative Tile"]);
            float pipe=width*.46f;
            Weather(group,"Water Stain",new Vector3(pipe,.8f,-3.115f),new Vector2(.45f,1.4f),.55f);
            Weather(group,"Moss",new Vector3(pipe,.18f,-3.125f),new Vector2(.65f,.28f),.4f);
            Weather(group,"Discoloration",new Vector3(width*.1f,1.5f,-3.11f),new Vector2(1.1f,1.6f),.22f);
            if(left && building.position.z>7)Weather(group,"Concrete Crack",new Vector3(-width*.45f,1.3f,-3.12f),new Vector2(.5f,1.5f),.5f);
            Weather(group,"Mold",new Vector3(-width*.43f,.3f,-3.13f),new Vector2(.45f,.4f),.25f);
            foreach(var meter in building.GetComponentsInChildren<Transform>().Where(t=>t.name=="Electrical Meter").ToArray())
            {
                var panel=Group("Meter Cover",building);panel.localPosition=meter.localPosition+Vector3.back*.115f;Pin("Meter Dial",panel,Vector3.zero,.07f,.012f,materials["Service Plastic"]);Pin("Meter Glass",panel,Vector3.back*.015f,.065f,.008f,glass);
                Box("Meter Readout",panel,new Vector3(0,.015f,-.025f),new Vector3(.08f,.025f,.01f),materials["Window Rubber"]);
            }
        }
        var shop=GameObject.Find("Minato Sushi").transform;
        Weather(shop,"Peeling Paint",new Vector3(-2.5f,.7f,-.12f),new Vector2(.22f,1.3f),.45f);
        Weather(shop,"Rust",new Vector3(-2.18f,1.1f,-.245f),new Vector2(.18f,1.5f),.35f);
        Weather(shop,"Dirt",new Vector3(-.7f,.13f,-.24f),new Vector2(2.8f,.22f),.5f);
        // Small fittings are additional editable objects; the road and building masses retain their transforms.
        foreach(int side in new[]{-1,1})foreach(float z in new[]{3.5f,8.5f,13.5f})
        {
            var grate=Group("Drain Grate",detailRoot);grate.position=new Vector3(side*3.02f,.06f,z);
            Box("Recess",grate,Vector3.zero,new Vector3(.28f,.035f,.52f),materials["Window Rubber"]);
            for(int i=0;i<10;i++)Box("Grate Bar",grate,new Vector3(0,.025f,-.225f+i*.05f),new Vector3(.26f,.025f,.014f),materials["Old Steel"]);
            var stain=Quad("Drain Stain",grate,new Vector3(0,.03f,0),new Vector2(.48f,.8f),AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/Weathering/Ground Stain.mat"));stain.localRotation=Quaternion.Euler(90,0,0);
        }
        foreach(int side in new[]{-1,1})
        {
            var strip=Quad("Damp Edge",detailRoot,new Vector3(side*2.83f,.013f,8),new Vector2(.4f,13),AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/Weathering/Dirt.mat"));strip.localRotation=Quaternion.Euler(90,0,0);
        }
        // Give one existing service recess a purpose rather than changing its opening.
        var cabinet=Group("Service Cabinet",detailRoot);cabinet.position=new Vector3(3.48f,.05f,8.15f);cabinet.rotation=Quaternion.Euler(0,90,0);
        Box("Cabinet",cabinet,new Vector3(0,.8f,0),new Vector3(.7f,1.6f,.38f),materials["Service Plastic"]);
        Box("Door Panel",cabinet,new Vector3(0,.8f,-.205f),new Vector3(.62f,1.48f,.035f),materials["Painted Shutter"]);
        Box("Latch",cabinet,new Vector3(.22f,.82f,-.24f),new Vector3(.025f,.13f,.025f),materials["Old Steel"]);
        for(int i=0;i<5;i++)Box("Cabinet Vent",cabinet,new Vector3(0,.35f+i*.035f,-.23f),new Vector3(.4f,.012f,.015f),materials["Window Rubber"]);
        foreach(var mat in materials.Values)EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        File.WriteAllText("References/Analysis/SurfacePass.txt","Hero section: z=0–16 m. Existing box meshes beveled: "+beveled+". Sources: Assets/Alley/Textures/PBR/Sources.json. Nine reusable weathering prefab assets created. No main building or road transform changed.");
    }
}
