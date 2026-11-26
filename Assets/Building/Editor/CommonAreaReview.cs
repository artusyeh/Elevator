using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    static void SaveCommonModule(Transform root,string path)
    {
        var clone=UnityEngine.Object.Instantiate(root.gameObject);clone.name=root.name;clone.transform.SetParent(null);clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);clone.transform.localScale=Vector3.one;
        try{PrefabUtility.SaveAsPrefabAsset(clone,path);}finally{UnityEngine.Object.DestroyImmediate(clone);}
    }
    public static string FinishCommonAreaDetails()
    {
        EnsureApartment();var apartment=GameObject.Find("Apartment").transform;
        var colors=new[]{new Color(.24f,.22f,.18f),new Color(.20f,.24f,.21f),new Color(.23f,.23f,.22f)};
        for(int i=0;i<3;i++){var m=new Material(M("Household Rubber"));string path=Base+"Materials/Household Mat "+i+".mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing==null)AssetDatabase.CreateAsset(m,path);else{UnityEngine.Object.DestroyImmediate(m);m=existing;}m.SetColor("_BaseColor",colors[i]);EditorUtility.SetDirty(m);mats["Household Mat "+i]=m;}
        for(int floor=2;floor<=6;floor++)
        {
            var room=apartment.Find("Floor "+floor+"/Residential Corridor");var households=room.Find("Households");
            int index=0;foreach(var mat in room.Cast<Transform>().Where(t=>t.name=="Doormat")){mat.GetComponent<Renderer>().sharedMaterial=M("Household Mat "+((floor+index++)%3));EditorUtility.SetDirty(mat.GetComponent<Renderer>());}
            foreach(var rack in households.GetComponentsInChildren<Transform>().Where(t=>t.name=="Shoe Rack").ToArray())if(rack.Find("Rubber Foot")==null)
                foreach(float x in new[]{-.255f,.255f})foreach(float z in new[]{-.13f,.13f})B("Rubber Foot",rack,new Vector3(x,.003f,z),new Vector3(.022f,.006f,.022f),"Household Rubber",false,.002f);
            CommonDynamic(households);
            var services=room.Find("Ceiling Services");
            if(services.Find("Riser Wall Clamp")==null)
            {
                foreach(float y in new[]{.55f,1.65f,2.30f})
                {
                    var clamp=G("Riser Wall Clamp",services);clamp.localPosition=new Vector3(-1.94f,y,.34f);clamp.localRotation=Quaternion.Euler(90,0,0);
                    CabinRing("Split Ring",clamp,Vector3.zero,CabinRingMesh("Riser Clamp",.059f,.0035f),M("Cabin Aged Steel"));
                    B("Wall Anchor",services,new Vector3(-1.996f,y,.34f),new Vector3(.009f,.046f,.11f),"Cabin Aged Steel",false,.002f);
                    B("Electrical Riser Clip",services,new Vector3(.95f,y,.021f),new Vector3(.044f,.027f,.038f),"Old Detector Plastic",false,.003f);
                }
            }
            CommonDynamic(services);SaveCommonModule(services,CommonPrefabs+"Corridor Services "+floor+".prefab");
            foreach(Transform household in households)
            {
                if(household.GetComponentsInChildren<Collider>().Length==0)
                {var b=BoundsOf(household);var collider=household.gameObject.AddComponent<BoxCollider>();collider.center=household.InverseTransformPoint(b.center);collider.size=new Vector3(b.size.z,b.size.y,b.size.x);}
                SaveCommonModule(household,CommonPrefabs+household.name+".prefab");
            }
        }
        // Keep independently reusable room modules centred at their own origins.
        foreach(var group in apartment.GetComponentsInChildren<Transform>().Where(t=>t.name=="Ceiling Services"&&t.parent.name!="Residential Corridor").ToArray())
        {
            string label=group.parent.name=="Stairwell"?"Stair Services "+(Mathf.RoundToInt(group.parent.position.y/2.9f)+1):group.parent.name+" Services";
            SaveCommonModule(group,CommonPrefabs+label+".prefab");
        }
        SaveApartment();return CheckCommonAreaDetails();
    }
    public static string CheckCommonAreaDetails()
    {
        var apartment=GameObject.Find("Apartment").transform;Physics.SyncTransforms();var log=new StringBuilder();
        var groups=apartment.GetComponentsInChildren<Transform>().Where(t=>t.name=="Ceiling Services").ToArray();
        log.AppendLine("Service groups="+groups.Length+"; added light components="+groups.Sum(t=>t.GetComponentsInChildren<Light>().Length));
        int mounted=0,total=0;foreach(var r in groups.SelectMany(t=>t.GetComponentsInChildren<Transform>()).Where(t=>t.name=="Exit Sign"||t.name=="Fire Alarm"||t.name=="Ventilation Grille"))
        {total++;RaycastHit hit;var direction=r.TransformDirection(Vector3.forward);if(Physics.Raycast(r.position-direction*.06f,direction,out hit,.10f)&&hit.distance<.10f)mounted++;else log.AppendLine("CHECK MOUNT: "+AnimationUtility.CalculateTransformPath(r,null));}
        log.AppendLine("Wall-backed signs, alarms and grilles="+mounted+"/"+total);
        int clusters=0,clear=0,grounded=0;float minWalkingWidth=4f;
        for(int floor=2;floor<=6;floor++)
        {
            var room=apartment.Find("Floor "+floor+"/Residential Corridor");var households=room.Find("Households");
            foreach(Transform h in households)
            {
                clusters++;var b=BoundsOf(h);if(Mathf.Abs(b.min.y-room.position.y)<.009f)grounded++;
                float nearest=Mathf.Min(Mathf.Abs(b.min.x),Mathf.Abs(b.max.x));minWalkingWidth=Mathf.Min(minWalkingWidth,nearest*2);if(nearest>1.25f)clear++;else log.AppendLine("CHECK PASSAGE: "+h.name+" "+b);
            }
            log.AppendLine("Floor "+floor+": "+households.childCount+" household clusters / 6 entrances; beams="+room.Find("Ceiling Services").Cast<Transform>().Count(t=>t.name=="Concrete Downstand"));
        }
        log.AppendLine("Grounded clusters="+grounded+"/"+clusters+"; clusters outside 2.5 m central passage="+clear+"/"+clusters+"; conservative symmetric clearance="+minWalkingWidth.ToString("F2")+" m");
        var overhead=groups.SelectMany(g=>g.GetComponentsInChildren<Renderer>()).Where(r=>r.name=="Concrete Downstand"||r.name=="Landing Downstand"||r.name=="Drain Collector"||r.name=="Lobby Drain Collector");
        log.AppendLine("Lowest new beam/collector clearance="+overhead.Min(r=>r.bounds.min.y-r.transform.parent.position.y).ToString("F3")+" m");
        var shaders=groups.SelectMany(g=>g.GetComponentsInChildren<Renderer>()).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).Distinct().ToArray();log.AppendLine("Supported materials="+shaders.All(m=>m.shader.isSupported&&m.shader.name!="Hidden/InternalErrorShader"));
        File.WriteAllText(CommonReview+"Placement Checks.txt",log.ToString());return log.ToString();
    }
    public static string CaptureCommonAreaDetails()
    {
        var mood=UnityEngine.Object.FindFirstObjectByType<LightingMoodController>();var prior=mood.CurrentMood;mood.SetHorror(true);
        try
        {
            var names=new[]{"Lobby services","Residential ceiling","Shoes and storage","Child household","Door decoration","Residential plants","Utility ceiling","Security room ceiling","Stair landing"};
            var at=new[]{new Vector3(.45f,1.73f,1.6f),new Vector3(-.10f,4.63f,8.0f),new Vector3(-.10f,4.63f,4.9f),new Vector3(.10f,10.43f,6.0f),new Vector3(.15f,10.43f,4.7f),new Vector3(.1f,7.53f,1.2f),new Vector3(-2.75f,1.73f,7.15f),new Vector3(-3.4f,1.73f,2.5f),new Vector3(-3.8f,4.63f,9.0f)};
            var look=new[]{new Vector3(.2f,2.4f,7.0f),new Vector3(-.6f,5.10f,3.0f),new Vector3(-1.75f,3.35f,6.15f),new Vector3(-1.75f,8.97f,7.07f),new Vector3(-1.90f,9.85f,5.05f),new Vector3(1.72f,6.13f,2.95f),new Vector3(-5.2f,2.68f,7.7f),new Vector3(-5.3f,2.5f,5.5f),new Vector3(-5.1f,5.05f,9.5f)};
            for(int i=0;i<names.Length;i++){string name="Apartment details "+names[i];Capture(name,at[i],look[i]);File.Copy("References/Apartment/Captures/"+name+".png",CommonReview+names[i]+".png",true);}
        }
        finally{mood.SetMood(prior);}
        return "Captured nine player-height views using the existing Horror lighting preset.";
    }
    public static string WalkCommonAreaDetails()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter Play Mode first.");var walker=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var cc=walker.GetComponent<CharacterController>();var at=walker.transform.position;var rot=walker.transform.rotation;var sb=new StringBuilder();int passed=0,total=0;
        try
        {
            for(int floor=1;floor<=6;floor++)foreach(float x in new[]{-.80f,0f,.80f})
            {
                float y=(floor-1)*2.9f;cc.enabled=false;walker.transform.position=new Vector3(x,y+.08f,1.0f);cc.enabled=true;walker.transform.rotation=Quaternion.identity;Physics.SyncTransforms();
                int ticks=0;while(walker.transform.position.z<11.95f&&ticks++<650)walker.Step(Vector2.up,.02f,false);bool ok=walker.transform.position.z>=11.95f;total++;if(ok)passed++;else sb.AppendLine("Blocked floor="+floor+" x="+x+" at="+walker.transform.position);
            }
        }
        finally{cc.enabled=false;walker.transform.SetPositionAndRotation(at,rot);cc.enabled=true;Physics.SyncTransforms();}
        sb.AppendLine("Actual CharacterController corridor walks="+passed+"/"+total+" across all six floors.");File.WriteAllText(CommonReview+"Runtime Checks.txt",sb.ToString());return sb.ToString();
    }
}
