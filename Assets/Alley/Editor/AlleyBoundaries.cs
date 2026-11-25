using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AlleyBoundaries
{
    static Transform root;
    static Material concrete,plaster,metal,road;
    static Transform Group(string name,Transform parent) {var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material mat)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.position=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;g.isStatic=true;return g;
    }
    static Material Load(string name){return AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/"+name+".mat");}
    [MenuItem("Alley/Revise Boundaries and Background")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Exit Play Mode first.");
        root=GameObject.Find("Alley").transform;
        if(GameObject.Find("Street Continuation")!=null)throw new System.Exception("Boundary revision is already present.");
        var group=Group("Street Continuation",root);
        concrete=Load("Cast concrete");plaster=Load("Warm mineral plaster");metal=Load("Oxidized steel");road=Load("Damp asphalt");
        // A continuous ground apron reaches under every frontage and passage.
        Box("Ground",group,new Vector3(0,-.27f,26),new Vector3(80,.25f,105),road);
        var buildings=GameObject.Find("Buildings").transform;
        foreach(Transform building in buildings)
        {
            if(building.name!="Townhouse" && building.name!="Apartment Building")continue;
            var facade=building.Find("Facade Volume");if(facade==null)continue;
            float w=facade.localScale.x;float h=facade.localScale.y;
            float side=building.position.x<0?-1:1;
            // Short setbacks remain legible as service slots, never open holes to the skybox.
            Box("Service Wall",group,new Vector3(building.position.x+side*3.1f,h*.5f,building.position.z),new Vector3(.35f,h,9),concrete);
            Box("Foundation",group,new Vector3(building.position.x,-.13f,building.position.z),new Vector3(8,.25f,w+1.2f),concrete);
        }
        // Neighborhood masses sit behind the close architecture and seal diagonal sightlines.
        for(int side=-1;side<=1;side+=2)
        {
            for(int i=0;i<6;i++)Box("Neighboring Building",group,new Vector3(side*(13+i%2),4.3f+(i%3)*.65f,-2+i*12),new Vector3(8,8.6f+(i%3)*1.3f,13),i%2==0?concrete:plaster);
            Box("Boundary Wall",group,new Vector3(side*18,2.5f,26),new Vector3(.5f,5,86),concrete);
        }
        // The east opening becomes a walkable dogleg service alley, with a screened end.
        var lane=Group("Service Alley",group);
        Box("Paving",lane,new Vector3(8,-.055f,25),new Vector3(10,.12f,2.1f),road);
        Box("North Wall",lane,new Vector3(8,2.1f,26.15f),new Vector3(10,4.2f,.3f),plaster);
        Box("South Wall",lane,new Vector3(8,1.8f,23.85f),new Vector3(10,3.6f,.3f),concrete);
        Box("Corner Wall",lane,new Vector3(12.8f,2.2f,25),new Vector3(.4f,4.4f,3),concrete);
        Box("End Building",lane,new Vector3(15,4.1f,25),new Vector3(4,8.2f,8),plaster);
        var gate=GameObject.Find("Passage Gate");if(gate!=null) {gate.transform.position=new Vector3(8.7f,1.2f,25);gate.transform.localScale=new Vector3(.15f,2.4f,2);gate.name="Service Gate";}
        for(int i=0;i<12;i++)Box("Gate Slat",lane,new Vector3(8.58f,.15f+i*.19f,25),new Vector3(.06f,.075f,1.9f),metal);
        var fixture=Group("Service Light",lane);fixture.position=new Vector3(7.3f,2.5f,25);var light=fixture.gameObject.AddComponent<Light>();light.type=LightType.Point;light.range=5;light.intensity=1.1f;light.color=new Color(.75f,.8f,1);
        Box("Bulkhead Light",lane,new Vector3(7.2f,2.65f,26),new Vector3(.4f,.12f,.18f),Load("Vending light emission"));
        Box("Drain Channel",lane,new Vector3(8,.013f,25.87f),new Vector3(9,.03f,.17f),metal);
        // Add walls behind either end, rather than letting the floor stop against the sky.
        Box("South Neighbor",group,new Vector3(4,4,-7),new Vector3(16,8,7),plaster);
        Box("South Garden Wall",group,new Vector3(0,1.25f,-2.25f),new Vector3(8,2.5f,.35f),concrete);
        Box("North Neighbor",group,new Vector3(0,5,61),new Vector3(29,10,8),concrete);
        Box("North Return Wall",group,new Vector3(0,1.8f,56.3f),new Vector3(25,3.6f,.35f),plaster);
        var importer=AssetImporter.GetAtPath("Assets/Alley/Textures/Tokyo at Night.jpg") as TextureImporter;
        importer.maxTextureSize=8192;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        var sky=new Material(Shader.Find("Alley/Tokyo Night"));sky.name="Tokyo Night";sky.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/Tokyo at Night.jpg"));
        sky.SetVector("_Crop",new Vector4(.839f,.79f,.032f,.21f));sky.SetFloat("_Bearing",0);sky.SetFloat("_Elevation",25);sky.SetVector("_AngularSize",new Vector4(28,43,0,0));sky.SetFloat("_PhotoExposure",.7f);
        AssetDatabase.CreateAsset(sky,"Assets/Alley/Materials/Tokyo Night.mat");RenderSettings.skybox=sky;
        DynamicGI.UpdateEnvironment();
        var views=GameObject.Find("Viewpoints").transform;
        AddView(views,"Alley and Tower",new Vector3(.2f,1.7f,6),new Vector3(-8,0,0));
        AddView(views,"Side Alley",new Vector3(.4f,1.7f,25),new Vector3(0,90,0));
        AddView(views,"Roofline and Tower",new Vector3(.3f,1.7f,17),new Vector3(-18,0,0));
        AddView(views,"Street Return",new Vector3(.2f,1.7f,34),new Vector3(0,180,0));
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
    static void AddView(Transform parent,string name,Vector3 position,Vector3 rotation){var g=Group(name,parent);g.position=position;g.rotation=Quaternion.Euler(rotation);}
    public static void Capture()
    {
        var p=Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var cam=p.eyes;
        var pos=cam.transform.position;var rot=cam.transform.rotation;var oldTarget=cam.targetTexture;var oldActive=RenderTexture.active;
        Directory.CreateDirectory("References/Captures/Revision");
        try {foreach(Transform v in GameObject.Find("Viewpoints").transform)
        {
            var rt=new RenderTexture(1600,1000,24);var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);
            try {cam.transform.SetPositionAndRotation(v.position,v.rotation);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes("References/Captures/Revision/"+v.name+".png",image.EncodeToPNG());}
            finally {cam.targetTexture=oldTarget;RenderTexture.active=oldActive;Object.DestroyImmediate(image);rt.Release();Object.DestroyImmediate(rt);}
        }} finally {cam.transform.SetPositionAndRotation(pos,rot);cam.targetTexture=oldTarget;RenderTexture.active=oldActive;}
    }
}
