using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string FixOfficeReceptionWallJoint()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;var lobby=apt.Find("Ground Floor/Lobby");int count=0;
        foreach(var r in lobby.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&(r.name=="Tiled Wall"||r.name=="Skirting")&&Mathf.Abs(r.bounds.center.x+2.09f)<.02f&&Mathf.Abs(r.bounds.center.z-3.25f)<.03f).ToArray())
        {
            var bounds=r.bounds;float end=4.94f;float start=bounds.min.z;var at=r.transform.localPosition;at.z=(start+end)*.5f;var size=bounds.size;size.z=end-start;
            LiftResize(r.transform,at,size);PrefabUtility.RecordPrefabInstancePropertyModifications(r.GetComponent<MeshFilter>());PrefabUtility.RecordPrefabInstancePropertyModifications(r.transform);count++;
        }
        SaveCommonModule(apt.Find("Ground Floor"),Base+"Prefabs/Entrance Interior.prefab");SaveApartment();return "Trimmed "+count+" overlapping reception wall/skirting pieces to the actual jamb boundary.";
    }
    public static string FitSecurityOfficeDoor()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;var door=apt.Find("Ground Floor/Management Room/Management Door");var swing=door.GetComponent<StairAccessDoor>();
        swing.openingAngle=-85;swing.open=false;swing.leaf.localRotation=Quaternion.identity;
        if(swing.leaf.Find("Clearance Fit")==null)
        {
            const float ratio=.88f/.94f;
            foreach(Transform part in swing.leaf){var position=part.localPosition;position.x*=ratio;part.localPosition=position;if(part.name=="Door Leaf"||part.name=="Kick Plate"){var scale=part.localScale;scale.x*=ratio;part.localScale=scale;}}
            var pivot=swing.leaf.localPosition;pivot.x+=.035f;swing.leaf.localPosition=pivot;G("Clearance Fit",swing.leaf);
        }
        if(door.Find("Header Closure")==null)
        {
            B("Header Closure",door,new Vector3(0,2.247f,-.012f),new Vector3(1.08f,.078f,.19f),"Green Steel",true,.002f);
            foreach(float x in new[]{-.459f,.459f})B("Closing Rebate",door,new Vector3(x,1.073f,-.055f),new Vector3(.032f,2.14f,.018f),"Green Steel",false,.002f);
            B("Header Rebate",door,new Vector3(0,2.13f,-.055f),new Vector3(.95f,.033f,.018f),"Green Steel",false,.002f);
            B("Bottom Seal",door,new Vector3(0,.014f,-.048f),new Vector3(.94f,.023f,.009f),"Black Rubber",false,.002f);
        }
        foreach(var rebate in door.Cast<Transform>().Where(t=>t.name=="Closing Rebate"))rebate.GetComponent<MeshFilter>().sharedMesh=Rounded(new Vector3(.060f,2.14f,.018f),.002f);
        if(swing.leaf.Find("Interior Lever Handle")==null)
        foreach(var part in swing.leaf.Cast<Transform>().Where(t=>new[]{"Lock Plate","Handle Spindle","Lever Handle","Keyhole"}.Contains(t.name)).ToArray())
        {var inside=UnityEngine.Object.Instantiate(part.gameObject,swing.leaf).transform;inside.name="Interior "+part.name;var p=part.localPosition;p.z=-p.z;inside.localPosition=p;}
        CommonDynamic(door);SaveCommonModule(door,BuildingBase+"Prefabs/Management Door.prefab");SaveCommonModule(apt.Find("Ground Floor"),Base+"Prefabs/Entrance Interior.prefab");SaveApartment();
        return "Security Office door: header gap closed, perimeter rebates and bottom seal, matching inner handle, opening limited to 85 degrees.";
    }
}
