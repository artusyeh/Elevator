using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static partial class ApartmentBuilder
{
 public static string FinishStructuralCleanup()
 {
  var path=Base+"Prefabs/Side Streets.prefab";var shared=PrefabUtility.LoadPrefabContents(path);
  foreach(var details in shared.GetComponentsInChildren<Transform>().Where(t=>t.name=="Household Details").ToArray())foreach(var fixture in details.Cast<Transform>().Where(t=>t.name=="Porch Light")){fixture.localPosition+=Vector3.left*.20f;PrefabUtility.RecordPrefabInstancePropertyModifications(fixture);}
  SharedSave(shared.transform,"Side Streets");PrefabUtility.UnloadPrefabContents(shared);
  if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="Alley")EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");int stands=0,mounts=0;
  foreach(var ac in GameObject.Find("Alley").GetComponentsInChildren<Transform>().Where(t=>t.name=="Outdoor AC Condenser").ToArray())
  {
   var parent=ac.parent;var cabinet=ac.Find("Weathered Cabinet");var b=InParentBounds(cabinet,parent);
   if(ac.localPosition.y>.8f){var wall=parent.Find("Facade Volume");float wallZ=wall==null?-3.0f:InParentBounds(wall,parent).min.z;FitOutdoorUnit(ac,wallZ);mounts++;continue;}
   foreach(var child in ac.Cast<Transform>().Where(t=>new[]{"Support Bracket","Mounting Plate","Mounting Bolt"}.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
   var p=ac.localPosition;p.y+=.17f-b.min.y;ac.localPosition=p;b=InParentBounds(cabinet,parent);var stand=G("Condenser Plinths",parent);
   foreach(float x in new[]{b.center.x-.25f,b.center.x+.25f})
   {
    B("Concrete Plinth",stand,new Vector3(x,.087f,b.center.z),new Vector3(.13f,.154f,.40f),"Concrete",true,.004f);
    B("Isolation Pad",stand,new Vector3(x,.167f,b.center.z),new Vector3(.12f,.006f,.22f),"Black Rubber",false,.001f);
   }
   stands++;
  }
  SaveApartment();return stands+" low condensers seated on concrete plinths and isolation pads; "+mounts+" high units given complete wall anchors.";
 }
}
