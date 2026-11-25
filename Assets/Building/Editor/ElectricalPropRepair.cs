using System.IO;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static void FitElectricalProps(Transform room)
    {
        var meters=room.Find("Meter Cabinet");
        var warning=meters.Find("Label")??meters.Find("Electrical Warning");
        warning.name="Electrical Warning";
        warning.localPosition=new Vector3(-.4157f,1.62f,-.60f);
        warning.localRotation=Quaternion.Euler(0,-90,0);
        var backing=meters.Find("Warning Backing");
        if(backing==null)backing=B("Warning Backing",meters,Vector3.zero,new Vector3(.235f,.295f,.004f),"Elevator Frame Metal",false,.001f);
        backing.localPosition=new Vector3(-.418f,1.62f,-.60f);backing.localRotation=Quaternion.Euler(0,-90,0);
        var broom=room.Find("Broom");
        broom.SetPositionAndRotation(room.TransformPoint(new Vector3(-5.65f,0,7.12f)),room.rotation*Quaternion.Euler(0,0,10));
        var pos=broom.position;pos.y+=room.position.y+.003f-BoundsOf(broom).min.y;broom.position=pos;
        var bucket=room.Find("Cleaning Bucket");
        bucket.SetPositionAndRotation(room.TransformPoint(new Vector3(-5.04f,0,7.18f)),room.rotation*Quaternion.Euler(0,-25,0));
        pos=bucket.position;pos.y+=room.position.y+.003f-BoundsOf(bucket).min.y;bucket.position=pos;
        PrefabUtility.RecordPrefabInstancePropertyModifications(broom);PrefabUtility.RecordPrefabInstancePropertyModifications(bucket);
    }
    public static string RepairElectricalProps()
    {
        EnsureApartment();Directory.CreateDirectory("References/Electrical Props");
        string backup="References/Electrical Props/Apartment.before.unity";if(!File.Exists(backup))File.Copy("Assets/Scenes/Apartment.unity",backup);
        FitElectricalProps(GameObject.Find("Ground Floor").transform.Find("Electrical Room"));
        string path=BuildingBase+"Prefabs/Electrical Room.prefab";var room=PrefabUtility.LoadPrefabContents(path);
        try{FitElectricalProps(room.transform);PrefabUtility.SaveAsPrefabAsset(room,path);}finally{PrefabUtility.UnloadPrefabContents(room);}
        path=BuildingBase+"Prefabs/Meter Cabinet.prefab";var meters=PrefabUtility.LoadPrefabContents(path);
        try{var label=meters.transform.Find("Label")??meters.transform.Find("Electrical Warning");if(label!=null)UnityEngine.Object.DestroyImmediate(label.gameObject);PrefabUtility.SaveAsPrefabAsset(meters,path);}finally{PrefabUtility.UnloadPrefabContents(meters);}
        SaveApartment();return "Warning mounted against tiled wall; broom and bucket grounded and separated; shared room prefab updated.";
    }
}
