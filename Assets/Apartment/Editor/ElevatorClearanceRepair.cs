using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static Bounds LiftLocalBounds(Renderer renderer,Transform basis)
    {
        var b=renderer.bounds;var local=new Bounds(basis.InverseTransformPoint(b.min),Vector3.zero);
        foreach(float x in new[]{b.min.x,b.max.x})foreach(float y in new[]{b.min.y,b.max.y})foreach(float z in new[]{b.min.z,b.max.z})local.Encapsulate(basis.InverseTransformPoint(new Vector3(x,y,z)));
        return local;
    }
    static void LiftResize(Transform part,Vector3 position,Vector3 size)
    {
        part.localPosition=position;part.GetComponent<MeshFilter>().sharedMesh=Rounded(size,.001f);
        var collider=part.GetComponent<BoxCollider>();if(collider!=null)collider.size=size;
    }
    static IEnumerable<Bounds> LiftSubtract(Bounds source,Bounds cut)
    {
        if(!source.Intersects(cut)){yield return source;yield break;}
        var lo=Vector3.Max(source.min,cut.min);var hi=Vector3.Min(source.max,cut.max);
        if(hi.x<=lo.x||hi.y<=lo.y||hi.z<=lo.z){yield return source;yield break;}
        Vector3 a=source.min,b=source.max;
        var pieces=new[]{
            new[]{a,new Vector3(lo.x,b.y,b.z)},new[]{new Vector3(hi.x,a.y,a.z),b},
            new[]{new Vector3(lo.x,a.y,a.z),new Vector3(hi.x,lo.y,b.z)},new[]{new Vector3(lo.x,hi.y,a.z),new Vector3(hi.x,b.y,b.z)},
            new[]{new Vector3(lo.x,lo.y,a.z),new Vector3(hi.x,hi.y,lo.z)},new[]{new Vector3(lo.x,lo.y,hi.z),new Vector3(hi.x,hi.y,b.z)}};
        foreach(var pair in pieces){var size=pair[1]-pair[0];if(size.x>.001f&&size.y>.001f&&size.z>.001f)yield return new Bounds((pair[0]+pair[1])*.5f,size);}
    }
    static int LiftWallPockets(Transform landing)
    {
        int count=0;
        var cuts=new[]{new Bounds(new Vector3(-.93f,1.09f,.09f),new Vector3(.76f,2.22f,.24f)),new Bounds(new Vector3(.93f,1.09f,.09f),new Vector3(.76f,2.22f,.24f))};
        foreach(var renderer in landing.parent.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&new[]{"Plaster Wall","Tiled Wall","Skirting"}.Contains(r.name)).ToArray())
        {
            var bounds=LiftLocalBounds(renderer,landing);if(bounds.size.z>.30f||!cuts.Any(c=>c.Intersects(bounds)))continue;
            var pieces=new List<Bounds>{bounds};foreach(var cut in cuts)pieces=pieces.SelectMany(b=>LiftSubtract(b,cut)).ToList();
            if(pieces.Count==1&&(pieces[0].size-bounds.size).sqrMagnitude<.000001f)continue;
            var material=renderer.sharedMaterial;renderer.enabled=false;foreach(var collider in renderer.GetComponents<Collider>())collider.enabled=false;
            foreach(var piece in pieces)
            {
                var part=B("Wall Return",landing,piece.center,piece.size,"Concrete",true,.001f);part.GetComponent<Renderer>().sharedMaterial=material;
                part.SetParent(renderer.transform,true);
            }
            count++;
        }
        return count;
    }
    static bool LiftCutRenderer(MeshRenderer renderer,Transform basis,Bounds[] cuts,string pieceName)
    {
        var bounds=LiftLocalBounds(renderer,basis);var pieces=new List<Bounds>{bounds};
        foreach(var cut in cuts)pieces=pieces.SelectMany(b=>LiftSubtract(b,cut)).ToList();
        if(pieces.Count==1&&(pieces[0].size-bounds.size).sqrMagnitude<.000001f)return false;
        var material=renderer.sharedMaterial;renderer.enabled=false;foreach(var collider in renderer.GetComponents<Collider>())collider.enabled=false;
        foreach(var piece in pieces)
        {
            var part=B(pieceName,renderer.transform,Vector3.zero,piece.size,"Concrete",true,.001f);
            part.SetPositionAndRotation(basis.TransformPoint(piece.center),basis.rotation);part.GetComponent<Renderer>().sharedMaterial=material;
        }
        return true;
    }
    public static string FinishElevatorClearance()
    {
        EnsureApartment();var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();var shaft=lift.transform.Find("Elevator Shaft");int trimmed=0;
        var rearCut=new Bounds(new Vector3(0,8.5f,.045f),new Vector3(2.68f,18f,.14f));
        foreach(var renderer in GameObject.Find("Apartment").GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&!r.transform.IsChildOf(lift.cabin)&&new[]{"Plaster Wall","Tiled Wall","Skirting","Wall Return"}.Contains(r.name)).ToArray())
        {
            var bounds=LiftLocalBounds(renderer,shaft);if(bounds.size.z>.30f||!bounds.Intersects(rearCut))continue;
            if(LiftCutRenderer(renderer,shaft,new[]{rearCut},"Wall Return"))trimmed++;
        }
        foreach(var landing in lift.landings)
        {
            foreach(var seal in landing.Find("Entrance Details").Cast<Transform>().Where(t=>t.name=="Pocket Seal"))LiftResize(seal,new Vector3(seal.localPosition.x,1.075f,-.030f),new Vector3(.012f,2.15f,.012f));
            foreach(var leaf in new[]{landing.Find("Left Elevator Door"),landing.Find("Right Elevator Door")})
            {
                LiftResize(leaf,new Vector3(leaf.localPosition.x,1.07f,.015f),new Vector3(.546f,2.10f,.05f));
                var edge=leaf.Find("Door Details/Meeting Edge");LiftResize(edge,new Vector3(edge.localPosition.x,0,-.0265f),new Vector3(.006f,2.08f,.006f));
                var seal=leaf.Find("Meeting Seal");if(seal!=null)LiftResize(seal,new Vector3(seal.localPosition.x,0,.029f),new Vector3(.008f,2.10f,.006f));
            }
        }
        foreach(var leaf in new[]{lift.cabinLeft,lift.cabinRight})
        {
            LiftResize(leaf,new Vector3(leaf.localPosition.x,1.07f,.075f),new Vector3(.55f,2.10f,.032f));
            var edge=leaf.Find("Meeting Edge");LiftResize(edge,new Vector3(edge.localPosition.x,0,.019f),new Vector3(.007f,2.09f,.006f));
        }
        foreach(var t in lift.cabin.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
        string path=Base+"Prefabs/Side Streets.prefab";var streets=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var ground=streets.GetComponentsInChildren<MeshRenderer>().FirstOrDefault(r=>r.name=="Rear Ground"&&r.enabled);
            if(ground!=null){LiftCutRenderer(ground,shaft,new[]{new Bounds(new Vector3(0,-.3f,.9f),new Vector3(2.7f,1f,2.2f))},"Ground Return");PrefabUtility.SaveAsPrefabAsset(streets,path);}
        }
        finally{PrefabUtility.UnloadPrefabContents(streets);}
        var cabinClone=UnityEngine.Object.Instantiate(lift.cabin.gameObject);cabinClone.name="Elevator Cabin";cabinClone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(cabinClone,Base+"Prefabs/Elevator Cabin.prefab");UnityEngine.Object.DestroyImmediate(cabinClone);
        var landingClone=UnityEngine.Object.Instantiate(lift.landings[0].gameObject);landingClone.name="Elevator Landing";landingClone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(landingClone,Base+"Prefabs/Elevator Landing.prefab");UnityEngine.Object.DestroyImmediate(landingClone);
        SaveApartment();return "Trimmed "+trimmed+" remaining wall backs clear of the moving sill/doors; rear service ground now excludes the shaft pit.";
    }
    public static string RepairElevatorClearance()
    {
        EnsureApartment();var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();
        if(lift.transform.Find("Elevator Shaft")!=null)return "Clearance repair already installed.";
        System.IO.Directory.CreateDirectory("References/Elevator Clearance");System.IO.File.Copy("Assets/Scenes/Apartment.unity","References/Elevator Clearance/Apartment.before.unity",true);
        var cabin=lift.cabin;int cutWalls=0;
        foreach(var landing in lift.landings)
        {
            cutWalls+=LiftWallPockets(landing);
            foreach(var old in landing.Cast<Transform>().Where(t=>t.name=="Shaft Wall"||t.name=="Shaft Back Wall").ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
            foreach(var jamb in landing.Cast<Transform>().Where(t=>t.name=="Door Jamb"))LiftResize(jamb,new Vector3(jamb.localPosition.x,1.13f,-.07f),new Vector3(.12f,2.26f,.09f));
            var details=landing.Find("Entrance Details");
            foreach(var cover in details.Cast<Transform>().Where(t=>t.name=="Door Pocket Cover"))LiftResize(cover,new Vector3(Mathf.Sign(cover.localPosition.x)*.94f,1.10f,-.046f),new Vector3(.74f,2.20f,.036f));
            foreach(var reveal in details.Cast<Transform>().Where(t=>t.name=="Door Reveal"))LiftResize(reveal,new Vector3(reveal.localPosition.x,1.075f,-.037f),new Vector3(.018f,2.15f,.024f));
            foreach(var seal in details.Cast<Transform>().Where(t=>t.name=="Pocket Seal"))LiftResize(seal,new Vector3(seal.localPosition.x,1.075f,-.017f),new Vector3(.012f,2.15f,.012f));
            LiftResize(details.Find("Lintel Return"),new Vector3(0,2.16f,-.035f),new Vector3(1.14f,.055f,.14f));
        }
        foreach(var wall in cabin.Cast<Transform>().Where(t=>t.name=="Side Panel"))LiftResize(wall,new Vector3(wall.localPosition.x,1.15f,.925f),new Vector3(.08f,2.3f,1.55f));
        LiftResize(cabin.Find("Cabin Ceiling"),new Vector3(0,2.33f,.925f),new Vector3(1.66f,.12f,1.55f));
        LiftResize(cabin.Find("Cabin Floor"),new Vector3(0,-.055f,.88f),new Vector3(1.5f,.11f,1.44f));
        var returns=cabin.Find("Doorway Returns");
        foreach(var wall in returns.Cast<Transform>().Where(t=>t.name=="Inner Door Return"))LiftResize(wall,new Vector3(Mathf.Sign(wall.localPosition.x)*.935f,1.07f,.135f),new Vector3(.79f,2.14f,.06f));
        foreach(var trim in returns.Cast<Transform>().Where(t=>t.name=="Inner Jamb Trim"))LiftResize(trim,new Vector3(trim.localPosition.x,1.07f,.173f),new Vector3(.017f,2.14f,.018f));
        LiftResize(returns.Find("Inner Header"),new Vector3(0,2.205f,.135f),new Vector3(1.66f,.17f,.06f));
        LiftResize(returns.Find("Floor Apron"),new Vector3(0,-.055f,.08f),new Vector3(1.54f,.11f,.16f));
        LiftResize(returns.Find("Inner Sill"),new Vector3(0,.006f,.08f),new Vector3(1.09f,.012f,.16f));
        int groove=0;foreach(var part in returns.Cast<Transform>().Where(t=>t.name=="Sill Groove")){var p=part.localPosition;p.z=.025f+groove++*.045f;part.localPosition=p;}
        foreach(var skirting in cabin.Find("Cabin Details").Cast<Transform>().Where(t=>t.name=="Cabin Skirting"))LiftResize(skirting,new Vector3(skirting.localPosition.x,.07f,.8775f),new Vector3(.015f,.13f,1.395f));
        var shaft=G("Elevator Shaft",lift.transform);shaft.SetPositionAndRotation(cabin.position,cabin.rotation);
        foreach(float side in new[]{-1f,1f})B("Side Wall",shaft,new Vector3(side*.98f,8.6f,1.095f),new Vector3(.18f,18.1f,1.81f),"Concrete");
        B("Rear Wall",shaft,new Vector3(0,8.6f,1.91f),new Vector3(2.14f,18.1f,.18f),"Concrete");
        B("Pit Floor",shaft,new Vector3(0,-.50f,.90f),new Vector3(2.7f,.10f,2.2f),"Concrete");
        B("Shaft Ceiling",shaft,new Vector3(0,17.70f,.90f),new Vector3(2.7f,.10f,2.2f),"Concrete");
        for(int floor=0;floor<6;floor++)B("Landing Spandrel",shaft,new Vector3(0,floor*2.9f+2.55f,-.10f),new Vector3(2.68f,.70f,.12f),"Concrete");
        foreach(var t in cabin.GetComponentsInChildren<Transform>()){GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);var renderer=t.GetComponent<Renderer>();if(renderer!=null)renderer.lightmapIndex=-1;}
        var clone=UnityEngine.Object.Instantiate(cabin.gameObject);clone.name="Elevator Cabin";clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/Elevator Cabin.prefab");UnityEngine.Object.DestroyImmediate(clone);
        var landingClone=UnityEngine.Object.Instantiate(lift.landings[0].gameObject);landingClone.name="Elevator Landing";landingClone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(landingClone,Base+"Prefabs/Elevator Landing.prefab");UnityEngine.Object.DestroyImmediate(landingClone);
        SaveApartment();return "Continuous shaft installed with cabin clearance; "+cutWalls+" wall pieces recessed for landing door pockets; cabin tracks, returns, floor and ceiling corrected.";
    }
}
