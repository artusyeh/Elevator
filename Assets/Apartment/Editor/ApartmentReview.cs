using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    public static string Connect()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");
        var door=GameObject.Find("Aoba Apartments").transform.Find("Closed Entry Door");
        var entrance=door.GetComponent<SceneEntrance>();if(entrance==null)entrance=door.gameObject.AddComponent<SceneEntrance>();entrance.destination="Apartment";entrance.arrivalPosition=new Vector3(0,.06f,1.5f);entrance.arrivalRotation=Vector3.zero;entrance.prompt="Enter Aoba Apartments";
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        var scenes=EditorBuildSettings.scenes.ToList();foreach(var path in new[]{"Assets/Scenes/Alley.unity","Assets/Scenes/Apartment.unity"})if(!scenes.Any(s=>s.path==path))scenes.Add(new EditorBuildSettingsScene(path,true));else scenes.First(s=>s.path==path).enabled=true;EditorBuildSettings.scenes=scenes.ToArray();
        EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");return "Two-way entrance connected with E; both scenes registered, existing build entries retained.";
    }
    public static void Capture(string name,Vector3 at,Vector3 target)
    {
        var cam=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>().eyes;var p=cam.transform.position;var q=cam.transform.rotation;var old=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1600,1000,24);var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);
        try{cam.transform.SetPositionAndRotation(at,Quaternion.LookRotation(target-at));foreach(var m in UnityEngine.Object.FindObjectsByType<ApartmentMirror>(FindObjectsSortMode.None))m.RenderFrom(cam);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes("References/Apartment/Captures/"+name+".png",image.EncodeToPNG());}
        finally{cam.transform.SetPositionAndRotation(p,q);cam.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
    }
    public static string CaptureAll()
    {var views=GameObject.Find("Viewpoints").transform;foreach(Transform t in views)Capture(t.name,t.position,t.position+t.forward*4);return "Captured "+views.childCount+" player-height views.";}
    public static string Bake(){return Lightmapping.BakeAsync()?"Apartment indirect-light bake started":"Bake did not start";}
    public static string Probes()
    {
        var root=GameObject.Find("Lighting").transform;
        foreach(var spec in new[]{new Vector3(0,1.4f,5),new Vector3(-4,1.4f,4),new Vector3(3.9f,1.2f,10.8f),new Vector3(0,4.4f,6)})
        {var g=G("Reflection Probe",root);g.position=spec;var p=g.gameObject.AddComponent<ReflectionProbe>();p.mode=UnityEngine.Rendering.ReflectionProbeMode.Baked;p.resolution=256;p.size=spec.x>3?new Vector3(2,2.5f,2):new Vector3(6,3,10);p.boxProjection=true;p.blendDistance=.8f;p.intensity=.6f;Lightmapping.BakeReflectionProbe(p,Base+"Materials/"+Guid.NewGuid().ToString("N")+".exr");}
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Local reflections baked.";
    }
    public static string Walk()
    {
        var walker=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var cc=walker.GetComponent<CharacterController>();var saved=walker.transform.position;var rotation=walker.transform.rotation;var log=new System.Text.StringBuilder();
        Action<Vector3> teleport=at=>{cc.enabled=false;walker.transform.position=at;cc.enabled=true;Physics.SyncTransforms();};
        Func<Vector3,bool> walk=target=>{for(int i=0;i<450;i++){var delta=target-walker.transform.position;delta.y=0;if(delta.magnitude<.065f){for(int j=0;j<15;j++)walker.Step(Vector2.zero,.02f,false);return true;}walker.transform.rotation=Quaternion.LookRotation(delta);walker.Step(new Vector2(0,Mathf.Min(1,delta.magnitude/(walker.speed*.02f))),.02f,false);}return false;};
        try
        {
            teleport(new Vector3(0,.1f,1.5f));foreach(var point in new[]{new Vector3(0,0,5.77f),new Vector3(-2.9f,0,5.77f),new Vector3(-4.3f,0,5.2f)})log.AppendLine("Office route "+point+": "+walk(point)+" reached "+walker.transform.position+" grounded="+cc.isGrounded);
            teleport(new Vector3(0,.1f,8.7f));foreach(var point in new[]{new Vector3(0,0,10.8f),new Vector3(3.9f,0,10.8f)})log.AppendLine("Cabin route "+point+": "+walk(point)+" reached "+walker.transform.position+" grounded="+cc.isGrounded);
            teleport(new Vector3(-4.66f,.12f,9.35f));
            for(int floor=0;floor<5;floor++){foreach(var point in new[]{new Vector3(-4.66f,0,13),new Vector3(-2.92f,0,13),new Vector3(-2.92f,0,9.3f),new Vector3(-4.66f,0,9.3f)}){bool pass=walk(point);log.AppendLine("Stair level "+(floor+1)+" "+point+": "+pass+" reached "+walker.transform.position+" grounded="+cc.isGrounded);if(!pass)break;}}
        }
        finally{cc.enabled=false;walker.transform.SetPositionAndRotation(saved,rotation);cc.enabled=true;Physics.SyncTransforms();}
        File.WriteAllText("References/Apartment/Walkthrough.txt",log.ToString());return log.ToString();
    }
}
