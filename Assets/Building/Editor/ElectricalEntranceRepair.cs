using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    static void EntranceBox(Transform part,Bounds bounds)
    {
        part.position=bounds.center;part.GetComponent<MeshFilter>().sharedMesh=Rounded(bounds.size,.001f);
        var collider=part.GetComponent<BoxCollider>();if(collider!=null)collider.size=bounds.size;
    }
    static void FitElectricalEndWall(Transform room)
    {
        foreach(var renderer in room.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&new[]{"Tiled Wall","Plaster Wall","Skirting"}.Contains(r.name)))
        {
            var b=renderer.bounds;if(b.min.z<8.08f||b.max.z>8.30f||b.max.x<-2.19f||b.min.x>-2.18f)continue;
            var min=b.min;var max=b.max;max.x=-2.18f;EntranceBox(renderer.transform,new Bounds((min+max)*.5f,max-min));
        }
        var seal=room.Find("Electrical Room Door/Header Seal");
        if(seal!=null)LiftResize(seal,new Vector3(0,2.135f,.011f),new Vector3(.944f,.030f,.025f));
    }
    static void FitElectricalHeader(Transform lobby)
    {
        var old=lobby.Find("Electrical Entrance");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        foreach(var renderer in lobby.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&r.name=="Door Return").ToArray())
        {
            var b=renderer.bounds;
            if(b.min.x<-2.19f||b.max.x>-1.98f||b.min.z<6.279f||b.max.z>9.001f)continue;
            if(b.min.y>2.212f){UnityEngine.Object.DestroyImmediate(renderer.gameObject);continue;}
            // Close the original centimetre-wide seam next to the office doorway too.
            if(Mathf.Abs(b.min.z-6.28f)<.001f){var min=b.min;min.z=6.27f;EntranceBox(renderer.transform,new Bounds((min+b.max)*.5f,b.max-min));}
        }
        var surround=G("Electrical Entrance",lobby);
        B("Plaster Wall",surround,new Vector3(-2.09f,2.49625f,7.635f),new Vector3(.18f,.5675f,2.73f),"Yellowed Plaster",true,.001f);
        foreach(var r in surround.GetComponentsInChildren<MeshRenderer>()){r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.Off;}
    }
    public static string RepairElectricalEntrance()
    {
        EnsureApartment();Directory.CreateDirectory("References/Electrical Entrance");string backup="References/Electrical Entrance/Apartment.before.unity";if(!File.Exists(backup))File.Copy("Assets/Scenes/Apartment.unity",backup);
        var ground=GameObject.Find("Ground Floor").transform;FitElectricalHeader(ground.Find("Lobby"));FitElectricalEndWall(ground.Find("Electrical Room"));SaveApartment();
        string roomPath=BuildingBase+"Prefabs/Electrical Room.prefab";var room=PrefabUtility.LoadPrefabContents(roomPath);
        try{FitElectricalEndWall(room.transform);PrefabUtility.SaveAsPrefabAsset(room,roomPath);}finally{PrefabUtility.UnloadPrefabContents(room);}
        string previewPath=Base+"Prefabs/Entrance Interior.prefab";var preview=PrefabUtility.LoadPrefabContents(previewPath);
        try{FitElectricalHeader(preview.transform.Find("Lobby"));PrefabUtility.SaveAsPrefabAsset(preview,previewPath);}finally{PrefabUtility.UnloadPrefabContents(preview);}
        SaveApartment();return "Continuous doorway header installed, coplanar end-wall faces recessed and top door seal fitted in Apartment and the shared entrance preview.";
    }
}
