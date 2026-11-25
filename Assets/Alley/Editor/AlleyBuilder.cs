using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using AlleyStudy;

public static class AlleyBuilder
{
    const string Folder = "Assets/Alley/";
    static Dictionary<string,Material> mats = new Dictionary<string,Material>();
    static Material plaster, concrete, asphalt, wood, dark, metal, glass, warm, white, red, green, paper;
    static Transform architecture, detail, services;
    static System.Random random;
    static Vector3 V(float x,float y,float z) { return new Vector3(x,y,z); }
    static Transform Group(string name,Transform parent=null) { var g = new GameObject(AlleyNaming.Clean(name));g.transform.SetParent(parent,false);return g.transform; }
    static Texture2D Tex(string name) { return AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Textures/"+name); }
    static Material Mat(string name,Color color,float smooth=0,float metallic=0,string texture=null, float emission=0)
    {
        string path=Folder+"Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null) { m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path); }
        m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metallic);
        if(texture!=null) m.SetTexture("_BaseMap",Tex(texture));
        if(emission>0) { m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive; }
        return m;
    }
    static GameObject Box(string name,Transform parent,Vector3 p,Vector3 s,Material m,bool collision=true)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=AlleyNaming.Clean(name);g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=s;
        var r=g.GetComponent<Renderer>();r.sharedMaterial=m;g.isStatic=true;
        if(m==plaster || m==concrete || m==asphalt || m==wood)
        {
            var key=m.name+"_"+Mathf.RoundToInt(Mathf.Max(s.x,s.z)*10)+"_"+Mathf.RoundToInt(s.y*10);
            Material tiled;
            if(!mats.TryGetValue(key,out tiled)) {tiled=new Material(m);tiled.mainTextureScale=new Vector2(Mathf.Max(s.x,s.z)/2.8f,Mathf.Max(s.y,s.z)/2.8f);AssetDatabase.CreateAsset(tiled,Folder+"Materials/"+key+".mat");mats[key]=tiled;}
            r.sharedMaterial=tiled;
        }
        if(!collision) UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }
    static GameObject Rod(string name,Transform parent,Vector3 a,Vector3 b,float radius,Material m,bool collider=false)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=AlleyNaming.Clean(name);g.transform.SetParent(parent,false);g.transform.localPosition=(a+b)*.5f;g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);g.transform.localScale=V(radius*2,(b-a).magnitude*.5f,radius*2);g.GetComponent<Renderer>().sharedMaterial=m;g.isStatic=true;
        if(!collider) UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static void Lamp(Transform p,Vector3 at,Color c,float intensity,float range, bool shadows=false)
    {
        var g=Group("Practical light",p);g.localPosition=at;var l=g.gameObject.AddComponent<Light>();l.type=LightType.Point;l.color=c;l.intensity=intensity;l.range=range;l.shadows=shadows?LightShadows.Soft:LightShadows.None;
    }
    static void Sign(Transform p,string name,string texture,Vector3 at,Vector2 size)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name=AlleyNaming.Clean(name);g.transform.SetParent(p,false);g.transform.localPosition=at;g.transform.localScale=V(size.x,size.y,1);UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
        var m=Mat(name.Replace(" ","_")+"_sign",Color.white,.18f,0,texture,.12f);g.GetComponent<Renderer>().sharedMaterial=m;g.isStatic=true;
    }
    static void Cable(Transform parent,Vector3 a,Vector3 b,float sag,float thickness=0.014f)
    {
        Vector3 last=a;
        for(int i=1;i<=16;i++) {float t=i/16f;Vector3 next=Vector3.Lerp(a,b,t)-Vector3.up*(4*sag*t*(1-t));Rod("Overhead service cable",parent,last,next,thickness,dark);last=next;}
    }
    static void Window(Transform p,float x,float y,float z,float w,float h,bool lit)
    {
        Box("Window recess",p,V(x,y,z),V(w+.18f,h+.18f,.14f),dark,false);
        Box("Window Glass",p,V(x,y,z-.085f),V(w,h,.035f),lit?warm:glass,false);
        for(int j=-1;j<=1;j++)Box("Aluminium mullion",p,V(x+j*w*.5f,y,z-.13f),V(.035f,h+.08f,.04f),metal,false);
        Box("Window sill",p,V(x,y-h*.5f-.06f,z-.15f),V(w+.25f,.08f,.24f),metal,false);
        if(lit) for(int i=0;i<5;i++)Box("Curtain fold",p,V(x-w*.43f+i*w*.215f,y,z-.112f),V(.018f,h,.016f),paper,false);
    }
    static void Building(int index, bool left,float z,float width,int floors,Material facade)
    {
        float offset=(index%3==0?.35f:0);float front=(left?-1:1)*(3.3f+offset);
        var p=Group((left?"West / ":"East / ")+index+" — "+floors+" storeys",architecture);
        p.position=V(front+(left?-3:3),0,z);p.rotation=Quaternion.Euler(0,left?-90:90,0);
        float height=floors*2.8f;
        Box("Facade volume",p,V(0,height/2,0),V(width,height,6),facade);
        Box("Concrete plinth",p,V(0,.25f,-3.05f),V(width,.5f,.18f),concrete);
        Box("Roof parapet",p,V(0,height+.13f,0),V(width+.2f,.26f,6.25f),concrete);
        Box("Rain eave",p,V(0,2.82f,-3.24f),V(width+.12f,.12f,.75f),dark);
        for(int f=1;f<floors;f++)
        {
            Box("Floor division",p,V(0,f*2.8f,-3.1f),V(width,.09f,.18f),concrete,false);
            for(int w=0;w<2;w++)Window(p,(w==0?-1:1)*width*.25f,f*2.8f+1.3f,-3.08f,width*.32f,1.35f,(index+f+w)%3==0);
            if((index+f)%2==0)
            {
                Box("Balcony slab",p,V(0,f*2.8f+.03f,-3.5f),V(width*.85f,.14f,.95f),concrete);
                Rod("Balcony top rail",p,V(-width*.42f,f*2.8f+1.1f,-3.95f),V(width*.42f,f*2.8f+1.1f,-3.95f),.025f,metal);
                for(float x=-width*.4f;x<=width*.4f;x+=.23f)Rod("Balcony spindle",p,V(x,f*2.8f+.1f,-3.95f),V(x,f*2.8f+1.1f,-3.95f),.015f,metal);
            }
        }
        float dx=-width*.34f;
        Box("Recessed residential door",p,V(dx,1.08f,-3.11f),V(.92f,2.16f,.16f),wood);
        Box("Frosted door panel",p,V(dx,1.42f,-3.21f),V(.68f,1.08f,.025f),glass,false);
        Rod("Door pull",p,V(dx+.3f,.95f,-3.26f),V(dx+.3f,1.2f,-3.26f),.02f,metal);
        Box("Door step",p,V(dx,.07f,-3.43f),V(1.25f,.14f,.75f),concrete);
        if(index!=0)Window(p,width*.23f,1.65f,-3.1f,width*.34f,1.6f,index%4==1);
        float px=width*.46f;
        Rod("Rainwater downpipe",p,V(px,.15f,-3.17f),V(px,height-.2f,-3.17f),.046f,metal);
        for(float y=.65f;y<height;y+=1.7f)Box("Pipe strap",p,V(px,y,-3.18f),V(.14f,.06f,.13f),dark,false);
        AC(p,V(width*.28f,2.15f,-3.35f));
        Box("Electrical meter",p,V(-width*.47f,1.65f,-3.2f),V(.22f,.32f,.18f),metal,false);
        Rod("Meter conduit",p,V(-width*.47f,.15f,-3.19f),V(-width*.47f,2.8f,-3.19f),.015f,metal);
        if(index%3==1) {Box("Porch fixture",p,V(dx,2.5f,-3.28f),V(.35f,.14f,.2f),warm,false);Lamp(p,V(dx,2.42f,-3.65f),new Color(1,.72f,.43f),1.8f,4);}
    }
    static void AC(Transform p,Vector3 pos)
    {
        var g=Group("Outdoor AC condenser",p);g.localPosition=pos;
        Box("Weathered cabinet",g,Vector3.zero,V(.8f,.48f,.32f),paper,false);
        for(int i=0;i<10;i++)Box("Vent grille",g,V(-.17f,-.19f+i*.042f,-.17f),V(.42f,.013f,.012f),metal,false);
        Box("Service panel",g,V(.27f,0,-.17f),V(.18f,.4f,.02f),concrete,false);
        Rod("Coolant pipe",g,V(.4f,-.05f,.04f),V(.55f,-.5f,.04f),.028f,paper);
        for(int s=-1;s<=1;s+=2)Box("Support bracket",g,V(s*.28f,-.28f,0),V(.05f,.05f,.45f),metal,false);
    }
    static void Ring(Transform p,string name,Vector3 at,float radius,float tube,Material mat)
    {
        for(int i=0;i<32;i++) {float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Rod(name,p,at+V(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0),at+V(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius,0),tube,mat);}
    }
    static GameObject Bicycle()
    {
        var p=Group("City Bicycle");
        for(int s=-1;s<=1;s+=2){Ring(p,"Rubber tyre",V(s*.55f,.36f,0),.34f,.024f,dark);Ring(p,"Wheel rim",V(s*.55f,.36f,0),.31f,.009f,metal);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Rod("Spoke",p,V(s*.55f,.36f,0),V(s*.55f+Mathf.Cos(a)*.3f,.36f+Mathf.Sin(a)*.3f,0),.0025f,metal);}}
        Vector3 a1=V(-.55f,.36f,0),b1=V(-.18f,.83f,0),c1=V(.03f,.36f,0),d1=V(.4f,.85f,0),e1=V(.55f,.36f,0);
        Rod("Frame",p,a1,b1,.018f,green);Rod("Frame",p,b1,c1,.018f,green);Rod("Frame",p,c1,a1,.018f,green);Rod("Step-through tube",p,c1,d1,.02f,green);Rod("Top tube",p,b1,d1,.014f,green);Rod("Fork",p,d1,e1,.018f,metal);
        Rod("Handlebar stem",p,d1,V(.38f,1.07f,0),.015f,metal);Rod("Handlebar",p,V(.38f,1.07f,-.23f),V(.38f,1.07f,.23f),.016f,metal);
        Box("Saddle",p,V(-.2f,.94f,0),V(.26f,.06f,.17f),dark,false);Box("Rear rack",p,V(-.55f,.76f,0),V(.4f,.025f,.17f),metal,false);
        Box("Basket base",p,V(.65f,.85f,0),V(.34f,.025f,.32f),metal,false);
        for(int s=-1;s<=1;s+=2) {Rod("Basket rim",p,V(.49f,1.08f,s*.16f),V(.81f,1.08f,s*.16f),.009f,metal);for(int i=0;i<5;i++)Rod("Basket wire",p,V(.49f+i*.08f,.85f,s*.16f),V(.49f+i*.08f,1.08f,s*.16f),.004f,metal);}
        Box("Collision proxy",p,V(0,.5f,0),V(1.6f,1,.35f),dark).GetComponent<Renderer>().enabled=false;
        return p.gameObject;
    }
    static GameObject Vending()
    {
        var p=Group("Vending Machine");
        Box("Cabinet",p,V(0,.94f,0),V(1.05f,1.88f,.72f),red);
        Box("Lit display recess",p,V(-.08f,1.24f,-.369f),V(.8f,.98f,.025f),white,false);
        Color[] cols={new Color(.6f,.09f,.05f),new Color(.12f,.35f,.46f),new Color(.7f,.6f,.24f),new Color(.18f,.36f,.2f)};
        for(int row=0;row<3;row++)for(int col=0;col<6;col++)
        {var m=Mat("Drink_"+col,cols[col%4],.4f,.1f);Rod("Drink can",p,V(-.4f+col*.125f,.95f+row*.27f,-.41f),V(-.4f+col*.125f,1.09f+row*.27f,-.41f),.035f,m);Box("Selection button",p,V(-.4f+col*.125f,.9f+row*.27f,-.42f),V(.065f,.025f,.025f),warm,false);}
        Box("Coin panel",p,V(.42f,1.1f,-.38f),V(.13f,.43f,.04f),dark,false);
        Box("Retrieval slot",p,V(-.08f,.36f,-.38f),V(.72f,.18f,.04f),dark,false);
        Box("Display header",p,V(0,1.83f,-.39f),V(.93f,.08f,.03f),white,false);
        Lamp(p,V(0,1.1f,-.7f),new Color(.65f,.82f,1),2.2f,4);
        return p.gameObject;
    }
    static GameObject Bin()
    {
        var p=Group("Garbage Bin");Box("Bin body",p,V(0,.43f,0),V(.55f,.82f,.5f),green);Box("Hinged lid",p,V(0,.87f,0),V(.6f,.07f,.55f),dark);return p.gameObject;
    }
    static GameObject SavePrefab(GameObject source,string name)
    {
        var path=Folder+"Prefabs/"+name+".prefab";var prefab=PrefabUtility.SaveAsPrefabAsset(source,path);UnityEngine.Object.DestroyImmediate(source);return prefab;
    }
    static Transform Place(GameObject prefab,Transform parent,Vector3 pos,float angle)
    {
        var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localRotation=Quaternion.Euler(0,angle,0);return g.transform;
    }
    static void Pot(Transform p,Vector3 at)
    {
        Rod("Terracotta pot",p,at,at+Vector3.up*.3f,.15f,red);
        for(int i=0;i<6;i++){float a=i*1.05f;Rod("Plant Stem",p,at+V(0,.28f,0),at+V(Mathf.Cos(a)*.2f,.55f+(i%2)*.2f,Mathf.Sin(a)*.2f),.017f,green);var g=Box("Leaf",p,at+V(Mathf.Cos(a)*.17f,.52f,Mathf.Sin(a)*.17f),V(.08f,.22f,.035f),green,false);g.transform.localRotation=Quaternion.Euler(0,i*60,30);}
    }
    [MenuItem("Alley/Create initial Alley scene")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode before building.");
        if(File.Exists("Assets/Scenes/Alley.unity"))throw new Exception("Alley already exists; refusing to overwrite it.");
        var previous=SceneManager.GetActiveScene();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
        random=new System.Random(20);mats.Clear();
        AssetDatabase.Refresh();
        foreach(var path in Directory.GetFiles(Folder+"Textures","*nor_gl*.jpg")) {var importer=AssetImporter.GetAtPath(path.Replace('\\','/')) as TextureImporter;if(importer!=null){importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();}}
        plaster=Mat("Warm mineral plaster",new Color(.65f,.63f,.56f),.12f,0,"plastered_wall_02_diff_1k.jpg");
        plaster.SetTexture("_BumpMap",Tex("plastered_wall_02_nor_gl_1k.jpg"));plaster.EnableKeyword("_NORMALMAP");
        concrete=Mat("Cast concrete",new Color(.55f,.55f,.52f),.2f,0,"concrete.png");
        asphalt=Mat("Damp asphalt",new Color(.55f,.57f,.6f),.57f,0,"asphalt_02_diff_1k.jpg");asphalt.SetTexture("_BumpMap",Tex("asphalt_02_nor_gl_1k.jpg"));asphalt.SetFloat("_BumpScale",.45f);asphalt.EnableKeyword("_NORMALMAP");
        wood=Mat("Dark stained timber",new Color(.115f,.083f,.058f),.24f);dark=Mat("Charcoal rubber",new Color(.025f,.032f,.034f),.25f);
        metal=Mat("Oxidized steel",new Color(.19f,.22f,.23f),.46f,.6f);glass=Mat("Unlit window",new Color(.045f,.085f,.12f),.7f,.35f);
        warm=Mat("Warm practical emission",new Color(1,.68f,.32f),.1f,0,null,2.2f);white=Mat("Vending light emission",new Color(.72f,.85f,1),.15f,0,null,1.5f);
        red=Mat("Muted vermilion paint",new Color(.32f,.065f,.043f),.25f);green=Mat("Service green",new Color(.13f,.21f,.17f),.2f);paper=Mat("Aged ivory enamel",new Color(.59f,.59f,.5f),.25f);
        var root=Group("Alley");architecture=Group("Buildings",root);var street=Group("Street",root);detail=Group("Props",root);services=Group("Utilities",root);var lights=Group("Lighting",root);
        Box("Road",street,V(0,-.13f,26),V(7,.25f,56),asphalt);
        Box("Side passage floor",street,V(7,-.12f,25),V(12,.24f,1.6f),asphalt);
        for(int s=-1;s<=1;s+=2)
        {
            for(int j=0;j<54;j++) {Box("Precast gutter",street,V(s*3.02f,.015f,j),V(.36f,.07f,.97f),concrete);Box("Curb block",street,V(s*3.32f,.065f,j),V(.18f,.18f,.97f),concrete);}
            for(int j=0;j<13;j++)Box("Worn edge marking",street,V(s*2.68f,.006f,j*4+1),V(.055f,.009f,3.5f),paper,false);
        }
        for(int j=0;j<5;j++) {var cover=Rod("Manhole cover",street,V((j%2==0?.4f:-.6f),.004f,6+j*10),V((j%2==0?.4f:-.6f),.02f,6+j*10),.31f,metal);for(int k=0;k<7;k++)Box("Manhole grooves",street,V((j%2==0?.4f:-.6f)-.21f+k*.07f,.024f,6+j*10),V(.017f,.005f,.4f),dark,false);}
        for(int j=0;j<8;j++)
        {
            float z=3.4f+j*6.5f;
            Building(j,true,z,5.8f+(j%2)*.3f,2+j%3,j%3==1?concrete:plaster);
            if(j!=3)Building(j+10,false,z+1.5f,5.6f,2+(j+1)%3,j%2==0?plaster:concrete);
        }
        // The developed first corner: shop faces the road; all fittings are editable.
        var shop=Group("Minato Sushi",detail);shop.position=V(-3.25f,0,6.2f);shop.rotation=Quaternion.Euler(0,-90,0);
        Box("Timber shop frontage",shop,V(0,1.5f,0),V(5.7f,3,.18f),wood);
        Box("Closed aluminium shutter",shop,V(-.65f,1.2f,-.13f),V(3.2f,2.35f,.12f),metal);
        for(int j=0;j<56;j++)Box("Shutter slat",shop,V(-.65f,.06f+j*.042f,-.21f),V(3.18f,.016f,.018f),concrete,false);
        for(int s=-1;s<=1;s+=2)Box("Shutter guide",shop,V(-.65f+s*1.65f,1.2f,-.22f),V(.07f,2.5f,.07f),dark,false);
        Box("Shutter bottom handle",shop,V(-.65f,.28f,-.25f),V(.24f,.04f,.035f),dark,false);
        Window(shop,1.88f,1.45f,-.16f,1.05f,1.75f,true);
        for(int i=0;i<6;i++)Box("Timber lattice",shop,V(1.38f+i*.2f,1.45f,-.31f),V(.035f,1.85f,.045f),wood,false);
        Box("Shop sign backing",shop,V(0,2.72f,-.2f),V(5.6f,.52f,.18f),wood,false);Sign(shop,"Minato Sushi","sushi-sign.png",V(0,2.72f,-.301f),new Vector2(4.1f,.44f));
        Sign(shop,"Closed today","closed-sign.png",V(-.7f,1.35f,-.245f),new Vector2(.75f,.19f));
        var awning=Box("Deep metal awning",shop,V(0,3.12f,-.45f),V(5.9f,.09f,1.1f),dark);awning.transform.localRotation=Quaternion.Euler(-7,0,0);
        for(int i=0;i<30;i++)Box("Awning standing seam",shop,V(-2.9f+i*.2f,3.16f,-.45f),V(.015f,.025f,1.1f),metal,false);
        Rod("Lantern bracket",shop,V(2.5f,2.85f,-.2f),V(2.5f,2.85f,-.9f),.025f,metal);
        Rod("Paper lantern",shop,V(2.5f,1.95f,-.86f),V(2.5f,2.65f,-.86f),.23f,warm);
        for(int i=0;i<14;i++) {var ring=Rod("Lantern rib",shop,V(2.5f,1.95f+i*.05f,-.86f),V(2.5f,1.96f+i*.05f,-.86f),.237f,red);}
        Lamp(shop,V(2.5f,2.15f,-1.1f),new Color(1,.58f,.25f),3.6f,6,true);
        Lamp(shop,V(-1,2.8f,-.7f),new Color(1,.79f,.5f),2.1f,5);
        var vending=SavePrefab(Vending(),"Vending Machine");var bike=SavePrefab(Bicycle(),"City Bicycle");var bin=SavePrefab(Bin(),"Garbage Bin");
        Place(vending,detail,V(3.37f,0,12.5f),90);Place(vending,detail,V(3.37f,0,13.65f),90);
        Place(bike,detail,V(-2.83f,0,10.1f),82);Place(bike,detail,V(2.96f,0,18),95);Place(bike,detail,V(2.97f,0,19.4f),87);Place(bike,detail,V(-2.84f,0,36),79);
        for(int i=0;i<3;i++)Place(bin,detail,V(-2.92f,0,22+i*.7f),0);
        var garbage=Group("Garbage Collection",detail);garbage.position=V(-3.25f,0,23);garbage.rotation=Quaternion.Euler(0,-90,0);Sign(garbage,"Collection notice","notice.png",V(0,1.7f,-.08f),new Vector2(.9f,.3f));
        for(int i=0;i<5;i++){var bag=GameObject.CreatePrimitive(PrimitiveType.Sphere);bag.name="Garbage Bag";bag.transform.SetParent(detail,false);bag.transform.position=V(-2.8f+(i%2)*.38f,.19f,25+i*.27f);bag.transform.localScale=V(.45f,.4f,.5f);bag.GetComponent<Renderer>().sharedMaterial=dark;}
        for(int i=0;i<11;i++){Pot(detail,V((i%2==0?-2.95f:3.05f),.1f,3+i*4.4f));}
        for(int i=0;i<4;i++){Box("Cardboard delivery carton",detail,V(-2.9f,.2f+i*.3f,8.5f),V(.5f,.3f,.4f),paper);Box("Packing tape",detail,V(-2.9f,.36f+i*.3f,8.5f),V(.07f,.008f,.4f),concrete,false);}
        for(int s=-1;s<=1;s+=2)for(int j=0;j<3;j++)
        {
            float z=3+j*19+(s==1?7:0);float x=s*3.05f;
            Rod("Utility pole",services,V(x,0,z),V(x,9.8f,z),.11f,concrete,true);
            Box("Pole caution band",services,V(x,.75f,z),V(.23f,.38f,.23f),paper,false);
            Rod("Crossarm",services,V(x-.75f,8.9f,z),V(x+.75f,8.9f,z),.045f,metal);
            for(int k=-1;k<=1;k++) {Rod("Insulator",services,V(x+k*.6f,8.9f,z),V(x+k*.6f,9.15f,z),.055f,paper);if(j<2)Cable(services,V(x+k*.6f,9.15f,z),V(x+k*.6f,9.15f,z+19),.65f);}
            Rod("Streetlamp arm",services,V(x,5.4f,z),V(x-s*.8f,5.4f,z),.035f,metal);
            Box("Streetlamp diffuser",services,V(x-s*.8f,5.36f,z),V(.65f,.09f,.2f),white,false);Lamp(services,V(x-s*.8f,5.15f,z),new Color(.72f,.83f,1),4.5f,9,true);
            Cable(services,V(x,7.1f,z),V(-x,7.5f,z+4),.8f,.012f);
        }
        var apartment=Group("Aoba Apartments",architecture);apartment.position=V(.7f,0,54);
        Box("Apartment facade",apartment,V(0,5.6f,1.6f),V(12,11.2f,4),concrete);
        Box("Recessed entrance",apartment,V(0,1.35f,-.45f),V(2.5f,2.7f,.2f),dark);
        Box("Closed entry door",apartment,V(.1f,1.2f,-.58f),V(1.1f,2.4f,.08f),glass);
        Box("Entry canopy",apartment,V(0,3,-1),V(3.5f,.18f,1.8f),metal);Sign(apartment,"Aoba residence","apartment-sign.png",V(0,2.7f,-.65f),new Vector2(1.7f,.35f));Lamp(apartment,V(0,2.8f,-1.4f),new Color(1,.8f,.55f),3.5f,6);
        for(int f=1;f<4;f++)for(int j=-2;j<=2;j++)Window(apartment,j*2.2f,f*2.8f+1.2f,-.48f,1.35f,1.4f,(f+j)%3==0);
        Box("Passage Gate",street,V(12,.9f,25),V(.2f,1.8f,1.6f),dark);
        Box("Passage north wall",architecture,V(7.5f,1.4f,26),V(9,2.8f,.24f),plaster);
        Box("Passage south wall",architecture,V(7.5f,1.4f,24),V(9,2.8f,.24f),concrete);
        Box("Start boundary",street,V(0,.7f,-2.1f),V(7,1.4f,.2f),dark);
        var sky=Mat("Tokyo night panorama placeholder",Color.white);
        sky.shader=Shader.Find("Skybox/Panoramic");sky.SetTexture("_MainTex",Tex("TokyoNight_Placeholder_360.png"));sky.SetFloat("_Exposure",1.05f);sky.SetFloat("_Rotation",180);RenderSettings.skybox=sky;
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.12f,.18f,.29f);RenderSettings.ambientEquatorColor=new Color(.07f,.095f,.15f);RenderSettings.ambientGroundColor=new Color(.035f,.039f,.055f);RenderSettings.ambientIntensity=1;
        RenderSettings.fog=true;RenderSettings.fogColor=new Color(.047f,.071f,.105f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.008f;
        var moon=Group("Moonlight",lights);moon.rotation=Quaternion.Euler(48,-35,0);var ml=moon.gameObject.AddComponent<Light>();ml.type=LightType.Directional;ml.color=new Color(.48f,.61f,.88f);ml.intensity=.35f;ml.shadows=LightShadows.Soft;
        var volume=Group("Night Lighting",lights).gameObject.AddComponent<Volume>();volume.isGlobal=true;
        var profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Folder+"Materials/AlleyNightVolume.asset");volume.sharedProfile=profile;
        var bloom=profile.Add<Bloom>();bloom.intensity.Override(.22f);bloom.threshold.Override(1.2f);var tone=profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);var color=profile.Add<ColorAdjustments>();color.postExposure.Override(.65f);color.contrast.Override(10);color.saturation.Override(-9);
        var player=Group("Player",root);player.position=V(.2f,.15f,2);
        var capsule=player.gameObject.AddComponent<CharacterController>();capsule.height=1.78f;capsule.radius=.28f;capsule.center=V(0,.89f,0);capsule.stepOffset=.25f;
        var cam=Group("First Person Camera",player).gameObject.AddComponent<Camera>();cam.transform.localPosition=V(0,1.65f,0);cam.fieldOfView=67;cam.nearClipPlane=.06f;cam.farClipPlane=220;cam.tag="MainCamera";cam.allowHDR=true;cam.gameObject.AddComponent<AudioListener>();cam.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        var controller=player.gameObject.AddComponent<AlleyWalker>();controller.eyes=cam;
        var views=Group("Viewpoints",root);
        for(int i=0;i<4;i++){var view=Group(new[]{"Street Entrance","Sushi Shop","Vending Corner","Apartment Entrance"}[i],views);view.position=new[]{V(.2f,1.7f,2),V(1.5f,1.7f,10.3f),V(-1.7f,1.7f,16),V(.2f,1.7f,39)}[i];view.rotation=Quaternion.Euler(0,new[]{0f,-125f,150f,0f}[i],0);}
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,"Assets/Scenes/Alley.unity");
        if(previous.IsValid() && previous.path=="" && !previous.isDirty)EditorSceneManager.CloseScene(previous,true);
        Selection.activeGameObject=root.gameObject;
        if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.LookAt(V(0,1.65f,8),Quaternion.Euler(8,-25,0),14);
        Debug.Log("Alley study built and saved: 52m editable street, original prefabs, four review positions. No horror systems.");
    }
    [MenuItem("Alley/Capture review viewpoints")]
    public static void Capture()
    {
        var player=UnityEngine.Object.FindFirstObjectByType<AlleyWalker>();if(player==null)throw new Exception("Open Alley first.");
        var cam=player.eyes;var oldP=cam.transform.position;var oldR=cam.transform.rotation;var oldT=cam.targetTexture;
        Directory.CreateDirectory("References/Captures");
        try
        {
            foreach(Transform v in GameObject.Find("Viewpoints").transform)
            {
                var rt=new RenderTexture(1600,1000,24,RenderTextureFormat.ARGB32);cam.transform.SetPositionAndRotation(v.position,v.rotation);cam.targetTexture=rt;cam.Render();var active=RenderTexture.active;RenderTexture.active=rt;
                var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes("References/Captures/"+v.name+".png",image.EncodeToPNG());RenderTexture.active=active;cam.targetTexture=oldT;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
        finally {cam.transform.SetPositionAndRotation(oldP,oldR);cam.targetTexture=oldT;}
    }
}
