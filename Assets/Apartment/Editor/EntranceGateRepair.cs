using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static partial class ApartmentBuilder
{
 static void GateStrip(Transform parent,Vector3 a,Vector3 b)
 {var t=B("Folding Steel Strip",parent,(a+b)*.5f,new Vector3(.024f,(b-a).magnitude,.006f),"Green Steel",false,.001f);t.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
 public static string CompleteEntranceGate()
 {
  if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");System.IO.Directory.CreateDirectory("References/Entrance Gate");var path=Base+"Prefabs/Aoba Exterior.prefab";System.IO.File.Copy(path,"References/Entrance Gate/Aoba Exterior.before.prefab",true);var root=PrefabUtility.LoadPrefabContents(path);
  try
  {
   var gate=root.transform.Find("Security Gate");foreach(Transform t in gate.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);gate.localPosition=new Vector3(0,0,-.065f);gate.localRotation=Quaternion.identity;
   foreach(float x in new[]{-1.145f,1.145f})B("Gate Jamb",gate,new Vector3(x,1.09f,0),new Vector3(.05f,2.18f,.10f),"Green Steel",true,.002f);
   B("Top Guide Track",gate,new Vector3(0,2.175f,0),new Vector3(2.34f,.065f,.12f),"Green Steel",false,.003f);
   B("Bottom Guide Track",gate,new Vector3(0,.013f,0),new Vector3(2.30f,.026f,.12f),"Stainless Steel",false,.001f);
   B("Bottom Track Slot",gate,new Vector3(0,.027f,0),new Vector3(2.23f,.002f,.014f),"Black Rubber",false,.0002f);
   foreach(int side in new[]{-1,1})
   {
    var leaf=G(side<0?"Left Gate Leaf":"Right Gate Leaf",gate);float start=side<0?-1.108f:.018f,end=side<0?-.018f:1.108f;
    for(int bay=0;bay<=5;bay++)
    {
     float x=Mathf.Lerp(start,end,bay/5f);B("Gate Stile",leaf,new Vector3(x,1.087f,0),new Vector3(.021f,2.094f,.020f),"Green Steel",false,.001f);
     B("Top Guide Shoe",leaf,new Vector3(x,2.143f,0),new Vector3(.031f,.024f,.038f),"Stainless Steel",false,.001f);
     B("Bottom Guide Shoe",leaf,new Vector3(x,.040f,0),new Vector3(.031f,.022f,.038f),"Stainless Steel",false,.001f);
    }
    for(int bay=0;bay<5;bay++)for(int row=0;row<4;row++)
    {
     float x0=Mathf.Lerp(start,end,bay/5f),x1=Mathf.Lerp(start,end,(bay+1)/5f),y0=.052f+row*.517f,y1=y0+.517f;
     GateStrip(leaf,new Vector3(x0,y0,-.014f),new Vector3(x1,y1,-.014f));GateStrip(leaf,new Vector3(x1,y0,-.023f),new Vector3(x0,y1,-.023f));
     Rod("Pivot Rivet",leaf,new Vector3((x0+x1)*.5f,(y0+y1)*.5f,-.030f),new Vector3((x0+x1)*.5f,(y0+y1)*.5f,-.035f),.008f,"Stainless Steel");
    }
    B("Leading Stile",leaf,new Vector3(side*.018f,1.086f,-.004f),new Vector3(.029f,2.10f,.032f),"Green Steel",false,.001f);
   }
   B("Lock Case",gate,new Vector3(.045f,1.065f,-.038f),new Vector3(.10f,.18f,.035f),"Green Steel",false,.003f);
   B("Latch Keeper",gate,new Vector3(-.035f,1.065f,-.038f),new Vector3(.045f,.11f,.035f),"Stainless Steel",false,.002f);
   Rod("Pull Handle",gate,new Vector3(.07f,1.04f,-.078f),new Vector3(.07f,1.19f,-.078f),.011f,"Stainless Steel");
   foreach(float y in new[]{1.04f,1.19f})Rod("Handle Stand-off",gate,new Vector3(.07f,y,-.055f),new Vector3(.07f,y,-.078f),.009f,"Stainless Steel");
   Rod("Key Cylinder",gate,new Vector3(.045f,1.005f,-.058f),new Vector3(.045f,1.005f,-.064f),.012f,"Brass");
   foreach(float x in new[]{-1.145f,1.145f})foreach(float y in new[]{.18f,1.1f,2.02f})Rod("Frame Anchor",gate,new Vector3(x,y,-.052f),new Vector3(x,y,-.06f),.008f,"Stainless Steel");
   SharedSave(root.transform,"Aoba Exterior");
  }
  finally{PrefabUtility.UnloadPrefabContents(root);}
  AssetDatabase.SaveAssets();return "Complete paired folding gate fitted to the shared entrance: connected stiles/strips, guides, pivots, latch and anchors.";
 }
 public static string EntranceGateViews()
 {
  EnsureApartment();Capture("Gate Inside",new Vector3(0,1.65f,2.3f),new Vector3(0,1.3f,-2));Capture("Gate Apartment",new Vector3(0,1.65f,-3),new Vector3(0,1.3f,1));SaveApartment();EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");Capture("Gate Alley",new Vector3(22,1.65f,61),new Vector3(22,1.3f,65));SaveApartment();
  foreach(string name in new[]{"Gate Inside","Gate Apartment","Gate Alley"})System.IO.File.Copy("References/Apartment/Captures/"+name+".png","References/Entrance Gate/"+name+".png",true);return "Matching entrance captures saved in both scenes.";
 }
}
