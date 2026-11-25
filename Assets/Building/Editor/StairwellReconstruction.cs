using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string StairReview="References/Stairwell/";
    const float StairWindowHeight=3.12f;
    [Serializable]class StairMeshData{public Vector3[] vertices,normals;public int[] triangles;}
    static Mesh StairFlightMesh()
    {
        string path=BuildingBase+"Models/Concrete Flight.asset";var data=JsonUtility.FromJson<StairMeshData>(File.ReadAllText(StairReview+"Concrete Flight.json"));var mesh=new Mesh{name="Concrete Flight"};mesh.vertices=data.vertices;mesh.normals=data.normals;mesh.triangles=data.triangles;mesh.uv=data.vertices.Select(v=>new Vector2(v.x,v.z)).ToArray();mesh.RecalculateBounds();mesh.RecalculateTangents();var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing!=null){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);return mesh;
    }
    static void StairMaterials()
    {
        CorridorEndMaterials();
        Surface("Stair Concrete","Assets/Alley/Textures/PBR/concrete_wall_006/",.80f,new Color(.58f,.59f,.55f),.28f);
        Surface("Stair Green Paint","Assets/Alley/Textures/PBR/painted_plaster_wall/",1.40f,new Color(.25f,.32f,.26f),.23f);
        var plaster=Plain("Stair Plaster",new Color(.73f,.72f,.64f),.15f);plaster.CopyPropertiesFromMaterial(M("Yellowed Plaster"));plaster.SetColor("_BaseColor",new Color(.72f,.72f,.64f));EditorUtility.SetDirty(plaster);
        var rail=Plain("Stair Rail",new Color(.42f,.49f,.43f),.32f,.65f);rail.CopyPropertiesFromMaterial(M("Green Steel"));rail.SetColor("_BaseColor",new Color(.48f,.53f,.46f));EditorUtility.SetDirty(rail);
    }
    static void StairWall(Transform parent,string name,Bounds box,Bounds[] cuts,string material)
    {
        var pieces=new List<Bounds>{box};foreach(var cut in cuts)pieces=pieces.SelectMany(b=>LiftSubtract(b,cut)).ToList();foreach(var b in pieces)if(b.size.minComponent()>.0001f)B(name,parent,b.center,b.size,material,true,.001f);
    }
    static void StairEnclosure(Transform apartment)
    {
        var old=apartment.Find("Stairwell Enclosure");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);var root=G("Stairwell Enclosure",apartment);
        var windows=Enumerable.Range(0,5).Select(i=>new Bounds(new Vector3(-3.8f,i*2.9f+StairWindowHeight,13.98f),new Vector3(1.30f,.80f,.7f))).ToArray();
        var doors=Enumerable.Range(0,6).Select(i=>new Bounds(new Vector3(-2.21f,i*2.9f+1.10625f,9.56f),new Vector3(.7f,2.2125f,1.06f))).ToArray();
        for(int floor=0;floor<6;floor++)
        {
            float level=floor*2.9f,full=floor==5?2.78f:2.9f;
            foreach(bool lower in new[]{true,false})
            {
                float height=lower?1.45f:full-1.45f,y=level+(lower?height*.5f:1.45f+height*.5f);string mat=lower?"Stair Green Paint":"Stair Plaster",name=lower?"Painted Wall":"Plaster Wall";
                StairWall(root,name,new Bounds(new Vector3(-5.41f,y,11.185f),new Vector3(.18f,height,5.59f)),new Bounds[0],mat);
                StairWall(root,name,new Bounds(new Vector3(-2.21f,y,11.185f),new Vector3(.18f,height,5.59f)),doors,mat);
                StairWall(root,name,new Bounds(new Vector3(-3.8f,y,8.39f),new Vector3(3.4f,height,.18f)),new Bounds[0],mat);
                StairWall(root,name,new Bounds(new Vector3(-3.8f,y,13.98f),new Vector3(3.4f,height,.18f)),windows,mat);
            }
        }
        B("Roof Slab",root,new Vector3(-3.8f,17.37f,11.185f),new Vector3(3.4f,.18f,5.59f),"Stair Concrete",true,.002f);
        B("Foundation",root,new Vector3(-3.8f,-.24f,11.185f),new Vector3(3.4f,.18f,5.59f),"Stair Concrete",true,.002f);
    }
    static Transform StairRail(Transform parent,Vector3 a,Vector3 b,float radius=.022f)
    {var rod=Rod("Handrail",parent,a,b,radius,"Stair Rail");var c=rod.gameObject.AddComponent<CapsuleCollider>();c.direction=1;c.height=2;c.radius=.5f;return rod;}
    static void StairGuard(Transform parent,Vector3 a,Vector3 b,float bottom)
    {
        StairRail(parent,a,b);int n=Mathf.CeilToInt(Vector3.Distance(a,b)/.13f);for(int i=0;i<=n;i++){var at=Vector3.Lerp(a,b,i/(float)n);StairRail(parent,new Vector3(at.x,bottom,at.z),at,.008f);}
    }
    static void FlightRailing(Transform parent,float x,bool reverse)
    {
        Vector3 a=reverse?new Vector3(x,2.41f,12.60f):new Vector3(x,.96f,9.74f),b=reverse?new Vector3(x,3.86f,10.26f):new Vector3(x,2.41f,12.08f);
        StairRail(parent,a,b);StairRail(parent,b,b+Vector3.forward*(reverse?-.40f:.52f));
        for(int i=0;i<9;i++)
        {
            float z=reverse?12.34f-(i+.5f)*.26f:10+(i+.5f)*.26f,top=(reverse?1.45f:0)+(i+1)*(2.9f/18),t=(z-a.z)/(b.z-a.z);var end=new Vector3(x,Mathf.Lerp(a.y,b.y,Mathf.Clamp01(t)),z);
            StairRail(parent,new Vector3(x,top,z),end,.009f);B("Post Foot",parent,new Vector3(x,top+.005f,z),new Vector3(.055f,.010f,.055f),"Stair Rail",false,.001f);
        }
        float postZ=reverse?9.86f:9.96f,baseY=reverse?2.9f:0,railY=reverse?3.86f:Mathf.Lerp(a.y,b.y,(postZ-a.z)/(b.z-a.z));StairRail(parent,new Vector3(x,baseY,postZ),new Vector3(x,railY,postZ),.010f);
    }
    static void PrepareStairDoor(Transform stairs)
    {
        var door=stairs.Find("Stairwell Door");var old=door.GetComponent<LockedDoor>();if(old!=null)UnityEngine.Object.DestroyImmediate(old);var pivot=door.Find("Door Pivot");
        if(pivot==null)
        {
            pivot=G("Door Pivot",door);pivot.localPosition=new Vector3(-.473f,0,-.014f);
            foreach(var t in door.Cast<Transform>().Where(t=>new[]{"Door Leaf","Kick Plate","Lock Plate","Handle Spindle","Lever Handle","Keyhole","Door Hinge","Door Closer","Closer Arm","Stairwell Plaque"}.Contains(t.name)).ToArray())t.SetParent(pivot,true);
        }
        var control=door.GetComponent<StairAccessDoor>()??door.gameObject.AddComponent<StairAccessDoor>();control.leaf=pivot;control.locked=true;control.open=false;pivot.localRotation=Quaternion.identity;
        foreach(var t in pivot.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
        var threshold=stairs.Find("Stair Threshold");LiftResize(threshold,new Vector3(-2.20f,-.02f,9.56f),new Vector3(.20f,.04f,.95f));
    }
    static void StairFixture(Transform parent,Vector3 at,Quaternion mounting,Quaternion beam)
    {
        var root=G("Wall Light",parent);root.localPosition=at;root.localRotation=mounting;
        var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/Residential Fluorescent.prefab"),root);model.name="Covered Fluorescent";
        var lightRoot=G("Light",root);lightRoot.position=root.position+root.rotation*Vector3.down*.095f;lightRoot.rotation=beam;var light=lightRoot.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.useColorTemperature=true;light.colorTemperature=5100;light.color=new Color(.91f,.96f,1);light.intensity=3.8f;light.range=4.4f;light.spotAngle=145;light.innerSpotAngle=100;light.shadows=LightShadows.Soft;light.shadowNearPlane=.04f;light.shadowBias=.012f;light.shadowNormalBias=.025f;light.lightmapBakeType=LightmapBakeType.Realtime;
    }
    static void StairWindow(Transform parent)
    {
        var w=G("Landing Window",parent);w.localPosition=new Vector3(-3.8f,StairWindowHeight,13.89f);
        foreach(float x in new[]{-.674f,.674f}){B("Window Reveal",w,new Vector3(x,0,.16f),new Vector3(.045f,.85f,.36f),"Stair Plaster",true,.001f);B("Window Frame",w,new Vector3(x,0,-.016f),new Vector3(.065f,.88f,.055f),"Stair Rail",false,.002f);}
        foreach(float y in new[]{-.416f,.416f}){B("Window Reveal",w,new Vector3(0,y,.16f),new Vector3(1.39f,.045f,.36f),"Stair Plaster",true,.001f);B("Window Frame",w,new Vector3(0,y,-.016f),new Vector3(1.41f,.055f,.055f),"Stair Rail",false,.002f);}
        foreach(float x in new[]{-.674f,.674f})B("Outer Frame",w,new Vector3(x,0,.35f),new Vector3(.065f,.88f,.055f),"Stair Rail",false,.002f);
        foreach(float y in new[]{-.416f,.416f})B("Outer Frame",w,new Vector3(0,y,.35f),new Vector3(1.41f,.055f,.055f),"Stair Rail",false,.002f);
        B("Mullion",w,new Vector3(0,0,.16f),new Vector3(.035f,.79f,.40f),"Stair Rail",false,.002f);B("Window Sill",w,new Vector3(0,-.4635f,.18f),new Vector3(1.48f,.065f,.66f),"Stair Concrete",true,.003f);
        var glass=Q("Window Glass",w,new Vector3(0,0,.31f),new Vector2(1.28f,.78f),M("Corridor Window Glass"),Quaternion.identity);var collider=glass.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(1,1,.012f);
        foreach(float x in new[]{-.50f,-.30f,-.10f,.10f,.30f,.50f})Rod("Window Bar",w,new Vector3(x,-.37f,.41f),new Vector3(x,.37f,.41f),.008f,"Stair Rail");
        foreach(float y in new[]{-.37f,.37f}){B("Bar Rail",w,new Vector3(0,y,.41f),new Vector3(1.20f,.025f,.025f),"Stair Rail",false,.002f);foreach(float x in new[]{-.57f,.57f})Rod("Bar Anchor",w,new Vector3(x,y,.41f),new Vector3(x,y,.35f),.011f,"Stair Rail");}
        Lamp(w,new Vector3(0,.15f,.65f),.55f,3.2f,7800,Quaternion.LookRotation(new Vector3(0,-.35f,-1)),LightType.Spot);w.Find("Light").name="Night Light";ObscureStairWindow(w,Mathf.RoundToInt(parent.position.y/2.9f)+1);
    }
    static void RebuildStairFloor(Transform stairs,int floor,Mesh flight)
    {
        foreach(var t in stairs.Cast<Transform>().Where(t=>!new[]{"Stairwell Door","Door Surround","Stair Threshold","Emergency Light"}.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        var structure=G("Structure",stairs);B("Floor Landing",structure,new Vector3(-3.81f,-.10f,9.24f),new Vector3(3.02f,.20f,1.52f),"Stair Concrete",true,.001f);
        if(floor<5)
        {
            foreach(bool reverse in new[]{false,true})
            {
                var t=G("Concrete Flight",structure);t.localPosition=new Vector3(reverse?-2.94f:-4.68f,reverse?1.45f:0,reverse?12.34f:10);t.localRotation=Quaternion.Euler(0,reverse?180:0,0);t.gameObject.AddComponent<MeshFilter>().sharedMesh=flight;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=M("Stair Concrete");t.gameObject.AddComponent<MeshCollider>().sharedMesh=flight;
            }
            B("Intermediate Landing",structure,new Vector3(-3.81f,1.35f,13.115f),new Vector3(3.02f,.20f,1.55f),"Stair Concrete",true,.001f);
            StairWindow(stairs);StairFixture(stairs,new Vector3(-5.319f,3.38f,12.94f),Quaternion.LookRotation(Vector3.up,Vector3.left),Quaternion.LookRotation(new Vector3(1,-.60f,-.30f)));
        }
        RepairLandingRailings(stairs,floor);
        StairFixture(stairs,new Vector3(-3.95f,2.13f,8.481f),Quaternion.Euler(-90,0,0),Quaternion.LookRotation(new Vector3(0,-.55f,1)));
        PipeRun(stairs,new Vector3(-5.28f,0,13.58f),new Vector3(-5.28f,2.9f,13.58f),.027f);PipeRun(stairs,new Vector3(-5.285f,2.55f,8.7f),new Vector3(-5.285f,2.55f,13.55f),.010f);
        Weather(stairs,"Water Stain",new Vector3(-3.8f+(floor%3-.8f)*.15f,2.04f,13.889f),new Vector2(1.1f,.60f),Quaternion.identity);
        Weather(stairs,"Moss",new Vector3(-5.319f,.26f+floor%2*.15f,12.8f-floor*.11f),new Vector2(.65f,.30f),Quaternion.Euler(0,-90,0));
        Weather(stairs,"Water Stain",new Vector3(-5.319f,1.42f,10.9f+floor*.17f),new Vector2(.45f,1.15f),Quaternion.Euler(0,-90,0));
        if(floor<5)Weather(stairs,"Ground Stain",new Vector3(-3.95f+floor*.08f,1.451f,13.10f),new Vector2(.65f,.46f),Quaternion.Euler(90,0,floor*17));
        MountFloorPlaque(stairs,floor+1,new Vector3(-5.314f,1.74f,9.2f),Quaternion.Euler(0,-90,0));PrepareStairDoor(stairs);
        var circuit=stairs.GetComponent<LightCircuit>();var night=stairs.GetComponentsInChildren<Light>().Where(l=>l.name=="Night Light").ToArray();circuit.lights=stairs.GetComponentsInChildren<Light>().Where(l=>!night.Contains(l)&&!l.GetComponentsInParent<Transform>().Any(t=>t.name=="Emergency Light")).ToArray();circuit.emitters=stairs.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&r.sharedMaterials.Any(m=>m!=null&&m.HasProperty("_EmissionColor")&&m.GetColor("_EmissionColor").maxColorComponent>.001f)&&!r.GetComponentsInParent<Transform>().Any(t=>t.name=="Emergency Light")).ToArray();EditorUtility.SetDirty(circuit);
    }
    static void FitStairFacade(Transform exterior,bool interior)
    {
        var facade=exterior.Find("Building Facade");var windows=facade.Cast<Transform>().Where(t=>t.name=="Stair Window").OrderBy(t=>t.localPosition.y).ToArray();
        for(int i=0;i<windows.Length;i++){var p=windows[i].localPosition;p.y=i*2.9f+StairWindowHeight;windows[i].localPosition=p;windows[i].gameObject.SetActive(i<5);PrefabUtility.RecordPrefabInstancePropertyModifications(windows[i]);if(interior)foreach(var r in windows[i].GetComponentsInChildren<Renderer>(true)){r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}}
        foreach(var r in facade.GetComponentsInChildren<Renderer>(true).Where(r=>new[]{"Rear Window Base","Rear Window Lintel"}.Contains(r.name))){r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);foreach(var c in r.GetComponents<Collider>()){c.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(c);}}
        foreach(var old in facade.Cast<Transform>().Where(t=>t.name=="Stairwell Facade").ToArray())
        {if(!interior||PrefabUtility.IsAddedGameObjectOverride(old.gameObject))UnityEngine.Object.DestroyImmediate(old.gameObject);}
        // The prefab owns the wall cuts. Scene instances only override the glazing;
        // adding another facade here leaves stale coplanar walls after a prefab update.
        if(interior)return;
        var fill=G("Stairwell Facade",facade);var cuts=Enumerable.Range(0,5).Select(i=>new Bounds(new Vector3(-3.8f,i*2.9f+StairWindowHeight,14.12f),new Vector3(1.3f,.80f,.6f))).ToArray();StairWall(fill,"Facade Panel",new Bounds(new Vector3(-3.8f,8.64f,14.12f),new Vector3(1.35f,17.28f,.18f)),cuts,"Concrete");
    }
    public static string ReconstructStairwell()
    {
        EnsureApartment();Directory.CreateDirectory(StairReview);if(!File.Exists(StairReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",StairReview+"Apartment.before.unity");StairMaterials();PrepareResidentialFixture();var mesh=StairFlightMesh();var apartment=GameObject.Find("Apartment").transform;StairEnclosure(apartment);
        for(int i=0;i<6;i++)RebuildStairFloor(GameObject.Find(i==0?"Ground Floor":"Floor "+(i+1)).transform.Find("Stairwell"),i,mesh);
        FitStairFacade(GameObject.Find("Aoba Exterior").transform,true);string path=Base+"Prefabs/Aoba Exterior.prefab";var prefab=PrefabUtility.LoadPrefabContents(path);try{FitStairFacade(prefab.transform,false);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())FitShadowPolicy(root.transform);SaveApartment();return "Rebuilt ten solid concrete flights and six landings; continuous enclosure, physical rails, five aligned windows, eleven mounted lamps and six configurable access doors. Saved Apartment.";
    }
}
static class StairBoundsExtensions{public static float minComponent(this Vector3 v){return Mathf.Min(v.x,Mathf.Min(v.y,v.z));}}
