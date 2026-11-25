using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static int RaiseCabinPanels(Transform root)
    {
        int count=0;
        foreach(var cabin in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Elevator Cabin"))
        {
            var panel=cabin.Find("Control Panel");if(panel==null)continue;
            var at=panel.localPosition;at.y=.28f;panel.localPosition=at;EditorUtility.SetDirty(panel);count++;
        }
        return count;
    }
    public static string RaiseCabinControlPanel()
    {
        EnsureApartment();Directory.CreateDirectory("References/Elevator Ergonomics");string backup="References/Elevator Ergonomics/Apartment.before.unity";if(!File.Exists(backup))File.Copy("Assets/Scenes/Apartment.unity",backup);
        var cabin=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;
        Capture("Control Panel Before Raising",cabin.TransformPoint(new Vector3(-.55f,1.65f,1.05f)),cabin.TransformPoint(new Vector3(.71f,1.45f,.37f)));
        RaiseCabinPanels(GameObject.Find("Apartment").transform);
        foreach(string name in new[]{"Elevator Cabin","Elevator Landing","Entrance Interior"})
        {
            string path=Base+"Prefabs/"+name+".prefab";var prefab=PrefabUtility.LoadPrefabContents(path);
            try{RaiseCabinPanels(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        }
        AssetDatabase.ImportAsset(CabinTextures+"Labels/Capacity Plate.png",ImportAssetOptions.ForceUpdate);AssetDatabase.ImportAsset(CabinTextures+"Labels/Manufacturer Plate.png",ImportAssetOptions.ForceUpdate);
        Capture("Raised Control Panel",cabin.TransformPoint(new Vector3(-.55f,1.65f,1.05f)),cabin.TransformPoint(new Vector3(.71f,1.45f,.37f)));
        Capture("Corrected Elevator Plates",cabin.TransformPoint(new Vector3(.13f,1.65f,.71f)),cabin.TransformPoint(new Vector3(-.75f,1.78f,.39f)));
        foreach(string name in new[]{"Control Panel Before Raising","Raised Control Panel","Corrected Elevator Plates"})File.Copy("References/Apartment/Captures/"+name+".png","References/Elevator Ergonomics/"+name+".png",true);
        SaveApartment();return "Whole cabin panel raised 28 cm in scene and shared prefabs; two plate textures reimported and views saved.";
    }
}
