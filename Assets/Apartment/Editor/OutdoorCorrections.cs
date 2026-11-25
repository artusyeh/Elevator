using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
public static partial class ApartmentBuilder
{
 public static string OutdoorCorrections()
 {
  var offPath=Base+"Materials/Unlit Curtain.mat";var off=AssetDatabase.LoadAssetAtPath<Material>(offPath);if(off==null){off=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Curtain Soft.mat"));AssetDatabase.CreateAsset(off,offPath);}off.DisableKeyword("_EMISSION");off.SetColor("_EmissionColor",Color.black);off.SetColor("_BaseColor",new Color(.22f,.25f,.24f));off.globalIlluminationFlags=MaterialGlobalIlluminationFlags.EmissiveIsBlack;
  string path=Base+"Prefabs/Side Streets.prefab";var prefab=PrefabUtility.LoadPrefabContents(path);int count=0;
  foreach(var r in prefab.GetComponentsInChildren<Renderer>().Where(r=>r.name=="Window Curtain")){if(r.sharedMaterial.name=="Curtain Soft")r.sharedMaterial=off;count++;}
  var end=G("Service Lane Fixtures",prefab.transform);
  foreach(float x in new[]{-23f,40f}){var at=new Vector3(x,0,49)-Address;B("Service Lamp Mount",end,at+new Vector3(1.65f,2.45f,0),new Vector3(.08f,.23f,.16f),"Green Steel",false);ResidentialLight(end,at+new Vector3(1.61f,2.45f,0));Lamp(end,at+new Vector3(.8f,2.3f,0),1.2f,6,4100,Quaternion.identity);Prop("Potted Foliage",end,at+new Vector3(1.3f,.01f,1.7f),0,.6f,false);}
  // Restrained reflected light at the focal corner reveals facade setbacks above the porches.
  Lamp(end,new Vector3(-3.7f,4.6f,58.5f)-Address,1.2f,7.5f,4700,Quaternion.identity);
  SharedSave(prefab.transform,"Side Streets");PrefabUtility.UnloadPrefabContents(prefab);AssetDatabase.SaveAssets();return "Corrected curtain emission and lit both service bends; "+count+" window surfaces reviewed.";
 }
}
