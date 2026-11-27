using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static Transform CommonJunction(Transform parent,Vector3 at)
    {
        var g=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CommonPrefabs+"Cast Junction Box.prefab"),parent)).transform;
        g.localPosition=at;g.localRotation=Quaternion.Euler(90,0,0);g.localScale=Vector3.one*.70f;
        B("Box Anchor",parent,at+Vector3.up*.020f,new Vector3(.11f,.014f,.11f),"Cabin Aged Steel",false,.002f);return g;
    }
    static void CommonCoupling(Transform root,Vector3 at,Vector3 direction)
    {
        var g=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CommonPrefabs+"Pipe Sleeve.prefab"),root)).transform;
        g.name="Drain Socket";g.localPosition=at;g.localRotation=Quaternion.FromToRotation(Vector3.up,direction);g.localScale=new Vector3(1.2f,.62f,1.2f);
    }
    static void CommonCorridorInfrastructure(Transform room,int floor)
    {
        var g=CommonGroup(room,"Ceiling Services");var beamZ=floor%2==0?new[]{4.35f,8.95f}:new[]{4.35f};
        foreach(float z in beamZ)B("Concrete Downstand",g,new Vector3(0,2.62f,z),new Vector3(4f,.12f,.23f),"Concrete",true,.003f);
        // Drainage falls gently toward the front riser, with branches into dwelling service zones.
        CommonRoute("Drain Collector",g,.055f,"Service Drain Enamel",new Vector3(-1.94f,2.415f,.34f),new Vector3(-1.55f,2.415f,.34f),new Vector3(-1.55f,2.465f,11.90f),new Vector3(-1.995f,2.465f,11.90f));
        CommonRoute("Drain Stack",g,.055f,"Service Drain Enamel",new Vector3(-1.94f,-.10f,.34f),new Vector3(-1.94f,2.9f,.34f));
        foreach(float z in new[]{1.05f,3.35f,5.65f,7.95f,10.25f,11.60f})
        {
            float y=Mathf.Lerp(2.415f,2.465f,(z-.34f)/11.56f);CommonHanger(g,new Vector3(-1.55f,y,z),.055f,2.68f);CommonCoupling(g,new Vector3(-1.55f,y,z+.10f),Vector3.forward);
        }
        foreach(float z in new[]{1.0f,4.15f,7.30f})
        {
            float y=Mathf.Lerp(2.415f,2.465f,(z-.34f)/11.56f);CommonRoute("Dwelling Waste Branch",g,.038f,"Service Drain Enamel",new Vector3(-1.55f,y,z),new Vector3(-1.992f,y,z));
            Rod("Wall Sleeve",g,new Vector3(-1.977f,y,z),new Vector3(-2.002f,y,z),.052f,"Cabin Aged Steel");
        }
        foreach(float side in new[]{-1f,1f})
        {
            // Retain the original smaller service mains and join the existing meter risers.
            foreach(float z in new[]{1.15f,3.8f,6.9f,9.6f,11.9f})CommonHanger(g,new Vector3(side*1.87f,2.48f,z),.035f,2.68f);
            foreach(float z in new[]{2.65f,5.8f,8.95f})CommonRoute("Meter Supply Connection",g,.012f,"Service Water Steel",new Vector3(side*1.87f,2.48f,z),new Vector3(side*1.94f,2.48f,z));
        }
        var main=new List<Vector3>{new Vector3(.95f,2.63f,.025f)};
        foreach(float z in beamZ){main.Add(new Vector3(.95f,2.63f,z-.32f));main.Add(new Vector3(.95f,2.51f,z-.20f));main.Add(new Vector3(.95f,2.51f,z+.20f));main.Add(new Vector3(.95f,2.63f,z+.32f));}
        main.Add(new Vector3(.95f,2.63f,12.12f));CommonRoute("Lighting Conduit",g,.009f,"Service Conduit Paint",main.ToArray());
        CommonRoute("Floor Electrical Riser",g,.012f,"Service Conduit Paint",new Vector3(.95f,-.10f,.025f),new Vector3(.95f,2.90f,.025f));
        foreach(float z in new[]{.8f,1.9f,3.2f,5.1f,7.2f,8.0f,10.0f,11.3f})CommonClip(g,new Vector3(.95f,2.63f,z),2.68f);
        foreach(float z in new[]{2.6f,6.5f,10.7f})
        {
            CommonJunction(g,new Vector3(.95f,2.653f,z));
            CommonRoute("Luminaire Feed",g,.008f,"Service Conduit Paint",new Vector3(.95f,2.65f,z),new Vector3(.16f,2.65f,z),new Vector3(.16f,2.67f,z));
            CommonClip(g,new Vector3(.48f,2.65f,z),2.68f);
        }
        CommonRoute("Camera Wiring",g,.004f,"Black Rubber",new Vector3(.95f,2.63f,11.9f),new Vector3(1.78f,2.63f,11.9f),new Vector3(1.78f,2.36f,11.9f));
        CommonRoute("Switch Conduit",g,.009f,"Service Conduit Paint",new Vector3(.95f,2.63f,12.12f),new Vector3(3.18f,2.63f,12.12f),new Vector3(3.18f,1.31f,12.12f));
        foreach(float x in new[]{1.4f,2.2f,2.95f})CommonClip(g,new Vector3(x,2.63f,12.12f),2.68f);
        CommonDetector(g,new Vector3(-.54f,2.677f,5.30f+floor%3*.21f));CommonWallEquipment(g,floor);
        CommonRoute("Alarm Loop",g,.006f,"Service Conduit Paint",new Vector3(.95f,2.63f,11.20f),new Vector3(-1.978f,2.63f,11.20f),new Vector3(-1.978f,2.65f,11.20f));
        CommonVent(g,new Vector3(-1.997f,2.30f,3.1f+floor%2*.23f),Quaternion.Euler(0,-90,0));
        CommonCeilingWear(g,2.68f,-1.60f,3.3f+floor*.34f,floor);CommonCeilingWear(g,2.68f,1.65f,10.4f-floor*.37f,floor+1);
        var patch=B("Old Ceiling Repair",g,new Vector3(.48f,2.6775f,7.90f+floor*.14f),new Vector3(.43f,.004f,.65f),"Stair Plaster",false,.001f);
        CommonDynamic(g);
        PrefabUtility.SaveAsPrefabAsset(g.gameObject,CommonPrefabs+"Corridor Services "+floor+".prefab");
    }
    static void CommonGroundInfrastructure()
    {
        var ground=GameObject.Find("Ground Floor").transform;var lobby=CommonGroup(ground.Find("Lobby"),"Ceiling Services");
        foreach(float z in new[]{4.65f,8.85f})B("Concrete Downstand",lobby,new Vector3(.60f,2.71f,z),new Vector3(5.4f,.14f,.24f),"Concrete",true,.003f);
        CommonRoute("Lobby Drain Collector",lobby,.055f,"Service Drain Enamel",new Vector3(-1.94f,2.54f,.34f),new Vector3(1.47f,2.54f,.34f),new Vector3(1.47f,2.58f,11.88f),new Vector3(1.47f,2.70f,12.18f),new Vector3(1.47f,2.70f,12.38f),new Vector3(2.15f,2.70f,12.38f),new Vector3(2.15f,2.70f,12.67f));
        Rod("Drain Wall Socket",lobby,new Vector3(2.15f,2.70f,12.47f),new Vector3(2.15f,2.70f,12.51f),.068f,"Service Drain Enamel");
        CommonRoute("Ground Drain Stack",lobby,.055f,"Service Drain Enamel",new Vector3(-1.94f,-.10f,.34f),new Vector3(-1.94f,2.9f,.34f));
        CommonHanger(lobby,new Vector3(-.7f,2.54f,.34f),.055f,2.78f);
        foreach(float z in new[]{1.2f,3.2f,5.4f,7.5f,9.8f,11.7f}){CommonHanger(lobby,new Vector3(1.47f,2.54f+(z-.34f)/12.11f*.04f,z),.055f,2.78f);CommonCoupling(lobby,new Vector3(1.47f,2.54f+(z-.34f)/12.11f*.04f,z+.1f),Vector3.forward);}
        // Electrical riser is continuous with the one on upper floors, fed from the real distribution room.
        CommonRoute("Main Distribution Feed",lobby,.015f,"Service Conduit Paint",new Vector3(-2.0f,2.67f,7.94f),new Vector3(.95f,2.67f,7.94f),new Vector3(.95f,2.67f,4.99f),new Vector3(.95f,2.60f,4.87f),new Vector3(.95f,2.60f,4.43f),new Vector3(.95f,2.67f,4.31f),new Vector3(.95f,2.67f,.025f),new Vector3(.95f,2.90f,.025f));
        CommonRoute("Lobby Lighting Supply",lobby,.009f,"Service Conduit Paint",new Vector3(-.75f,2.67f,7.94f),new Vector3(-.75f,2.75f,7.94f));
        foreach(float z in new[]{2.5f,6.5f,10.7f})
        {
            CommonJunction(lobby,new Vector3(-.75f,2.753f,z));
            CommonRoute("Lobby Lighting Feed",lobby,.009f,"Service Conduit Paint",new Vector3(-.75f,2.75f,z),new Vector3(.15f,2.75f,z),new Vector3(.15f,2.77f,z));
        }
        CommonRoute("Lobby Lighting Main",lobby,.009f,"Service Conduit Paint",new Vector3(-.75f,2.75f,.8f),new Vector3(-.75f,2.75f,4.33f),new Vector3(-.75f,2.60f,4.43f),new Vector3(-.75f,2.60f,4.87f),new Vector3(-.75f,2.75f,4.97f),new Vector3(-.75f,2.75f,8.53f),new Vector3(-.75f,2.60f,8.63f),new Vector3(-.75f,2.60f,9.07f),new Vector3(-.75f,2.75f,9.17f),new Vector3(-.75f,2.75f,12.0f));
        foreach(float z in new[]{1.5f,3.2f,5.4f,7.5f,9.8f,11.5f})CommonClip(lobby,new Vector3(-.75f,2.75f,z),2.78f);
        CommonRoute("Lobby Camera Cable",lobby,.004f,"Black Rubber",new Vector3(-.75f,2.75f,7.3f),new Vector3(-1.8f,2.75f,7.3f),new Vector3(-1.8f,2.48f,7.3f));
        CommonDetector(lobby,new Vector3(.35f,2.777f,5.5f));CommonCeilingWear(lobby,2.78f,1.48f,6.85f,2);CommonWallEquipment(lobby,0,true);
        CommonRoute("Lobby Alarm Feed",lobby,.006f,"Service Conduit Paint",new Vector3(-.75f,2.75f,11.2f),new Vector3(-1.978f,2.75f,11.2f),new Vector3(-1.978f,2.65f,11.2f));
        var office=CommonGroup(ground.Find("Management Room"),"Ceiling Services");
        CommonRoute("Office Circuit Feed",office,.011f,"Service Conduit Paint",new Vector3(-5.75f,2.65f,6.74f),new Vector3(-5.75f,2.68f,6.30f),new Vector3(-5.75f,2.68f,2.0f));
        CommonJunction(office,new Vector3(-5.75f,2.753f,4.55f));
        CommonRoute("Office Luminaire Feed",office,.009f,"Service Conduit Paint",new Vector3(-5.75f,2.68f,4.55f),new Vector3(-5.75f,2.75f,4.55f),new Vector3(-4.5f,2.75f,4.55f),new Vector3(-4.5f,2.77f,4.55f));
        foreach(float z in new[]{2.3f,3.4f,5.2f,6.2f})CommonClip(office,new Vector3(-5.75f,2.68f,z),2.78f);
        CommonDetector(office,new Vector3(-3.0f,2.777f,5.4f));CommonVent(office,new Vector3(-5.998f,2.37f,3.25f),Quaternion.Euler(0,-90,0));CommonCeilingWear(office,2.78f,-5.7f,5.9f,3);
        var utility=CommonGroup(ground.Find("Electrical Room"),"Ceiling Services");
        CommonRoute("Office Power Branch",utility,.011f,"Service Conduit Paint",new Vector3(-5.75f,2.65f,8.08f),new Vector3(-5.75f,2.65f,6.74f));
        CommonRoute("Building Distribution Branch",utility,.015f,"Service Conduit Paint",new Vector3(-5.75f,2.65f,8.08f),new Vector3(-5.75f,2.67f,7.94f),new Vector3(-2.0f,2.67f,7.94f));
        CommonJunction(utility,new Vector3(-3.15f,2.753f,7.25f));
        CommonRoute("Utility Luminaire Feed",utility,.009f,"Service Conduit Paint",new Vector3(-4.6f,2.67f,7.94f),new Vector3(-3.15f,2.67f,7.94f),new Vector3(-3.15f,2.75f,7.25f));
        CommonDetector(utility,new Vector3(-4.1f,2.777f,7.25f));CommonVent(utility,new Vector3(-5.8985f,2.36f,7.45f),Quaternion.Euler(0,-90,0));CommonCeilingWear(utility,2.78f,-5.55f,7.65f,4);
        foreach(var root in new[]{lobby,office,utility}){CommonDynamic(root);PrefabUtility.SaveAsPrefabAsset(root.gameObject,CommonPrefabs+root.parent.name+" Services.prefab");}
    }
    static void CommonStairInfrastructure(Transform stairs,int floor)
    {
        var g=CommonGroup(stairs,"Ceiling Services");
        // Keep utilities at the outer wall, away from the ascending flights and handrails.
        CommonRoute("Stair Alarm Riser",g,.009f,"Service Conduit Paint",new Vector3(-5.285f,-.10f,8.65f),new Vector3(-5.285f,2.9f,8.65f));
        CommonRoute("Landing Light Feed",g,.006f,"Service Conduit Paint",new Vector3(-5.285f,2.14f,8.65f),new Vector3(-5.285f,2.32f,8.50f),new Vector3(-3.95f,2.32f,8.50f),new Vector3(-3.95f,2.13f,8.50f));
        foreach(float y in new[]{.5f,1.3f,2.1f})B("Wall Cable Saddle",g,new Vector3(-5.30f,y,8.65f),new Vector3(.04f,.025f,.04f),"Old Detector Plastic",false,.003f);
        float ceiling=floor==5?2.78f:2.70f;
        B("Landing Downstand",g,new Vector3(-3.81f,ceiling-.05f,9.12f),new Vector3(3.02f,.10f,.22f),"Stair Concrete",true,.003f);
        CommonDetector(g,new Vector3(-4.6f,ceiling-.003f,9.50f));CommonCeilingWear(g,ceiling,-4.85f,9.22f,floor+1);
        CommonDynamic(g);PrefabUtility.SaveAsPrefabAsset(g.gameObject,CommonPrefabs+"Stair Services "+(floor+1)+".prefab");
    }
    public static string InstallGroundServices()
    {
        EnsureApartment();Directory.CreateDirectory(CommonReview);if(!File.Exists(CommonReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",CommonReview+"Apartment.before.unity");
        CommonGroundInfrastructure();CommonStairInfrastructure(GameObject.Find("Ground Floor").transform.Find("Stairwell"),0);SaveApartment();return "Lobby, security room, electrical room and ground stair landing services saved.";
    }
    public static string InstallSecondFloorDetails(){return CommonFloorDetails(2);}
    public static string InstallThirdFloorDetails(){return CommonFloorDetails(3);}
    public static string InstallFourthFloorDetails(){return CommonFloorDetails(4);}
    public static string InstallFifthFloorDetails(){return CommonFloorDetails(5);}
    public static string InstallSixthFloorDetails(){return CommonFloorDetails(6);}
    static string CommonFloorDetails(int floor)
    {
        EnsureApartment();var root=GameObject.Find("Floor "+floor).transform;CommonCorridorInfrastructure(root.Find("Residential Corridor"),floor);
        CommonStairInfrastructure(root.Find("Stairwell"),floor-1);CommonHouseholds(root.Find("Residential Corridor"),floor);SaveApartment();return "Saved floor "+floor+" services and individually dressed entrances.";
    }
}
