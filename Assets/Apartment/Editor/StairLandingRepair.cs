using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static int FitStairThresholds(GameObject root)
    {
        int count=0;
        foreach(var threshold in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Stair Threshold"))
        {
            var stairs=threshold.parent;var landing=stairs.Find("Floor Landing");var door=stairs.Find("Stairwell Door");
            var floor=stairs.parent.Find("Residential Corridor/Corridor Floor");if(floor==null)floor=stairs.parent.Find("Lobby/Terrazzo Floor");
            if(landing==null||floor==null||door==null)continue;
            var inside=landing.GetComponent<Renderer>().bounds;var outside=floor.GetComponent<Renderer>().bounds;
            var plate=door.Find("Door Threshold").GetComponent<Renderer>().bounds;
            float width=outside.min.x-inside.max.x;if(width<=0||width>.6f)throw new Exception("Unexpected stair landing gap.");
            var size=new Vector3(width,.04f,plate.size.z);
            threshold.SetPositionAndRotation(new Vector3((outside.min.x+inside.max.x)*.5f,outside.max.y-.02f,plate.center.z),Quaternion.identity);
            threshold.GetComponent<MeshFilter>().sharedMesh=Rounded(size,.0005f);threshold.GetComponent<BoxCollider>().size=size;
            threshold.GetComponent<Renderer>().sharedMaterial=floor.GetComponent<Renderer>().sharedMaterial;
            count++;
        }
        return count;
    }
    public static string RepairStairLandings()
    {
        EnsureApartment();System.IO.Directory.CreateDirectory("References/Stair Landings");
        string backup="References/Stair Landings/Apartment.before.unity";if(!System.IO.File.Exists(backup))System.IO.File.Copy("Assets/Scenes/Apartment.unity",backup);
        int count=FitStairThresholds(GameObject.Find("Apartment"));
        string path=Base+"Prefabs/Entrance Interior.prefab";var preview=PrefabUtility.LoadPrefabContents(path);int shared=0;
        try{shared=FitStairThresholds(preview);if(shared>0)PrefabUtility.SaveAsPrefabAsset(preview,path);}
        finally{PrefabUtility.UnloadPrefabContents(preview);}
        SaveApartment();return "Fitted "+count+" stair landing strips and "+shared+" entrance preview strips flush between their adjoining floors.";
    }
    public static string CheckStairLandingFloors()
    {
        EnsureApartment();Physics.SyncTransforms();var report=new System.Text.StringBuilder();int samples=0,passed=0;
        foreach(var threshold in GameObject.Find("Apartment").GetComponentsInChildren<Transform>().Where(t=>t.name=="Stair Threshold"))
        {
            var stairs=threshold.parent;var landing=stairs.Find("Floor Landing").GetComponent<Renderer>();
            var floor=stairs.parent.Find("Residential Corridor/Corridor Floor");if(floor==null)floor=stairs.parent.Find("Lobby/Terrazzo Floor");
            var slab=floor.GetComponent<Renderer>();var bridge=threshold.GetComponent<Renderer>();float level=slab.bounds.max.y;
            float overlapCorridor=Mathf.Min(bridge.bounds.max.x,slab.bounds.max.x)-Mathf.Max(bridge.bounds.min.x,slab.bounds.min.x);
            float overlapStairs=Mathf.Min(bridge.bounds.max.x,landing.bounds.max.x)-Mathf.Max(bridge.bounds.min.x,landing.bounds.min.x);
            report.AppendLine(stairs.parent.name+": corridor overlap="+overlapCorridor.ToString("F5")+" stair overlap="+overlapStairs.ToString("F5")+" level error="+(bridge.bounds.max.y-level).ToString("F5"));
            for(int x=0;x<9;x++)foreach(float z in new[]{bridge.bounds.center.z-.25f,bridge.bounds.center.z,bridge.bounds.center.z+.25f})
            {
                float at=Mathf.Lerp(bridge.bounds.min.x-.05f,bridge.bounds.max.x+.05f,x/8f);
                var hits=Physics.RaycastAll(new Vector3(at,level+.10f,z),Vector3.down,.3f).Where(h=>h.collider.transform==threshold||h.collider.transform==floor||h.collider.transform==landing.transform).ToArray();
                samples++;if(hits.Any(h=>Mathf.Abs(h.point.y-level)<.001f))passed++;
            }
        }
        report.AppendLine("Level collision samples: "+passed+"/"+samples);System.IO.File.WriteAllText("References/Stair Landings/Checks.txt",report.ToString());
        return report.ToString();
    }
}
