using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class ApartmentBuilder
{
    static int AgeInteriorFloors(Transform root)
    {
        int changed=0;
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            if(!r.sharedMaterials.Any(m=>m!=null&&(m.name=="Terrazzo"||m.shader.name=="Building/Aged Terrazzo")))continue;
            var mesh=r.GetComponent<MeshFilter>();if(mesh==null||mesh.sharedMesh==null)continue;
            var size=mesh.sharedMesh.bounds.size;if(size.y>.30f)continue;
            string name=r.name=="Terrazzo Floor"?"Lobby Terrazzo":r.name=="Stair Threshold"?(size.x>.23f?"Lobby Stair Terrazzo":"Corridor Stair Terrazzo"):r.name.Replace("Floor","Terrazzo");
            string path=Base+"Materials/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(M("Terrazzo"));mat.name=name;AssetDatabase.CreateAsset(mat,path);}
            mat.shader=Shader.Find("Building/Aged Terrazzo");
            mat.SetVector("_Meters",new Vector4(4,4,0,0));
            mat.SetVector("_FloorHalfSize",new Vector4(size.x*.5f,size.z*.5f,0,0));
            mat.SetVector("_WearOffset",r.name=="Office Floor"?new Vector4(3.4f,7.1f,0,0):Vector4.zero);
            mat.SetFloat("_Metallic",0);mat.SetFloat("_BumpScale",.30f);
            mat.SetFloat("_WearStrength",.80f);mat.SetFloat("_EdgeGrime",r.name=="Stair Threshold"?.22f:.52f);
            mat.SetFloat("_LocalStains",r.name=="Stair Threshold"?.08f:.62f);
            if(r.name=="Elevator Landing Floor")
            {
                var main=r.transform.parent.Find("Corridor Floor");
                if(main!=null){var delta=r.bounds.center-main.GetComponent<Renderer>().bounds.center;mat.SetVector("_WearOffset",new Vector4(delta.x,delta.z,0,0));}
                mat.SetVector("_JoinedEdge",new Vector4(-1,-size.z*.5f,size.z*.5f,1));mat.SetFloat("_LocalStains",0);
            }
            else if(r.name=="Corridor Floor")
            {
                var landing=r.transform.parent.Find("Elevator Landing Floor");
                if(landing!=null){var area=landing.GetComponent<Renderer>().bounds;mat.SetVector("_JoinedEdge",new Vector4(1,area.min.z-r.bounds.center.z,area.max.z-r.bounds.center.z,1));}
            }
            mat.SetColor("_BaseColor",new Color(.94f,.93f,.90f,1));
            EditorUtility.SetDirty(mat);r.sharedMaterial=mat;EditorUtility.SetDirty(r);changed++;
        }
        return changed;
    }
    public static string AgeApartmentFloors()
    {
        EnsureApartment();Directory.CreateDirectory("References/Interior Floors");
        string backup="References/Interior Floors/Apartment.before.unity";if(!File.Exists(backup))File.Copy("Assets/Scenes/Apartment.unity",backup);
        var shader=Shader.Find("Building/Aged Terrazzo");if(shader==null||ShaderUtil.ShaderHasError(shader))return "Floor shader not ready; nothing changed.";
        int count=AgeInteriorFloors(GameObject.Find("Apartment").transform);
        foreach(string name in new[]{"Entrance Interior","Residential Corridor","Management Room","Stairwell"})
        {
            string path=Base+"Prefabs/"+name+".prefab";if(AssetDatabase.LoadAssetAtPath<GameObject>(path)==null)continue;
            var prefab=PrefabUtility.LoadPrefabContents(path);
            try{AgeInteriorFloors(prefab.transform);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        }
        SaveApartment();return "Aged "+count+" existing interior floor surfaces; shared preview and corridor prefabs updated. Geometry, colliders and lights unchanged.";
    }
}
