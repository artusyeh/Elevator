using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class AlleyReview
{
    public static void Refine()
    {
        var root=GameObject.Find("Alley");
        if(root==null)throw new System.Exception("Open Alley first.");
        if(GameObject.Find("Street Reflections")!=null)return;
        // Move authored modules to form two gentle changes of direction. No runtime generation.
        string[] groups={"Buildings","Street","Props","Utilities"};
        foreach(string name in groups) foreach(Transform t in GameObject.Find(name).transform)
        {
            float z=t.position.z;float shift=z<17?0:z<30?Mathf.SmoothStep(0,.8f,(z-17)/13):z<43?Mathf.SmoothStep(.8f,-.35f,(z-30)/13):-.35f;
            if(t.name=="Road")continue;
            t.position+=Vector3.right*shift;
        }
        var window=AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/InteriorCurtain.mat");
        if(window!=null)foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.name=="Window Glass" && r.sharedMaterial.name=="Warm practical emission")r.sharedMaterial=window;
        var concrete=AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Cast concrete.mat");
        var plaster=AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Warm mineral plaster.mat");
        foreach(int s in new[]{-1,1})
        {
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Side-passage boundary wall";wall.transform.SetParent(GameObject.Find("Buildings").transform);wall.transform.position=new Vector3(8.1f,1.4f,25+s);wall.transform.localScale=new Vector3(9,2.8f,.24f);wall.GetComponent<Renderer>().sharedMaterial=s==1?plaster:concrete;wall.isStatic=true;
        }
        // Study-level reflection capture: actual nearby geometry reflected in the damp street.
        var pg=new GameObject("Street Reflections");pg.transform.SetParent(GameObject.Find("Lighting").transform);pg.transform.position=new Vector3(0,2,15);
        var probe=pg.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Baked;probe.resolution=128;probe.size=new Vector3(28,20,85);probe.center=new Vector3(0,3,11);probe.boxProjection=true;probe.hdr=true;probe.intensity=.7f;
        Lightmapping.BakeReflectionProbe(probe,"Assets/Alley/Materials/StreetReflection.exr");
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
    public static string Validate()
    {
        Physics.SyncTransforms();
        var walker=Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();
        int holes=0;for(int z=1;z<53;z++)if(!Physics.Raycast(new Vector3(0,1,z),Vector3.down,2))holes++;
        int missing=0;foreach(var g in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(g.gameObject);
        string result="Scene="+UnityEngine.SceneManagement.SceneManager.GetActiveScene().path+"; missing scripts="+missing+"; center-route floor holes="+holes+"; controllers="+Object.FindObjectsByType<AlleyStudy.AlleyWalker>(FindObjectsSortMode.None).Length+"; cameras="+Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length+"; player="+walker.transform.position+"; renderers="+Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length;
        Directory.CreateDirectory("References/Analysis");File.WriteAllText("References/Analysis/Validation.txt",result);return result;
    }
}
