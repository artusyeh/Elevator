using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static partial class ApartmentBuilder
{
 public static string WindowAndEntranceChecks()
 {
  var path=Base+"Prefabs/Aoba Exterior.prefab";var ext=PrefabUtility.LoadPrefabContents(path);var notice=ext.transform.Find("Building Notices");notice.localPosition=new Vector3(1.65f,1.04f,-.225f);
  var facade=ext.transform.Find("Building Facade");
  foreach(var rear in facade.Cast<Transform>().Where(t=>t.name=="Rear Facade").ToArray())
  {
   float y=rear.localPosition.y-1.4f;UnityEngine.Object.DestroyImmediate(rear.gameObject);
   foreach(var segment in new[]{new Vector3(-5.35f,1.4f,1.75f),new Vector3(1.0875f,1.4f,8.425f)})B("Rear Wall",facade,new Vector3(segment.x,y+segment.y,14.12f),new Vector3(segment.z,2.8f,.18f),"Concrete");
   B("Rear Window Base",facade,new Vector3(-3.8f,y+.825f,14.12f),new Vector3(1.35f,1.65f,.18f),"Concrete");B("Rear Window Lintel",facade,new Vector3(-3.8f,y+2.66f,14.12f),new Vector3(1.35f,.32f,.18f),"Concrete");
  }
  foreach(var t in facade.Cast<Transform>().Where(t=>t.name=="Stair Window"))foreach(var child in t.Cast<Transform>().Where(c=>c.name=="Window Recess").ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
  SharedSave(ext.transform,"Aoba Exterior");PrefabUtility.UnloadPrefabContents(ext);
  EnsureApartment();
  foreach(var stair in GameObject.Find("Apartment").GetComponentsInChildren<Transform>().Where(t=>t.name=="Stairwell").ToArray())
  {
   var pane=stair.Find("Barred Window");if(pane==null)continue;pane.GetComponent<Renderer>().sharedMaterial=M("Dirty Glass");
   foreach(var wall in stair.Cast<Transform>().Where(t=>t.name=="Plaster Wall"&&Mathf.Abs(t.localPosition.z-13.98f)<.01f).ToArray())
   {
    UnityEngine.Object.DestroyImmediate(wall.gameObject);B("Stair Window Spandrel",stair,new Vector3(-3.8f,1.4375f,13.98f),new Vector3(3.4f,.435f,.18f),"Yellowed Plaster");B("Stair Window Lintel",stair,new Vector3(-3.8f,2.7025f,13.98f),new Vector3(3.4f,.395f,.18f),"Yellowed Plaster");
    foreach(float x in new[]{-4.9875f,-2.6125f})B("Stair Window Pier",stair,new Vector3(x,2.08f,13.98f),new Vector3(1.025f,.85f,.18f),"Yellowed Plaster");
   }
  }
  SaveApartment();return "Stairwell glazing aligned with real apertures; notices mounted on the entrance pier.";
 }
}



