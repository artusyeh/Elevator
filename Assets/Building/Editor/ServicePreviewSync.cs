using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    static void NameServices()
    {
        var system=UnityEngine.Object.FindFirstObjectByType<CctvSystem>();
        foreach(var c in system.channels)if(c.cameraNumber>=3&&c.cameraNumber<=7)c.name=CircuitNames[c.cameraNumber-1]+" Camera";
        foreach(var toggle in UnityEngine.Object.FindObjectsByType<PowerSwitch>(FindObjectsSortMode.None))if(toggle.transform.parent.name=="Distribution Panel")toggle.name=toggle.mainBreaker?"Main Breaker":toggle.circuit.label+" Breaker";
    }
    public static string SyncServicePreview()
    {
        EnsureApartment();var circuits=BuildingCircuits();NameServices();
        foreach(var r in GameObject.Find("Apartment").GetComponentsInChildren<MeshRenderer>()){r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.Off;}
        SaveApartment();string path=Base+"Prefabs/Entrance Interior.prefab";var preview=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var old=preview.transform.Find("Maintenance Room");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            if(preview.transform.Find("Electrical Room")==null){var room=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/Electrical Room.prefab"),preview.transform);room.name="Electrical Room";room.transform.localPosition=Vector3.zero;}
            var lobby=preview.transform.Find("Lobby");var aperture=new Bounds(new Vector3(-2.09f,1.096f,7.5f),new Vector3(.3f,2.233f,1.06f));
            foreach(var r in lobby.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&new[]{"Tiled Wall","Plaster Wall","Skirting"}.Contains(r.name)).ToArray())if(r.bounds.size.x<.3f&&r.bounds.Intersects(aperture))LiftCutRenderer(r,preview.transform,new[]{aperture},"Door Return");
            if(lobby.Find("Emergency Light")==null)EmergencyFixture(lobby,new Vector3(1.7f,2.35f,12.46f),circuits[12]);
            var car=preview.GetComponentsInChildren<Transform>().First(t=>t.name=="Elevator Cabin");if(car.Find("Emergency Light")==null)EmergencyFixture(car,car.TransformPoint(new Vector3(.3f,2.12f,1.54f)),circuits[12]);
            foreach(var r in preview.GetComponentsInChildren<MeshRenderer>()){r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.Off;}
            PrefabUtility.SaveAsPrefabAsset(preview,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(preview);}
        // Keep the reusable mailbox asset free of the old baked illumination as well.
        string bankPath=Base+"Prefabs/Mailboxes.prefab";var bank=PrefabUtility.LoadPrefabContents(bankPath);
        try{foreach(var r in bank.GetComponentsInChildren<MeshRenderer>()){r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.Off;}PrefabUtility.SaveAsPrefabAsset(bank,bankPath);}finally{PrefabUtility.UnloadPrefabContents(bank);}
        EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");NameServices();SaveApartment();EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");return "Electrical room preview, emergency supplies, mailbox bank and descriptive camera/breaker names synchronized across scenes.";
    }
}
