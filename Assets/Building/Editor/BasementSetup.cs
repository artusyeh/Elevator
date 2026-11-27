using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    static void BasementSign(Transform root,string message,Vector3 at,float yaw=0,float width=.6f)
    {
        var plate=B("Service Sign",root,at,new Vector3(width,.16f,.018f),"Aged Plastic",false,.004f);plate.localRotation=Quaternion.Euler(0,yaw,0);
        Text(message,plate,new Vector3(0,0,-.011f),.057f,Quaternion.identity,new Color(.08f,.10f,.09f));
    }
    static void BasementWall(Transform root,string name,float x,float z,float length,bool alongZ=false)
    {B(name,root,new Vector3(x,1.30f,z),alongZ?new Vector3(.18f,2.6f,length):new Vector3(length,2.6f,.18f),"Concrete",true,.004f);}
    static void BasementLight(Transform root,Vector3 at,PowerCircuit circuit)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/Corridor Light.prefab");
        var fixture=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);fixture.transform.localPosition=at;
        // Keep the existing flush housing; replace its original lighting with one modest motivated source.
        foreach(var light in fixture.GetComponentsInChildren<Light>(true))UnityEngine.Object.DestroyImmediate(light.gameObject);
        foreach(var old in fixture.GetComponentsInChildren<LightCircuit>(true))UnityEngine.Object.DestroyImmediate(old);
        Lamp(fixture.transform,new Vector3(0,-.13f,0),18f,4.3f,4900,Quaternion.Euler(90,0,0),LightType.Spot);
        Lamp(fixture.transform,new Vector3(0,-.12f,0),.8f,2.8f,4900,Quaternion.identity,LightType.Point);
        foreach(var r in fixture.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
        foreach(var l in fixture.GetComponentsInChildren<Light>()){l.useColorTemperature=false;l.color=new Color(.68f,.85f,.91f);l.spotAngle=115;l.shadows=l.type==LightType.Point?LightShadows.None:LightShadows.Soft;l.shadowCustomResolution=256;}
        BindCircuit(fixture.transform,circuit);
    }
    public static string BuildBasement()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;if(apt.Find("B1")!=null)return "B1 already exists";
        var ground=apt.Find("Ground Floor");var b=G("B1",apt);b.localPosition=Vector3.down*2.9f;
        var access=b.gameObject.AddComponent<BasementAccess>();
        var power=UnityEngine.Object.FindFirstObjectByType<BuildingPower>();
        var circuit=ScriptableObject.CreateInstance<PowerCircuit>();circuit.id="Basement";circuit.label="B1 Services";circuit.startsEnabled=true;
        AssetDatabase.CreateAsset(circuit,BuildingBase+"Circuits/Basement.asset");power.circuits=power.circuits.Concat(new[]{circuit}).ToArray();
        var emergency=AssetDatabase.LoadAssetAtPath<PowerCircuit>(BuildingBase+"Circuits/Emergency.asset");
        var concrete=new Material(M("Concrete"));concrete.name="Basement Damp Concrete";concrete.SetColor("_BaseColor",new Color(.38f,.42f,.40f));concrete.SetFloat("_Smoothness",.32f);AssetDatabase.CreateAsset(concrete,Base+"Materials/Basement Damp Concrete.mat");mats[concrete.name]=concrete;
        var hall=G("Service Corridor",b);var electric=G("Main Electrical Room",b);var pump=G("Water Pump Room",b);var storage=G("Maintenance Storage",b);
        B("Basement Slab",b,new Vector3(-.45f,-.12f,6.4f),new Vector3(11.3f,.24f,12.2f),"Basement Damp Concrete",true,.001f);
        // Stair footprint remains open above both flights; the remaining ceiling is a sealed concrete soffit.
        B("Concrete Soffit",b,new Vector3(-.45f,2.69f,4.35f),new Vector3(11.3f,.18f,8.1f),"Concrete");
        B("Landing Soffit",b,new Vector3(.6f,2.69f,10.5f),new Vector3(5.0f,.18f,4.2f),"Concrete");
        BasementWall(b,"West Foundation",-6.1f,6.4f,12.2f,true);BasementWall(b,"East Foundation",5.2f,5.0f,9.4f,true);
        BasementWall(b,"South Foundation",-.45f,.3f,11.3f);BasementWall(b,"North Foundation",.6f,12.5f,5.2f);
        // Narrow centre corridor, with real door openings to the three service rooms.
        foreach(var span in new[]{new Vector2(.4f,1.0f),new Vector2(2.1f,3.9f),new Vector2(5.0f,8.4f)})BasementWall(hall,"Electrical Partition",-1.1f,(span.x+span.y)*.5f,span.y-span.x,true);
        B("Electrical Door Lintel",hall,new Vector3(-1.1f,2.40f,4.45f),new Vector3(.18f,.40f,1.1f),"Concrete");
        B("Storage Door Lintel",hall,new Vector3(-1.1f,2.40f,1.55f),new Vector3(.18f,.40f,1.1f),"Concrete");
        BasementWall(electric,"Store Separation",-3.6f,3.15f,5.0f);BasementWall(electric,"Electrical Rear Wall",-3.6f,8.4f,5.0f);
        BasementWall(pump,"Pump South Wall",3.15f,3.15f,4.1f);BasementWall(pump,"Pump Rear Wall",3.15f,8.8f,4.1f);
        BasementWall(hall,"Pump Partition",1.15f,3.8f,1.3f,true);BasementWall(hall,"Pump Partition",1.15f,7.18f,3.24f,true);
        B("Pump Door Lintel",hall,new Vector3(1.15f,2.4f,5.0f),new Vector3(.18f,.4f,1.1f),"Concrete");
        BasementWall(b,"Unused Bay Closure",3.15f,1.72f,2.85f,true);
        BasementWall(b,"Landing End",1.0f,12.5f,4.4f);
        foreach(float z in new[]{2.8f,6.5f,9.8f})B("Support Beam",b,new Vector3(-.5f,2.48f,z),new Vector3(10.9f,.28f,.28f),"Concrete");
        // Relocate the actual main distribution assembly: its circuit assets and switch references remain intact.
        var utility=ground.Find("Electrical Room");var main=utility.Find("Distribution Panel");
        var secondary=UnityEngine.Object.Instantiate(main.gameObject,utility).transform;secondary.name="Secondary Distribution Panel";
        foreach(var control in secondary.GetComponentsInChildren<PowerSwitch>(true))
            if(control.mainBreaker||control.circuit==null||!new[]{"Lobby","Security Room","CCTV Cameras","CCTV Monitors","Maintenance"}.Contains(control.circuit.id))UnityEngine.Object.DestroyImmediate(control.gameObject);
        main.SetParent(electric,true);main.localPosition=new Vector3(-4.45f,0,8.16f);main.localRotation=Quaternion.identity;
        CircuitSwitch(main,circuit,new Vector3(.21f,.60f,-.16f));
        BasementSign(electric,"MAIN DISTRIBUTION",new Vector3(-4.45f,2.35f,8.25f));
        BasementSign(utility,"LOCAL SERVICES",new Vector3(-4.34f,2.37f,8.06f),0,.65f);
        // Small guard locker fits the room's unchanged west wall, clear of the existing doorway and tools.
        var locker=G("Guard Locker",utility);locker.localPosition=new Vector3(-5.62f,0,7.18f);locker.localRotation=Quaternion.Euler(0,90,0);
        B("Locker Body",locker,new Vector3(0,.92f,0),new Vector3(.45f,1.84f,.36f),"Green Steel");
        B("Locker Door",locker,new Vector3(0,.94f,-.19f),new Vector3(.40f,1.73f,.025f),"Green Steel",false);
        for(int i=0;i<5;i++)B("Vent Slot",locker,new Vector3(0,1.50f+i*.035f,-.205f),new Vector3(.22f,.009f,.004f),"Black Rubber",false);
        B("Locker Handle",locker,new Vector3(.13f,.95f,-.22f),new Vector3(.025f,.13f,.035f),"Stainless Steel",false);
        // Pump skid, motor bodies, flanged pipework, shutoff wheels and an elevated header tank.
        var skid=G("Twin Booster Pumps",pump);skid.localPosition=new Vector3(3.65f,0,6.9f);
        B("Concrete Plinth",skid,new Vector3(0,.09f,0),new Vector3(1.65f,.18f,1.5f),"Concrete");
        foreach(float x in new[]{-.46f,.46f})
        {
            B("Pump Foot",skid,new Vector3(x,.24f,0),new Vector3(.48f,.12f,.88f),"Green Steel");
            Rod("Motor Housing",skid,new Vector3(x,.48f,-.31f),new Vector3(x,.48f,.25f),.21f,"Green Steel");
            for(int fin=0;fin<8;fin++)Rod("Cooling Fin",skid,new Vector3(x,.48f,-.28f+fin*.065f),new Vector3(x,.48f,-.26f+fin*.065f),.23f,"Green Steel");
            Rod("Pump Volute",skid,new Vector3(x,.48f,.26f),new Vector3(x,.48f,.49f),.25f,"Green Steel");
            Rod("Discharge",skid,new Vector3(x,.48f,.49f),new Vector3(x,1.70f,.49f),.065f,"Green Steel");
            foreach(float y in new[]{.78f,1.40f})Rod("Bolted Flange",skid,new Vector3(x,y-.025f,.49f),new Vector3(x,y+.025f,.49f),.11f,"Stainless Steel");
            Rod("Suction",skid,new Vector3(x,.48f,-.31f),new Vector3(x,.48f,-.61f),.065f,"Green Steel");
            Rod("Suction Riser",skid,new Vector3(x,.48f,-.61f),new Vector3(x,2.2f,-.61f),.065f,"Green Steel");
        }
        PipeRun(pump,new Vector3(3.19f,2.2f,6.29f),new Vector3(4.11f,2.2f,6.29f),.065f);
        PipeRun(pump,new Vector3(4.11f,2.2f,6.29f),new Vector3(4.95f,2.2f,6.29f),.065f);
        PipeRun(pump,new Vector3(4.95f,.2f,6.29f),new Vector3(4.95f,2.5f,6.29f),.065f);
        PipeRun(pump,new Vector3(3.19f,1.7f,7.39f),new Vector3(4.95f,1.7f,7.39f),.065f);
        PipeRun(pump,new Vector3(4.95f,1.7f,7.39f),new Vector3(4.95f,2.5f,7.39f),.065f);
        var tank=G("Pressure Vessel",pump);tank.localPosition=new Vector3(4.55f,0,4.2f);
        Rod("Pressure Tank",tank,new Vector3(0,.25f,0),new Vector3(0,1.6f,0),.30f,"Green Steel");
        foreach(float x in new[]{-.20f,.20f})B("Tank Leg",tank,new Vector3(x,.13f,0),new Vector3(.055f,.26f,.08f),"Green Steel");
        PipeRun(pump,new Vector3(4.55f,.7f,4.2f),new Vector3(4.95f,.7f,4.2f),.035f);PipeRun(pump,new Vector3(4.95f,.7f,4.2f),new Vector3(4.95f,.7f,6.29f),.035f);
        // Grounded maintenance storage, spare fuses and room equipment.
        for(int row=0;row<3;row++)
        {B("Storage Shelf",storage,new Vector3(-4.7f,.35f+row*.55f,1.1f),new Vector3(1.7f,.06f,.55f),"Old Wood");for(int i=0;i<2;i++)B("Supply Case",storage,new Vector3(-5.15f+i*.85f,.51f+row*.55f,1.10f),new Vector3(.55f,.25f,.4f),"Aged Plastic");}
        foreach(float x in new[]{-5.5f,-3.9f})foreach(float z in new[]{.86f,1.34f})B("Rack Upright",storage,new Vector3(x,.88f,z),new Vector3(.04f,1.76f,.04f),"Green Steel");
        for(int i=0;i<3;i++)Rod("Spare Fuse",utility,new Vector3(-3.0f+i*.035f,.683f,8.035f),new Vector3(-3.0f+i*.035f,.683f,8.085f),.011f,"Aged Plastic");
        // Continuous service routes connect main distribution, risers, fixtures and pumps.
        PipeRun(hall,new Vector3(.80f,2.25f,1.0f),new Vector3(.80f,2.25f,11.8f),.07f);
        PipeRun(hall,new Vector3(.80f,2.25f,6.3f),new Vector3(4.95f,2.25f,6.3f),.07f);
        PipeRun(hall,new Vector3(.80f,2.25f,11.8f),new Vector3(.80f,2.75f,11.8f),.07f);
        for(int i=0;i<2;i++)B("Cable Tray Rail",hall,new Vector3(-.65f+i*.24f,2.25f,6.5f),new Vector3(.025f,.09f,10.5f),"Green Steel",false);
        for(float z=1.4f;z<11.8f;z+=.32f)B("Cable Tray Rung",hall,new Vector3(-.53f,2.21f,z),new Vector3(.27f,.022f,.022f),"Green Steel",false);
        for(int i=0;i<3;i++)PipeRun(hall,new Vector3(-.61f+i*.07f,2.23f,1.4f),new Vector3(-.61f+i*.07f,2.23f,11.7f),.008f);
        PipeRun(hall,new Vector3(-.53f,2.23f,7.8f),new Vector3(-4.4f,2.23f,7.8f),.022f);
        for(float z=2;z<12;z+=2) {Rod("Pipe Hanger",hall,new Vector3(.8f,2.3f,z),new Vector3(.8f,2.6f,z),.008f,"Stainless Steel");B("Tray Hanger",hall,new Vector3(-.53f,2.43f,z),new Vector3(.34f,.32f,.025f),"Green Steel",false);}
        // Reuse the proven two-flight stairs one storey down; no new stair dimensions or steepness.
        var stairs=UnityEngine.Object.Instantiate(ground.Find("Stairwell").gameObject,b).transform;stairs.name="Stairwell";stairs.localPosition=Vector3.zero;
        foreach(var t in stairs.Cast<Transform>().Where(t=>t.name=="Landing Window"||t.name=="Floor Sign"||t.name=="Ceiling Services").ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        var stairDoor=stairs.GetComponentInChildren<StairAccessDoor>();stairDoor.requiredKeyId="Maintenance";stairDoor.basementAccess=access;stairDoor.locked=true;stairDoor.open=false;stairDoor.leaf.localRotation=Quaternion.identity;
        var enclosure=G("Stairwell Enclosure",b);
        BasementWall(enclosure,"West Stair Wall",-5.41f,11.19f,5.6f,true);BasementWall(enclosure,"Stair South Wall",-3.8f,8.39f,3.4f);BasementWall(enclosure,"Stair Rear Wall",-3.8f,13.98f,3.4f);
        BasementWall(enclosure,"Stair Jamb Wall",-2.21f,8.71f,.63f,true);BasementWall(enclosure,"Stair Jamb Wall",-2.21f,12.04f,3.9f,true);
        B("Stair Door Lintel",enclosure,new Vector3(-2.21f,2.4f,9.56f),new Vector3(.18f,.4f,1.06f),"Concrete");
        // Extend the landing and shaft without copying the moving cabin.
        var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();var landing=UnityEngine.Object.Instantiate(lift.landings[0].gameObject,b).transform;landing.name="Elevator Landing B1";landing.position=lift.landings[0].position+Vector3.down*2.9f;
        var duplicateCar=landing.Find("Elevator Cabin");if(duplicateCar!=null)UnityEngine.Object.DestroyImmediate(duplicateCar.gameObject);
        foreach(var sliding in landing.GetComponentsInChildren<SlidingDoors>())UnityEngine.Object.DestroyImmediate(sliding);
        foreach(var call in landing.GetComponentsInChildren<ElevatorButton>())call.floor=0;
        lift.landings=lift.landings.Concat(new[]{landing}).ToArray();lift.leftDoors=lift.leftDoors.Concat(new[]{landing.Find("Left Elevator Door")}).ToArray();lift.rightDoors=lift.rightDoors.Concat(new[]{landing.Find("Right Elevator Door")}).ToArray();lift.landingFloors=new[]{1,2,3,4,5,6,0};lift.basementAccess=access;
        foreach(var d in new[]{lift.leftDoors.Last(),lift.rightDoors.Last()}){var p=d.localPosition;p.x=d==lift.leftDoors.Last()?-.275f:.275f;d.localPosition=p;}
        B("Shaft Back",b,new Vector3(5.12f,1.35f,10.8f),new Vector3(.18f,2.7f,2.65f),"Concrete");
        foreach(float z in new[]{9.45f,12.15f})B("Shaft Side",b,new Vector3(4.18f,1.35f,z),new Vector3(2.06f,2.7f,.18f),"Concrete");
        foreach(float z in new[]{9.15f,12.15f})B("Lift Front Return",b,new Vector3(3.2f,1.35f,z),new Vector3(.18f,2.7f,1.1f),"Concrete");
        B("Lift Header",b,new Vector3(3.2f,2.5f,10.8f),new Vector3(.18f,.4f,2.2f),"Concrete");
        var panel=lift.cabin.Find("Control Panel");var button=G("B1 Button",panel);button.localPosition=new Vector3(0,.60f,-.035f);
        Rod("Button Bezel",button,Vector3.zero,new Vector3(0,0,-.013f),.028f,"Stainless Steel");var cap=Rod("Button Cap",button,new Vector3(0,0,-.013f),new Vector3(0,0,-.020f),.022f,"Brass");
        Text("B1",button,new Vector3(0,0,-.022f),.025f,Quaternion.identity,new Color(.14f,.12f,.09f));
        var press=button.gameObject.AddComponent<ElevatorButton>();press.floor=0;press.action=ElevatorButton.Action.Floor;press.cap=cap;press.lamp=cap.GetComponent<Renderer>();var buttonHit=button.gameObject.AddComponent<BoxCollider>();buttonHit.size=new Vector3(.075f,.075f,.06f);
        lift.displays=lift.displays.Concat(landing.GetComponentsInChildren<ElevatorFloorDisplay>()).ToArray();
        foreach(var display in lift.displays){Text("B",display.transform,new Vector3(-.05f,0,-.014f),.09f,Quaternion.identity,new Color(.75f,.06f,.02f));display.basementPrefix=display.transform.GetChild(display.transform.childCount-1).GetComponent<Renderer>();display.basementPrefix.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/World Labels.mat");display.basementPrefix.enabled=false;}
        // Maintenance key occupies one empty existing hook.
        var board=ground.Find("Management Room").GetComponentsInChildren<Transform>(true).First(t=>t.name=="Key Hooks");var hook=board.Cast<Transform>().First(t=>t.name=="Empty Hook");hook.name="Maintenance Key";hook.Find("Key and Tag").gameObject.SetActive(true);
        var key=hook.gameObject.AddComponent<KeyPickup>();key.keyId="Maintenance";key.label="B1 maintenance key";key.allowReturn=true;key.visualRoot=hook.Find("Key and Tag").gameObject;
        var keyHit=hook.gameObject.AddComponent<BoxCollider>();keyHit.center=new Vector3(.015f,-.045f,-.044f);keyHit.size=new Vector3(.08f,.14f,.08f);
        Text("B1",key.visualRoot.transform,new Vector3(.03f,-.08f,-.055f),.022f,Quaternion.identity,new Color(.65f,.60f,.44f));
        // Signage, drains, damp patches and motivated light fixtures.
        BasementSign(hall,"B1  SERVICES",new Vector3(-1.0f,1.65f,7.0f),-90,.85f);
        BasementSign(electric,"MAIN ELECTRICAL",new Vector3(-1.0f,2.27f,4.45f),-90,.85f);
        BasementSign(pump,"WATER PUMPS",new Vector3(1.05f,2.27f,5),90,.7f);
        BasementSign(storage,"MAINTENANCE",new Vector3(-1.0f,2.27f,1.55f),-90,.7f);
        BasementSign(stairs,"B1",new Vector3(-5.30f,1.65f,9.2f),-90,.3f);
        foreach(float z in new[]{2.0f,5.5f,9.0f,11.5f})BasementLight(hall,new Vector3(.05f,2.57f,z),circuit);
        BasementLight(electric,new Vector3(-3.6f,2.57f,6.1f),circuit);BasementLight(pump,new Vector3(3.1f,2.57f,5.5f),circuit);BasementLight(storage,new Vector3(-3.6f,2.57f,1.7f),circuit);
        EmergencyFixture(hall,b.TransformPoint(new Vector3(.3f,2.2f,12.36f)),emergency);EmergencyFixture(electric,b.TransformPoint(new Vector3(-2.3f,2.2f,8.25f)),emergency);
        for(float z=3.8f;z<8.2f;z+=.27f)B("Drainage Grate",pump,new Vector3(1.48f,.004f,z),new Vector3(.28f,.008f,.022f),"Stainless Steel",false,.001f);
        B("Drainage Channel",pump,new Vector3(1.48f,.001f,6.0f),new Vector3(.32f,.002f,4.5f),"Black Rubber",false,.001f);
        for(int i=0;i<7;i++)Weather(b,"Ground Stain",new Vector3(i%2==0?.45f:3.2f,.002f,2+i*1.4f),new Vector2(.60f+i%3*.2f,.9f),Quaternion.Euler(90,0,i*31));
        Weather(pump,"Water Stain",new Vector3(5.105f,1.4f,6.4f),new Vector2(1.5f,1.7f),Quaternion.Euler(0,90,0));
        Weather(electric,"Water Stain",new Vector3(-5.999f,1.6f,5.0f),new Vector2(1.2f,1.6f),Quaternion.Euler(0,-90,0));
        var cctv=UnityEngine.Object.FindFirstObjectByType<CctvSystem>();var channels=cctv.channels.ToList();var cameraCircuit=AssetDatabase.LoadAssetAtPath<PowerCircuit>(BuildingBase+"Circuits/CCTV Cameras.asset");
        channels.Add(InstallCamera(b,b,"B1 Service Camera",9,"B1 Services",b.TransformPoint(new Vector3(.85f,2.3f,11.6f)),b.TransformPoint(new Vector3(0,1,4)),cameraCircuit));
        channels.Add(InstallCamera(b,b,"B1 Electrical Camera",10,"B1 Electrical",b.TransformPoint(new Vector3(-1.5f,2.3f,3.5f)),b.TransformPoint(new Vector3(-4.5f,1.2f,7.6f)),cameraCircuit));cctv.channels=channels.ToArray();
        CommonDynamic(b);SaveCommonModule(b,BuildingBase+"Prefabs/B1 Services.prefab");SaveCommonModule(utility,BuildingBase+"Prefabs/Electrical Room.prefab");
        SaveCommonModule(ground,Base+"Prefabs/Entrance Interior.prefab");SaveCommonModule(board.parent,BuildingBase+"Prefabs/Key Cabinet.prefab");
        EditorUtility.SetDirty(lift);EditorUtility.SetDirty(power);EditorUtility.SetDirty(cctv);SaveApartment();
        return "B1 built: main electrical, pump room, maintenance store, service corridor, elevator 0=B1, extended stairs, key-controlled access, 2 CCTV channels, local GF panel and guard locker.";
    }
}
