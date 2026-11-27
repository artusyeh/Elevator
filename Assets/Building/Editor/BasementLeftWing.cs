using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string FitLeftWingFloorAndLights()
    {
        EnsureApartment();var b=GameObject.Find("Apartment").transform.Find("B1");var wing=b.Find("Left Service Wing");var floor=wing.Find("Service Wing Floor");
        LiftResize(floor,new Vector3(11.20f,-.12f,1.725f),new Vector3(12f,.24f,2.89f));
        var material=b.Find("Basement Slab").GetComponent<Renderer>().sharedMaterial;
        int index=0;
        foreach(var renderer in b.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&r.sharedMaterial==material).ToArray())
        {
            var filter=renderer.GetComponent<MeshFilter>();if(filter==null||!filter.sharedMesh.isReadable)continue;
            var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);mesh.name="Aligned Basement Floor";var vertices=mesh.vertices;var normals=mesh.normals;var uv=mesh.uv;
            for(int i=0;i<vertices.Length;i++)if(normals[i].y>.5f){var p=b.InverseTransformPoint(filter.transform.TransformPoint(vertices[i]));uv[i]=new Vector2(p.x+.45f,6.4f-p.z);}
            mesh.uv=uv;mesh.RecalculateTangents();string path=BuildingBase+"Models/Aligned Basement Floor "+index+++".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old!=null){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);filter.sharedMesh=mesh;renderer.sharedMaterial=material;PrefabUtility.RecordPrefabInstancePropertyModifications(filter);
        }
        foreach(var circuit in wing.GetComponentsInChildren<LightCircuit>(true))circuit.enabled=false;
        foreach(var light in wing.GetComponentsInChildren<Light>(true))light.enabled=false;
        foreach(var renderer in wing.GetComponentsInChildren<Renderer>(true))
        {
            var assigned=renderer.sharedMaterials;bool change=false;
            for(int i=0;i<assigned.Length;i++)if(assigned[i]!=null&&assigned[i].HasProperty("_EmissionColor")&&assigned[i].GetColor("_EmissionColor").maxColorComponent>.001f)
            {
                string path=Base+"Materials/"+assigned[i].name+" Wing Off.mat";var off=AssetDatabase.LoadAssetAtPath<Material>(path);if(off==null){off=new Material(assigned[i]);AssetDatabase.CreateAsset(off,path);}off.SetColor("_EmissionColor",Color.black);off.DisableKeyword("_EMISSION");EditorUtility.SetDirty(off);assigned[i]=off;change=true;
            }
            if(change)renderer.sharedMaterials=assigned;
        }
        SaveCommonModule(b,BuildingBase+"Prefabs/B1 Services.prefab");SaveApartment();return "Floor tucked under side walls, continuous floor UV alignment, matching material; left-wing lights and emissive covers switched off.";
    }
    public static string ExtendBasementLeftWing()
    {
        EnsureApartment();var b=GameObject.Find("Apartment").transform.Find("B1");if(b.Find("Left Service Wing")!=null)return "Left wing already exists.";
        var unused=b.Find("Unused Bay Closure");unused.gameObject.SetActive(false);
        var east=b.Find("East Foundation").GetComponent<MeshRenderer>();
        LiftCutRenderer(east,b,new[]{new Bounds(new Vector3(5.2f,1.30f,1.725f),new Vector3(.4f,2.65f,2.67f))},"East Wall Return");
        var wing=G("Left Service Wing",b);
        B("Service Wing Floor",wing,new Vector3(11.20f,-.12f,1.725f),new Vector3(12f,.24f,2.67f),"Basement Damp Concrete",true,.001f);
        B("Service Wing Ceiling",wing,new Vector3(11.21f,2.69f,1.725f),new Vector3(11.98f,.18f,2.85f),"Concrete",true,.001f);
        foreach(float z in new[]{.30f,3.15f})B("Service Wing Wall",wing,new Vector3(11.20f,1.30f,z),new Vector3(12f,2.60f,.18f),"Concrete",true,.001f);
        B("Service Wing End Wall",wing,new Vector3(17.29f,1.30f,1.725f),new Vector3(.18f,2.60f,3.03f),"Concrete",true,.001f);
        PipeRun(wing,new Vector3(.80f,2.25f,1),new Vector3(16.95f,2.25f,1),.07f);
        PipeRun(wing,new Vector3(16.95f,2.25f,1),new Vector3(16.95f,.3f,1),.07f);
        for(float x=4;x<17;x+=2)Rod("Pipe Hanger",wing,new Vector3(x,2.3f,1),new Vector3(x,2.6f,1),.008f,"Stainless Steel");
        var circuit=AssetDatabase.LoadAssetAtPath<PowerCircuit>(BuildingBase+"Circuits/Basement.asset");
        foreach(float x in new[]{3.9f,7.5f,11.1f,14.8f})BasementLight(wing,new Vector3(x,2.57f,1.72f),circuit);
        Weather(wing,"Water Stain",new Vector3(17.195f,1.2f,1.8f),new Vector2(.9f,1.5f),Quaternion.Euler(0,90,0));
        // Exterior ground layers sit above the original B1 footprint. Clear only the new wing's volume.
        var worldCut=new Bounds(b.TransformPoint(new Vector3(11.2f,1.5f,1.725f)),new Vector3(12.25f,3.2f,3.0f));
        foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&!r.transform.IsChildOf(GameObject.Find("Apartment").transform)&&r.bounds.size.y<.6f&&r.bounds.Intersects(worldCut)).ToArray())
        {var localCut=new Bounds(worldCut.center-b.position,worldCut.size);LiftCutRenderer(r,b,new[]{localCut},"Service Wing Ground Return");PrefabUtility.RecordPrefabInstancePropertyModifications(r);foreach(var c in r.GetComponents<Collider>())PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
        CommonDynamic(wing);SaveCommonModule(b,BuildingBase+"Prefabs/B1 Services.prefab");SaveApartment();
        return "Left service bay extended 12 metres beyond the original outer wall, about 16 metres from the corridor; straight dead end retained.";
    }
}
