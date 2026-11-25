using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    const string BuildingBase="Assets/Building/";
    static readonly string[] CircuitNames={"Lobby","Security Room","Second Floor","Third Floor","Fourth Floor","Fifth Floor","Sixth Floor","Stairwell","Elevator","CCTV Cameras","CCTV Monitors","Entrance","Emergency","Maintenance"};
    static PowerCircuit[] BuildingCircuits()
    {
        foreach(string folder in new[]{"Circuits","Prefabs"})if(!AssetDatabase.IsValidFolder(BuildingBase+folder))AssetDatabase.CreateFolder("Assets/Building",folder);
        return CircuitNames.Select(name=>{string path=BuildingBase+"Circuits/"+name+".asset";var circuit=AssetDatabase.LoadAssetAtPath<PowerCircuit>(path);if(circuit==null){circuit=ScriptableObject.CreateInstance<PowerCircuit>();circuit.id=name;circuit.label=name;circuit.backupSupply=name=="Emergency";AssetDatabase.CreateAsset(circuit,path);}return circuit;}).ToArray();
    }
    static Material BuildingLabel(string name)
    {var mat=Plain(name+" Label",Color.white,.10f);mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(BuildingBase+"Textures/"+name+".png"));return mat;}
    static void ServicePlate(string label,Transform parent,Vector3 at,Vector2 size)
    {Q("Label",parent,at,size,BuildingLabel(label),Quaternion.identity);}
    static LightCircuit BindCircuit(Transform parent,PowerCircuit circuit,Transform exclude=null)
    {
        var receiver=parent.gameObject.AddComponent<LightCircuit>();receiver.circuit=circuit;
        receiver.lights=parent.GetComponentsInChildren<Light>(true).Where(l=>exclude==null||!l.transform.IsChildOf(exclude)).ToArray();
        receiver.emitters=parent.GetComponentsInChildren<Renderer>(true).Where(r=>(exclude==null||!r.transform.IsChildOf(exclude))&&r.sharedMaterials.Any(m=>m!=null&&m.HasProperty("_EmissionColor")&&m.GetColor("_EmissionColor").maxColorComponent>.001f)).ToArray();
        foreach(var light in receiver.lights)light.lightmapBakeType=LightmapBakeType.Realtime;return receiver;
    }
    static Transform EmergencyFixture(Transform parent,Vector3 position,PowerCircuit circuit)
    {
        var root=G("Emergency Light",parent);root.position=position;
        B("Bulkhead Housing",root,Vector3.zero,new Vector3(.18f,.085f,.055f),"Elevator Frame Metal",false,.014f);
        B("Frosted Lens",root,new Vector3(0,0,-.033f),new Vector3(.14f,.055f,.018f),"Backup Diffuser",false,.012f);
        Lamp(root,new Vector3(0,-.04f,-.11f),.24f,3.1f,4100,Quaternion.identity);BindCircuit(root,circuit);return root;
    }
    static CctvChannel InstallCamera(Transform registry,Transform mount,string name,int number,string location,Vector3 position,Vector3 target,PowerCircuit circuit,Transform existing=null)
    {
        var assembly=G(name,registry);assembly.SetPositionAndRotation(position,Quaternion.LookRotation(target-position));
        if(existing==null)Prop("Security Camera",assembly,Vector3.zero,0,.23f,false);
        Rod("Lens Bezel",assembly,new Vector3(0,.035f,.13f),new Vector3(0,.035f,.18f),.028f,"Elevator Frame Metal");
        Rod("Lens Glass",assembly,new Vector3(0,.035f,.18f),new Vector3(0,.035f,.184f),.021f,"Black Rubber");
        // Vertical ceiling hanger and a conduit route fixed to the supporting surface.
        bool car=registry.name=="Elevator Cabin";bool outside=number==1;
        var anchorWorld=outside?new Vector3(position.x,position.y+.17f,position.z+.295f):new Vector3(position.x,position.y+(car?.27f:number==2?.30f:.32f),position.z);
        var anchor=assembly.InverseTransformPoint(anchorWorld);
        Rod("Mounting Stem",assembly,new Vector3(0,.07f,-.04f),anchor,.011f,"Green Steel");
        var plate=B("Mounting Plate",assembly,anchor,new Vector3(.10f,.026f,.10f),"Green Steel",false,.003f);plate.rotation=outside?Quaternion.Euler(90,0,0):Quaternion.identity;
        Rod("Camera Cable",assembly,new Vector3(.035f,.06f,-.07f),anchor+Vector3.right*.025f,.0045f,"Black Rubber");
        var camera=G("Feed Camera",assembly).gameObject.AddComponent<Camera>();camera.transform.localPosition=new Vector3(0,.035f,.205f);camera.enabled=false;camera.fieldOfView=92;camera.nearClipPlane=.08f;camera.farClipPlane=number==1?45:number==8?4:18;camera.allowHDR=false;camera.allowMSAA=false;camera.cullingMask=~(1<<5);
        var data=camera.gameObject.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=false;data.renderShadows=false;
        var channel=assembly.gameObject.AddComponent<CctvChannel>();channel.location=location;channel.cameraNumber=number;channel.cameraSource=camera;channel.circuit=circuit;channel.label=AssetDatabase.LoadAssetAtPath<Texture2D>(BuildingBase+"Textures/"+location+" Label.png");return channel;
    }
    static void InstallMonitors(CctvSystem system,Transform room,PowerCircuit circuit)
    {
        var screens=room.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Surveillance Screen").OrderByDescending(r=>r.bounds.center.y).ThenBy(r=>r.bounds.center.z).ToArray();
        var material=Plain("Live CCTV",Color.white,.2f);material.shader=Shader.Find("Building/CCTV Screen");material.SetTexture("_NoSignalTex",AssetDatabase.LoadAssetAtPath<Texture2D>(BuildingBase+"Textures/No Signal.png"));material.SetFloat("_Gain",1.6f);material.SetFloat("_Power",1);material.SetFloat("_Signal",0);
        var monitors=new List<CctvMonitor>();int[] initial={0,1,2,7};
        for(int i=0;i<screens.Length;i++)
        {
            var screen=screens[i];var root=screen.GetComponentsInParent<Transform>().First(t=>t.name=="CRT Monitor");
            var monitor=root.gameObject.AddComponent<CctvMonitor>();monitor.monitorId="Desk Monitor "+(i+1);monitor.screen=screen;monitor.system=system;monitor.circuit=circuit;monitor.initialChannel=initial[i];screen.sharedMaterial=material;
            var b=LiftLocalBounds(screen,root);var collider=screen.gameObject.GetComponent<BoxCollider>();if(collider==null)collider=screen.gameObject.AddComponent<BoxCollider>();var sb=screen.GetComponent<MeshFilter>().sharedMesh.bounds;collider.center=sb.center;collider.size=new Vector3(sb.size.x,sb.size.y,Mathf.Max(sb.size.z,.006f));
            monitors.Add(monitor);
        }
        system.monitors=monitors.ToArray();
        var buttons=room.GetComponentsInChildren<Transform>().Where(t=>t.name=="Console Button").OrderBy(t=>t.position.z).Take(4).ToArray();
        for(int i=0;i<buttons.Length;i++){var t=buttons[i];var control=t.gameObject.AddComponent<CctvControl>();control.monitor=monitors[i];t.gameObject.AddComponent<BoxCollider>().size=t.GetComponent<MeshFilter>().sharedMesh.bounds.size;var label=Q("Monitor Label",t,new Vector3(.065f,.002f,0),new Vector2(.075f,.022f),BuildingLabel("Monitor "+(i+1)+" Plate"),Quaternion.Euler(90,0,0));}
    }
    static void CircuitSwitch(Transform panel,PowerCircuit circuit,Vector3 at,bool main=false)
    {
        var root=G(main?"Main Breaker":"Circuit Breaker",panel);root.localPosition=at;
        B("Breaker Body",root,Vector3.zero,new Vector3(.105f,.08f,.045f),"Aged Plastic",false,.005f);
        var lever=B("Breaker Lever",root,new Vector3(0,0,-.035f),new Vector3(.045f,.05f,.038f),main?"Red Enamel":"Black Rubber",false,.003f);
        var light=Rod("Power Lens",root,new Vector3(.037f,.026f,-.024f),new Vector3(.037f,.026f,-.028f),.0055f,"Power Indicator");
        var control=root.gameObject.AddComponent<PowerSwitch>();control.circuit=circuit;control.mainBreaker=main;control.lever=lever;control.lamp=light.GetComponent<Renderer>();
        var hit=root.gameObject.AddComponent<BoxCollider>();hit.size=new Vector3(.13f,.10f,.095f);hit.center=new Vector3(0,0,-.022f);
        ServicePlate((main?"Main Supply":circuit.label)+" Plate",root,new Vector3(.132f,0,-.026f),new Vector2(.145f,.037f));
    }
    static void VoltageMeter(Transform parent,Vector3 at,PowerCircuit circuit)
    {
        var root=G("Voltage Meter",parent);root.localPosition=at;
        Rod("Meter Case",root,Vector3.zero,new Vector3(0,0,-.055f),.105f,"Elevator Frame Metal");
        Q("Meter Dial",root,new Vector3(0,0,-.058f),new Vector2(.19f,.19f),BuildingLabel("Voltage Dial"),Quaternion.identity);
        var pivot=G("Needle Pivot",root);pivot.localPosition=new Vector3(0,-.014f,-.061f);
        Rod("Needle",pivot,Vector3.zero,new Vector3(0,.073f,0),.0014f,"Black Rubber");
        Rod("Needle Hub",root,new Vector3(0,-.014f,-.062f),new Vector3(0,-.014f,-.066f),.006f,"Black Rubber");
        var meter=root.gameObject.AddComponent<PowerMeter>();meter.circuit=circuit;meter.needle=pivot;
    }
    static Transform ElectricalRoom(Transform ground,PowerCircuit[] circuits)
    {
        var room=ground.Find("Electrical Room")??ground.Find("Maintenance Room");room.name="Electrical Room";
        var old=room.Find("Apartment Door");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var floor=room.Find("Floor");LiftResize(floor,new Vector3(-4,-.10f,7.465f),new Vector3(3.8f,.2f,1.27f));
        var lobby=ground.Find("Lobby");var aperture=new Bounds(new Vector3(-2.09f,1.096f,7.50f),new Vector3(.30f,2.233f,1.06f));
        foreach(var r in lobby.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&new[]{"Tiled Wall","Plaster Wall","Skirting"}.Contains(r.name)).ToArray())if(r.bounds.size.x<.3f&&r.bounds.Intersects(aperture))LiftCutRenderer(r,ground,new[]{aperture},"Door Return");
        var door=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/Stairwell Door.prefab"),room);
        PrefabUtility.UnpackPrefabInstance(door,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);door.name="Electrical Room Door";door.transform.SetPositionAndRotation(new Vector3(-2.09f,0,7.5f),Quaternion.Euler(0,-90,0));UnityEngine.Object.DestroyImmediate(door.GetComponent<LockedDoor>());
        var sign=door.transform.Find("Stairwell Plaque");foreach(var r in sign.GetComponentsInChildren<Renderer>())if(r.name=="Stairwell Plaque")r.sharedMaterial=BuildingLabel("Electrical Room Plate");
        var pivot=G("Door Pivot",door.transform);pivot.localPosition=new Vector3(-.473f,0,-.014f);
        foreach(var part in door.transform.Cast<Transform>().Where(t=>new[]{"Door Leaf","Kick Plate","Lock Plate","Handle Spindle","Lever Handle","Keyhole","Door Hinge","Door Closer","Closer Arm","Stairwell Plaque"}.Contains(t.name)).ToArray()){part.SetParent(pivot,true);foreach(var t in part.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);}
        var swing=door.AddComponent<MaintenanceDoor>();swing.leaf=pivot;
        room.Find("Cleaning Bucket").position=new Vector3(-5.48f,.01f,7.02f);room.Find("Broom").position=new Vector3(-5.65f,.01f,6.98f);
        var panel=G("Distribution Panel",room);panel.position=new Vector3(-4.34f,0,7.96f);
        B("Steel Cabinet",panel,new Vector3(0,1.35f,0),new Vector3(1.32f,1.67f,.22f),"Green Steel",true,.012f);
        B("Recessed Face",panel,new Vector3(0,1.35f,-.121f),new Vector3(1.20f,1.53f,.035f),"Green Steel",false,.007f);
        CircuitSwitch(panel,null,new Vector3(-.40f,2.0f,-.16f),true);
        for(int i=0;i<circuits.Length;i++){int col=i/7,row=i%7;CircuitSwitch(panel,circuits[i],new Vector3(-.40f+col*.61f,1.78f-row*.165f,-.16f));}
        foreach(float x in new[]{-.61f,.61f})foreach(float y in new[]{.57f,2.13f})Rod("Cover Screw",panel,new Vector3(x,y,-.12f),new Vector3(x,y,-.14f),.005f,"Brass");
        for(int i=0;i<5;i++){float x=-.4f+i*.20f;Rod("Incoming Conduit",panel,new Vector3(x,2.18f,.02f),new Vector3(x,2.65f,.02f),.016f,"Green Steel");B("Conduit Saddle",panel,new Vector3(x,2.45f,.015f),new Vector3(.06f,.025f,.05f),"Elevator Frame Metal",false);}
        var meters=G("Meter Cabinet",room);meters.position=new Vector3(-5.48f,0,7.98f);B("Meter Enclosure",meters,new Vector3(0,1.57f,0),new Vector3(.58f,1.02f,.19f),"Green Steel",true,.012f);
        VoltageMeter(meters,new Vector3(0,1.82f,-.10f),circuits[0]);VoltageMeter(meters,new Vector3(0,1.40f,-.10f),circuits[12]);
        ServicePlate("Electrical Warning",meters,new Vector3(0,.87f,-.101f),new Vector2(.23f,.29f));
        Rod("Meter Supply",meters,new Vector3(.19f,2.1f,.02f),new Vector3(.19f,2.65f,.02f),.018f,"Green Steel");
        Prop("Utility Cabinet",room,new Vector3(-5.62f,0,7.1f),90,.62f,false);
        var tools=G("Service Shelf",room);tools.position=new Vector3(-3.50f,0,8.01f);B("Shelf",tools,new Vector3(0,.65f,-.08f),new Vector3(.42f,.035f,.25f),"Old Wood",true);
        for(int i=0;i<3;i++){Rod("Ceramic Fuse",tools,new Vector3(-.13f+i*.09f,.68f,-.15f),new Vector3(-.13f+i*.09f,.75f,-.15f),.015f,"Aged Plastic");Rod("Fuse End",tools,new Vector3(-.13f+i*.09f,.75f,-.15f),new Vector3(-.13f+i*.09f,.77f,-.15f),.0155f,"Brass");}
        Rod("Screwdriver",tools,new Vector3(.13f,.67f,-.05f),new Vector3(.13f,.67f,-.20f),.004f,"Stainless Steel");Rod("Tool Handle",tools,new Vector3(.13f,.67f,-.20f),new Vector3(.13f,.67f,-.27f),.012f,"Red Enamel");
        Notice("Caretaker Notes",room,new Vector3(-3.40f,1.55f,8.095f),new Vector2(.28f,.36f),Quaternion.identity);
        Fluorescent(room,new Vector3(-4.0f,2.73f,7.40f),3.2f,4200);
        Weather(room,"Water Stain",new Vector3(-5.72f,1.85f,8.095f),new Vector2(.30f,.75f),Quaternion.identity);
        BindCircuit(room,circuits[13]);return room;
    }
    static void AssetsForServices()
    {
        var indicator=Plain("Power Indicator",new Color(.12f,.27f,.14f),.35f);indicator.EnableKeyword("_EMISSION");indicator.SetColor("_EmissionColor",new Color(.05f,.34f,.12f));
        var backup=Plain("Backup Diffuser",new Color(.55f,.57f,.48f),.25f);backup.EnableKeyword("_EMISSION");backup.SetColor("_EmissionColor",new Color(.45f,.48f,.37f));
        foreach(string id in AssetDatabase.FindAssets("t:Texture2D",new[]{BuildingBase+"Textures"})){var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(id));importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();}
    }
    public static string InstallBuildingServices()
    {
        EnsureApartment();if(GameObject.Find("Apartment").transform.Find("Building Services")!=null)return "Building services already installed.";
        Directory.CreateDirectory("References/Infrastructure");File.Copy("Assets/Scenes/Apartment.unity","References/Infrastructure/Apartment.before.unity",true);
        var circuits=BuildingCircuits();AssetsForServices();var apartment=GameObject.Find("Apartment").transform;var ground=apartment.Find("Ground Floor");var services=G("Building Services",apartment);
        var power=services.gameObject.AddComponent<BuildingPower>();power.circuits=circuits;
        var cctv=services.gameObject.AddComponent<CctvSystem>();var cameras=G("Cameras",services);var channels=new List<CctvChannel>();
        var existing=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Security Camera").ToArray();
        channels.Add(InstallCamera(cameras,cameras,"Entrance Camera",1,"Entrance",new Vector3(-1.5f,2.45f,-.32f),new Vector3(.2f,1.2f,-3.2f),circuits[9],existing.FirstOrDefault(t=>t.parent.name=="Aoba Exterior")));
        channels.Add(InstallCamera(cameras,cameras,"Lobby Camera",2,"Lobby",new Vector3(-1.8f,2.48f,7.3f),new Vector3(.2f,1.15f,2.0f),circuits[9],existing.FirstOrDefault(t=>t.parent.name=="Lobby")));
        for(int floor=2;floor<=6;floor++)channels.Add(InstallCamera(cameras,cameras,"Corridor Camera",floor+1,floor+"F Corridor",new Vector3(1.78f,(floor-1)*2.9f+2.36f,11.9f),new Vector3(-.2f,(floor-1)*2.9f+1.15f,4.5f),circuits[9]));
        var elevator=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();var cabin=elevator.cabin;elevator.powerCircuit=circuits[8];
        channels.Add(InstallCamera(cabin,cabin,"Cabin Camera",8,"Lift Car",cabin.TransformPoint(new Vector3(-.55f,2.0f,1.39f)),cabin.TransformPoint(new Vector3(0,.95f,.35f)),circuits[9]));cctv.channels=channels.ToArray();
        InstallMonitors(cctv,ground.Find("Management Room"),circuits[10]);
        BindCircuit(ground.Find("Lobby"),circuits[0],cabin);BindCircuit(ground.Find("Management Room"),circuits[1]);BindCircuit(cabin,circuits[8]);
        for(int floor=2;floor<=6;floor++)BindCircuit(apartment.Find("Floor "+floor+"/Residential Corridor"),circuits[floor]);
        foreach(var stair in apartment.Cast<Transform>().Where(t=>t.Find("Stairwell")!=null))BindCircuit(stair.Find("Stairwell"),circuits[7]);
        var electrical=ElectricalRoom(ground,circuits);var emergency=G("Emergency Lighting",services);
        for(int floor=0;floor<6;floor++)EmergencyFixture(emergency,new Vector3(1.7f,floor*2.9f+2.35f,12.46f),circuits[12]);
        EmergencyFixture(electrical,new Vector3(-3.1f,2.35f,8.06f),circuits[12]);EmergencyFixture(cabin,cabin.TransformPoint(new Vector3(.30f,2.12f,1.54f)),circuits[12]);
        foreach(var renderer in apartment.GetComponentsInChildren<MeshRenderer>()){renderer.lightmapIndex=-1;renderer.lightProbeUsage=LightProbeUsage.Off;}
        foreach(var t in cabin.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
        var clone=UnityEngine.Object.Instantiate(cabin.gameObject);clone.name="Elevator Cabin";clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/Elevator Cabin.prefab");UnityEngine.Object.DestroyImmediate(clone);
        // Shared facade lighting follows the same circuit in both scenes.
        string exteriorPath=Base+"Prefabs/Aoba Exterior.prefab";var exterior=PrefabUtility.LoadPrefabContents(exteriorPath);try{BindCircuit(exterior.transform,circuits[11]);PrefabUtility.SaveAsPrefabAsset(exterior,exteriorPath);}finally{PrefabUtility.UnloadPrefabContents(exterior);}
        SaveApartment();return "Installed eight live CCTV channels, four switchable CRTs, fourteen power circuits, physical breakers and electrical room, emergency lighting and elevator battery rescue.";
    }
}
