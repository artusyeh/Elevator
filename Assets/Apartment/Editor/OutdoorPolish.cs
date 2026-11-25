using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
public static partial class ApartmentBuilder
{
 const string OutdoorReview="References/Outdoor Polish/";
 static Transform LocalAC(Transform p,Vector3 at,float yaw=180){return Prop("Outdoor Condenser",p,at,yaw,.43f);}
 static void ResidentialLight(Transform p,Vector3 at)
 {
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Alley/Prefabs/Lighting/Residential Light.prefab");var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.name="Porch Light";g.transform.SetParent(p,false);g.transform.localPosition=at;g.transform.localRotation=Quaternion.identity;
  Lamp(g.transform,new Vector3(0,-.07f,-.12f),.85f,4,3900,Quaternion.identity);var l=g.GetComponentInChildren<Light>();l.lightmapBakeType=LightmapBakeType.Realtime;
 }
 static void LivingWindow(Transform p,Vector3 at,float width,float height,bool warm,bool grille)
 {
  ExteriorWindow(p,at,width,height,false,grille);
  // Curtains sit behind the frames and are softly lit rather than using mirror-like glass over a solid wall.
  var curtain=B("Window Curtain",p,at+Vector3.back*.051f,new Vector3(width-.11f,height-.11f,.006f),"Paper",false);
  curtain.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/"+(warm?"Curtain Amber":"Curtain Soft")+".mat");
  if(warm)Lamp(p,at+new Vector3(0,-height*.2f,-.22f),.13f,1.5f,3000,Quaternion.identity);
 }
 static void LivedInHouse(Transform house,int index)
 {
  var body=house.Find("Building");if(body==null)return;var size=body.GetComponent<MeshFilter>().sharedMesh.bounds.size;
  float front=-size.z*.5f-.12f; foreach(var oldWindow in house.Cast<Transform>().Where(t=>new[]{"Window Recess","Window Glass","Window Frame","Window Sill","Window Grille","Water Stain"}.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(oldWindow.gameObject);
  var details=G("Household Details",house);
  B("Entrance Recess",details,new Vector3(size.x*.20f,1.02f,front),new Vector3(.96f,2.04f,.11f),"Black Rubber");
  B("Entrance Door",details,new Vector3(size.x*.20f,1.02f,front-.065f),new Vector3(.82f,2,.035f),"Old Wood");
  foreach(float x in new[]{size.x*.20f-.45f,size.x*.20f+.45f})B("Door Frame",details,new Vector3(x,1.06f,front-.095f),new Vector3(.05f,2.12f,.05f),"Green Steel",false);
  Rod("Door Pull",details,new Vector3(size.x*.20f+.29f,.94f,front-.11f),new Vector3(size.x*.20f+.29f,1.1f,front-.11f),.014f,"Stainless Steel");
  B("Entrance Canopy",details,new Vector3(size.x*.20f,2.23f,front-.26f),new Vector3(1.35f,.08f,.66f),"Green Steel",false);
  ResidentialLight(details,new Vector3(size.x*.20f-.65f,2.0f,front-.07f));
  for(int floor=0;floor<Math.Min(4,(int)(size.y/2.8f));floor++)
  {
   float x=-size.x*.34f,y=.57f+floor*2.8f;LocalAC(details,new Vector3(x,y,front-.28f));
   foreach(float side in new[]{-.22f,.22f})B("AC Bracket",details,new Vector3(x+side,y-.04f,front-.20f),new Vector3(.035f,.04f,.5f),"Green Steel",false);
   LivingWindow(details,new Vector3(-size.x*.24f,1.65f+floor*2.8f,front+.075f),1.02f,1.1f,(floor+index)%4==0,floor==0);
  }
  PipeRun(details,new Vector3(size.x*.42f,.1f,front-.08f),new Vector3(size.x*.42f,size.y-.15f,front-.08f),.035f);
  PipeRun(details,new Vector3(-size.x*.44f,.3f,front-.08f),new Vector3(-size.x*.44f,2.6f,front-.08f),.012f);
  B("Meter Box",details,new Vector3(-size.x*.44f,1.43f,front-.11f),new Vector3(.20f,.32f,.13f),"Aged Plastic",false);
  for(int i=0;i<3;i++)Rod("Wall Conduit",details,new Vector3(-size.x*.45f+i*.055f,2.6f,front-.09f),new Vector3(size.x*.40f,2.6f+i*.04f,front-.09f),.008f,"Black Rubber");
  B("Rain Gutter",details,new Vector3(0,size.y-.07f,front),new Vector3(size.x,.07f,.10f),"Green Steel",false);
  if(index%2==0){Prop("Potted Foliage",details,new Vector3(size.x*.20f+.85f,.01f,front-.3f),index*27,.55f,false);Prop("Cleaning Bucket",details,new Vector3(size.x*.20f-1,.01f,front-.24f),0,.25f,false);}
  Weather(details,"Water Stain",new Vector3(size.x*.42f,1.1f,front+.025f),new Vector2(.45f,1.9f),Quaternion.identity);
 }
 static void CornerApartments(Transform parent)
 {
  var root=G("Corner Apartments",parent);root.localPosition=new Vector3(-2.5f,0,65.6f)-Address;
  var concrete=Plain("Corner Concrete",new Color(.60f,.60f,.54f),.24f);concrete.SetTexture("_BaseMap",M("Concrete").GetTexture("_BaseMap"));concrete.SetTextureScale("_BaseMap",Vector2.one*.45f);
  var lower=B("Main Building",root,new Vector3(0,4.35f,.6f),new Vector3(6.7f,8.7f,8),"Concrete");lower.GetComponent<Renderer>().sharedMaterial=concrete;
  B("Stair Tower",root,new Vector3(-3.75f,5.65f,1.2f),new Vector3(1.15f,11.3f,6.8f),"Yellowed Plaster");
  B("Upper Setback",root,new Vector3(.5f,10.05f,2),new Vector3(4.9f,2.7f,5.2f),"Concrete");
  B("Roof Coping",root,new Vector3(0,8.73f,.6f),new Vector3(6.9f,.16f,8.2f),"Green Steel",false);
  B("Roof Coping",root,new Vector3(.5f,11.43f,2),new Vector3(5.1f,.16f,5.4f),"Concrete",false);
  for(int level=0;level<3;level++)
  {
   float y=level*2.85f;
   B("Facade Belt",root,new Vector3(0,y+.13f,-3.48f),new Vector3(6.7f,.15f,.18f),"Concrete",false);
   LivingWindow(root,new Vector3(-1.95f,y+1.60f,-3.46f),1.4f,1.35f,level==1,level==0);
   LivingWindow(root,new Vector3(.95f,y+1.60f,-3.46f),1.7f,1.35f,level==2,false);
   if(level>0)
   {
    B("Balcony Slab",root,new Vector3(.95f,y-.02f,-3.90f),new Vector3(2.5f,.13f,.92f),"Concrete");
    Rod("Balcony Handrail",root,new Vector3(-.27f,y+.98f,-4.32f),new Vector3(2.17f,y+.98f,-4.32f),.019f,"Green Steel");
    foreach(float x in new[]{-.27f,2.17f})Rod("Balcony Return",root,new Vector3(x,y+.98f,-4.32f),new Vector3(x,y+.98f,-3.46f),.019f,"Green Steel");
    for(int i=0;i<14;i++)Rod("Balcony Railing",root,new Vector3(-.27f+i*.187f,y+.06f,-4.32f),new Vector3(-.27f+i*.187f,y+.98f,-4.32f),.008f,"Green Steel");
   }
   LocalAC(root,new Vector3(2.65f,y+.6f,-3.78f));
   foreach(float dx in new[]{2.42f,2.88f})B("AC Support",root,new Vector3(dx,y+.48f,-3.65f),new Vector3(.04f,.04f,.52f),"Green Steel",false);
  }
  B("Recessed Entry",root,new Vector3(-.6f,1.05f,-3.53f),new Vector3(1.0f,2.1f,.11f),"Old Wood");B("Entry Awning",root,new Vector3(-.6f,2.3f,-3.87f),new Vector3(1.5f,.10f,.9f),"Green Steel",false);
  ResidentialLight(root,new Vector3(-1.3f,2.05f,-3.65f));Prop("Entrance Intercom",root,new Vector3(.05f,1.4f,-3.64f),0,.15f,false);
  Prop("Potted Fern",root,new Vector3(-1.3f,.01f,-3.83f),0,.55f,false);
  foreach(float x in new[]{-3.28f,3.25f})PipeRun(root,new Vector3(x,.1f,-3.62f),new Vector3(x,8.7f,-3.62f),.035f);
  B("Electrical Cabinet",root,new Vector3(-2.85f,1.0f,-3.60f),new Vector3(.34f,.5f,.14f),"Green Steel",false);
  PipeRun(root,new Vector3(-2.85f,1.25f,-3.61f),new Vector3(-2.85f,2.7f,-3.61f),.012f);
  B("Roof Water Tank",root,new Vector3(-1.65f,9.2f,1.5f),new Vector3(1.2f,.9f,1.5f),"Aged Plastic",false);
  for(int i=0;i<3;i++)Weather(root,"Water Stain",new Vector3(-2.4f+i*2.2f,4.9f,-3.507f),new Vector2(.5f,2.2f),Quaternion.identity);
 }
 public static string OutdoorPolish()
 {
  if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");Directory.CreateDirectory(OutdoorReview);
  var sc=SceneManager.GetActiveScene();if(sc.isDirty)EditorSceneManager.SaveScene(sc);
  foreach(string name in new[]{"Alley","Apartment"})File.Copy("Assets/Scenes/"+name+".unity",OutdoorReview+name+".before.unity",true);
  string path=Base+"Prefabs/Side Streets.prefab";File.Copy(path,OutdoorReview+"Side Streets.before.prefab",true);var prefab=PrefabUtility.LoadPrefabContents(path);var root=prefab.transform;
  if(root.Find("Neighborhood Layers")!=null)throw new Exception("Outdoor polish already installed.");
  var focal=root.Cast<Transform>().First(t=>t.name=="Neighboring House"&&Mathf.Abs(t.localPosition.x+Address.x+3)<.1f&&t.localPosition.z+Address.z>60);UnityEngine.Object.DestroyImmediate(focal.gameObject);CornerApartments(root);
  int index=0;foreach(var house in root.Cast<Transform>().Where(t=>t.name=="Neighboring House").ToArray())LivedInHouse(house,index++);
  var layers=G("Neighborhood Layers",root);
  B("Continuous Ground",layers,new Vector3(5,-.50f,45)-Address,new Vector3(165,.3f,170),"Concrete");
  // Overlapping sightlines through three staggered rows, with real facades instead of flat masking sheets.
  for(int i=0;i<9;i++)Neighbor(layers,new Vector3(-37+i*11,0,78+(i%3)*1.7f)-Address,new Vector3(11.4f,8.5f+(i%4)*2.3f,8+(i%2)*2),i+2);
  for(int i=0;i<10;i++)Neighbor(layers,new Vector3(-43+i*11.5f,0,101+(i%3)*2)-Address,new Vector3(12,13+(i%4)*3,12),i);
  for(int side=-1;side<=1;side+=2)for(int i=0;i<8;i++)
  {
   float x=side<0?-34:52;var block=G("Residential Block",layers);block.localPosition=new Vector3(x,0,-5+i*12)-Address;block.localRotation=Quaternion.Euler(0,side<0?-90:90,0);
   Neighbor(block,Vector3.zero,new Vector3(12.5f,8+(i%4)*2.5f,10),i);
  }
  // Fill the old alley's rear service gaps while leaving all existing playable gaps and recesses intact.
  foreach(float x in new[]{-17f,17f})for(int i=0;i<5;i++)Neighbor(layers,new Vector3(x,0,2+i*11)-Address,new Vector3(9,7+(i%3)*2.8f,11.5f),i);
  foreach(float x in new[]{-25f,42f})Neighbor(layers,new Vector3(x,0,38)-Address,new Vector3(13,10,10),3);
  foreach(float x in new[]{-27f,45f})Neighbor(layers,new Vector3(x,0,63)-Address,new Vector3(7,8,7),1);
  // The gaps between primary houses become roofed service recesses with equipment, not views onto unbuilt land.
  var service=G("Service Courts",root);foreach(float x in new[]{-16.5f,-7.5f,1.5f,10.5f,31f})
  {
   B("Court Floor",service,new Vector3(x,-.12f,63)-Address,new Vector3(1.15f,.24f,5.5f),"Concrete");
   B("Service Rear Wall",service,new Vector3(x,1.5f,65.7f)-Address,new Vector3(1.2f,3,.18f),"Concrete");
   B("Service Canopy",service,new Vector3(x,2.8f,62.5f)-Address,new Vector3(1.1f,.06f,2.5f),"Green Steel",false);
   Prop("Cleaning Bucket",service,new Vector3(x,.01f,62.3f)-Address,0,.28f,false);
  }
  foreach(var l in root.GetComponentsInChildren<Light>())l.lightmapBakeType=LightmapBakeType.Realtime;
  foreach(var fixture in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Side Street Lamp"))
  {var light=fixture.GetComponentInChildren<Light>();light.intensity=6.5f;light.range=7;Lamp(fixture,new Vector3(0,3.5f,-.25f),.55f,5,4500,Quaternion.identity);}
  SharedSave(root,"Side Streets");PrefabUtility.UnloadPrefabContents(prefab);
  foreach(string name in new[]{"Alley","Apartment"})
  {
   EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");RenderSettings.skybox=AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Tokyo Night.mat");
   var walker=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();walker.eyes.clearFlags=CameraClearFlags.Skybox;walker.eyes.farClipPlane=250;
   SaveApartment();
  }
  EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");return "Layered neighborhood, redesigned corner apartments, lived-in side streets and shared atmospheric sky installed.";
 }
 public static string OutdoorPolishViews()
 {
  EnsureApartment();ConnectionCapture("Outdoor View",new Vector3(0,1.65f,2),new Vector3(0,1.4f,-8));File.Copy(ConnectionFolder+"Outdoor View.png",OutdoorReview+"From Apartment.png",true);SaveApartment();EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");var w=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();
  string[] names={"Player Spawn","Main Street","Junction Forward","Left Street","Right Street","Apartment Entrance","Night Sky","Neighborhood Overview","Left Bend","Right Bend"};
  Vector3[] at={w.eyes.transform.position,new Vector3(0,1.65f,27),new Vector3(0,1.65f,56.7f),new Vector3(-8,1.65f,58),new Vector3(9,1.65f,58),new Vector3(22,1.65f,59),new Vector3(0,1.65f,28),new Vector3(7,60,47),new Vector3(-23,1.65f,54),new Vector3(40,1.65f,54)};
  Vector3[] target={new Vector3(0,1.5f,60),new Vector3(0,1.5f,62),new Vector3(-2.5f,2.2f,62),new Vector3(-24,1.5f,58),new Vector3(30,1.5f,58),new Vector3(22,1.5f,65),new Vector3(0,25,44),new Vector3(7,0,47.01f),new Vector3(-23,1.4f,46),new Vector3(40,1.4f,46)};
  for(int i=0;i<names.Length;i++){Capture(names[i],at[i],target[i]);File.Copy("References/Apartment/Captures/"+names[i]+".png",OutdoorReview+names[i]+".png",true);}SaveApartment();return "Player-height, sky, overview and both bend captures saved.";
 }
}

