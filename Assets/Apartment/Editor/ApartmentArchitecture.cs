using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    static void Wall(Transform p,Vector3 at,float length,float height,bool alongZ=false,float bottom=0)
    {
        float tile=Mathf.Min(1.22f-bottom,height);
        if(tile>0)B("Tiled Wall",p,at+Vector3.up*(bottom+tile*.5f),alongZ?new Vector3(.18f,tile,length):new Vector3(length,tile,.18f),"Green Tile");
        float start=Mathf.Max(bottom,1.22f);float rest=bottom+height-start;
        if(rest>0)B("Plaster Wall",p,at+Vector3.up*(start+rest*.5f),alongZ?new Vector3(.18f,rest,length):new Vector3(length,rest,.18f),"Yellowed Plaster");
        if(bottom<.15f)B("Skirting",p,at+Vector3.up*.065f,alongZ?new Vector3(.195f,.13f,length):new Vector3(length,.13f,.195f),"Concrete");
    }
    static void PipeRun(Transform room,Vector3 a,Vector3 b,float radius=.025f)
    {
        Rod("Service Pipe",room,a,b,radius,"Green Steel");float length=Vector3.Distance(a,b);int n=Mathf.Max(1,Mathf.FloorToInt(length/1.3f));
        for(int i=0;i<=n;i++){var at=Vector3.Lerp(a,b,i/(float)n);var dir=(b-a).normalized;Rod("Pipe Coupling",room,at-dir*.024f,at+dir*.024f,radius*1.28f,"Stainless Steel");}
    }
    static Transform Gate(Transform room,Vector3 at,float width,float yaw,bool open=false)
    {
        var root=G("Security Gate",room);root.localPosition=at;root.localRotation=Quaternion.Euler(0,yaw,0);
        foreach(float x in new[]{-width*.5f,width*.5f})B("Gate Frame",root,new Vector3(x,1.08f,0),new Vector3(.045f,2.16f,.06f),"Green Steel");
        B("Gate Header",root,new Vector3(0,2.17f,0),new Vector3(width+.08f,.05f,.08f),"Stainless Steel");
        float gap=open?.85f:0;int count=open?3:10;
        for(int i=0;i<count;i++)
        {
            float left=-width*.5f+i*(width-gap)/count,right=-width*.5f+(i+1)*(width-gap)/count;
            Rod("Gate Stile",root,new Vector3(left,.05f,0),new Vector3(left,2.13f,0),.011f,"Green Steel");
            for(int row=0;row<4;row++){float y=.07f+row*.5f;Rod("Folding Brace",root,new Vector3(left,y,-.018f),new Vector3(right,y+.5f,-.018f),.009f,"Green Steel");Rod("Folding Brace",root,new Vector3(right,y,-.035f),new Vector3(left,y+.5f,-.035f),.009f,"Green Steel");}
        }
        B("Gate Lock",root,new Vector3(open?-width*.5f+.22f:0,1.04f,-.06f),new Vector3(.06f,.11f,.03f),"Stainless Steel",false);return root;
    }
    static Transform Door(Transform room,Vector3 at,float yaw,string number)
    {
        var root=G("Apartment Door",room);root.localPosition=at;root.localRotation=Quaternion.Euler(0,yaw,0);
        B("Door Leaf",root,new Vector3(0,1.04f,.05f),new Vector3(.86f,2.08f,.055f),"Old Wood");
        foreach(float x in new[]{-.47f,.47f})B("Door Jamb",root,new Vector3(x,1.08f,.08f),new Vector3(.065f,2.16f,.12f),"Green Steel");
        B("Door Header",root,new Vector3(0,2.18f,.08f),new Vector3(1,.08f,.12f),"Green Steel");
        Rod("Door Handle",root,new Vector3(.31f,.94f,-.005f),new Vector3(.31f,.94f,-.045f),.024f,"Brass");
        B("Letter Slot",root,new Vector3(0,.4f,.012f),new Vector3(.27f,.045f,.018f),"Brass",false);
        B("Number Plate",root,new Vector3(0,1.8f,.012f),new Vector3(.17f,.07f,.015f),"Brass",false);
        Text(number,root,new Vector3(0,1.8f,0),.07f,Quaternion.identity,new Color(.07f,.06f,.035f));
        Gate(root,new Vector3(0,0,-.12f),1.04f,0);
        B("Doorbell",root,new Vector3(.62f,1.36f,.02f),new Vector3(.045f,.085f,.035f),"Aged Plastic",false);
        return root;
    }
    static void Mailboxes(Transform room)
    {
        var root=G("Mailboxes",room);root.localPosition=new Vector3(1.96f,.55f,3.2f);root.localRotation=Quaternion.Euler(0,90,0);
        B("Mailbox Housing",root,new Vector3(0,.65f,.1f),new Vector3(2.45f,1.4f,.22f),"Green Steel");
        for(int row=0;row<3;row++)for(int col=0;col<6;col++)
        {float x=(col-2.5f)*.395f,y=.21f+row*.44f;var box=G("Mailbox",root);box.localPosition=new Vector3(x,y,-.035f);B("Mailbox Door",box,Vector3.zero,new Vector3(.374f,.415f,.032f),"Stainless Steel",false);B("Letter Opening",box,new Vector3(0,.105f,-.019f),new Vector3(.29f,.025f,.006f),"Black Rubber",false);B("Label Holder",box,new Vector3(0,.015f,-.021f),new Vector3(.17f,.065f,.012f),"Brass",false);Text((row+1).ToString()+(col+1).ToString("00"),box,new Vector3(0,.014f,-.029f),.045f,Quaternion.identity,new Color(.7f,.68f,.55f));Rod("Lock",box,new Vector3(.12f,-.115f,-.02f),new Vector3(.12f,-.115f,-.03f),.016f,"Brass");}
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base+"Prefabs/Mailboxes.prefab");
    }
    static void Lobby(Transform floor)
    {
        var lobby=G("Lobby",floor);
        B("Terrazzo Floor",lobby,new Vector3(.6f,-.12f,6.25f),new Vector3(5.2f,.24f,12.5f),"Terrazzo");
        B("Ceiling",lobby,new Vector3(.6f,2.88f,6.25f),new Vector3(5.4f,.2f,12.7f),"Concrete");
        Wall(lobby,new Vector3(2.09f,0,4.55f),9.1f,2.78f,true);
        Wall(lobby,new Vector3(3.29f,0,9.56f),.9f,2.78f,true);Wall(lobby,new Vector3(3.29f,0,12.1f),.8f,2.78f,true);Wall(lobby,new Vector3(3.29f,0,10.8f),1.65f,.54f,true,2.24f);
        Wall(lobby,new Vector3(2.69f,0,9.1f),1.02f,2.78f);
        Wall(lobby,new Vector3(.6f,0,12.59f),5.56f,2.78f);
        // Reception wall is framed around a real opening, with a separate office doorway.
        Wall(lobby,new Vector3(-2.09f,0,.75f),1.5f,2.78f,true);Wall(lobby,new Vector3(-2.09f,0,3.25f),3.4f,.9f,true);Wall(lobby,new Vector3(-2.09f,0,3.25f),3.4f,.48f,true,2.3f);
        Wall(lobby,new Vector3(-2.09f,0,5.12f),.36f,2.78f,true);Wall(lobby,new Vector3(-2.09f,0,6.95f),1.34f,2.78f,true);Wall(lobby,new Vector3(-2.09f,0,5.77f),1.0f,.5f,true,2.28f);
        Wall(lobby,new Vector3(-2.09f,0,8.35f),1.3f,2.78f,true);Wall(lobby,new Vector3(-2.09f,0,11.5f),2.1f,2.78f,true);
        for(int i=0;i<4;i++)B("Reception Mullion",lobby,new Vector3(-2.075f,1.6f,1.55f+i*1.12f),new Vector3(.095f,1.42f,.035f),"Stainless Steel",false);
        foreach(float y in new[]{.92f,2.28f})B("Reception Frame",lobby,new Vector3(-2.075f,y,3.25f),new Vector3(.12f,.055f,3.48f),"Stainless Steel",false);
        Q("Reception Glass",lobby,new Vector3(-2.075f,1.6f,3.25f),new Vector2(3.35f,1.3f),M("Dirty Glass"),Quaternion.Euler(0,-90,0));
        B("Reception Sill",lobby,new Vector3(-2.01f,.91f,3.25f),new Vector3(.36f,.075f,3.58f),"Stainless Steel");
        B("Office Door Jamb",lobby,new Vector3(-2.085f,1.1f,5.27f),new Vector3(.15f,2.2f,.065f),"Green Steel");B("Office Door Jamb",lobby,new Vector3(-2.085f,1.1f,6.27f),new Vector3(.15f,2.2f,.065f),"Green Steel");
        // Glass door seals the scene boundary; interaction changes scenes.
        Wall(lobby,new Vector3(-1.6f,0,-.09f),.8f,2.78f);Wall(lobby,new Vector3(1.6f,0,-.09f),.8f,2.78f);Wall(lobby,new Vector3(0,0,-.09f),2.4f,.46f,false,2.32f);
        var entrance=B("Entrance Door",lobby,new Vector3(0,1.12f,0),new Vector3(2.3f,2.24f,.06f),"Dirty Glass");entrance.localRotation=Quaternion.identity;
        var travel=entrance.gameObject.AddComponent<SceneEntrance>();travel.destination="Alley";travel.arrivalPosition=new Vector3(.35f,.12f,51.4f);travel.arrivalRotation=new Vector3(0,180,0);travel.prompt="Return to the alley";
        foreach(float x in new[]{-1.16f,0,1.16f})B("Entrance Stile",lobby,new Vector3(x,1.15f,.025f),new Vector3(.04f,2.3f,.06f),"Stainless Steel");
        Gate(lobby,new Vector3(0,0,.10f),2.36f,180,true);
        B("Entrance Mat",lobby,new Vector3(0,.012f,.9f),new Vector3(1.7f,.024f,.75f),"Black Rubber",false);
        Mailboxes(lobby);
        var bench=G("Bench",lobby);bench.localPosition=new Vector3(1.57f,0,5.35f);bench.localRotation=Quaternion.Euler(0,90,0);
        for(int i=0;i<4;i++)B("Bench Slat",bench,new Vector3(0,.42f,(i-1.5f)*.09f),new Vector3(1.65f,.045f,.075f),"Old Wood",false);
        foreach(float x in new[]{-.62f,.62f})foreach(float z in new[]{-.11f,.11f})Rod("Bench Leg",bench,new Vector3(x,.02f,z),new Vector3(x,.40f,z),.02f,"Green Steel");
        Notice("Building Notices",lobby,new Vector3(1.988f,1.55f,7.5f),new Vector2(1.3f,.85f),Quaternion.Euler(0,90,0));
        B("Notice Board Frame",lobby,new Vector3(2.005f,1.55f,7.5f),new Vector3(.045f,.91f,1.36f),"Old Wood",false);
        Prop("Folded Umbrella",lobby,new Vector3(-1.65f,.02f,1.0f),25,.85f,false);Prop("Folded Umbrella",lobby,new Vector3(-1.51f,.02f,1.04f),-10,.83f,false);
        B("Umbrella Basket",lobby,new Vector3(-1.62f,.22f,1.03f),new Vector3(.38f,.44f,.31f),"Green Steel",false);
        Prop("Potted Foliage",lobby,new Vector3(1.54f,.01f,1.05f),15,.65f,false);
        Prop("Security Camera",lobby,new Vector3(-1.8f,2.48f,7.3f),135,.23f,false);
        Prop("Fire Extinguisher",lobby,new Vector3(1.76f,.18f,8.65f),-90,.65f);
        for(int i=0;i<3;i++){PipeRun(lobby,new Vector3(1.82f-i*.12f,2.58f,.25f),new Vector3(1.82f-i*.12f,2.58f,12.4f),.025f+i*.006f);}
        PipeRun(lobby,new Vector3(1.82f,.12f,8.4f),new Vector3(1.82f,2.58f,8.4f),.036f);
        foreach(float z in new[]{2.9f,6.8f,10.8f})Fluorescent(lobby,new Vector3(.15f,2.77f,z),z<9?7:6);
        for(int i=0;i<5;i++){Weather(lobby,"Water Stain",new Vector3(1.993f,1.93f,1.5f+i*2.1f),new Vector2(.8f,1.3f),Quaternion.Euler(0,90,0));Weather(lobby,"Peeling Paint",new Vector3(1.99f,1.85f,2+i*2.1f),new Vector2(.8f,1.2f),Quaternion.Euler(0,90,0));}
        Weather(lobby,"Mold",new Vector3(-1.992f,.25f,7.5f),new Vector2(1.1f,.4f),Quaternion.Euler(0,-90,0));
        Maintenance(floor);Office(floor);Elevator(lobby,0);Stairs(floor,0);
    }
    static void Maintenance(Transform floor)
    {
        var room=G("Electrical Room",floor);B("Floor",room,new Vector3(-4,-.1f,7.5f),new Vector3(3.8f,.2f,1.2f),"Concrete");B("Ceiling",room,new Vector3(-4,2.88f,7.5f),new Vector3(3.8f,.2f,1.2f),"Concrete");Wall(room,new Vector3(-5.99f,0,7.5f),1.4f,2.78f,true);Wall(room,new Vector3(-4,0,8.19f),4,2.78f);Door(room,new Vector3(-2.02f,0,7.0f),-90,"STORE");Prop("Cleaning Bucket",room,new Vector3(-3.4f,.01f,7.5f),0,.34f,false);Prop("Broom",room,new Vector3(-5.65f,.01f,7.6f),0,1.3f,false);
    }
    static void Stairs(Transform floor,int level)
    {
        var room=G("Stairwell",floor);float y=level*2.9f;
        B("Floor Landing",room,new Vector3(-3.8f,y-.10f,9.25f),new Vector3(3.05f,.2f,1.5f),"Concrete");
        Wall(room,new Vector3(-5.41f,y,11.2f),5.4f,2.9f,true);Wall(room,new Vector3(-3.8f,y,13.98f),3.4f,2.9f);
        Wall(room,new Vector3(-2.21f,y,12.0f),4,2.9f,true);Wall(room,new Vector3(-3.8f,y,8.39f),3.4f,2.9f);
        B("Barred Window",room,new Vector3(-3.8f,y+2.08f,13.875f),new Vector3(1.35f,.85f,.05f),"Black Rubber",false);
        for(int i=0;i<7;i++)Rod("Window Bar",room,new Vector3(-4.4f+i*.2f,y+1.69f,13.83f),new Vector3(-4.4f+i*.2f,y+2.47f,13.83f),.009f,"Green Steel");
        Text((level+1).ToString(),room,new Vector3(-5.308f,y+1.9f,10.5f),.65f,Quaternion.Euler(0,-90,0),new Color(.38f,.11f,.05f));
        if(level<5)
        {
            const float rise=2.9f/18,going=.26f;
            var flight=G("Stair Flight",room);
            for(int i=0;i<9;i++)
            {float top=(i+1)*rise;B("Concrete Tread",flight,new Vector3(-4.66f,y+top-.09f,10+(i+.5f)*going),new Vector3(1.25f,.18f,going+.005f),"Concrete");B("Step Nosing",flight,new Vector3(-4.66f,y+top-.009f,10+i*going+.018f),new Vector3(1.24f,.018f,.035f),"Stainless Steel",false);float top2=1.45f+top;B("Concrete Tread",flight,new Vector3(-2.92f,y+top2-.09f,12.34f-(i+.5f)*going),new Vector3(1.25f,.18f,going+.005f),"Concrete");}
            B("Intermediate Landing",room,new Vector3(-3.8f,y+1.35f,12.99f),new Vector3(3.05f,.2f,1.3f),"Concrete");
            foreach(bool reverse in new[]{false,true})
            {float x=reverse?-3.59f:-3.99f;Vector3 a=new Vector3(x,y+(reverse?2.9f:0)+.94f,10),b=new Vector3(x,y+1.45f+.94f,12.34f);Rod("Handrail",room,a,b,.027f,"Green Steel");for(int i=0;i<=9;i++){float t=i/9f;var at=Vector3.Lerp(a,b,t);Rod("Railing Spindle",room,at-Vector3.up*.89f,at,.011f,"Green Steel");}var guard=B("Railing Collision",room,(a+b)*.5f-Vector3.up*.45f,new Vector3(.08f,.9f,2.4f),"Black Rubber");guard.GetComponent<Renderer>().enabled=false;}
            var slab=B("Stair Soffit",room,new Vector3(-4.66f,y+.58f,11.17f),new Vector3(1.25f,.14f,2.75f),"Concrete",false);slab.localRotation=Quaternion.Euler(-31.8f,0,0);
            slab=B("Stair Soffit",room,new Vector3(-2.92f,y+2.03f,11.17f),new Vector3(1.25f,.14f,2.75f),"Concrete",false);slab.localRotation=Quaternion.Euler(31.8f,0,0);
        }
        else B("Stairwell Ceiling",room,new Vector3(-3.8f,y+2.83f,11.2f),new Vector3(3.4f,.16f,5.5f),"Concrete");
        Fluorescent(room,new Vector3(-3.9f,y+2.73f,13.35f),3.6f,4400);
        PipeRun(room,new Vector3(-5.28f,y+.1f,13.1f),new Vector3(-5.28f,y+2.9f,13.1f),.035f);
        Weather(room,"Water Stain",new Vector3(-5.307f,y+1.7f,11.6f),new Vector2(1.4f,2.0f),Quaternion.Euler(0,-90,0));
    }
    static void Residential(Transform floor,int level)
    {
        var room=G("Residential Corridor",floor);float y=level*2.9f;
        B("Corridor Floor",room,new Vector3(0,y-.12f,6.25f),new Vector3(4.2f,.24f,12.5f),"Terrazzo");B("Corridor Ceiling",room,new Vector3(0,y+2.79f,6.25f),new Vector3(4.2f,.22f,12.7f),"Concrete");
        Wall(room,new Vector3(0,y,-.09f),4.4f,2.68f);Wall(room,new Vector3(.55f,y,12.59f),5.5f,2.68f);
        foreach(int side in new[]{-1,1})
        {
            Wall(room,new Vector3(side*2.09f,y,4.5f),9,2.68f,true);
            if(side>0){Wall(room,new Vector3(3.29f,y,9.5f),.8f,2.68f,true);Wall(room,new Vector3(3.29f,y,12.15f),.7f,2.68f,true);Wall(room,new Vector3(2.7f,y,9.1f),1,2.68f);}
            else Wall(room,new Vector3(-2.09f,y,11.8f),1.5f,2.68f,true);
            for(int bay=0;bay<3;bay++)
            {
                float z=1.9f+bay*3.15f;var door=Door(room,new Vector3(side*1.84f,y,z),side>0?90:-90,(level+1).ToString()+(bay*2+(side>0?2:1)).ToString("00"));
                B("Doormat",room,new Vector3(side*1.35f,y+.012f,z),new Vector3(.5f,.024f,.76f),"Black Rubber",false);
                Prop("Entrance Intercom",room,new Vector3(side*1.95f,y+1.45f,z+.69f),side>0?90:-90,.15f,false);
                if((bay+level+side)%3==0)Prop("Delivery Carton",room,new Vector3(side*1.44f,y+.005f,z+.85f),15,.34f,false);
                if((bay+level)%2==0)Prop("Potted Fern",room,new Vector3(side*1.54f,y+.005f,z-.88f),bay*23,.48f,false);
                B("Meter Housing",room,new Vector3(side*1.95f,y+2.05f,z+.75f),new Vector3(.17f,.31f,.18f),"Aged Plastic",false);
                PipeRun(room,new Vector3(side*1.94f,y+2.1f,z+.75f),new Vector3(side*1.94f,y+2.54f,z+.75f),.013f);
            }
            PipeRun(room,new Vector3(side*1.87f,y+2.48f,.15f),new Vector3(side*1.87f,y+2.48f,12.35f),.035f);
        }
        foreach(float z in new[]{2.6f,6.5f,10.7f})Fluorescent(room,new Vector3(0,y+2.68f,z),5.5f,4400);
        Prop("Fire Extinguisher",room,new Vector3(1.7f,y+.16f,9.0f),-90,.65f);
        Text((level+1).ToString(),room,new Vector3(.15f,y+2.3f,12.48f),.4f,Quaternion.identity,new Color(.4f,.1f,.04f));
        Elevator(room,level);Stairs(floor,level);
        if(level==1)PrefabUtility.SaveAsPrefabAsset(room.gameObject,Base+"Prefabs/Residential Corridor.prefab");
    }
    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");if(File.Exists("Assets/Scenes/Apartment.unity"))throw new Exception("Apartment scene already exists; edit the saved scene rather than rebuilding.");
        var player=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();PrefabUtility.SaveAsPrefabAsset(player.gameObject,Base+"Prefabs/Player.prefab");
        var volume=UnityEngine.Object.FindFirstObjectByType<Volume>();var profile=volume!=null?UnityEngine.Object.Instantiate(volume.sharedProfile):ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile,Base+"Materials/Apartment Post Processing.asset");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var apartment=G("Apartment",null);
        for(int i=0;i<6;i++){var floor=G(i==0?"Ground Floor":"Floor "+(i+1),apartment);if(i==0)Lobby(floor);else Residential(floor,i);}
        var lightRoot=G("Lighting",apartment);var v=lightRoot.gameObject.AddComponent<Volume>();v.isGlobal=true;v.sharedProfile=profile;
        ColorAdjustments color;if(profile.TryGet<ColorAdjustments>(out color))color.postExposure.value=.35f;
        Bloom bloom;if(profile.TryGet<Bloom>(out bloom)){bloom.intensity.value=.10f;bloom.threshold.value=1.1f;}
        RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.055f,.065f,.062f);RenderSettings.ambientEquatorColor=new Color(.045f,.049f,.04f);RenderSettings.ambientGroundColor=new Color(.025f,.025f,.02f);RenderSettings.reflectionIntensity=.65f;RenderSettings.fog=false;
        var pg=G("Light Probes",lightRoot).gameObject.AddComponent<LightProbeGroup>();var points=new System.Collections.Generic.List<Vector3>();for(int floor=0;floor<6;floor++)foreach(float x in new[]{-3.9f,0,1.5f})foreach(float z in new[]{2.5f,6,10.5f})foreach(float y in new[]{.45f,1.7f})points.Add(new Vector3(x,y+floor*2.9f,z));pg.probePositions=points.ToArray();
        var settings=new LightingSettings();settings.name="Apartment Lighting";settings.bakedGI=true;settings.realtimeGI=false;settings.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;settings.lightmapResolution=6;settings.lightmapMaxSize=2048;settings.indirectResolution=2;settings.directSampleCount=32;settings.indirectSampleCount=64;settings.environmentSampleCount=32;settings.maxBounces=4;settings.mixedBakeMode=MixedLightingMode.IndirectOnly;AssetDatabase.CreateAsset(settings,Base+"Materials/Apartment Lighting.lighting");Lightmapping.lightingSettings=settings;
        var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/Player.prefab"));g.name="Player";g.transform.SetParent(apartment);g.transform.position=new Vector3(0,.05f,1.5f);g.transform.rotation=Quaternion.identity;g.GetComponent<AlleyStudy.AlleyWalker>().eyes.farClipPlane=80;
        var sp=G("Entrance Spawn",apartment);sp.position=g.transform.position;
        var view=G("Viewpoints",apartment);View(view,"Entering",new Vector3(0,1.65f,1.6f),new Vector3(-.3f,1.45f,7));View(view,"Entrance",new Vector3(.1f,1.65f,7.3f),new Vector3(0,1.5f,.1f));View(view,"Reception",new Vector3(.5f,1.65f,3.6f),new Vector3(-3.4f,1.3f,3.5f));View(view,"Management Room",new Vector3(-5.3f,1.65f,5.65f),new Vector3(-2.85f,1.3f,3.1f));View(view,"Elevator Lobby",new Vector3(-.5f,1.65f,8.7f),new Vector3(3.1f,1.5f,10.8f));View(view,"Elevator Cabin",new Vector3(3.7f,1.65f,10.8f),new Vector3(4.8f,1.3f,11.15f));View(view,"Stairwell",new Vector3(-3.8f,1.65f,9.1f),new Vector3(-4.5f,1.9f,12.8f));View(view,"Residential Corridor",new Vector3(0,4.55f,11.7f),new Vector3(0,4.4f,1));
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,"Assets/Scenes/Apartment.unity");return "Apartment scene created with ground floor, office, cabin, corridor modules and six connected stair landings.";
    }
    static void View(Transform parent,string name,Vector3 at,Vector3 target){var t=G(name,parent);t.position=at;t.rotation=Quaternion.LookRotation(target-at);}
}
