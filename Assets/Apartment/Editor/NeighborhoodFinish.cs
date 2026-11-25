using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
public static partial class ApartmentBuilder
{
 public static string FinishNeighborhood()
 {
  if(SceneManager.GetActiveScene().name!="Alley")EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");
  var source=GameObject.Find("Alley").GetComponentsInChildren<Transform>().First(t=>t.name=="Outdoor AC Condenser");
  var ac=UnityEngine.Object.Instantiate(source.gameObject);ac.name="Outdoor Condenser";ac.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(ac,Base+"Prefabs/Outdoor Condenser.prefab");UnityEngine.Object.DestroyImmediate(ac);
  var path=Base+"Prefabs/Aoba Exterior.prefab";var ext=PrefabUtility.LoadPrefabContents(path);
  var details=G("Exterior Equipment",ext.transform);
  for(int floor=0;floor<6;floor++)
  {float y=floor*2.9f;var unit=Prop("Outdoor Condenser",details,new Vector3(3.8f,y+.5f,-.48f),180,.43f);foreach(float x in new[]{3.56f,4.03f}){B("AC Wall Bracket",details,new Vector3(x,y+.27f,-.39f),new Vector3(.035f,.035f,.48f),"Green Steel",false);B("Bracket Plate",details,new Vector3(x,y+.39f,-.19f),new Vector3(.055f,.28f,.025f),"Green Steel",false);}PipeRun(details,new Vector3(4.3f,y+.5f,-.27f),new Vector3(4.3f,y+2.4f,-.27f),.014f);}
  foreach(var l in ext.GetComponentsInChildren<Light>())l.lightmapBakeType=LightmapBakeType.Realtime;
  SharedSave(ext.transform,"Aoba Exterior");PrefabUtility.UnloadPrefabContents(ext);
  path=Base+"Prefabs/Side Streets.prefab";var context=PrefabUtility.LoadPrefabContents(path);
  foreach(var house in context.transform.Cast<Transform>().Where(t=>t.name=="Neighboring House"&&t.localPosition.z+64<54&&t.localPosition.x+22>8))house.localRotation=Quaternion.Euler(0,180,0);
  foreach(var l in context.GetComponentsInChildren<Light>()){l.intensity=5.2f;l.spotAngle=145;l.innerSpotAngle=95;l.lightmapBakeType=LightmapBakeType.Realtime;}
  var dressing=G("Residential Details",context.transform);
  foreach(var spec in new[]{new Vector3(12,.01f,60.4f),new Vector3(28,.01f,60.5f),new Vector3(-8,.01f,60.5f)}){Prop("Potted Fern",dressing,spec-Address,0,.5f,false);Prop("Drink Crates",dressing,spec-Address+Vector3.right*.65f,0,.28f,false);}
  Prop("Basket Bicycle",dressing,new Vector3(26,.01f,60.4f)-Address,90,1.04f,false);
  // Small warm bounce under the lamps gives readable nearby facades, without uniform fill.
  foreach(float x in new[]{-15f,9f,30f})Lamp(dressing,new Vector3(x,2.9f,59.2f)-Address,.8f,5.5f,4200,Quaternion.identity);
  SharedSave(context.transform,"Side Streets");PrefabUtility.UnloadPrefabContents(context);
  var sky=G("Exterior Sky Light",null);foreach(var light in GameObject.Find("Alley").GetComponentsInChildren<Light>().Where(l=>l.type==LightType.Directional)){var copy=CopyPart(light.transform,sky);copy.position-=Address;}SharedSave(sky,"Exterior Sky Light");UnityEngine.Object.DestroyImmediate(sky.gameObject);
  EditorSceneManager.SaveScene(SceneManager.GetActiveScene());EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");if(GameObject.Find("Exterior Sky Light")==null)SharedInstance("Exterior Sky Light",Vector3.zero);
  UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>().eyes.clearFlags=CameraClearFlags.Skybox;
  var volume=G("Exterior Exposure",null).gameObject.AddComponent<Volume>();volume.isGlobal=false;volume.priority=5;volume.blendDistance=.6f;volume.sharedProfile=AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Alley/Materials/AlleyNightVolume.asset");var box=volume.gameObject.AddComponent<BoxCollider>();box.isTrigger=true;box.center=new Vector3(0,5,-8);box.size=new Vector3(80,30,15.5f);
  SaveApartment();return "Shared facade equipment, occupied street details, readable pools of light, matching outdoor sky light and exterior exposure installed.";
 }
}
