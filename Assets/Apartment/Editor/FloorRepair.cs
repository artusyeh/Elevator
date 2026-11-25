using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static partial class ApartmentBuilder
{
 static void FloorSlab(Transform t,float top,float depth)
 {
  var collider=t.GetComponent<BoxCollider>();var size=collider.size;size.y=depth;collider.size=size;t.GetComponent<MeshFilter>().sharedMesh=Rounded(size,.002f);var p=t.position;p.y=top-depth*.5f;t.position=p;
 }
 public static string RepairFloorLayers()
 {
  EnsureApartment();System.IO.Directory.CreateDirectory("References/Floor Repair");System.IO.File.Copy("Assets/Scenes/Apartment.unity","References/Floor Repair/Apartment.before.unity",true);int floors=0,ceilings=0;
  var root=GameObject.Find("Apartment").transform;
  foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Corridor Floor").ToArray()){var b=t.GetComponent<Renderer>().bounds;FloorSlab(t,b.max.y,.04f);floors++;}
  foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Corridor Ceiling").ToArray()){var b=t.GetComponent<Renderer>().bounds;float top=b.max.y-.04f;FloorSlab(t,top,.18f);ceilings++;}
  var lobby=root.Find("Ground Floor/Lobby/Ceiling");FloorSlab(lobby,2.86f,.08f);ceilings++;SaveApartment();
  foreach(string name in new[]{"Residential Corridor","Entrance Interior"})
  {
   var path=Base+"Prefabs/"+name+".prefab";var g=PrefabUtility.LoadPrefabContents(path);
   try{foreach(var t in g.GetComponentsInChildren<Transform>().Where(t=>t.name=="Corridor Floor").ToArray())FloorSlab(t,t.GetComponent<Renderer>().bounds.max.y,.04f);foreach(var t in g.GetComponentsInChildren<Transform>().Where(t=>t.name=="Corridor Ceiling").ToArray())FloorSlab(t,t.GetComponent<Renderer>().bounds.max.y-.04f,.18f);if(name=="Entrance Interior")FloorSlab(g.transform.Find("Lobby/Ceiling"),2.86f,.08f);SharedSave(g.transform,name);}finally{PrefabUtility.UnloadPrefabContents(g);}
  }
  var exteriorPath=Base+"Prefabs/Aoba Exterior.prefab";var ext=PrefabUtility.LoadPrefabContents(exteriorPath);try{var pavement=ext.transform.Find("Pavement");var collider=pavement.GetComponent<BoxCollider>();var size=collider.size;size.z=4;collider.size=size;pavement.GetComponent<MeshFilter>().sharedMesh=Rounded(size,.002f);pavement.localPosition=new Vector3(0,-.12f,-2);SharedSave(ext.transform,"Aoba Exterior");}finally{PrefabUtility.UnloadPrefabContents(ext);}
  SaveApartment();return floors+" terrazzo floor layers separated from "+ceilings+" ceiling slabs; duplicated court pavement removed.";
 }
 public static string FloorRepairViews()
 {
  EnsureApartment();Capture("Corridor Floor Repaired",new Vector3(0,7.45f,10),new Vector3(0,5.8f,5.5f));Capture("Court Floor Repaired",new Vector3(0,1.65f,-3),new Vector3(0,0,-5));
  foreach(string n in new[]{"Corridor Floor Repaired","Court Floor Repaired"})System.IO.File.Copy("References/Apartment/Captures/"+n+".png","References/Floor Repair/"+n+".png",true);return "Corridor and court surface review images captured.";
 }
}
