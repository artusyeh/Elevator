using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string OpenBasementStructure()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;
        var cut=new Bounds(new Vector3(-.45f,-1.4f,7.2f),new Vector3(11.8f,3.4f,14.0f));int count=0;
        foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&!r.transform.IsChildOf(apt)&&r.bounds.center.y<.05f&&r.bounds.center.y>-.8f&&r.bounds.size.y<.6f&&r.bounds.Intersects(cut)).ToArray())
        {if(LiftCutRenderer(r,apt,new[]{cut},"Foundation Ground Return"))count++;PrefabUtility.RecordPrefabInstancePropertyModifications(r);foreach(var c in r.GetComponents<Collider>())PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
        var pit=apt.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Pit Floor");pit.position=new Vector3(pit.position.x,-3.4f,pit.position.z);PrefabUtility.RecordPrefabInstancePropertyModifications(pit);
        var foundation=apt.Find("Stairwell Enclosure/Foundation");if(foundation!=null){foundation.position=new Vector3(foundation.position.x,-3.14f,foundation.position.z);PrefabUtility.RecordPrefabInstancePropertyModifications(foundation);}
        var railings=apt.Find("Ground Floor/Stairwell/Railings");
        var opening=new Bounds(new Vector3(-2.94f,.65f,10.0f),new Vector3(1.32f,1.35f,.32f));
        foreach(var c in railings.GetComponentsInChildren<Collider>(true))if(c.enabled&&c.bounds.Intersects(opening)){c.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
        foreach(var r in railings.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.enabled&&r.bounds.Intersects(opening)))
        {
            var mf=r.GetComponent<MeshFilter>();if(mf==null||!mf.sharedMesh.isReadable)continue;
            var mesh=UnityEngine.Object.Instantiate(mf.sharedMesh);mesh.name="B1 Descent Handrail Opening";var vertices=mesh.vertices;
            for(int s=0;s<mesh.subMeshCount;s++){var old=mesh.GetTriangles(s);var keep=new System.Collections.Generic.List<int>();for(int i=0;i<old.Length;i+=3){var p=(r.transform.TransformPoint(vertices[old[i]])+r.transform.TransformPoint(vertices[old[i+1]])+r.transform.TransformPoint(vertices[old[i+2]]))/3;if(opening.Contains(p))continue;keep.Add(old[i]);keep.Add(old[i+1]);keep.Add(old[i+2]);}mesh.SetTriangles(keep,s);}
            string path=BuildingBase+"Models/B1 Descent "+r.GetInstanceID()+".asset";AssetDatabase.CreateAsset(mesh,path);mf.sharedMesh=mesh;PrefabUtility.RecordPrefabInstancePropertyModifications(mf);
        }
        SaveApartment();return "Opened basement foundation void in "+count+" ground layers; lowered shaft pit.";
    }
    public static string SaveBasementUpdates()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;
        SaveCommonModule(apt.Find("B1"),BuildingBase+"Prefabs/B1 Services.prefab");
        SaveCommonModule(apt.Find("Ground Floor/Electrical Room"),BuildingBase+"Prefabs/Electrical Room.prefab");
        SaveCommonModule(apt.Find("Ground Floor"),Base+"Prefabs/Entrance Interior.prefab");
        SaveCommonModule(UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin,Base+"Prefabs/Elevator Cabin.prefab");
        SaveApartment();return "B1 and ground utility saved";
    }
    public static string PolishBasementEquipment()
    {
        EnsureApartment();var b=GameObject.Find("Apartment").transform.Find("B1");var pump=b.Find("Water Pump Room");
        if(pump.Find("Valve Equipment")!=null)return "Already fitted";
        var details=G("Valve Equipment",pump);
        foreach(float x in new[]{3.19f,4.11f})
        {
            Rod("Valve Body",details,new Vector3(x,1.13f,7.39f),new Vector3(x,1.30f,7.39f),.095f,"Green Steel");
            Rod("Valve Stem",details,new Vector3(x,1.2f,7.39f),new Vector3(x,1.2f,7.16f),.012f,"Brass");
            for(int i=0;i<24;i++){float a=i*Mathf.PI/12,c=(i+1)*Mathf.PI/12;Rod("Valve Wheel Rim",details,new Vector3(x+Mathf.Cos(a)*.13f,1.2f+Mathf.Sin(a)*.13f,7.15f),new Vector3(x+Mathf.Cos(c)*.13f,1.2f+Mathf.Sin(c)*.13f,7.15f),.009f,"Red Enamel");}
            for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;Rod("Valve Spoke",details,new Vector3(x,1.2f,7.15f),new Vector3(x+Mathf.Cos(a)*.13f,1.2f+Mathf.Sin(a)*.13f,7.15f),.007f,"Red Enamel");}
            foreach(float y in new[]{.78f,1.40f})for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Rod("Flange Bolt",details,new Vector3(x+Mathf.Cos(a)*.087f,y-.035f,7.39f+Mathf.Sin(a)*.087f),new Vector3(x+Mathf.Cos(a)*.087f,y+.035f,7.39f+Mathf.Sin(a)*.087f),.008f,"Stainless Steel");}
        }
        BasementSign(b,"B1",new Vector3(3.08f,1.65f,11.75f),90,.3f);
        foreach(var l in b.Find("Elevator Landing B1").GetComponentsInChildren<Light>()){l.useColorTemperature=false;l.color=new Color(.7f,.84f,.90f);}
        foreach(var l in b.GetComponentsInChildren<Light>().Where(l=>l.name=="Fixture Bounce"))l.intensity=1.5f;
        CommonDynamic(details);SaveApartment();return "Valves, fasteners and B1 landing signage fitted";
    }
    public static string TestBasementRuntime()
    {
        if(!Application.isPlaying)return "Enter Play Mode first";
        var log=new StringBuilder();var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();var player=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var cc=player.GetComponent<CharacterController>();
        BuildingKeys.Return("Maintenance");lift.RequestFloor(0);log.AppendLine("B1 locked without maintenance key: "+!lift.IsRequested(0));
        var key=UnityEngine.Object.FindObjectsByType<KeyPickup>(FindObjectsSortMode.None).First(k=>k.keyId=="Maintenance");key.Interact(null);
        cc.enabled=false;player.transform.position=lift.cabin.TransformPoint(new Vector3(0,.06f,.9f));cc.enabled=true;
        lift.RequestFloor(0);for(int i=0;i<140;i++)lift.Tick(.1f);
        log.AppendLine("Elevator reaches B1: "+(lift.currentFloor==0&&!lift.Moving)+"; cabin="+lift.cabin.position+" rider="+player.transform.position+" display="+lift.DisplayedFloor);
        lift.RequestFloor(1);for(int i=0;i<140;i++)lift.Tick(.1f);log.AppendLine("Elevator returns 1F: "+(lift.currentFloor==1&&!lift.Moving));
        var b=GameObject.Find("Apartment").transform.Find("B1");var access=b.GetComponent<BasementAccess>();access.available=false;lift.RequestFloor(0);log.AppendLine("B1 availability blocks requests: "+!lift.IsRequested(0));access.available=true;
        foreach(var door in b.GetComponentsInChildren<StairAccessDoor>()){door.SetLocked(false);door.open=true;door.Tick(2);}
        Physics.SyncTransforms();
        cc.enabled=false;player.transform.position=new Vector3(-4.68f,-2.86f,9.5f);cc.enabled=true;
        Func<Vector3,bool> walk=target=>{int ticks=0,stuck=0;while(ticks++<900){var d=target-player.transform.position;d.y=0;if(d.magnitude<.09f)return true;var before=player.transform.position;player.transform.rotation=Quaternion.LookRotation(d);player.Step(Vector2.up,.02f,false);Physics.SyncTransforms();if((player.transform.position-before).sqrMagnitude<.000001f&&++stuck>20)return false;}return false;};
        bool stairs=true;foreach(var target in new[]{new Vector3(-4.68f,-1.45f,13.1f),new Vector3(-2.94f,-1.45f,13.1f),new Vector3(-2.94f,0,9.5f)})if(!walk(target)){stairs=false;break;}
        log.AppendLine("Physical stairs B1 to 1F: "+stairs+" end="+player.transform.position);
        bool down=stairs;foreach(var target in new[]{new Vector3(-2.94f,-1.45f,13.1f),new Vector3(-4.68f,-1.45f,13.1f),new Vector3(-4.68f,-2.9f,9.5f)})if(!walk(target)){down=false;break;}
        log.AppendLine("Physical stairs 1F to B1: "+down+" end="+player.transform.position);
        var main=b.Find("Main Electrical Room/Distribution Panel").GetComponentsInChildren<PowerSwitch>().First(s=>s.mainBreaker);
        main.Interact(null);log.AppendLine("Relocated main breaker cuts supply: "+!BuildingPower.MainSupply);main.Interact(null);
        var cameras=UnityEngine.Object.FindFirstObjectByType<CctvSystem>().channels.Where(c=>c.cameraNumber>=9).ToArray();foreach(var camera in cameras)camera.RenderFeed();log.AppendLine("Basement CCTV: "+string.Join(", ",cameras.Select(c=>c.location+"="+(c.RenderCount>0)).ToArray()));
        return log.ToString();
    }
    public static string BasementScreenshots()
    {
        Capture("B1 Service Corridor",new Vector3(0,-1.2f,10.8f),new Vector3(0,-1.6f,3.5f));
        Capture("B1 Main Electrical",new Vector3(-1.7f,-1.2f,4.0f),new Vector3(-4.5f,-1.6f,7.8f));
        Capture("B1 Water Pumps",new Vector3(1.7f,-1.2f,4.4f),new Vector3(3.8f,-1.7f,6.9f));
        Capture("B1 Elevator Landing",new Vector3(.2f,-1.2f,10.0f),new Vector3(3.2f,-1.45f,10.8f));
        Capture("B1 Staircase Entrance",new Vector3(-.2f,-1.2f,10.6f),new Vector3(-2.2f,-1.5f,9.56f));
        Capture("B1 Maintenance Storage",new Vector3(-1.5f,-1.2f,2.5f),new Vector3(-4.6f,-1.8f,1.1f));
        return "Six B1 player-height screenshots saved";
    }
}
