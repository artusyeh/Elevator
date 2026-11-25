using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string AuditStairwellStructure()
    {
        EnsureApartment();Physics.SyncTransforms();var log=new StringBuilder();int tested=0,failed=0;float worst=0;
        Action<Vector3,float> support=(point,height)=>{RaycastHit hit;tested++;if(!Physics.Raycast(point,Vector3.down,out hit,.35f)){failed++;return;}float error=Mathf.Abs(hit.point.y-height);worst=Mathf.Max(worst,error);if(error>.015f)failed++;};
        for(int floor=0;floor<6;floor++)
        {
            float y=floor*2.9f;var stairs=GameObject.Find(floor==0?"Ground Floor":"Floor "+(floor+1)).transform.Find("Stairwell");
            foreach(float x in new[]{-5.15f,-3.81f,-2.47f})foreach(float z in new[]{8.66f,9.72f})support(new Vector3(x,y+.20f,z),y);
            support(new Vector3(-2.2f,y+.20f,9.56f),y);
            int before=failed;
            if(floor<5)
            {
                foreach(var flight in stairs.Find("Structure").Cast<Transform>().Where(t=>t.name=="Concrete Flight"))for(int step=0;step<9;step++)foreach(float x in new[]{-.53f,0,.53f})
                {float rise=(step+1)*2.9f/18;var p=flight.TransformPoint(new Vector3(x,rise+.15f,(step+.5f)*.26f));support(p,flight.position.y+rise);}
                foreach(float x in new[]{-5.15f,-3.81f,-2.47f})foreach(float z in new[]{12.77f,13.68f})support(new Vector3(x,y+1.65f,z),y+1.45f);
                var window=stairs.Find("Landing Window");int blocked=0;foreach(float x in new[]{-.3f,.3f})foreach(float wy in new[]{-.26f,0,.26f})blocked+=Physics.RaycastAll(window.position+new Vector3(x,wy,-.09f),Vector3.forward,.70f).Count(h=>h.collider.name!="Window Glass");log.AppendLine("Floor "+(floor+1)+" rear aperture opaque blockers="+blocked);
            }
            var door=stairs.Find("Stairwell Door").GetComponent<StairAccessDoor>();log.AppendLine("Floor "+(floor+1)+": structure sample failures="+(failed-before)+"; door locked by default="+(door.locked&&!door.open)+"; circuit lights="+stairs.GetComponent<LightCircuit>().lights.Length);
        }
        log.AppendLine("Support samples="+tested+"; failed="+failed+"; maximum surface height error="+worst.ToString("F4")+"m");File.WriteAllText(StairReview+"Structure Checks.txt",log.ToString());return log.ToString();
    }
    public static string CaptureStairwellOverview()
    {
        EnsureApartment();var apartment=GameObject.Find("Apartment").transform;var enclosure=apartment.Find("Stairwell Enclosure");var stairs=apartment.GetComponentsInChildren<Transform>().Where(t=>t.name=="Stairwell"&&t.Find("Structure")!=null).ToArray();var renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);var original=renderers.Select(r=>r.enabled).ToArray();var cam=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>().eyes;
        var pose=cam.transform.position;var rotation=cam.transform.rotation;bool ortho=cam.orthographic;float size=cam.orthographicSize,aspect=cam.aspect;var clear=cam.clearFlags;var background=cam.backgroundColor;var target=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1200,1800,24);var image=new Texture2D(1200,1800,TextureFormat.RGB24,false);var helper=new GameObject("Review Light");
        try
        {
            foreach(var r in renderers)
            {
                bool inside=r.transform.IsChildOf(enclosure)||stairs.Any(t=>r.transform.IsChildOf(t));if(!inside){r.enabled=false;continue;}
                // A documented inspection cutaway, never saved into the playable scene.
                if(r.transform.IsChildOf(enclosure)&&(r.bounds.center.x> -2.4f||r.bounds.center.z<8.6f||r.name=="Roof Slab"))r.enabled=false;
            }
            var light=helper.AddComponent<Light>();light.type=LightType.Directional;light.shadows=LightShadows.None;light.intensity=.6f;light.color=new Color(.8f,.86f,1);helper.transform.rotation=Quaternion.Euler(28,-35,0);
            cam.orthographic=true;cam.orthographicSize=9.5f;cam.aspect=2f/3;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.035f,.04f,.05f);cam.transform.SetPositionAndRotation(new Vector3(5.0f,11.7f,-.5f),Quaternion.LookRotation(new Vector3(-3.8f,8.5f,11.2f)-new Vector3(5.0f,11.7f,-.5f)));cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1200,1800),0,0);image.Apply();File.WriteAllBytes(StairReview+"Overview.png",image.EncodeToPNG());
        }
        finally
        {
            for(int i=0;i<renderers.Length;i++)if(renderers[i]!=null)renderers[i].enabled=original[i];cam.transform.SetPositionAndRotation(pose,rotation);cam.orthographic=ortho;cam.orthographicSize=size;cam.aspect=aspect;cam.clearFlags=clear;cam.backgroundColor=background;cam.targetTexture=target;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(helper);UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        }
        return "Whole-height inspection cutaway saved; all temporary visibility and lighting restored";
    }
    public static string CaptureStairwellReview()
    {
        Directory.CreateDirectory(StairReview);
        var names=new[]{"Ground Entrance","Looking Up","Looking Down","Landing","Window","Fixture","Access Door","Top Landing"};
        var ats=new[]{new Vector3(-.8f,1.65f,9.56f),new Vector3(-4.68f,1.65f,9.4f),new Vector3(-2.94f,16.15f,9.8f),new Vector3(-4.85f,3.10f,12.9f),new Vector3(-3.85f,3.10f,12.55f),new Vector3(-4.3f,3.15f,12.1f),new Vector3(-3.8f,4.55f,9.45f),new Vector3(-4.8f,16.15f,9.0f)};
        var targets=new[]{new Vector3(-2.09f,1.2f,9.56f),new Vector3(-4.55f,2.6f,12.75f),new Vector3(-2.94f,12.6f,12.0f),new Vector3(-2.9f,3.5f,11.4f),new Vector3(-3.8f,2.8f,14.25f),new Vector3(-5.30f,3.38f,12.94f),new Vector3(-2.09f,4.05f,9.56f),new Vector3(-3.8f,15.2f,11.0f)};
        for(int i=0;i<names.Length;i++){Capture("Stairwell "+names[i],ats[i],targets[i]);File.Copy("References/Apartment/Captures/Stairwell "+names[i]+".png",StairReview+names[i]+".png",true);}return "Eight stairwell player-height views saved";
    }
    public static string ExportStairwellPrefabs()
    {
        EnsureApartment();var root=new GameObject("Stairwell");try
        {
            var enclosure=UnityEngine.Object.Instantiate(GameObject.Find("Apartment").transform.Find("Stairwell Enclosure").gameObject,root.transform);enclosure.name="Enclosure";
            for(int i=0;i<6;i++){var source=GameObject.Find(i==0?"Ground Floor":"Floor "+(i+1)).transform.Find("Stairwell");var floor=UnityEngine.Object.Instantiate(source.gameObject,root.transform);floor.name=i==0?"Ground Landing":"Floor "+(i+1)+" Landing";floor.transform.position=source.position;}
            PrefabUtility.SaveAsPrefabAsset(root,BuildingBase+"Prefabs/Stairwell.prefab");
        }finally{UnityEngine.Object.DestroyImmediate(root);}
        var stairs=GameObject.Find("Ground Floor").transform.Find("Stairwell");var door=UnityEngine.Object.Instantiate(stairs.Find("Stairwell Door").gameObject);try{door.name="Stairwell Door";door.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(door,Base+"Prefabs/Stairwell Door.prefab");}finally{UnityEngine.Object.DestroyImmediate(door);}SaveApartment();return "Complete reusable stairwell and configurable access-door prefabs saved";
    }
    public static string RunStairwellWalkthrough()
    {
        if(!EditorApplication.isPlaying)return "Enter Play Mode first";var walker=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var cc=walker.GetComponent<CharacterController>();var scanner=walker.GetComponent<InteractionScanner>();var report=new StringBuilder();var doors=GameObject.Find("Apartment").GetComponentsInChildren<StairAccessDoor>();foreach(var door in doors){door.SetLocked(false);door.open=true;door.Tick(2);}Physics.SyncTransforms();
        cc.enabled=false;walker.transform.position=new Vector3(-.75f,.06f,9.56f);cc.enabled=true;Cursor.lockState=CursorLockMode.Locked;float minimumClearance=99;int samples=0;
        Func<Vector3,bool> walk=target=>
        {
            int ticks=0,stuck=0;while(ticks++<1400)
            {
                var d=target-walker.transform.position;d.y=0;if(d.magnitude<.075f)break;var before=walker.transform.position;walker.transform.rotation=Quaternion.LookRotation(d);walker.Step(Vector2.up,.02f,false);Physics.SyncTransforms();if(Vector3.Distance(before,walker.transform.position)<.0001f)stuck++;else stuck=0;if(stuck>90)break;
                if(ticks%12==0){samples++;var hits=Physics.RaycastAll(walker.transform.position+Vector3.up*.25f,Vector3.up,4).Where(h=>!h.collider.transform.IsChildOf(walker.transform)).ToArray();if(hits.Length>0)minimumClearance=Mathf.Min(minimumClearance,hits.Min(h=>h.distance)+.25f);}
            }
            var flat=target-walker.transform.position;bool ok=new Vector2(flat.x,flat.z).magnitude<.10f&&Mathf.Abs(flat.y)<.25f;report.AppendLine((ok?"PASS ":"FAIL ")+target.ToString("F2")+" actual="+walker.transform.position.ToString("F2")+" ticks="+ticks);return ok;
        };
        bool success=walk(new Vector3(-4.68f,0,9.56f));
        for(int floor=0;floor<5&&success;floor++)
        {
            float y=floor*2.9f;foreach(var p in new[]{new Vector3(-4.68f,y+1.45f,12.55f),new Vector3(-4.68f,y+1.45f,13.13f),new Vector3(-2.94f,y+1.45f,13.13f),new Vector3(-2.94f,y+2.9f,9.56f),new Vector3(-1.65f,y+2.9f,9.56f),new Vector3(-4.68f,y+2.9f,9.56f)})if(!(success=walk(p)))break;
        }
        if(success)for(int floor=4;floor>=0&&success;floor--)
        {
            float y=floor*2.9f;foreach(var p in new[]{new Vector3(-2.94f,y+2.9f,9.56f),new Vector3(-2.94f,y+1.45f,13.13f),new Vector3(-4.68f,y+1.45f,13.13f),new Vector3(-4.68f,y,9.56f),new Vector3(-1.65f,y,9.56f),new Vector3(-4.68f,y,9.56f)})if(!(success=walk(p)))break;
        }
        report.AppendLine("Complete ground-to-top-and-back walk="+success+"; minimum sampled overhead clearance="+minimumClearance.ToString("F3")+"m; samples="+samples);File.WriteAllText(StairReview+"Walkthrough.txt",report.ToString());return report.ToString();
    }
}
