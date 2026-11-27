using System.Linq;
using UnityEngine;
using UnityEditor;
public static partial class ApartmentBuilder
{
    public static string FitServiceNameplates()
    {
        EnsureApartment();AssetDatabase.Refresh();var apt=GameObject.Find("Apartment").transform;int count=0;
        foreach(var plate in apt.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Service Sign"||t.name=="Management Nameplate").ToArray())
        {
            var text=plate.GetComponentInChildren<TextMesh>();if(text==null)continue;string label=text.text;
            var bitmap=AssetDatabase.LoadAssetAtPath<Texture2D>(BuildingBase+"Textures/Service Nameplates/"+label+".png");if(bitmap==null)continue;
            foreach(var txt in plate.GetComponentsInChildren<TextMesh>())UnityEngine.Object.DestroyImmediate(txt.gameObject);
            var mat=Plain(label+" Physical Nameplate",Color.white,.12f);mat.SetTexture("_BaseMap",bitmap);mat.DisableKeyword("_EMISSION");mat.SetColor("_EmissionColor",Color.black);
            var mesh=plate.GetComponent<MeshFilter>().sharedMesh;var size=mesh.bounds.size;
            var backing=plate.GetComponent<Renderer>();backing.sharedMaterial=M("Green Steel");
            Q("Printed Nameplate",plate,new Vector3(0,0,-size.z*.5f-.0007f),new Vector2(size.x-.012f,size.y-.008f),mat,Quaternion.identity);
            foreach(float x in new[]{-size.x*.46f,size.x*.46f})Rod("Plate Screw",plate,new Vector3(x,0,-size.z*.5f-.001f),new Vector3(x,0,-size.z*.5f-.002f),.002f,"Stainless Steel");
            CommonDynamic(plate);count++;
        }
        SaveCommonModule(apt.Find("Ground Floor/Management Room/Management Door"),BuildingBase+"Prefabs/Management Door.prefab");
        SaveBasementUpdates();return "Replaced "+count+" floating text signs with printed physical nameplates.";
    }
}
