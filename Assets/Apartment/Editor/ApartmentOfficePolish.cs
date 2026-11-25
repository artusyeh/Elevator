using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class ApartmentBuilder
{
    static void RecordPose(Transform t){if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);}
    static void SeatOn(Transform t,Vector3 at,float yaw)
    {t.SetPositionAndRotation(at,Quaternion.Euler(0,yaw,0));t.position+=Vector3.up*(at.y+.002f-BoundsOf(t).min.y);RecordPose(t);}
    static Transform Collect(string name,Transform parent,params Transform[] children)
    {var group=G(name,parent);foreach(var t in children)if(t!=null)t.SetParent(group,true);return group;}
    static void Lead(Transform parent,params Vector3[] points)
    {for(int i=0;i<points.Length-1;i++)Rod("Equipment Cable",parent,points[i],points[i+1],.0035f,"Black Rubber");}
    static void Mug(Transform parent,Vector3 at)
    {
        var root=G("Caretaker Mug",parent);root.localPosition=at;
        var mesh=new Mesh();var vs=new System.Collections.Generic.List<Vector3>();var uv=new System.Collections.Generic.List<Vector2>();var tris=new System.Collections.Generic.List<int>();float[] radii={.041f,.044f,.0365f,.034f},heights={.005f,.095f,.095f,.012f};
        for(int j=0;j<4;j++)for(int i=0;i<=32;i++){float a=i*Mathf.PI/16;vs.Add(new Vector3(Mathf.Cos(a)*radii[j],heights[j],Mathf.Sin(a)*radii[j]));uv.Add(new Vector2(i/32f,j/3f));if(j<3&&i<32){int k=j*33+i;tris.AddRange(new[]{k,k+33,k+1,k+1,k+33,k+34});}}
        mesh.SetVertices(vs);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();var path=Base+"Models/Caretaker Mug.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing==null)AssetDatabase.CreateAsset(mesh,path);else{UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}
        var enamel=Plain("Mug Enamel",new Color(.30f,.33f,.25f),.5f);root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=enamel;
        for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;Rod("Mug Handle",root,new Vector3(.047f+Mathf.Cos(a)*.024f,.049f+Mathf.Sin(a)*.03f,0),new Vector3(.047f+Mathf.Cos(b)*.024f,.049f+Mathf.Sin(b)*.03f,0),.0045f,"Mug Enamel");}
        Plain("Coffee",new Color(.028f,.014f,.006f),.78f);Rod("Coffee",root,new Vector3(0,.018f,0),new Vector3(0,.065f,0),.033f,"Coffee");
        var clone=UnityEngine.Object.Instantiate(root.gameObject);clone.name="Caretaker Mug";clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/Caretaker Mug.prefab");UnityEngine.Object.DestroyImmediate(clone);
    }
    public static string PolishOffice()
    {
        EnsureApartment();var room=GameObject.Find("Ground Floor").transform.Find("Management Room");if(room.Find("Workstation")!=null)return "Workstation polish is already applied.";
        if(AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/Cardboard.mat")==null)AssetDatabase.CreateAsset(new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Cardboard.mat")),Base+"Materials/Cardboard.mat");
        var desk=room.Find("Management Desk");var items=room.Find("Desk Items");var chair=room.Find("Office Chair");float top=BoundsOf(desk).max.y;
        var workstation=Collect("Workstation",room,desk,items,chair);
        chair.SetPositionAndRotation(new Vector3(-4.08f,.005f,2.99f),Quaternion.Euler(0,90,0));RecordPose(chair);
        var chairBounds=BoundsOf(chair);var chairCollider=chair.gameObject.AddComponent<BoxCollider>();chairCollider.center=chair.InverseTransformPoint(chairBounds.center);chairCollider.size=new Vector3(chairBounds.size.z/chair.lossyScale.x,chairBounds.size.y/chair.lossyScale.y,chairBounds.size.x/chair.lossyScale.z);
        var deskBounds=BoundsOf(desk);var deskCollider=desk.gameObject.AddComponent<BoxCollider>();deskCollider.center=desk.InverseTransformPoint(deskBounds.center);deskCollider.size=new Vector3(deskBounds.size.z/desk.lossyScale.x,deskBounds.size.y/desk.lossyScale.y,deskBounds.size.x/desk.lossyScale.z);
        var rack=G("Monitor Rack",items);B("Rack Base",rack,new Vector3(-3.02f,top+.01f,3.75f),new Vector3(.51f,.02f,1.06f),"Green Steel",false);
        B("Monitor Shelf",rack,new Vector3(-3.02f,top+.352f,3.75f),new Vector3(.51f,.016f,1.06f),"Green Steel",false);
        foreach(float z in new[]{3.23f,4.27f})Rod("Rack Upright",rack,new Vector3(-2.77f,top+.012f,z),new Vector3(-2.77f,top+.69f,z),.009f,"Green Steel");
        var monitors=items.Cast<Transform>().Where(t=>t.name=="CRT Monitor").OrderBy(t=>t.position.y).ThenBy(t=>t.position.z).ToArray();
        for(int i=0;i<monitors.Length;i++)SeatOn(monitors[i],new Vector3(-3.02f,top+(i<2?.02f:.36f),3.50f+(i%2)*.49f),-100);
        var console=items.Find("CCTV Console");console.position=new Vector3(-3.40f,top+.030f,3.78f);
        var chairNotes=items.Find("Work Surface");chairNotes.name="Telephone Station";chairNotes.SetPositionAndRotation(new Vector3(-3.05f,top+.003f,2.59f),Quaternion.Euler(0,90,0));
        var lamp=items.Find("Desk Lamp");SeatOn(lamp,new Vector3(-2.86f,top,2.93f),-90);
        var taskLight=items.Cast<Transform>().First(t=>t.GetComponent<Light>()!=null);taskLight.name="Desk Light";taskLight.position=new Vector3(-3.12f,top+.31f,2.98f);
        // Maintain a clear writing area between the telephone and monitor controls.
        var logbook=G("Maintenance Logbook",items);logbook.position=new Vector3(-3.24f,top+.003f,2.98f);logbook.rotation=Quaternion.Euler(0,90,0);
        B("Logbook Cover",logbook,new Vector3(0,.006f,0),new Vector3(.38f,.012f,.29f),"Black Rubber",false,.003f);
        foreach(float x in new[]{-.091f,.091f})B("Page Block",logbook,new Vector3(x,.018f,0),new Vector3(.176f,.012f,.27f),"Paper",false,.001f);
        var log=items.Find("Maintenance Log");log.SetParent(logbook,false);log.localPosition=new Vector3(0,.025f,0);log.localRotation=Quaternion.Euler(90,0,0);log.localScale=new Vector3(.353f,.257f,1);
        B("Page Fold",logbook,new Vector3(0,.0255f,0),new Vector3(.004f,.001f,.26f),"Paper",false,.0001f);
        var pen=items.Find("Pen");var penDelta=new Vector3(-3.46f,top+.014f,2.88f)-pen.position;pen.position+=penDelta;
        var documentStacks=items.Cast<Transform>().Where(t=>t.name=="Document Stack").OrderBy(t=>t.position.z).ToArray();
        for(int i=0;i<documentStacks.Length;i++){documentStacks[i].position=new Vector3(-2.88f,top+.004f,3.19f+i*.315f);documentStacks[i].rotation=Quaternion.Euler(0,i==1?4:-3,0);}
        var radio=Collect("Radio",items,items.Find("Walkie Talkie"),items.Find("Radio Aerial"));radio.position=new Vector3(-3.43f,top+.05f,3.33f)-radio.GetComponentsInChildren<Transform>().First(t=>t.name=="Walkie Talkie").position;
        // A small personal shelf separates food and drink from paperwork and electronics.
        var personal=G("Personal Shelf",room);B("Shelf Board",personal,new Vector3(-4.27f,.96f,1.75f),new Vector3(1.06f,.04f,.40f),"Old Wood",false);
        foreach(float x in new[]{-4.65f,-3.9f}){B("Shelf Wall Plate",personal,new Vector3(x,.84f,1.57f),new Vector3(.045f,.24f,.025f),"Green Steel",false);Rod("Shelf Bracket",personal,new Vector3(x,.74f,1.58f),new Vector3(x,.94f,1.91f),.009f,"Green Steel");}
        var thermos=Collect("Thermos",personal,items.Find("Thermos Body"),items.Find("Thermos Cap"));thermos.position+=new Vector3(-4.50f,.982f,1.74f)-new Vector3(-2.78f,top,4.22f);
        var meal=Collect("Noodle Cup",personal,items.Find("Noodle Cup"),items.Find("Chopstick"));meal.position+=new Vector3(-4.12f,.982f,1.73f)-new Vector3(-3.3f,top,2.47f);
        Mug(personal,new Vector3(-3.91f,.982f,1.77f));Weather(personal,"Ground Stain",new Vector3(-3.93f,.9815f,1.76f),new Vector2(.10f,.10f),Quaternion.Euler(90,0,0));
        // Replace the solid shelf block and unsupported boards with one coherent rack.
        var oldShelf=room.Find("Bookshelf");UnityEngine.Object.DestroyImmediate(oldShelf.gameObject);
        var services=room.Find("Office Service Details");foreach(var shelf in services.Cast<Transform>().Where(t=>t.name=="Shelf").ToArray())UnityEngine.Object.DestroyImmediate(shelf.gameObject);
        var storage=G("Archive Shelves",room);
        foreach(float x in new[]{-5.94f,-5.42f})foreach(float z in new[]{2.18f,4.22f})B("Shelf Upright",storage,new Vector3(x,.94f,z),new Vector3(.035f,1.88f,.035f),"Green Steel",false);
        foreach(float y in new[]{.18f,.65f,1.15f,1.68f})B("Storage Shelf",storage,new Vector3(-5.68f,y,3.20f),new Vector3(.55f,.035f,2.08f),"Old Wood",false);
        foreach(float z in new[]{2.18f,4.22f})Rod("Shelf Brace",storage,new Vector3(-5.94f,.20f,z),new Vector3(-5.42f,1.65f,z),.008f,"Green Steel");
        var crate=room.Find("Storage Crate");crate.SetParent(storage,true);crate.localScale*=.8f;SeatOn(crate,new Vector3(-5.68f,.198f,2.59f),90);
        var carton=services.Cast<Transform>().First(t=>t.name=="Delivery Carton");carton.SetParent(storage,true);carton.localScale*=.34f/BoundsOf(carton).size.y;SeatOn(carton,new Vector3(-5.68f,.198f,3.47f),0);
        var books=room.Find("Books");books.SetParent(storage,true);SeatOn(books,new Vector3(-5.69f,1.168f,2.66f),90);
        var handbook=items.Find("Notebook");handbook.name="Reference Handbook";handbook.SetParent(storage,true);SeatOn(handbook,new Vector3(-5.69f,1.168f,3.45f),0);
        int page=0;foreach(var paper in services.Cast<Transform>().Where(t=>t.name=="Filed Document").ToArray()){int stack=page/9,sheet=page%9;paper.SetParent(storage,true);paper.position=new Vector3(-5.70f,.671f+sheet*.0022f,2.5f+stack*.49f);page++;}
        var label=BoundsOf(crate);Notice("Supply Label",storage,new Vector3(label.max.x+.0015f,label.center.y,2.59f),new Vector2(.18f,.06f),Quaternion.Euler(0,-90,0));
        Notice("Archive Label",storage,new Vector3(-5.397f,1.15f,3.22f),new Vector2(.30f,.06f),Quaternion.Euler(0,-90,0));
        // Fill the crate with maintenance consumables, kept below the shelf above.
        var supplies=G("Maintenance Supplies",storage);for(int i=0;i<3;i++)B("Supply Carton",supplies,new Vector3(-5.70f+i*.10f,.38f,2.59f),new Vector3(.075f,.19f,.12f),"Cardboard",false);
        // Purposeful records wall, and a clear approach from the office entrance.
        var cabinet=room.Find("Filing Cabinet");cabinet.position+=Vector3.forward*.49f;var fan=room.Find("Desk Fan");fan.position+=Vector3.forward*.49f;fan.rotation=Quaternion.Euler(0,-15,0);
        var records=Collect("Records Wall",room,cabinet,fan,room.Find("Key Cabinet"),room.Find("Wall Clock"),room.Find("Clock Dial"),room.Find("Building Plan"),room.Find("Calendar"));
        var plan=records.Find("Building Plan");plan.position=new Vector3(-3.80f,2.39f,6.638f);plan.localScale=new Vector3(.72f,.36f,1);
        var calendar=records.Find("Calendar");calendar.position=new Vector3(-2.72f,1.62f,6.638f);calendar.localScale=new Vector3(.35f,.49f,1);
        B("Key Return Ledge",records,new Vector3(-3.8f,1.02f,6.48f),new Vector3(.72f,.035f,.32f),"Old Wood",false);
        foreach(float x in new[]{-4.07f,-3.53f})Rod("Ledge Support",records,new Vector3(x,.83f,6.62f),new Vector3(x,1.0f,6.34f),.008f,"Green Steel");
        Notice("Key Return",records,new Vector3(-3.8f,1.24f,6.638f),new Vector2(.32f,.09f),Quaternion.identity);
        var notes=G("Maintenance Notes",room);B("Clipboard",notes,new Vector3(-2.188f,1.65f,5.115f),new Vector3(.012f,.34f,.22f),"Old Wood",false);
        Notice("Caretaker Notes",notes,new Vector3(-2.196f,1.65f,5.115f),new Vector2(.19f,.30f),Quaternion.Euler(0,90,0));
        B("Clipboard Clip",notes,new Vector3(-2.202f,1.805f,5.115f),new Vector3(.018f,.025f,.065f),"Stainless Steel",false);
        var power=G("Desk Services",workstation);B("Power Outlet",power,new Vector3(-2.201f,.34f,4.2f),new Vector3(.04f,.13f,.09f),"Aged Plastic",false);
        Lead(power,new Vector3(-2.83f,top+.17f,4.07f),new Vector3(-2.79f,top+.05f,4.2f),new Vector3(-2.80f,.22f,4.2f),new Vector3(-2.24f,.22f,4.2f),new Vector3(-2.23f,.34f,4.2f));
        Lead(power,new Vector3(-3.30f,top+.02f,3.98f),new Vector3(-3.2f,top+.012f,4.16f),new Vector3(-2.88f,top+.012f,4.16f),new Vector3(-2.84f,top+.15f,4.10f));
        foreach(var renderer in room.GetComponentsInChildren<MeshRenderer>()){var s=renderer.bounds.size;if(s.x*s.y+s.x*s.z+s.y*s.z<1)GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,GameObjectUtility.GetStaticEditorFlags(renderer.gameObject)&~StaticEditorFlags.ContributeGI);}
        SaveApartment();return "Caretaker workstation, records wall, archive rack and personal shelf reorganized; chair now faces its writing position.";
    }
    public static string OfficeViews()
    {
        EnsureApartment();Capture("Office Interior",new Vector3(-5.25f,1.65f,5.65f),new Vector3(-3.45f,1.20f,3.20f));Capture("Office Reception",new Vector3(.35f,1.65f,3.9f),new Vector3(-4.35f,1.3f,3.45f));Capture("Office Desk Level",new Vector3(-4.10f,1.18f,2.68f),new Vector3(-3.0f,1.0f,3.40f));Capture("Office Records",new Vector3(-3.75f,1.65f,3.8f),new Vector3(-4.5f,1.5f,6.2f));Capture("Locked Stairwell",new Vector3(.2f,1.65f,10.1f),new Vector3(-2.09f,1.25f,9.5f));
        foreach(string name in new[]{"Office Interior","Office Reception","Office Desk Level","Office Records","Locked Stairwell"})File.Copy("References/Apartment/Captures/"+name+".png","References/Apartment/Office Polish/"+name+".png",true);return "Saved interior, reception-window, desk-level, records wall and stair door views.";
    }
    public static string FinishOfficePlacement()
    {
        EnsureApartment();var room=GameObject.Find("Ground Floor").transform.Find("Management Room");var items=room.Find("Workstation/Desk Items");var storage=room.Find("Archive Shelves");
        var stacks=items.Cast<Transform>().Where(t=>t.name=="Document Stack").OrderBy(t=>t.position.z).ToArray();
        foreach(var stack in stacks){stack.SetParent(storage,true);SeatOn(stack,new Vector3(-5.69f,1.699f,2.55f+Array.IndexOf(stacks,stack)*.48f),0);}
        var supplies=storage.Find("Maintenance Supplies");int index=0;foreach(Transform t in supplies)t.position=new Vector3(-5.78f+index++*.10f,.337f,2.59f);
        var mount=items.Find("Lamp Mount");if(mount==null){var p=G("Lamp Mount",items);B("Clamp Plate",p,new Vector3(-2.86f,.774f,2.925f),new Vector3(.073f,.012f,.055f),"Green Steel",false,.002f);Rod("Lamp Socket",p,new Vector3(-2.86f,.780f,2.925f),new Vector3(-2.86f,.803f,2.925f),.012f,"Green Steel");}
        var returnLabel=room.Find("Records Wall/Key Return");returnLabel.position=new Vector3(-3.8f,1.14f,6.638f);
        var collider=storage.GetComponent<BoxCollider>();if(collider==null)collider=storage.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(-5.68f,.94f,3.2f);collider.size=new Vector3(.59f,1.88f,2.12f);
        SaveApartment();return "Paperwork stored clear of CRTs, supplies seated inside crate, lamp mount fitted, archive collision added.";
    }
    public static string CheckOfficeAccess()
    {
        EnsureApartment();var walker=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var cc=walker.GetComponent<CharacterController>();var position=walker.transform.position;var rotation=walker.transform.rotation;var log=new System.Text.StringBuilder();
        try
        {
            cc.enabled=false;walker.transform.position=new Vector3(0,.1f,1.5f);cc.enabled=true;Physics.SyncTransforms();
            foreach(var target in new[]{new Vector3(0,0,5.77f),new Vector3(-2.9f,0,5.77f),new Vector3(-4.4f,0,5.3f),new Vector3(-4.8f,0,4.45f),new Vector3(-4.8f,0,3.15f),new Vector3(-4.8f,0,2.5f),new Vector3(-4.8f,0,4.45f),new Vector3(-3.8f,0,5.6f)})
            {
                bool reached=false;for(int i=0;i<450;i++){var delta=target-walker.transform.position;delta.y=0;if(delta.magnitude<.07f){reached=true;break;}walker.transform.rotation=Quaternion.LookRotation(delta);walker.Step(new Vector2(0,Mathf.Min(1,delta.magnitude/(walker.speed*.02f))),.02f,false);}
                log.AppendLine("Office access "+target+": "+reached+" reached "+walker.transform.position);
            }
        }
        finally{cc.enabled=false;walker.transform.SetPositionAndRotation(position,rotation);cc.enabled=true;Physics.SyncTransforms();}
        File.WriteAllText("References/Apartment/Office Polish/Office Access Checks.txt",log.ToString());return log.ToString();
    }
}
