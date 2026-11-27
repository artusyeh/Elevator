using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class ApartmentBuilder
{
    public static string InstallManagementDoor()
    {
        EnsureApartment();
        var floor=GameObject.Find("Apartment").transform.Find("Ground Floor");
        var room=floor.Find("Management Room");
        if(room.Find("Management Door")!=null)return "Management door already installed.";
        var source=floor.GetComponentsInChildren<StairAccessDoor>(true).First(d=>d.name=="Stairwell Door");
        var door=UnityEngine.Object.Instantiate(source.gameObject,room).transform;
        door.name="Management Door";door.localPosition=new Vector3(-2.09f,0,5.77f);door.localRotation=Quaternion.Euler(0,270,0);
        if(PrefabUtility.IsPartOfPrefabInstance(door))PrefabUtility.UnpackPrefabInstance(door.gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        var latch=door.GetComponent<StairAccessDoor>();latch.requiredKeyId="Management";latch.locked=true;latch.open=false;latch.openingAngle=-95;latch.leaf.localRotation=Quaternion.identity;
        foreach(var t in door.GetComponentsInChildren<TextMesh>(true))UnityEngine.Object.DestroyImmediate(t.gameObject);
        foreach(var t in door.GetComponentsInChildren<Transform>(true).Where(t=>t.name.Contains("Plaque")||t.name.Contains("Notice")).ToArray())if(t!=null)UnityEngine.Object.DestroyImmediate(t.gameObject);
        var leaf=door.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Door Leaf");
        // Match the existing caretaker room's worn olive metalwork.
        var plate=B("Management Nameplate",latch.leaf,new Vector3(.47f,1.62f,-.034f),new Vector3(.29f,.09f,.009f),"Brass",false,.003f);
        Text("MANAGEMENT",plate,new Vector3(0,0,-.006f),.034f,Quaternion.identity,new Color(.12f,.13f,.10f));
        foreach(var old in floor.Find("Lobby").Cast<Transform>().Where(t=>t.name=="Office Door Jamb").ToArray())old.gameObject.SetActive(false);
        CommonDynamic(door);
        var electrical=floor.Find("Electrical Room");
        var shelf=electrical.Find("Service Shelf/Shelf").GetComponent<Renderer>().bounds;
        var actor=G("Management Room Key",electrical);
        var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/Door Key.prefab"),actor);
        model.transform.localRotation=Quaternion.Euler(90,0,22);
        var kb=BoundsOf(model.transform);float scale=.09f/Mathf.Max(kb.size.x,kb.size.z);model.transform.localScale*=scale;
        kb=BoundsOf(model.transform);
        Vector3 target=new Vector3(shelf.min.x+.055f,shelf.max.y+.001f,shelf.min.z+.055f);
        model.transform.position+=new Vector3(target.x-kb.center.x,target.y-kb.min.y,target.z-kb.center.z);
        var pickup=actor.gameObject.AddComponent<KeyPickup>();pickup.keyId="Management";pickup.label="Management room key";pickup.visualRoot=model;pickup.interactionRange=2.2f;
        kb=BoundsOf(model.transform);var hit=actor.gameObject.AddComponent<BoxCollider>();hit.center=actor.InverseTransformPoint(kb.center);hit.size=new Vector3(.13f,.055f,.11f);
        CommonDynamic(actor);
        PrefabUtility.RecordPrefabInstancePropertyModifications(model.transform);
        SaveCommonModule(door,BuildingBase+"Prefabs/Management Door.prefab");
        SaveCommonModule(actor,BuildingBase+"Prefabs/Management Room Key.prefab");
        SaveCommonModule(floor,Base+"Prefabs/Entrance Interior.prefab");
        SaveCommonModule(electrical,BuildingBase+"Prefabs/Electrical Room.prefab");
        SaveApartment();
        return "Management room starts locked; matching physical key on electrical room service shelf. Door swings into room.";
    }
}
