using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    const string SightlineReview="References/Window Sightlines/";
    static void FitMirrorSightline(Transform cabin)
    {
        foreach(string name in new[]{"Mirror","Mirror Frame"}){var t=cabin.Find(name);var p=t.localPosition;p.y=1.72f;t.localPosition=p;PrefabUtility.RecordPrefabInstancePropertyModifications(t);}
        var finishes=cabin.Find("Cabin Finishes");
        foreach(var screw in finishes.Cast<Transform>().Where(t=>t.name=="Screw"&&Mathf.Abs(Mathf.Abs(t.localPosition.x)-.605f)<.002f&&Mathf.Abs(t.localPosition.z-1.572f)<.002f))
        {var p=screw.localPosition;p.y=p.y<1.6f?1.305f:2.135f;screw.localPosition=p;PrefabUtility.RecordPrefabInstancePropertyModifications(screw);}
        var fan=finishes.Find("Ventilation Fan");fan.localPosition=new Vector3(.751f,2.115f,1.30f);fan.localRotation=Quaternion.Euler(0,90,0);PrefabUtility.RecordPrefabInstancePropertyModifications(fan);
        var emergency=cabin.Find("Emergency Light");emergency.localPosition=new Vector3(-.695f,2.12f,1.14f);emergency.localRotation=Quaternion.Euler(0,-90,0);PrefabUtility.RecordPrefabInstancePropertyModifications(emergency);
    }
    public static string CaptureWindowSightlines(string suffix)
    {
        Directory.CreateDirectory(SightlineReview);
        for(int i=0;i<5;i++)
        {
            float y=i*2.9f+1.45f+1.73f;string name="Stair Window "+(i+1)+" "+suffix;Capture(name,new Vector3(-3.60f,y,12.88f),new Vector3(-3.8f,y,14.1f));File.Copy("References/Apartment/Captures/"+name+".png",SightlineReview+name+".png",true);
        }
        Capture("Corridor Sill "+suffix,new Vector3(.35f,4.63f,.92f),new Vector3(0,4.1f,-.12f));File.Copy("References/Apartment/Captures/Corridor Sill "+suffix+".png",SightlineReview+"Corridor Sill "+suffix+".png",true);
        var cabin=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;Capture("Mirror Sightline "+suffix,cabin.TransformPoint(new Vector3(0,1.73f,.45f)),cabin.TransformPoint(new Vector3(0,1.73f,1.57f)));File.Copy("References/Apartment/Captures/Mirror Sightline "+suffix+".png",SightlineReview+"Mirror Sightline "+suffix+".png",true);return "Five stair windows, corridor sill and mirror captured at the actual walking eye height";
    }
    public static string RepairWindowSightlines()
    {
        EnsureApartment();Directory.CreateDirectory(SightlineReview);if(!File.Exists(SightlineReview+"Apartment.before.unity")){File.Copy("Assets/Scenes/Apartment.unity",SightlineReview+"Apartment.before.unity");CaptureWindowSightlines("Before");}
        StairEnclosure(GameObject.Find("Apartment").transform);
        for(int floor=0;floor<5;floor++)
        {
            var stairs=GameObject.Find(floor==0?"Ground Floor":"Floor "+(floor+1)).transform.Find("Stairwell");UnityEngine.Object.DestroyImmediate(stairs.Find("Landing Window").gameObject);StairWindow(stairs);FitShadowPolicy(stairs);
        }
        FitStairFacade(GameObject.Find("Aoba Exterior").transform,true);PolishStairwellSurfaces();
        string exteriorPath=Base+"Prefabs/Aoba Exterior.prefab";var exterior=PrefabUtility.LoadPrefabContents(exteriorPath);try{FitStairFacade(exterior.transform,false);PrefabUtility.SaveAsPrefabAsset(exterior,exteriorPath);}finally{PrefabUtility.UnloadPrefabContents(exterior);}
        for(int floor=2;floor<=6;floor++)FitCorridorEnd(GameObject.Find("Floor "+floor).transform.Find("Residential Corridor"),floor);OpenCorridorFacade(GameObject.Find("Aoba Exterior").transform);
        string corridorPath=Base+"Prefabs/Residential Corridor.prefab";var corridor=PrefabUtility.LoadPrefabContents(corridorPath);try{FitCorridorEnd(corridor.transform,2);FitShadowPolicy(corridor.transform);PrefabUtility.SaveAsPrefabAsset(corridor,corridorPath);}finally{PrefabUtility.UnloadPrefabContents(corridor);}
        SaveServicePrefab(GameObject.Find("Floor 2").transform.Find("Residential Corridor/Corridor End/Corridor Window"),"Corridor Window");
        FitMirrorSightline(UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin);
        foreach(string name in new[]{"Elevator Cabin","Elevator Landing","Entrance Interior"})
        {
            string path=Base+"Prefabs/"+name+".prefab";var prefab=PrefabUtility.LoadPrefabContents(path);try{foreach(var cabin in prefab.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Elevator Cabin"))FitMirrorSightline(cabin);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        }
        foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())FitShadowPolicy(root.transform);SaveApartment();CaptureWindowSightlines("After");return "Stair windows raised 32cm with matching wall/facade apertures; duplicate window frames removed; mirror raised 23cm with fasteners and fan clearance; scene and shared prefabs saved.";
    }
}
