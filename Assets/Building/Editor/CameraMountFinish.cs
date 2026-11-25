using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    static void FitCameraHeads()
    {
        var system=UnityEngine.Object.FindFirstObjectByType<CctvSystem>();
        foreach(var channel in system.channels.Where(c=>c.cameraSource!=null))
        {
            var assembly=channel.transform;if(assembly.parent.name=="Elevator Cabin"){var p=assembly.localPosition;p.z=1.34f;assembly.localPosition=p;}
            var model=assembly.Find("Security Camera");
            if(model==null)
            {
                var old=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="Security Camera"&&(t.parent.name=="Aoba Exterior"||t.parent.name=="Lobby")&&Vector3.Distance(t.position,assembly.position)<.1f);
                if(old!=null){foreach(var r in old.GetComponentsInChildren<Renderer>()){r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}foreach(var c in old.GetComponentsInChildren<Collider>()){c.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(c);}}
                model=Prop("Security Camera",assembly,Vector3.zero,0,.23f,false);
            }
            var parts=model.GetComponentsInChildren<MeshRenderer>();
            foreach(var part in parts)if(model.InverseTransformPoint(part.bounds.center).y<.20f){part.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(part);var collider=part.GetComponent<Collider>();if(collider!=null)collider.enabled=false;}
            var lens=parts.OrderByDescending(r=>model.InverseTransformPoint(r.bounds.center).z).First();var center=assembly.InverseTransformPoint(lens.bounds.center);
            foreach(string name in new[]{"Lens Bezel","Lens Glass","Mounting Stem","Camera Cable"}){var old=assembly.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);}
            Rod("Lens Bezel",assembly,center+Vector3.forward*.009f,center+Vector3.forward*.045f,.025f,"Elevator Frame Metal");
            Rod("Lens Glass",assembly,center+Vector3.forward*.045f,center+Vector3.forward*.049f,.018f,"Black Rubber");
            var anchor=assembly.Find("Mounting Plate").localPosition;
            Rod("Mounting Stem",assembly,new Vector3(0,center.y+.05f,center.z-.04f),anchor,.011f,"Green Steel");
            Rod("Camera Cable",assembly,new Vector3(.025f,center.y+.025f,center.z-.10f),anchor+Vector3.right*.025f,.0045f,"Black Rubber");
            channel.cameraSource.transform.localPosition=center+Vector3.forward*.08f;
            foreach(var r in assembly.GetComponentsInChildren<MeshRenderer>()){r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.Off;}
        }
    }
    public static string FinishCameraMounts()
    {
        EnsureApartment();FitCameraHeads();var system=UnityEngine.Object.FindFirstObjectByType<CctvSystem>();SaveServicePrefab(system.channels[2].transform,"Surveillance Camera");
        var car=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;foreach(var t in car.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
        var clone=UnityEngine.Object.Instantiate(car.gameObject);clone.name="Elevator Cabin";clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/Elevator Cabin.prefab");UnityEngine.Object.DestroyImmediate(clone);
        SaveApartment();EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");FitCameraHeads();SaveApartment();EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");return "Original camera heads and hoods reused, obsolete wall arms hidden, lenses aligned and ceiling/wall supports connected in both scenes.";
    }
}
