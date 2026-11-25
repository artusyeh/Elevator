using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static partial class ApartmentBuilder
{
 static Bounds InParentBounds(Transform item,Transform parent)
 {
  bool first=true;var result=new Bounds();foreach(var r in item.GetComponentsInChildren<Renderer>()){var b=r.bounds;foreach(float x in new[]{b.min.x,b.max.x})foreach(float y in new[]{b.min.y,b.max.y})foreach(float z in new[]{b.min.z,b.max.z}){var p=parent.InverseTransformPoint(new Vector3(x,y,z));if(first){result=new Bounds(p,Vector3.zero);first=false;}else result.Encapsulate(p);}}return result;
 }
 static void FitOutdoorUnit(Transform unit,float wallZ)
 {
  var parent=unit.parent;if(PrefabUtility.IsPartOfPrefabInstance(unit))PrefabUtility.UnpackPrefabInstance(unit.gameObject,PrefabUnpackMode.OutermostRoot,InteractionMode.AutomatedAction);
  unit.localRotation=Quaternion.identity;
  foreach(var t in unit.Cast<Transform>().Where(t=>new[]{"Support Bracket","Mounting Plate","Mounting Bolt","Coolant Pipe"}.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
  var cabinet=unit.Find("Weathered Cabinet");var b=InParentBounds(cabinet,parent);var fittings=G("Condenser Mount",parent);
  float rear=wallZ-.012f,front=b.min.z-.025f,bottom=b.min.y;
  foreach(float x in new[]{b.center.x-b.size.x*.32f,b.center.x+b.size.x*.32f})
  {
   B("Wall Anchor Plate",fittings,new Vector3(x,bottom-.11f,rear),new Vector3(.055f,.32f,.026f),"Green Steel",false,.002f);
   B("Support Arm",fittings,new Vector3(x,bottom-.022f,(front+rear)*.5f),new Vector3(.042f,.04f,rear-front),"Green Steel",false,.002f);
   Rod("Diagonal Brace",fittings,new Vector3(x,bottom-.24f,rear-.015f),new Vector3(x,bottom-.043f,front+.035f),.010f,"Green Steel");
   foreach(float y in new[]{bottom-.21f,bottom+.015f})Rod("Anchor Bolt",fittings,new Vector3(x,y,rear-.014f),new Vector3(x,y,rear-.023f),.008f,"Stainless Steel");
   B("Isolation Pad",fittings,new Vector3(x,bottom-.001f,b.center.z),new Vector3(.068f,.006f,.085f),"Black Rubber",false,.001f);
  }
  PipeRun(fittings,new Vector3(b.max.x+.025f,b.center.y,rear-.025f),new Vector3(b.max.x+.025f,b.min.y-.27f,rear-.025f),.012f);
  Rod("Refrigerant Connection",fittings,new Vector3(b.max.x-.015f,b.center.y,b.center.z),new Vector3(b.max.x+.025f,b.center.y,rear-.025f),.013f,"Black Rubber");
 }
 static void ProperDoor(Transform p,Vector3 origin,float width=.86f)
 {
  var root=G("Residential Entrance",p);root.localPosition=origin;
  B("Door Reveal",root,new Vector3(0,1.06f,0),new Vector3(width+.11f,2.12f,.035f),"Black Rubber",false,.002f);
  B("Door Leaf",root,new Vector3(0,1.045f,-.026f),new Vector3(width,2.07f,.045f),"Stair Door Paint",true,.003f);
  foreach(float x in new[]{-width*.5f-.026f,width*.5f+.026f})B("Door Jamb",root,new Vector3(x,1.07f,-.05f),new Vector3(.05f,2.14f,.10f),"Green Steel",false,.002f);
  B("Door Header",root,new Vector3(0,2.145f,-.05f),new Vector3(width+.15f,.065f,.10f),"Green Steel",false,.002f);
  B("Door Sill",root,new Vector3(0,.012f,-.055f),new Vector3(width+.12f,.024f,.20f),"Concrete",true,.002f);
  B("Lower Door Panel",root,new Vector3(0,.54f,-.051f),new Vector3(width-.13f,.8f,.008f),"Stair Door Paint",false,.002f);
  Rod("Peephole",root,new Vector3(0,1.59f,-.051f),new Vector3(0,1.59f,-.062f),.009f,"Brass");
  B("Latch Plate",root,new Vector3(width*.34f,1.0f,-.057f),new Vector3(.045f,.17f,.014f),"Stainless Steel",false,.002f);
  Rod("Handle Spindle",root,new Vector3(width*.34f,1.04f,-.065f),new Vector3(width*.34f,1.04f,-.102f),.011f,"Stainless Steel");
  Rod("Door Lever",root,new Vector3(width*.34f,1.04f,-.102f),new Vector3(width*.18f,1.04f,-.102f),.011f,"Stainless Steel");
  Rod("Lock Cylinder",root,new Vector3(width*.34f,.96f,-.065f),new Vector3(width*.34f,.96f,-.074f),.012f,"Brass");
  foreach(float y in new[]{.28f,1.04f,1.84f})Rod("Door Hinge",root,new Vector3(-width*.5f,y-.045f,-.053f),new Vector3(-width*.5f,y+.045f,-.053f),.009f,"Stainless Steel");
  B("Letter Flap",root,new Vector3(0,.78f,-.06f),new Vector3(.24f,.045f,.012f),"Stainless Steel",false,.002f);
  B("Canopy Wall Flashing",root,new Vector3(0,2.29f,.035f),new Vector3(1.4f,.14f,.06f),"Green Steel",false,.002f);
  B("Canopy Roof",root,new Vector3(0,2.29f,-.27f),new Vector3(1.4f,.065f,.67f),"Green Steel",false,.003f);
  B("Canopy Front Trim",root,new Vector3(0,2.275f,-.602f),new Vector3(1.43f,.09f,.025f),"Green Steel",false,.002f);
  foreach(float x in new[]{-.53f,.53f})
  {B("Canopy Wall Plate",root,new Vector3(x,2.04f,.018f),new Vector3(.06f,.40f,.035f),"Green Steel",false);Rod("Canopy Bracket",root,new Vector3(x,1.87f,0),new Vector3(x,2.249f,-.54f),.013f,"Green Steel");}
 }
 public static string StructuralCleanup()
 {
  if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");Directory.CreateDirectory("References/Structural Cleanup");var sc=SceneManager.GetActiveScene();if(sc.isDirty)EditorSceneManager.SaveScene(sc);
  File.Copy("Assets/Scenes/Alley.unity","References/Structural Cleanup/Alley.before.unity",true);
  int doors=0,units=0;
  foreach(string name in new[]{"Side Streets","Aoba Exterior"})
  {
   string path=Base+"Prefabs/"+name+".prefab";File.Copy(path,"References/Structural Cleanup/"+name+".before.prefab",true);var g=PrefabUtility.LoadPrefabContents(path);
   foreach(var details in g.GetComponentsInChildren<Transform>().Where(t=>t.name=="Household Details").ToArray())
   {
    var body=details.parent.Find("Building");float width=body.GetComponent<MeshFilter>().sharedMesh.bounds.size.x,wall=-body.GetComponent<MeshFilter>().sharedMesh.bounds.size.z*.5f;
    foreach(var t in details.Cast<Transform>().Where(t=>new[]{"Entrance Recess","Entrance Door","Door Frame","Door Pull","Entrance Canopy","AC Bracket"}.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
    ProperDoor(details,new Vector3(width*.2f,0,wall-.035f));doors++;
    foreach(var ac in details.Cast<Transform>().Where(t=>t.name=="Outdoor Condenser").ToArray()){var pos=ac.localPosition;pos.x=-width*.015f;pos.z=wall-.31f;pos.y+=.14f;ac.localPosition=pos;FitOutdoorUnit(ac,wall);units++;}
    // Equipment and entry remain separate; ground props are seated by their actual rendered bounds.
    foreach(var prop in details.Cast<Transform>().Where(t=>t.name=="Cleaning Bucket"||t.name=="Potted Foliage").ToArray()){var b=InParentBounds(prop,details);var pos=prop.localPosition;pos.y+=.01f-b.min.y;prop.localPosition=pos;PrefabUtility.RecordPrefabInstancePropertyModifications(prop);}
   }
   var corner=g.transform.Find("Corner Apartments");if(corner!=null)
   {
    foreach(var t in corner.Cast<Transform>().Where(t=>t.name=="Recessed Entry"||t.name=="Entry Awning"||t.name=="AC Support").ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
    ProperDoor(corner,new Vector3(-.6f,0,-3.505f));doors++;
    foreach(var ac in corner.Cast<Transform>().Where(t=>t.name=="Outdoor Condenser").ToArray()){FitOutdoorUnit(ac,-3.4f);units++;}
   }
   var equipment=g.transform.Find("Exterior Equipment");if(equipment!=null)
   {
    foreach(var t in equipment.Cast<Transform>().Where(t=>t.name=="AC Wall Bracket"||t.name=="Bracket Plate").ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
    foreach(var ac in equipment.Cast<Transform>().Where(t=>t.name=="Outdoor Condenser").ToArray()){FitOutdoorUnit(ac,-.19f);units++;}
   }
   SharedSave(g.transform,name);PrefabUtility.UnloadPrefabContents(g);
  }
  EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");int relocated=0;
  foreach(var building in GameObject.Find("Alley").transform.Find("Buildings").Cast<Transform>())
  {
   foreach(var ac in building.GetComponentsInChildren<Transform>().Where(t=>t.name=="Outdoor AC Condenser").ToArray())
   {
    var parent=ac.parent;var cab=ac.Find("Weathered Cabinet");if(cab==null)continue;var b=InParentBounds(cab,parent);
    var windows=parent.GetComponentsInChildren<Transform>().Where(t=>t.name=="Window Recess"||t.name=="Window Glass").ToArray();
    foreach(var w in windows)
    {var wb=InParentBounds(w,parent);bool overlap=b.max.x>wb.min.x-.12f&&b.min.x<wb.max.x+.12f&&b.max.y>wb.min.y-.12f&&b.min.y<wb.max.y+.12f;if(overlap){var pos=ac.localPosition;pos.y+=wb.min.y-.17f-b.max.y;ac.localPosition=pos;b=InParentBounds(cab,parent);relocated++;}}
   }
  }
  SaveApartment();return "Fitted "+doors+" complete door/canopy assemblies and "+units+" connected AC mounts. Main alley window conflicts relocated: "+relocated;
 }
 public static string StructuralViews()
 {
  if(SceneManager.GetActiveScene().name!="Alley")EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");
  Capture("Side Entrance Repaired",new Vector3(10,1.65f,58.4f),new Vector3(11.4f,1.1f,55));Capture("Side Units Repaired",new Vector3(15,1.65f,58),new Vector3(13,1.8f,54.8f));Capture("Corner Entry Repaired",new Vector3(0,1.65f,58),new Vector3(-3.1f,1.3f,62));Capture("Main Alley Equipment",new Vector3(0,1.65f,26),new Vector3(3,2.6f,31));
  foreach(string n in new[]{"Side Entrance Repaired","Side Units Repaired","Corner Entry Repaired","Main Alley Equipment"})File.Copy("References/Apartment/Captures/"+n+".png","References/Structural Cleanup/"+n+".png",true);return "Four player-height structural review views captured.";
 }
}
