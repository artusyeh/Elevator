using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static partial class ApartmentBuilder
{
 static void SealCabinOpening(Transform cabin)
 {
  if(cabin.Find("Doorway Returns")!=null)return;var g=G("Doorway Returns",cabin);
  foreach(float side in new[]{-1f,1f})
  {
   B("Inner Door Return",g,new Vector3(side*.655f,1.07f,-.005f),new Vector3(.23f,2.14f,.10f),"Pale Green Panels",true,.002f);
   B("Inner Jamb Trim",g,new Vector3(side*.548f,1.07f,.051f),new Vector3(.017f,2.14f,.018f),"Elevator Frame Metal",false,.001f);
  }
  B("Inner Header",g,new Vector3(0,2.205f,-.005f),new Vector3(1.54f,.17f,.10f),"Pale Green Panels",true,.002f);
  B("Floor Apron",g,new Vector3(0,-.03f,-.04f),new Vector3(1.54f,.06f,.22f),"Cabin Worn Floor",true,.001f);
  B("Inner Sill",g,new Vector3(0,.006f,-.04f),new Vector3(1.09f,.012f,.22f),"Elevator Frame Metal",false,.001f);
  for(int i=0;i<3;i++)B("Sill Groove",g,new Vector3(0,.0125f,-.105f+i*.045f),new Vector3(1.07f,.001f,.004f),"Black Rubber",false,.0002f);
 }
 static void SealLandingDoor(Transform landing)
 {
  var right=landing.Find("Right Elevator Door");if(right!=null&&right.Find("Meeting Seal")==null)B("Meeting Seal",right,new Vector3(-.274f,0,.029f),new Vector3(.008f,2.12f,.006f),"Black Rubber",false,.0005f);
 }
 public static string RepairElevatorEnclosure()
 {
  EnsureApartment();System.IO.Directory.CreateDirectory("References/Elevator Enclosure");System.IO.File.Copy("Assets/Scenes/Apartment.unity","References/Elevator Enclosure/Apartment.before.unity",true);
  var cabin=GameObject.Find("Elevator Cabin").transform;SealCabinOpening(cabin);SealLandingDoor(cabin.parent);SaveApartment();
  foreach(string name in new[]{"Elevator Cabin","Elevator Landing","Entrance Interior"})
  {
   string path=Base+"Prefabs/"+name+".prefab";var prefab=PrefabUtility.LoadPrefabContents(path);
   try{foreach(var c in prefab.GetComponentsInChildren<Transform>().Where(t=>t.name=="Elevator Cabin").ToArray())SealCabinOpening(c);foreach(var l in prefab.GetComponentsInChildren<Transform>().Where(t=>t.name=="Elevator Landing").ToArray())SealLandingDoor(l);SharedSave(prefab.transform,name);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
  }
  SaveApartment();return "Cabin side returns, header, floor apron, sill and meeting seal installed in scene and reusable assets.";
 }
 public static string ElevatorEnclosureViews()
 {
  EnsureApartment();var d=UnityEngine.Object.FindFirstObjectByType<SlidingDoors>();var a=d.left.localPosition;var b=d.right.localPosition;
  try{d.left.localPosition=new Vector3(-d.closedOffset,a.y,a.z);d.right.localPosition=new Vector3(d.closedOffset,b.y,b.z);Capture("Cabin Door Sealed",new Vector3(3.95f,1.65f,10.8f),new Vector3(3.22f,1.10f,10.8f));Capture("Cabin Corner Sealed",new Vector3(3.78f,1.65f,10.51f),new Vector3(3.32f,.08f,10.15f));}
  finally{d.left.localPosition=a;d.right.localPosition=b;}
  foreach(string n in new[]{"Cabin Door Sealed","Cabin Corner Sealed"})System.IO.File.Copy("References/Apartment/Captures/"+n+".png","References/Elevator Enclosure/"+n+".png",true);return "Closed-door and lower-corner review captures saved.";
 }
}
