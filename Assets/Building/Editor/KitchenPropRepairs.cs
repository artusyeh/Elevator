using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string SeatFloatingKettles()
    {
        EnsureApartment();var report=new List<string>();
        foreach(string id in new[]{"201","503"})
        {
            var room=GameObject.Find("Room "+id).transform;var sink=room.Find("kitchen-base-unit-sink");var kettle=room.Find("cream-dome-kettle");var bounds=BoundsOf(kettle);float top=float.NegativeInfinity;
            var ray=new Ray(new Vector3(bounds.center.x,Mathf.Max(bounds.max.y,BoundsOf(sink).max.y)+.1f,bounds.center.z),Vector3.down);
            foreach(var filter in sink.GetComponentsInChildren<MeshFilter>())
            {var collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;RaycastHit hit;if(collider.Raycast(ray,out hit,3))top=Mathf.Max(top,hit.point.y);UnityEngine.Object.DestroyImmediate(collider);}
            if(float.IsNegativeInfinity(top))throw new Exception("No draining board underneath kettle "+id);
            kettle.position+=Vector3.up*(top+.0005f-bounds.min.y);PrefabUtility.RecordPrefabInstancePropertyModifications(kettle);SaveCommonModule(room,InteriorPrefabs+"Room "+id+".prefab");report.Add(id+" kettle base clearance: "+((BoundsOf(kettle).min.y-top)*1000).ToString("F1")+" mm");
        }
        SaveApartment();return string.Join("\n",report);
    }
    const string RepairModels="Assets/Building/Models/Interiors/";
    static Mesh SavedRepairMesh(string name,List<Vector3> vertices,List<int> faces)
    {
        string path=RepairModels+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh==null){mesh=new Mesh{name=name};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(faces,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);return mesh;
    }
    static Vector3[] RoundedSinkLoop(Vector2 center,Vector2 size,float radius,float y)
    {
        var points=new List<Vector3>();for(int corner=0;corner<4;corner++)for(int i=0;i<8;i++)
        {float angle=(corner*90+i*90f/8)*Mathf.Deg2Rad;float cx=corner==0||corner==3?size.x*.5f-radius:-size.x*.5f+radius;float cz=corner<2?size.y*.5f-radius:-size.y*.5f+radius;points.Add(new Vector3(center.x+cx+Mathf.Cos(angle)*radius,y,center.y+cz+Mathf.Sin(angle)*radius));}return points.ToArray();
    }
    static Mesh ProperSinkInsert()
    {
        var loops=new[]{RoundedSinkLoop(Vector2.zero,new Vector2(.86f,.50f),.018f,.904f),RoundedSinkLoop(new Vector2(.19f,0),new Vector2(.39f,.39f),.027f,.904f),RoundedSinkLoop(new Vector2(.19f,0),new Vector2(.31f,.30f),.035f,.722f),Enumerable.Range(0,32).Select(i=>new Vector3(.19f+Mathf.Cos(i*Mathf.PI/16)*.035f,.722f,Mathf.Sin(i*Mathf.PI/16)*.035f)).ToArray()};
        var vertices=loops.SelectMany(x=>x).ToList();var tris=new List<int>();for(int layer=0;layer<3;layer++)for(int i=0;i<32;i++){int n=(i+1)%32,a=layer*32+i,b=(layer+1)*32+i,c=(layer+1)*32+n,d=layer*32+n;tris.AddRange(new[]{a,b,c,a,c,d});}return SavedRepairMesh("Formed Sink Basin",vertices,tris);
    }
    static Mesh ClearSinkInterior(Transform root)
    {
        string path=RepairModels+"Sink Body Cleared.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(saved!=null)return saved;
        var filter=root.GetComponentsInChildren<MeshFilter>().First(f=>f.name=="sink_unit_body");var source=filter.sharedMesh;var local=source.vertices.Select(v=>root.InverseTransformPoint(filter.transform.TransformPoint(v))).ToArray();var parents=Enumerable.Range(0,local.Length).ToArray();Func<int,int> find=null;find=i=>{while(parents[i]!=i){parents[i]=parents[parents[i]];i=parents[i];}return i;};
        var triangles=source.triangles;for(int i=0;i<triangles.Length;i+=3){parents[find(triangles[i])]=find(triangles[i+1]);parents[find(triangles[i+1])]=find(triangles[i+2]);}
        var remove=new HashSet<int>();foreach(var group in Enumerable.Range(0,local.Length).GroupBy(i=>find(i)))
        {var bound=new Bounds(local[group.First()],Vector3.zero);foreach(int i in group)bound.Encapsulate(local[i]);if(bound.min.x>-.45f&&bound.max.x<.45f&&bound.min.y>.70f&&bound.max.y<.915f)foreach(int i in group)remove.Add(i);}
        saved=UnityEngine.Object.Instantiate(source);saved.name="Sink Body Cleared";for(int slot=0;slot<source.subMeshCount;slot++){var old=source.GetTriangles(slot);var clean=new List<int>();for(int i=0;i<old.Length;i+=3)if(!remove.Contains(old[i]))clean.AddRange(new[]{old[i],old[i+1],old[i+2]});saved.SetTriangles(clean,slot);}
        var edited=source.vertices;for(int i=0;i<local.Length;i++){var p=local[i];if(!remove.Contains(i)&&p.y>.70f&&p.y<.862f&&p.z>.16f&&p.z<.215f){p.z=.215f;edited[i]=filter.transform.InverseTransformPoint(root.TransformPoint(p));}}saved.vertices=edited;saved.RecalculateBounds();AssetDatabase.CreateAsset(saved,path);return saved;
    }
    static void FitSinkInsert(Transform root,Mesh body,Mesh insert)
    {
        root.GetComponentsInChildren<MeshFilter>().First(f=>f.name=="sink_unit_body").sharedMesh=body;
        if(root.Find("Formed Stainless Basin")!=null)return;var group=G("Formed Stainless Basin",root);var surface=G("Continuous Basin",group);surface.gameObject.AddComponent<MeshFilter>().sharedMesh=insert;surface.gameObject.AddComponent<MeshRenderer>().sharedMaterial=M("Kitchen Aged Stainless");
        Rod("Drain Recess",group,new Vector3(.19f,.715f,0),new Vector3(.19f,.719f,0),.034f,"Black Rubber");
        foreach(float z in new[]{-.018f,0,.018f}){float w=Mathf.Sqrt(.032f*.032f-z*z);Rod("Drain Strainer",group,new Vector3(.19f-w,.720f,z),new Vector3(.19f+w,.720f,z),.0015f,"Kitchen Aged Stainless");}
        foreach(float x in new[]{-.33f,-.275f,-.22f,-.165f,-.11f})Rod("Draining Board Ridge",group,new Vector3(x,.906f,-.155f),new Vector3(x,.906f,.155f),.002f,"Kitchen Aged Stainless");CommonDynamic(group);
    }
    static void ProperHob(Transform room)
    {
        foreach(var old in room.Cast<Transform>().Where(t=>t.name=="Burner"))old.gameObject.SetActive(false);
        if(room.Find("Old Two Burner Hob")!=null)return;var top=room.Find("Gas Hob");var bounds=BoundsOf(top);var hob=G("Old Two Burner Hob",room);hob.localPosition=new Vector3(top.localPosition.x,bounds.max.y-room.position.y,top.localPosition.z);
        foreach(float x in new[]{-.135f,.135f})
        {
            Rod("Burner Base",hob,new Vector3(x,.001f,0),new Vector3(x,.008f,0),.072f,"Cabin Aged Steel");
            Rod("Cast Iron Burner Cap",hob,new Vector3(x,.010f,0),new Vector3(x,.019f,0),.044f,"Black Rubber");
            foreach(int side in new[]{-1,1}){B("Pan Support",hob,new Vector3(x+side*.059f,.026f,0),new Vector3(.048f,.015f,.013f),"Cabin Aged Steel",false,.003f);B("Pan Support",hob,new Vector3(x,.026f,side*.059f),new Vector3(.013f,.015f,.048f),"Cabin Aged Steel",false,.003f);}
            Rod("Gas Control Knob",hob,new Vector3(x,.008f,.215f),new Vector3(x,.026f,.215f),.017f,"Black Rubber");
        }
        CommonDynamic(hob);
    }
    static Mesh HollowTeaCup()
    {
        var vertices=new List<Vector3>();foreach(var ring in new[]{new Vector2(.45f,-1),new Vector2(.50f,.90f),new Vector2(.48f,1),new Vector2(.385f,.94f),new Vector2(.35f,-.72f)})for(int i=0;i<32;i++){float a=i*Mathf.PI/16;vertices.Add(new Vector3(Mathf.Cos(a)*ring.x,ring.y,Mathf.Sin(a)*ring.x));}
        var faces=new List<int>();for(int j=0;j<4;j++)for(int i=0;i<32;i++){int n=(i+1)%32;faces.AddRange(new[]{j*32+i,(j+1)*32+i,(j+1)*32+n,j*32+i,(j+1)*32+n,j*32+n});}vertices.Add(new Vector3(0,-.72f,0));for(int i=0;i<32;i++)faces.AddRange(new[]{160,128+(i+1)%32,128+i});return SavedRepairMesh("Hollow Ceramic Tea Cup",vertices,faces);
    }
    static float ActualTableHeight(Transform table,Vector3 at)
    {
        float height=float.NegativeInfinity;foreach(var filter in table.GetComponentsInChildren<MeshFilter>())
        {var c=filter.gameObject.AddComponent<MeshCollider>();c.sharedMesh=filter.sharedMesh;RaycastHit hit;if(c.Raycast(new Ray(at+Vector3.up,Vector3.down),out hit,2))height=Mathf.Max(height,hit.point.y);UnityEngine.Object.DestroyImmediate(c);}if(float.IsNegativeInfinity(height))throw new Exception("No table surface underneath prop");return height;
    }
    static void FixTeaTable(Transform room,Mesh cupMesh)
    {
        var table=room.Find("chinese_tea_table");var tray=room.Find("Tea Tray");var b=BoundsOf(tray);float top=ActualTableHeight(table,b.center);tray.position+=Vector3.up*(top+.001f-b.min.y);float trayTop=BoundsOf(tray).max.y;
        foreach(var cup in room.Cast<Transform>().Where(t=>t.name=="Tea Cup"&&t.gameObject.activeSelf))
        {cup.GetComponent<MeshFilter>().sharedMesh=cupMesh;var cb=BoundsOf(cup);cup.position+=Vector3.up*(trayTop+.0005f-cb.min.y);var tea=room.Cast<Transform>().Where(t=>t.name=="Tea Surface").OrderBy(t=>Mathf.Abs(t.position.x-cup.position.x)).First();var p=tea.position;p.y=BoundsOf(cup).max.y-.005f;tea.position=p;}
        int index=0;foreach(var box in room.Cast<Transform>().Where(t=>t.name=="Medicine Box")){var p=box.position;p.x=table.position.x-.16f+index++*.083f;p.z=table.position.z+.23f;box.position=p;var bb=BoundsOf(box);box.position+=Vector3.up*(ActualTableHeight(table,bb.center)+.001f-bb.min.y);}
    }
    static void GoodSchoolbooks(Transform room)
    {
        var study=room.Find("Family Study Corner");foreach(var part in study.Cast<Transform>().Where(t=>t.name=="Schoolbook"||t.name=="Page Edges"))part.gameObject.SetActive(false);
        if(study.Find("Worn Schoolbooks")!=null)return;var stack=G("Worn Schoolbooks",study);float baseY=study.Find("Scarred Desk Top").GetComponent<Renderer>().bounds.max.y-room.position.y+.0008f;
        string[] titles={"Arithmetic","Japanese","Science","Notebook"};for(int i=0;i<titles.Length;i++)
        {
            var book=G(titles[i],stack);book.localPosition=new Vector3(-3.84f+i*.004f,baseY+i*.023f,6.25f);book.localRotation=Quaternion.Euler(0,new[]{-5f,3f,-2f,7f}[i],0);
            var mat=Plain("Schoolbook "+titles[i],Color.white,.10f);mat.SetTexture("_BaseMap",DetailMap("Assets/Building/Textures/Interiors/Books/"+titles[i]+".png"));
            B("Lower Cover",book,new Vector3(0,.001f,0),new Vector3(.145f,.002f,.205f),"Interior Notebook Cover",false,.0005f);
            B("Aged Page Block",book,new Vector3(.001f,.011f,0),new Vector3(.139f,.018f,.199f),"Old Book Pages",false,.0005f);
            B("Upper Cover",book,new Vector3(0,.021f,0),new Vector3(.145f,.002f,.205f),"Interior Notebook Cover",false,.0005f);
            B("Cloth Spine",book,new Vector3(-.071f,.011f,0),new Vector3(.003f,.018f,.205f),"Interior Notebook Cover",false,.0005f);
            Q("Printed Cover",book,new Vector3(0,.0223f,0),new Vector2(.140f,.200f),mat,Quaternion.Euler(90,0,0));
        }
        var chair=room.Find("painted_wooden_chair_02");SeatInterior(chair,room,new Vector3(-3.57f,0,5.64f),13);CommonDynamic(stack);
    }
    static void FitStorageCartons(Transform room)
    {
        foreach(var shelf in room.Cast<Transform>().Where(t=>t.name=="Shelf_01"))
        {
            var bounds=BoundsOf(shelf);if(Mathf.Min(bounds.size.x,bounds.size.z)<.35f)shelf.localScale=new Vector3(shelf.localScale.x,shelf.localScale.y,shelf.localScale.z*1.90f);
            var boxes=room.Cast<Transform>().Where(t=>t.name=="Delivery Carton").OrderBy(t=>Vector2.Distance(new Vector2(t.position.x,t.position.z),new Vector2(shelf.position.x,shelf.position.z))).Take(3).OrderBy(t=>t.localPosition.y).ToArray();
            bool side=shelf.localPosition.z<2;for(int i=0;i<boxes.Length;i++){var box=boxes[i];box.localScale*=.21f/BoundsOf(box).size.y;SeatInterior(box,room,new Vector3(shelf.localPosition.x,.32f+i*.45f,shelf.localPosition.z),side?0:90);}
        }
        SeatStorageCartons(room);
    }
    public static string RepairKitchenAndDeskProps()
    {
        EnsureApartment();var steel=Plain("Kitchen Aged Stainless",new Color(.38f,.41f,.39f),.27f,.70f);
        var pages=Plain("Old Book Pages",new Color(.63f,.60f,.48f),.05f);pages.SetTexture("_BaseMap",DetailMap("Assets/Building/Textures/Interiors/Books/Page Edges.png"));pages.SetTextureScale("_BaseMap",new Vector2(4,10));
        var body=ClearSinkInterior(GameObject.Find("Room 201").transform.Find("kitchen-base-unit-sink"));var insert=ProperSinkInsert();
        foreach(string id in new[]{"201","306","503"})
        {
            var room=GameObject.Find("Room "+id).transform;var sink=room.Find("kitchen-base-unit-sink");FitSinkInsert(sink,body,insert);sink.localScale=new Vector3(sink.localScale.x,(id=="306"?.82f:.824f)/.900f,sink.localScale.z);PrefabUtility.RecordPrefabInstancePropertyModifications(sink);
            if(id!="306")
            {ProperHob(room);var kettle=room.Find("cream-dome-kettle");var at=sink.TransformPoint(new Vector3(-.22f,.907f,.025f));SeatInterior(kettle,room,room.InverseTransformPoint(at),0);PrefabUtility.RecordPrefabInstancePropertyModifications(kettle);}
            else{var bottle=room.Find("Cleaning Bottle");var at=bottle.position;var b=BoundsOf(bottle);bottle.position+=Vector3.up*(room.position.y+.827f-b.min.y);}
        }
        var source=PrefabUtility.LoadPrefabContents(InteriorPrefabs+"kitchen-base-unit-sink.prefab");try{FitSinkInsert(source.transform,body,insert);PrefabUtility.SaveAsPrefabAsset(source,InteriorPrefabs+"kitchen-base-unit-sink.prefab");}finally{PrefabUtility.UnloadPrefabContents(source);}
        FixTeaTable(GameObject.Find("Room 201").transform,HollowTeaCup());GoodSchoolbooks(GameObject.Find("Room 503").transform);FitStorageCartons(GameObject.Find("Room 602").transform);
        var calendar=M("Interior Calendar");calendar.SetTexture("_BaseMap",DetailMap("Assets/Building/Textures/Interiors/Calendar.png"));EditorUtility.SetDirty(calendar);
        foreach(string id in new[]{"201","306","503","602"})FinishInterior(GameObject.Find("Room "+id).transform,RoomPlan(id));AssetDatabase.SaveAssets();SaveApartment();return "Basin geometry and cabinet rails corrected; proper two-burner hobs; cups and medicine seated separately; detailed books and calendar; chair angled toward desk; cartons fitted within deeper shelves.";
    }
}
