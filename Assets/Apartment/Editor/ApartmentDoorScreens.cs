using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    public static string RepairStairDoors()
    {
        EnsureApartment();
        var paint=Plain("Stair Door Paint",new Color(.30f,.37f,.32f),.30f,.08f);
        paint.SetTexture("_BumpMap",M("Green Steel").GetTexture("_BumpMap"));paint.SetFloat("_BumpScale",.10f);paint.EnableKeyword("_NORMALMAP");
        Plain("Door Seal",new Color(.028f,.032f,.027f),.15f);
        Plain("Stair Plaque Enamel",new Color(.62f,.61f,.53f),.25f,.05f);
        int count=0;
        foreach(var locked in GameObject.Find("Apartment").GetComponentsInChildren<LockedDoor>())
        {
            var door=locked.transform;foreach(Transform child in door.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
            door.localPosition=new Vector3(-2.09f,0,9.56f);door.localRotation=Quaternion.Euler(0,-90,0);
            // The leaf is recessed behind the frame. Separate seals keep every visible seam deliberate.
            B("Door Leaf",door,new Vector3(0,1.067f,-.014f),new Vector3(.94f,2.11f,.045f),"Stair Door Paint",true,.003f);
            foreach(float x in new[]{-.502f,.502f})
            {
                B("Door Jamb",door,new Vector3(x,1.09f,-.012f),new Vector3(.056f,2.18f,.19f),"Stair Door Paint",true,.002f);
                B("Door Seal",door,new Vector3(Mathf.Sign(x)*.473f,1.067f,.011f),new Vector3(.008f,2.12f,.018f),"Door Seal",false,.0008f);
            }
            B("Door Header",door,new Vector3(0,2.18f,-.012f),new Vector3(1.06f,.065f,.19f),"Stair Door Paint",true,.002f);
            B("Header Seal",door,new Vector3(0,2.127f,.011f),new Vector3(.944f,.009f,.018f),"Door Seal",false,.0008f);
            B("Door Threshold",door,new Vector3(0,.0045f,-.006f),new Vector3(.948f,.009f,.15f),"Stainless Steel",false,.001f);
            B("Kick Plate",door,new Vector3(0,.205f,-.039f),new Vector3(.78f,.22f,.003f),"Stainless Steel",false,.0008f);
            B("Lock Plate",door,new Vector3(.344f,1.015f,-.043f),new Vector3(.048f,.16f,.01f),"Stainless Steel",false,.002f);
            Rod("Handle Spindle",door,new Vector3(.344f,1.045f,-.047f),new Vector3(.344f,1.045f,-.083f),.011f,"Stainless Steel");
            Rod("Lever Handle",door,new Vector3(.344f,1.045f,-.083f),new Vector3(.228f,1.045f,-.083f),.010f,"Stainless Steel");
            Rod("Keyhole",door,new Vector3(.344f,.967f,-.049f),new Vector3(.344f,.967f,-.052f),.006f,"Black Rubber");
            foreach(float y in new[]{.27f,1.07f,1.88f})Rod("Door Hinge",door,new Vector3(-.473f,y-.047f,-.040f),new Vector3(-.473f,y+.047f,-.040f),.010f,"Stainless Steel");
            B("Door Closer",door,new Vector3(-.24f,2.055f,-.057f),new Vector3(.20f,.055f,.042f),"Stainless Steel",false,.004f);
            Rod("Closer Arm",door,new Vector3(-.24f,2.09f,-.077f),new Vector3(-.09f,2.145f,-.10f),.006f,"Stainless Steel");
            var sign=G("Stairwell Plaque",door);sign.localPosition=new Vector3(0,1.61f,-.041f);
            B("Enamel Plate",sign,Vector3.zero,new Vector3(.235f,.118f,.005f),"Stair Plaque Enamel",false,.002f);
            Notice("Stairwell Plaque",sign,new Vector3(0,0,-.0033f),new Vector2(.231f,.114f),Quaternion.identity);
            foreach(float x in new[]{-.105f,.105f})Rod("Plaque Screw",sign,new Vector3(x,0,-.003f),new Vector3(x,0,-.0048f),.002f,"Stainless Steel");
            // Close the leftover opening beside and above the frame, aligned with the corridor wall face.
            var stairs=door.parent;var surround=stairs.Find("Door Surround");if(surround!=null)UnityEngine.Object.DestroyImmediate(surround.gameObject);surround=G("Door Surround",stairs);
            float height=stairs.parent.name=="Ground Floor"?2.78f:2.68f;
            Wall(surround,new Vector3(-2.09f,0,9.015f),.03f,height,true);
            float wallEnd=stairs.parent.name=="Ground Floor"?10.45f:11.05f;
            Wall(surround,new Vector3(-2.09f,0,(10.09f+wallEnd)*.5f),wallEnd-10.09f,height,true);
            Wall(surround,new Vector3(-2.09f,0,9.56f),1.06f,height-2.2125f,true,2.2125f);
            if(count++==0){var clone=UnityEngine.Object.Instantiate(door.gameObject);clone.name="Stairwell Door";clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/Stairwell Door.prefab");UnityEngine.Object.DestroyImmediate(clone);}
        }
        SaveApartment();return "Refitted "+count+" stair doors, continuous wall surrounds and Japanese enamel plaques.";
    }
    public static string FitMonitorScreens()
    {
        EnsureApartment();int count=0;Mesh fitted=null;
        foreach(var screen in GameObject.Find("Ground Floor").transform.Find("Management Room/Workstation/Desk Items").GetComponentsInChildren<Transform>().Where(t=>t.name=="Surveillance Screen"))
        {
            var monitor=screen.parent;var source=monitor.GetComponentsInChildren<MeshFilter>().First(f=>f.name=="Television_01");
            if(fitted==null)
            {
                // Extract the scanned curved glass rather than placing a rectangle over the bezel.
                var mesh=source.sharedMesh;var v=mesh.vertices;var n=mesh.normals;var triangles=mesh.triangles;
                var vertices=new System.Collections.Generic.List<Vector3>();var normals=new System.Collections.Generic.List<Vector3>();var uv=new System.Collections.Generic.List<Vector2>();var indices=new System.Collections.Generic.List<int>();
                Func<Vector3,bool> inside=p=>p.x>=-.127f&&p.x<=.258f&&p.y>=.113f&&p.y<=.405f&&p.z>.16f;
                for(int i=0;i<triangles.Length;i+=3)
                {
                    var a=v[triangles[i]];var b=v[triangles[i+1]];var c=v[triangles[i+2]];
                    if(!inside(a)||!inside(b)||!inside(c)||Vector3.Cross(b-a,c-a).normalized.z<.7f)continue;
                    for(int j=0;j<3;j++){int k=triangles[i+j];var p=v[k];vertices.Add(p+n[k]*.00055f);normals.Add(n[k]);uv.Add(new Vector2((.2571f-p.x)/.3834f,(p.y-.1141f)/.2901f));indices.Add(indices.Count);}
                }
                if(indices.Count<300)throw new Exception("Unexpected CRT glass geometry; no display changed.");
                fitted=new Mesh();fitted.name="CRT Glass Display";fitted.SetVertices(vertices);fitted.SetNormals(normals);fitted.SetUVs(0,uv);fitted.SetTriangles(indices,0);fitted.RecalculateTangents();fitted.RecalculateBounds();
                var path=Base+"Models/CRT Glass Display.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old==null)AssetDatabase.CreateAsset(fitted,path);else{EditorUtility.CopySerialized(fitted,old);UnityEngine.Object.DestroyImmediate(fitted);fitted=old;}
            }
            screen.SetParent(source.transform,false);screen.localPosition=Vector3.zero;screen.localRotation=Quaternion.identity;screen.localScale=Vector3.one;screen.GetComponent<MeshFilter>().sharedMesh=fitted;
            var renderer=screen.GetComponent<MeshRenderer>();renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            var m=renderer.sharedMaterial;m.SetColor("_BaseColor",new Color(.42f,.48f,.43f));m.SetColor("_EmissionColor",new Color(.50f,.59f,.52f));m.SetFloat("_Smoothness",.28f);m.EnableKeyword("_EMISSION");EditorUtility.SetDirty(m);count++;
        }
        SaveApartment();return "Fitted "+count+" CCTV images to the original curved glass, following each monitor's transform.";
    }
    public static string DoorScreenViews()
    {
        EnsureApartment();Capture("Stair Door Repaired",new Vector3(.1f,1.6f,10.6f),new Vector3(-2.09f,1.15f,9.56f));Capture("Stair Door Side",new Vector3(-.8f,1.65f,8.6f),new Vector3(-2.09f,1.2f,9.56f));
        Capture("Monitor Screens Repaired",new Vector3(-4.02f,1.38f,3.48f),new Vector3(-3.02f,1.13f,3.69f));Capture("Monitor Screens Side",new Vector3(-3.65f,1.28f,2.95f),new Vector3(-3.02f,1.11f,3.75f));
        foreach(string name in new[]{"Stair Door Repaired","Stair Door Side","Monitor Screens Repaired","Monitor Screens Side"})System.IO.File.Copy("References/Apartment/Captures/"+name+".png","References/Apartment/Door and Screens/"+name+".png",true);return "Four verification views captured.";
    }
}
