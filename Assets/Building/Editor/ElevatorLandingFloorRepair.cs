using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static int RemoveCabinNotice(Transform root)
    {
        int count=0;
        foreach(var cabin in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Elevator Cabin").ToArray())
        foreach(var notice in cabin.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Inspection Certificate").ToArray())
        {UnityEngine.Object.DestroyImmediate(notice.gameObject);count++;}
        return count;
    }
    static int FitElevatorLandingFloors(Transform root)
    {
        int count=0;
        foreach(var part in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Elevator Landing Floor"||t.name=="Elevator Landing Ceiling"))
        {
            var corridor=part.parent.Find("Corridor Floor");var main=part.name=="Elevator Landing Floor"?corridor:part.parent.Find("Corridor Ceiling");
            if(main==null)continue;
            var b=part.GetComponent<Renderer>().bounds;var adjacent=main.GetComponent<Renderer>().bounds;
            var lo=b.min;var hi=b.max;lo.x=adjacent.max.x;
            if(part.name=="Elevator Landing Floor")lo.y=hi.y-.04f;
            else hi.y=adjacent.max.y;
            var size=hi-lo;part.position=(lo+hi)*.5f;part.GetComponent<MeshFilter>().sharedMesh=Rounded(size,.001f);
            var collider=part.GetComponent<BoxCollider>();if(collider!=null)collider.size=size;
            EditorUtility.SetDirty(part);count++;
        }
        return count;
    }
    static void JoinLandingFloorFinish()
    {
        var corridor=M("Corridor Terrazzo");corridor.SetVector("_JoinedEdge",new Vector4(1,2.85f,6.25f,1));EditorUtility.SetDirty(corridor);
        var landing=M("Elevator Landing Terrazzo");landing.SetVector("_JoinedEdge",new Vector4(-1,-1.7f,1.7f,1));
        landing.SetVector("_FloorHalfSize",new Vector4(.60f,1.7f,0,0));landing.SetVector("_WearOffset",new Vector4(2.7f,4.55f,0,0));
        landing.SetFloat("_LocalStains",0);EditorUtility.SetDirty(landing);
    }
    public static string RepairElevatorLandingFloors()
    {
        EnsureApartment();Directory.CreateDirectory("References/Elevator Landings");
        string backup="References/Elevator Landings/Apartment.before.unity";if(!File.Exists(backup))File.Copy("Assets/Scenes/Apartment.unity",backup);
        var root=GameObject.Find("Apartment").transform;int slabs=FitElevatorLandingFloors(root),notices=RemoveCabinNotice(root);
        foreach(string name in new[]{"Elevator Cabin","Elevator Landing","Entrance Interior","Residential Corridor"})
        {
            string path=Base+"Prefabs/"+name+".prefab";var prefab=PrefabUtility.LoadPrefabContents(path);
            try{FitElevatorLandingFloors(prefab.transform);RemoveCabinNotice(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        }
        JoinLandingFloorFinish();SaveApartment();return "Repaired "+slabs+" landing floor/ceiling slabs; removed "+notices+" cabin notice. Shared prefabs updated.";
    }
}
