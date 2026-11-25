using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string RepairElevatorPanel()
    {
        EnsureApartment();
        var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();
        var panel=lift.cabin.Find("Control Panel");
        UnityEngine.Object.DestroyImmediate(panel.Find("Panel Face").gameObject);
        B("Panel Face",panel,new Vector3(0,1.1f,0),new Vector3(.23f,1.63f,.025f),"Stainless Steel",false);
        var ink=Plain("Lift Button Ink",new Color(.045f,.038f,.025f),.35f);
        foreach(var button in panel.GetComponentsInChildren<ElevatorButton>())
        {
            if(button.action==ElevatorButton.Action.Floor)
            {
                var label=panel.GetComponentsInChildren<TextMesh>().FirstOrDefault(t=>t.text==button.floor.ToString());
                if(label!=null){var p=label.transform.position;var local=panel.InverseTransformPoint(p);local.z=-.046f;label.transform.position=panel.TransformPoint(local);label.color=new Color(.08f,.065f,.04f);label.transform.SetParent(button.cap,true);}
            }
            else if(button.action==ElevatorButton.Action.Open||button.action==ElevatorButton.Action.Close)
            {
                var existing=button.cap.Find("Door Symbol");if(existing!=null)UnityEngine.Object.DestroyImmediate(existing.gameObject);
                var symbol=G("Door Symbol",panel);symbol.localPosition=new Vector3(button.cap.localPosition.x,.52f,-.044f);
                var vertices=new System.Collections.Generic.List<Vector3>();var triangles=new System.Collections.Generic.List<int>();
                for(int side=-1;side<=1;side+=2)
                {
                    float direction=side*(button.action==ElevatorButton.Action.Open?1:-1);float x=side*.010f;int n=vertices.Count;
                    vertices.Add(new Vector3(x+direction*.005f,0,0));vertices.Add(new Vector3(x-direction*.004f,.008f,0));vertices.Add(new Vector3(x-direction*.004f,-.008f,0));
                    triangles.Add(n);triangles.Add(n+(direction>0?2:1));triangles.Add(n+(direction>0?1:2));
                }
                var mesh=new Mesh();mesh.vertices=vertices.ToArray();mesh.triangles=triangles.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
                var path=Base+"Models/"+(button.action==ElevatorButton.Action.Open?"Door Open Symbol":"Door Close Symbol")+".asset";
                var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}
                symbol.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;symbol.gameObject.AddComponent<MeshRenderer>().sharedMaterial=ink;symbol.SetParent(button.cap,true);
            }
            else if(button.action==ElevatorButton.Action.Alarm)
            {
                if(button.cap.Find("Alarm Symbol")==null){Text("!",panel,new Vector3(0,.39f,-.046f),.035f,Quaternion.identity,new Color(.08f,.065f,.04f));var label=panel.Find("!");label.GetComponent<Renderer>().sharedMaterial=panel.GetComponentsInChildren<TextMesh>().First(t=>t.text=="1").GetComponent<Renderer>().sharedMaterial;label.name="Alarm Symbol";label.SetParent(button.cap,true);}
            }
        }
        var clone=UnityEngine.Object.Instantiate(lift.cabin.gameObject);clone.name="Elevator Cabin";clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
        PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/Elevator Cabin.prefab");UnityEngine.Object.DestroyImmediate(clone);
        SaveApartment();return "Cabin panel extended behind lower controls; door symbols and surface-mounted labels installed.";
    }
}
