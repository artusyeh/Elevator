using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string RepairBasementSeamsAndKey()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;var b=apt.Find("B1");
        var enclosure=b.Find("Stairwell Enclosure");
        if(enclosure.Find("Ground Junction West")==null)
        {
            B("Ground Junction West",enclosure,new Vector3(-5.41f,2.75f,11.19f),new Vector3(.18f,.30f,5.60f),"Concrete");
            B("Ground Junction East",enclosure,new Vector3(-2.21f,2.75f,11.19f),new Vector3(.18f,.30f,5.60f),"Concrete");
            B("Ground Junction Rear",enclosure,new Vector3(-3.80f,2.75f,13.98f),new Vector3(3.4f,.30f,.18f),"Concrete");
            B("Ground Junction Front",enclosure,new Vector3(-3.80f,2.75f,8.39f),new Vector3(3.4f,.30f,.18f),"Concrete");
        }
        // The main basement slab already covers this landing; two flush surfaces caused z-fighting.
        var landing=b.Find("Stairwell/Structure/Floor Landing");if(landing!=null)landing.gameObject.SetActive(false);
        var stairThreshold=b.Find("Stairwell/Stair Threshold");if(stairThreshold!=null)stairThreshold.gameObject.SetActive(false);
        var doorThreshold=b.Find("Stairwell").GetComponentInChildren<StairAccessDoor>(true).transform.Find("Door Threshold");
        if(doorThreshold!=null)
        {
            var surface=b.Find("Basement Slab").GetComponent<Renderer>().bounds;
            var bounds=doorThreshold.GetComponent<Renderer>().bounds;var pos=doorThreshold.position;pos.y=surface.max.y+bounds.extents.y+.001f;doorThreshold.position=pos;
            PrefabUtility.RecordPrefabInstancePropertyModifications(doorThreshold);
        }
        var duplicateWall=b.Find("Landing End");if(duplicateWall!=null)duplicateWall.gameObject.SetActive(false);
        var oldSurround=b.Find("Stairwell/Door Surround");if(oldSurround!=null)oldSurround.gameObject.SetActive(false);
        // One structural partition already separates the electrical room and stairwell.
        var doubledPartition=enclosure.Find("Stair South Wall");if(doubledPartition!=null)doubledPartition.gameObject.SetActive(false);
        var key=apt.GetComponentsInChildren<KeyPickup>(true).First(k=>k.keyId=="Maintenance");
        foreach(var text in key.GetComponentsInChildren<TextMesh>(true))UnityEngine.Object.DestroyImmediate(text.gameObject);
        var template=apt.GetComponentsInChildren<KeyPickup>(true).First(k=>k.keyId=="Room 503").GetComponentsInChildren<Transform>(true).First(t=>t.name=="Worn Room Tag");
        var tag=key.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Key Tag");
        var label=tag.Find("Worn B1 Tag");if(label==null){label=UnityEngine.Object.Instantiate(template.gameObject,tag,false).transform;label.name="Worn B1 Tag";}
        var mat=new Material(template.GetComponent<Renderer>().sharedMaterial);string path=Base+"Materials/B1 Key Insert.mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing!=null){UnityEngine.Object.DestroyImmediate(mat);mat=existing;}else AssetDatabase.CreateAsset(mat,path);
        mat.SetTexture("_BaseMap",DetailMap(BuildingBase+"Textures/Key Tags/B1.png"));label.GetComponent<Renderer>().sharedMaterial=mat;EditorUtility.SetDirty(mat);
        SaveCommonModule(key.GetComponentInParent<Transform>().GetComponentsInParent<Transform>().First(t=>t.name=="Key Cabinet"),BuildingBase+"Prefabs/Key Cabinet.prefab");
        SaveBasementUpdates();return "B1 stair enclosure joined to ground floor, duplicate floor landing disabled, B1 key uses the same printed worn label as room-number keys.";
    }
}
