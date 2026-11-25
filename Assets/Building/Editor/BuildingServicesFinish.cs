using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    public static string FitServiceSupports()
    {
        EnsureApartment();var ground=GameObject.Find("Ground Floor").transform;var room=ground.Find("Electrical Room");var shelf=room.Find("Service Shelf");
        if(shelf.Find("Shelf Bracket")==null)foreach(float x in new[]{-.15f,.15f})
        {
            var bracket=G("Shelf Bracket",shelf);bracket.localPosition=new Vector3(x,0,0);
            B("Wall Plate",bracket,new Vector3(0,.54f,.093f),new Vector3(.035f,.20f,.020f),"Green Steel",false,.001f);
            Rod("Shelf Support",bracket,new Vector3(0,.45f,.076f),new Vector3(0,.63f,.076f),.008f,"Green Steel");
            Rod("Shelf Support",bracket,new Vector3(0,.63f,.076f),new Vector3(0,.63f,-.18f),.008f,"Green Steel");
            Rod("Diagonal Brace",bracket,new Vector3(0,.45f,.076f),new Vector3(0,.63f,-.18f),.007f,"Green Steel");
        }
        foreach(string name in new[]{"Distribution Panel","Meter Cabinet"})
        {
            var cabinet=room.Find(name);if(cabinet.Find("Mounting Foot")!=null)continue;
            foreach(float x in name=="Distribution Panel"?new[]{-.50f,.50f}:new[]{-.20f,.20f})foreach(float y in name=="Distribution Panel"?new[]{.65f,2.0f}:new[]{1.15f,1.96f})
            B("Mounting Foot",cabinet,new Vector3(x,y,name=="Distribution Panel"?.128f:.115f),new Vector3(.065f,.065f,.045f),"Green Steel",false,.003f);
        }
        foreach(var t in GameObject.Find("Apartment").GetComponentsInChildren<Transform>().Where(t=>t.name=="Lighting Switch")){var p=t.position;p.x=3.18f;t.position=p;}
        foreach(var r in room.GetComponentsInChildren<MeshRenderer>()){r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.Off;}
        SaveServicePrefab(room,"Electrical Room");SaveServicePrefab(room.Find("Distribution Panel"),"Distribution Panel");SaveServicePrefab(room.Find("Meter Cabinet"),"Meter Cabinet");SaveApartment();
        string path=Base+"Prefabs/Entrance Interior.prefab";var preview=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var lobby=preview.transform.Find("Lobby");if(lobby.Find("Lighting Switch")==null){var copy=UnityEngine.Object.Instantiate(ground.Find("Lobby/Lighting Switch").gameObject,lobby);copy.name="Lighting Switch";copy.transform.localPosition=ground.Find("Lobby/Lighting Switch").localPosition;copy.transform.localRotation=ground.Find("Lobby/Lighting Switch").localRotation;}
            PrefabUtility.SaveAsPrefabAsset(preview,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(preview);}
        SaveApartment();return "Shelf braces and cabinet standoffs connected to the wall; local switches sit flush; preview updated.";
    }
    public static string PolishServiceRouting()
    {
        EnsureApartment();var circuits=BuildingCircuits();var ground=GameObject.Find("Ground Floor").transform;var room=ground.Find("Electrical Room");
        var panel=room.Find("Distribution Panel");
        foreach(var t in panel.Cast<Transform>().Where(t=>t.name=="Incoming Conduit")){var p=t.localPosition;p.z=.12f;t.localPosition=p;}
        foreach(var t in panel.Cast<Transform>().Where(t=>t.name=="Conduit Saddle")){var p=t.localPosition;p.z=.133f;t.localPosition=p;}
        var supply=room.Find("Meter Cabinet/Meter Supply");var at=supply.localPosition;at.z=.10f;supply.localPosition=at;
        if(room.Find("Supply Conduit")==null)
        {
            Rod("Supply Conduit",room,new Vector3(-5.75f,2.65f,8.08f),new Vector3(-3.85f,2.65f,8.08f),.018f,"Green Steel");
            Rod("Supply Riser",room,new Vector3(-5.75f,2.65f,8.08f),new Vector3(-5.75f,2.78f,8.08f),.018f,"Green Steel");
            B("Supply Junction",room,new Vector3(-5.75f,2.69f,8.08f),new Vector3(.12f,.12f,.055f),"Green Steel",false,.004f);
        }
        var dial=BuildingLabel("Voltage Dial");dial.SetFloat("_AlphaClip",1);dial.EnableKeyword("_ALPHATEST_ON");
        foreach(var root in GameObject.Find("Apartment").GetComponentsInChildren<LightCircuit>().Where(t=>t.name=="Emergency Light"))if(root.transform.Find("Mounting Backplate")==null)
        B("Mounting Backplate",root.transform,new Vector3(0,0,.045f),new Vector3(.16f,.075f,.035f),"Elevator Frame Metal",false,.004f);
        for(int floor=0;floor<6;floor++)
        {
            var parent=floor==0?ground.Find("Lobby"):GameObject.Find("Floor "+(floor+1)).transform.Find("Residential Corridor");
            if(parent.Find("Lighting Switch")!=null)continue;
            var root=G("Lighting Switch",parent);root.SetPositionAndRotation(new Vector3(3.17f,floor*2.9f+1.23f,12.13f),Quaternion.Euler(0,90,0));
            B("Switch Plate",root,Vector3.zero,new Vector3(.085f,.13f,.04f),"Aged Plastic",false,.004f);
            var lever=B("Switch Rocker",root,new Vector3(0,0,-.028f),new Vector3(.035f,.065f,.022f),"Aged Plastic",false,.002f);
            var toggle=root.gameObject.AddComponent<PowerSwitch>();toggle.circuit=circuits[floor==0?0:floor+1];toggle.lever=lever;
            root.gameObject.AddComponent<BoxCollider>().size=new Vector3(.09f,.14f,.07f);
        }
        foreach(var channel in UnityEngine.Object.FindFirstObjectByType<CctvSystem>().channels)channel.cameraSource.farClipPlane=channel.cameraNumber==1?45:channel.cameraNumber==8?4:18;
        SaveServicePrefab(panel,"Distribution Panel");SaveServicePrefab(room.Find("Meter Cabinet"),"Meter Cabinet");SaveServicePrefab(room,"Electrical Room");
        var cabin=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;foreach(var t in cabin.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
        var clone=UnityEngine.Object.Instantiate(cabin.gameObject);clone.name="Elevator Cabin";clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/Elevator Cabin.prefab");UnityEngine.Object.DestroyImmediate(clone);
        SaveApartment();EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");foreach(var channel in UnityEngine.Object.FindFirstObjectByType<CctvSystem>().channels)if(channel.cameraSource!=null)channel.cameraSource.farClipPlane=channel.cameraNumber==1?45:channel.cameraNumber==8?4:18;
        SaveApartment();EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");return "Connected incoming conduits, mounted emergency fixtures, fitted analog dials, and added six local lighting switches.";
    }
    static void SaveServicePrefab(Transform source,string name)
    {
        var clone=UnityEngine.Object.Instantiate(source.gameObject);clone.name=name;clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
        PrefabUtility.SaveAsPrefabAsset(clone,BuildingBase+"Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(clone);
    }
    public static string FinishBuildingServices()
    {
        EnsureApartment();var circuits=BuildingCircuits();var ground=GameObject.Find("Ground Floor").transform;var room=ground.Find("Electrical Room");
        room.Find("Cleaning Bucket").position=new Vector3(-4.95f,.01f,6.99f);room.Find("Broom").position=new Vector3(-5.40f,.01f,7.01f);room.Find("Utility Cabinet").position=new Vector3(-5.55f,0,7.78f);
        room.Find("Distribution Panel/Recessed Face").GetComponent<Renderer>().sharedMaterial=M("Green Steel");
        foreach(var light in room.GetComponent<LightCircuit>().lights)light.intensity*=.60f;
        SaveServicePrefab(room.Find("Distribution Panel"),"Distribution Panel");SaveServicePrefab(room.Find("Meter Cabinet"),"Meter Cabinet");SaveServicePrefab(room,"Electrical Room");
        var camera=UnityEngine.Object.FindFirstObjectByType<CctvSystem>().channels[2];SaveServicePrefab(camera.transform,"Surveillance Camera");
        SaveApartment();
        // Bind the existing preview lights, without introducing a second player or manager into the prefab.
        var path=Base+"Prefabs/Entrance Interior.prefab";var preview=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var cabin=preview.GetComponentsInChildren<Transform>().First(t=>t.name=="Elevator Cabin");
            var lobby=preview.GetComponentsInChildren<Transform>().First(t=>t.name=="Lobby");var office=preview.GetComponentsInChildren<Transform>().First(t=>t.name=="Management Room");
            if(lobby.GetComponent<LightCircuit>()==null)BindCircuit(lobby,circuits[0],cabin);
            if(office.GetComponent<LightCircuit>()==null)BindCircuit(office,circuits[1]);
            if(cabin.GetComponent<LightCircuit>()==null)BindCircuit(cabin,circuits[8]);
            foreach(var stair in preview.GetComponentsInChildren<Transform>().Where(t=>t.name=="Stairwell"))if(stair.GetComponent<LightCircuit>()==null)BindCircuit(stair,circuits[7]);
            foreach(var renderer in preview.GetComponentsInChildren<MeshRenderer>()){renderer.lightmapIndex=-1;renderer.lightProbeUsage=LightProbeUsage.Off;}
            PrefabUtility.SaveAsPrefabAsset(preview,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(preview);}
        EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");var services=GameObject.Find("Building Services");
        if(services==null)
        {
            services=new GameObject("Building Services");var power=services.AddComponent<BuildingPower>();power.circuits=circuits;
            var system=services.AddComponent<CctvSystem>();var registry=G("Cameras",services.transform);var offset=new Vector3(22,0,64);
            var old=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Security Camera").ToArray();
            var channels=new System.Collections.Generic.List<CctvChannel>();
            channels.Add(InstallCamera(registry,registry,"Entrance Camera",1,"Entrance",offset+new Vector3(-1.5f,2.45f,-.32f),offset+new Vector3(.2f,1.2f,-3.2f),circuits[9],old.FirstOrDefault(t=>t.parent.name=="Aoba Exterior")));
            channels.Add(InstallCamera(registry,registry,"Lobby Camera",2,"Lobby",offset+new Vector3(-1.8f,2.48f,7.3f),offset+new Vector3(.2f,1.15f,2),circuits[9],old.FirstOrDefault(t=>t.parent.name=="Lobby")));
            for(int floor=2;floor<=6;floor++)
            {
                var channel=G("Corridor Channel",registry).gameObject.AddComponent<CctvChannel>();channel.location=floor+"F Corridor";channel.cameraNumber=floor+1;channel.locationAvailable=false;channel.circuit=circuits[9];channel.label=AssetDatabase.LoadAssetAtPath<Texture2D>(BuildingBase+"Textures/"+channel.location+" Label.png");channels.Add(channel);
            }
            var interior=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).First(t=>t.name=="Elevator Cabin"&&t.gameObject.activeInHierarchy);
            channels.Add(InstallCamera(interior,interior,"Cabin Camera",8,"Lift Car",interior.TransformPoint(new Vector3(-.55f,2,1.39f)),interior.TransformPoint(new Vector3(0,.95f,.35f)),circuits[9]));
            system.channels=channels.ToArray();var office=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).First(t=>t.name=="Management Room"&&t.gameObject.activeInHierarchy);InstallMonitors(system,office,circuits[10]);
            foreach(var renderer in office.GetComponentsInChildren<MeshRenderer>()){renderer.lightmapIndex=-1;renderer.lightProbeUsage=LightProbeUsage.Off;}
        }
        SaveApartment();EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");
        // Make the new view's layer explicit so surveillance cameras never include player UI.
        string uiPath="Assets/Interaction/Prefabs/Interaction UI.prefab";var ui=PrefabUtility.LoadPrefabContents(uiPath);
        try{foreach(var t in ui.GetComponentsInChildren<Transform>())t.gameObject.layer=5;PrefabUtility.SaveAsPrefabAsset(ui,uiPath);}finally{PrefabUtility.UnloadPrefabContents(ui);}
        SaveApartment();return "Electrical props aligned and saved as reusable prefabs; Alley uses live preview feeds with unavailable upper floors marked honestly; power and UI remain scene-owned.";
    }
}
