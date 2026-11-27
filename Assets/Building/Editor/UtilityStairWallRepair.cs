using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class ApartmentBuilder
{
    public static string FitUtilityStairWall()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;var ground=apt.Find("Ground Floor");var lobby=ground.Find("Lobby");
        var section=G("Utility Stair Wall Joint",lobby);
        if(section.childCount==0)
        {
            var sources=lobby.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.enabled&&r.name=="Door Return"&&Mathf.Abs(r.bounds.center.z-8.515f)<.03f).ToArray();
            foreach(var r in sources){r.enabled=false;foreach(var c in r.GetComponents<Collider>())c.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
            foreach(var r in ground.Find("Stairwell/Door Surround").GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.enabled&&r.bounds.center.z<9.04f&&r.bounds.size.z<.1f).ToArray())
            {r.enabled=false;foreach(var c in r.GetComponents<Collider>())c.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
            B("Tiled Wall",section,new Vector3(-2.09f,.61f,8.5315f),new Vector3(.18f,1.22f,.997f),"Green Tile");
            B("Plaster Wall",section,new Vector3(-2.09f,2f,8.5315f),new Vector3(.18f,1.56f,.997f),"Yellowed Plaster");
            B("Skirting",section,new Vector3(-2.09f,.065f,8.5315f),new Vector3(.195f,.13f,.997f),"Concrete");
        }
        var backing=apt.Find("B1/Stairwell Outer Backing/East Front Backing");
        if(backing!=null){var size=backing.GetComponent<MeshFilter>().sharedMesh.bounds.size;size.y=3.2f;LiftResize(backing,new Vector3(-2.25f,1.3f,8.66f),size);}
        CommonDynamic(section);SaveCommonModule(ground,Base+"Prefabs/Entrance Interior.prefab");SaveCommonModule(apt.Find("B1"),BuildingBase+"Prefabs/B1 Services.prefab");SaveApartment();return "Replaced overlapping joint faces with continuous wall, moved backing behind stair wall.";
    }
}
