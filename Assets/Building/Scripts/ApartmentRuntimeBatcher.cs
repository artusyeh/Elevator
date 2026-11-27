using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class ApartmentRuntimeBatcher : MonoBehaviour
{
    [Tooltip("Combine draw submissions for small stationary pieces at runtime. Scene objects stay editable.")]
    public bool batchSmallGeometry=true;
    [Range(150,1024)]public int maximumVertices=512;
    public int BatchedObjects {get;private set;}
    IEnumerator Start()
    {
        // Circuit and mood setup must finish before checking material property blocks.
        yield return null;
        if(!batchSmallGeometry)yield break;
        var moving=new List<Transform>();
        foreach(var item in FindObjectsByType<StairAccessDoor>(FindObjectsSortMode.None))if(item.leaf!=null)moving.Add(item.leaf);
        foreach(var item in FindObjectsByType<RoomGateLatch>(FindObjectsSortMode.None))if(item.leaf!=null)moving.Add(item.leaf);
        foreach(var item in FindObjectsByType<MaintenanceDoor>(FindObjectsSortMode.None))if(item.leaf!=null)moving.Add(item.leaf);
        foreach(var item in FindObjectsByType<ElevatorButton>(FindObjectsSortMode.None))if(item.cap!=null)moving.Add(item.cap);
        foreach(var item in FindObjectsByType<PowerSwitch>(FindObjectsSortMode.None))if(item.lever!=null)moving.Add(item.lever);
        foreach(var item in FindObjectsByType<PhoneControls>(FindObjectsSortMode.None))moving.Add(item.transform);
        foreach(var item in FindObjectsByType<PhoneChargingStation>(FindObjectsSortMode.None))if(item.dockedPhone!=null)moving.Add(item.dockedPhone.transform);
        foreach(var clock in FindObjectsByType<NarrativeAnalogClock>(FindObjectsSortMode.None)){if(clock.hourPivot!=null)moving.Add(clock.hourPivot);if(clock.minutePivot!=null)moving.Add(clock.minutePivot);}
        foreach(var fault in FindObjectsByType<NarrativeLightFault>(FindObjectsSortMode.None))moving.Add(fault.transform);
        foreach(var item in FindObjectsByType<ElevatorController>(FindObjectsSortMode.None))
        {
            if(item.cabin!=null)moving.Add(item.cabin);
            // Landing leaves move independently of the cabin, including the B1 doors.
            if(item.leftDoors!=null)foreach(var door in item.leftDoors)if(door!=null)moving.Add(door);
            if(item.rightDoors!=null)foreach(var door in item.rightDoors)if(door!=null)moving.Add(door);
        }
        foreach(var item in FindObjectsByType<SlidingDoors>(FindObjectsSortMode.None)){if(item.left!=null)moving.Add(item.left);if(item.right!=null)moving.Add(item.right);}
        var scene=gameObject.scene;
        var objects=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(renderer=>
        {
            if(renderer.gameObject.scene!=scene||!renderer.enabled||renderer.isPartOfStaticBatch||renderer.HasPropertyBlock()||renderer.sharedMaterials.Length!=1)return false;
            var material=renderer.sharedMaterial;if(material==null||material.shader.name!="Universal Render Pipeline/Lit"||material.renderQueue>=2450)return false;
            var filter=renderer.GetComponent<MeshFilter>();if(filter==null||filter.sharedMesh==null||!filter.sharedMesh.isReadable||filter.sharedMesh.vertexCount>maximumVertices)return false;
            return renderer.GetComponentInParent<Rigidbody>()==null&&renderer.GetComponentInParent<Animator>()==null&&!moving.Any(parent=>renderer.transform.IsChildOf(parent));
        }).Select(renderer=>renderer.gameObject).ToArray();
        if(objects.Length>0)StaticBatchingUtility.Combine(objects,gameObject);BatchedObjects=objects.Length;
    }
}
