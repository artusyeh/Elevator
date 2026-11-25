using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    const string StairAccessReview="References/Stairwell Access/";
    static void MakeStairwellKeyPrefab()
    {
        var root=new GameObject("Stairwell Key");
        try
        {
            var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/Door Key.prefab"),root.transform);
            model.name="Key and Ring";model.transform.localRotation=Quaternion.Euler(90,25,0)*Quaternion.AngleAxis(120,Vector3.up);var bounds=BoundsOf(model.transform);
            model.transform.position-=new Vector3(bounds.center.x,bounds.min.y-.001f,bounds.center.z);PrefabUtility.RecordPrefabInstancePropertyModifications(model.transform);bounds=BoundsOf(model.transform);
            var pickup=root.AddComponent<KeyPickup>();pickup.keyId="Stairwell";
            var collider=root.AddComponent<BoxCollider>();collider.center=bounds.center;collider.size=bounds.size+new Vector3(.018f,.012f,.018f);
            foreach(var t in root.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
            PrefabUtility.SaveAsPrefabAsset(root,BuildingBase+"Prefabs/Stairwell Key.prefab");
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
    static void PlaceStairwellKey(Transform scope)
    {
        var workstation=scope.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Workstation");
        var old=workstation.Find("Stairwell Key");if(old!=null)return;
        var desk=workstation.GetComponentsInChildren<Renderer>(true).First(r=>r.name=="metal_office_desk").bounds;
        var key=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/Stairwell Key.prefab"),workstation);
        key.name="Stairwell Key";key.transform.position=new Vector3(desk.min.x+.13f,desk.max.y+.001f,desk.min.z+.30f);key.transform.rotation=Quaternion.Euler(0,-12,0);PrefabUtility.RecordPrefabInstancePropertyModifications(key.transform);
    }
    public static string SetUpStairwellAccess()
    {
        EnsureApartment();Directory.CreateDirectory(StairAccessReview);if(!File.Exists(StairAccessReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",StairAccessReview+"Apartment.before.unity");
        MakeStairwellKeyPrefab();PlaceStairwellKey(GameObject.Find("Management Room").transform);
        foreach(var door in GameObject.Find("Apartment").GetComponentsInChildren<StairAccessDoor>()){door.requiredKeyId="Stairwell";EditorUtility.SetDirty(door);}
        foreach(var window in GameObject.Find("Apartment").GetComponentsInChildren<Transform>().Where(t=>t.name=="Landing Window"||t.name=="Corridor Window"))
        {if(window.name=="Corridor Window")ObscureCorridorWindow(window,Mathf.RoundToInt((window.position.y-1.6f)/2.9f)+1);else window.Find("Window Glass").GetComponent<Renderer>().sharedMaterial=ObscuredGlassMaterial(Mathf.RoundToInt(window.parent.position.y/2.9f)+1);}
        string path=Base+"Prefabs/Residential Corridor.prefab";var prefab=PrefabUtility.LoadPrefabContents(path);try{ObscureCorridorWindow(prefab.transform.Find("Corridor End/Corridor Window"),2);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        path=Base+"Prefabs/Entrance Interior.prefab";prefab=PrefabUtility.LoadPrefabContents(path);try{PlaceStairwellKey(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        SaveServicePrefab(GameObject.Find("Floor 2").transform.Find("Residential Corridor/Corridor End/Corridor Window"),"Corridor Window");ExportStairwellPrefabs();SaveApartment();return "Pickable CC0 brass stairwell key placed on the security desk; six matching access locks configured; five corridor panes obscured and stairwell panes returned to a darker smoked finish. Shared prefabs saved.";
    }
    public static string CaptureStairwellAccess()
    {
        Directory.CreateDirectory(StairAccessReview);
        Action<string,Vector3,Vector3> view=(name,at,to)=>{Capture(name,at,to);File.Copy("References/Apartment/Captures/"+name+".png",StairAccessReview+name+".png",true);};
        var key=UnityEngine.Object.FindFirstObjectByType<KeyPickup>();if(key!=null){view("Security Desk Key",new Vector3(-4.10f,1.73f,2.72f),key.transform.position);view("Security Workstation",new Vector3(-4.45f,1.73f,4.40f),new Vector3(-3.32f,.91f,2.82f));}
        for(int floor=2;floor<=6;floor++){float y=(floor-1)*2.9f;view("Corridor "+floor,new Vector3(.10f,y+1.73f,1.45f),new Vector3(0,y+1.6f,0));}
        view("Stairwell Glass",new Vector3(-3.6f,14.78f,12.88f),new Vector3(-3.8f,14.78f,14.1f));return "Security key, all five corridor windows and dark stairwell glass captured";
    }
}
