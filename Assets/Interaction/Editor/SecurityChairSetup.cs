using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string SetupSecurityChair()
    {
        EnsureApartment();var room=GameObject.Find("Apartment").transform.Find("Ground Floor/Management Room");var chair=room.Find("Workstation/Office Chair");
        var seat=chair.GetComponent<ChairSeat>()??chair.gameObject.AddComponent<ChairSeat>();
        var marker=chair.Find("Seated Eye Position");if(marker==null)marker=G("Seated Eye Position",chair);
        // The model root is scaled; derive a seat pose in world metres.
        marker.position=chair.position+Vector3.up*1.20f;marker.rotation=Quaternion.Euler(0,chair.eulerAngles.y,0);seat.eyePosition=marker;
        if(chair.GetComponentsInChildren<Collider>().Length==0){var hit=chair.gameObject.AddComponent<BoxCollider>();hit.center=chair.InverseTransformPoint(chair.position+Vector3.up*.55f);hit.size=new Vector3(.55f,1.05f,.55f)/chair.lossyScale.x;}
        EditorUtility.SetDirty(seat);SaveCommonModule(chair,Base+"Prefabs/Office Chair.prefab");SaveCommonModule(GameObject.Find("Apartment").transform.Find("Ground Floor"),Base+"Prefabs/Entrance Interior.prefab");SaveApartment();
        return "Security chair: E sit / E stand, seated view faces desk; standing returns to approach position.";
    }
}
