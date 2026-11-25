using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static partial class ApartmentBuilder
{
 public static string FinishWindowSurroundings()
 {
  var path=Base+"Prefabs/Side Streets.prefab";var root=PrefabUtility.LoadPrefabContents(path);var rear=G("Rear Service Lane",root.transform);
  B("Rear Ground",rear,new Vector3(21,-.22f,85)-Address,new Vector3(39,.28f,20),"Concrete");
  foreach(float x in new[]{8f,20f,33f})Neighbor(rear,new Vector3(x,0,90)-Address,new Vector3(11,10+(x==20?3:0),9),2);
  StreetBox("Service Lane",rear,new Vector3(22,-.12f,82)-Address,new Vector3(30,.12f,4),"Rain Damp Asphalt");
  foreach(float x in new[]{5f,39f})B("Rear Boundary",rear,new Vector3(x,2,81)-Address,new Vector3(.25f,4,17),"Concrete");
  SharedSave(root.transform,"Side Streets");PrefabUtility.UnloadPrefabContents(root);return "Rear window views enclosed with a shared service lane and neighboring residential facades.";
 }
 public static string WideConnectionViews()
 {
  if(SceneManager.GetActiveScene().name!="Alley")EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");var cam=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>().eyes;float f=cam.fieldOfView;float far=cam.farClipPlane;try{cam.fieldOfView=95;cam.farClipPlane=250;ConnectionCapture("Full Building",new Vector3(22,1.65f,55.2f),new Vector3(22,8,64));cam.fieldOfView=67;ConnectionCapture("Street Layout",new Vector3(8,85,44),new Vector3(8,0,44.01f));}finally{cam.fieldOfView=f;cam.farClipPlane=far;}SaveApartment();return "Full-height wide lens and complete street plan captured.";
 }
 public static string CheckSideBranches()
 {
  var w=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var cc=w.GetComponent<CharacterController>();var pos=w.transform.position;var rot=w.transform.rotation;var log=new System.Text.StringBuilder();
  try{foreach(var target in new[]{new Vector3(-22,0,58),new Vector3(40,0,58)}){cc.enabled=false;w.transform.position=new Vector3(0,.1f,58);cc.enabled=true;Physics.SyncTransforms();bool ok=false;for(int i=0;i<1400;i++){var d=target-w.transform.position;d.y=0;if(d.magnitude<.1f){ok=true;break;}w.transform.rotation=Quaternion.LookRotation(d);w.Step(Vector2.up,.02f,false);}log.AppendLine(target+" reached="+ok+" at "+w.transform.position);}}
  finally{cc.enabled=false;w.transform.SetPositionAndRotation(pos,rot);cc.enabled=true;Physics.SyncTransforms();}System.IO.File.WriteAllText(ConnectionFolder+"Branch Checks.txt",log.ToString());return log.ToString();
 }
}
