using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
public static partial class ApartmentBuilder
{
 static ElevatorFloorDisplay LiftDisplay(Transform parent,Vector3 at,float scale=1)
 {
  var root=G("Floor Display",parent);root.localPosition=at;root.localScale=Vector3.one*scale;
  B("Display Housing",root,Vector3.zero,new Vector3(.19f,.14f,.015f),"Black Rubber",false,.003f);
  var display=root.gameObject.AddComponent<ElevatorFloorDisplay>();display.segments=new Renderer[7];
  Vector3[] points={new Vector3(-.015f,.047f,0),new Vector3(.013f,.024f,0),new Vector3(.013f,-.024f,0),new Vector3(-.015f,-.047f,0),new Vector3(-.043f,-.024f,0),new Vector3(-.043f,.024f,0),new Vector3(-.015f,0,0)};
  for(int i=0;i<7;i++){bool horizontal=i==0||i==3||i==6;display.segments[i]=B("LED Segment",root,points[i]+new Vector3(0,0,-.010f),horizontal?new Vector3(.047f,.007f,.003f):new Vector3(.007f,.038f,.003f),"Lift Red LED",false,.001f).GetComponent<Renderer>();}
  for(int i=0;i<2;i++){var arrow=G(i==0?"Up Arrow":"Down Arrow",root);arrow.localPosition=new Vector3(.056f,i==0?.026f:-.026f,-.012f);var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.011f,i==0?-.006f:.006f,0),new Vector3(.011f,i==0?-.006f:.006f,0),new Vector3(0,i==0?.010f:-.010f,0)};mesh.triangles=i==0?new[]{0,2,1}:new[]{0,1,2};mesh.RecalculateNormals();mesh.RecalculateBounds();string path=Base+"Models/"+(i==0?"Display Up Arrow":"Display Down Arrow")+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing==null)AssetDatabase.CreateAsset(mesh,path);else{UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}arrow.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var r=arrow.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=M("Lift Red LED");if(i==0)display.upArrow=r;else display.downArrow=r;}
  display.Show(1,0);return display;
 }
 static void WireLiftButton(Transform surface,Transform parent,Vector3 at,ElevatorButton.Action action,int floor)
 {
  var target=G("Button Interaction",parent);target.localPosition=at;var collider=target.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(.079f,.079f,.055f);
  var button=target.gameObject.AddComponent<ElevatorButton>();button.action=action;button.floor=floor;button.cap=surface;button.lamp=surface.GetComponent<Renderer>();button.lamp.sharedMaterial=M("Lift Button Lens");button.SetLamp(false);
 }
 public static string InstallElevatorControls()
 {
  EnsureApartment();if(UnityEngine.Object.FindFirstObjectByType<ElevatorController>()!=null)return "Lift controls already installed.";
  System.IO.Directory.CreateDirectory("References/Elevator Controls");System.IO.File.Copy("Assets/Scenes/Apartment.unity","References/Elevator Controls/Apartment.before.unity",true);
  var led=Plain("Lift Red LED",new Color(.28f,.008f,.003f),.2f);led.EnableKeyword("_EMISSION");led.SetColor("_EmissionColor",new Color(1.6f,.018f,.005f));
  var lens=Plain("Lift Button Lens",new Color(.35f,.32f,.22f),.37f,.15f);lens.EnableKeyword("_EMISSION");lens.SetColor("_EmissionColor",Color.black);
  var apartment=GameObject.Find("Apartment").transform;var control=G("Elevator Controls",apartment).gameObject.AddComponent<ElevatorController>();var landings=apartment.GetComponentsInChildren<Transform>().Where(t=>t.name=="Elevator Landing").OrderBy(t=>t.position.y).ToArray();
  control.landings=landings;control.leftDoors=landings.Select(t=>t.Find("Left Elevator Door")).ToArray();control.rightDoors=landings.Select(t=>t.Find("Right Elevator Door")).ToArray();
  control.cabin=GameObject.Find("Elevator Cabin").transform;var displays=new System.Collections.Generic.List<ElevatorFloorDisplay>();
  foreach(var landing in landings)
  {
   var oldControl=landing.GetComponent<SlidingDoors>();if(oldControl!=null)oldControl.enabled=false;
   foreach(var text in landing.Cast<Transform>().Where(t=>t.GetComponent<TextMesh>()!=null&&int.TryParse(t.GetComponent<TextMesh>().text,out _)).ToArray())UnityEngine.Object.DestroyImmediate(text.gameObject);
   displays.Add(LiftDisplay(landing,new Vector3(0,2.55f,-.092f)));
   foreach(var part in landing.Cast<Transform>().Where(t=>t.name=="Shaft Floor"||t.name=="Shaft Ceiling").ToArray())UnityEngine.Object.DestroyImmediate(part.gameObject);
   var call=landing.Find("Entrance Details/Call Buttons");if(call!=null){int floor=Array.IndexOf(landings,landing)+1;foreach(var button in call.Cast<Transform>().Where(t=>t.name=="Button Lens").ToArray())WireLiftButton(button,call,button.localPosition+new Vector3(0,0,-.025f),ElevatorButton.Action.Call,floor);}
   // Floors wait with their landing doors shut until the cabin arrives.
   var l=landing.Find("Left Elevator Door");var r=landing.Find("Right Elevator Door");var a=l.localPosition;var b=r.localPosition;a.x=landing==landings[0]?-.96f:-.275f;b.x=-a.x;l.localPosition=a;r.localPosition=b;
  }
  var cabin=control.cabin;var panel=cabin.Find("Control Panel");
  panel.localRotation=Quaternion.Euler(0,90,0);
  foreach(var rail in cabin.Cast<Transform>().Where(t=>t.name=="Handrail"&&t.localPosition.x>0))
  {rail.localPosition=new Vector3(.66f,.94f,1.045f);rail.localScale=new Vector3(.048f,.375f,.048f);}
  foreach(var bracket in cabin.Cast<Transform>().Where(t=>t.name=="Rail Bracket"&&t.localPosition.x>0&&t.localPosition.z<.5f))
  {var p=bracket.localPosition;p.z=.72f;bracket.localPosition=p;}
  foreach(var text in panel.Cast<Transform>().Where(t=>t.GetComponent<TextMesh>()!=null&&t.localPosition.y>1.7f).ToArray())UnityEngine.Object.DestroyImmediate(text.gameObject);
  displays.Add(LiftDisplay(panel,new Vector3(0,1.83f,-.032f),.85f));
  int buttonNumber=1;foreach(var cap in panel.Cast<Transform>().Where(t=>t.name=="Floor Button").OrderBy(t=>t.localPosition.y).ToArray())WireLiftButton(cap,panel,cap.localPosition+new Vector3(0,0,-.017f),ElevatorButton.Action.Floor,buttonNumber++);
  int doorNumber=0;foreach(var cap in panel.Cast<Transform>().Where(t=>t.name=="Door Button").OrderBy(t=>t.localPosition.x).ToArray())WireLiftButton(cap,panel,cap.localPosition+new Vector3(0,0,-.018f),doorNumber++==0?ElevatorButton.Action.Open:ElevatorButton.Action.Close,1);
  var alarm=panel.Find("Emergency Button");WireLiftButton(alarm,panel,alarm.localPosition+new Vector3(0,0,-.018f),ElevatorButton.Action.Alarm,1);
  var inner=G("Cabin Doors",cabin);
  control.cabinLeft=B("Left Cabin Door",inner,new Vector3(-.96f,1.06f,.075f),new Vector3(.55f,2.12f,.032f),"Elevator Door Paint",true,.002f);
  control.cabinRight=B("Right Cabin Door",inner,new Vector3(.96f,1.06f,.075f),new Vector3(.55f,2.12f,.032f),"Elevator Door Paint",true,.002f);
  foreach(var leaf in new[]{control.cabinLeft,control.cabinRight}){float sign=leaf==control.cabinLeft?1:-1;B("Meeting Edge",leaf,new Vector3(sign*.271f,0,.019f),new Vector3(.007f,2.11f,.006f),"Elevator Frame Metal",false,.001f);}
  control.displays=displays.ToArray();
  foreach(var t in cabin.GetComponentsInChildren<Transform>()){GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);var renderer=t.GetComponent<Renderer>();if(renderer!=null)renderer.lightmapIndex=-1;var l=t.GetComponent<Light>();if(l!=null)l.lightmapBakeType=LightmapBakeType.Realtime;}
  var walker=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var interactor=walker.GetComponent<ElevatorInteractor>();if(interactor==null)interactor=walker.gameObject.AddComponent<ElevatorInteractor>();interactor.eyes=walker.eyes;
  var clone=UnityEngine.Object.Instantiate(cabin.gameObject);clone.name="Elevator Cabin";clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/Elevator Cabin.prefab");UnityEngine.Object.DestroyImmediate(clone);
  SaveApartment();return "Six-floor lift movement, click targets, selected button emission, LED floor/direction displays, inner cabin doors and restrained camera vibration installed.";
 }
}
