using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class ApartmentBuilder
{
    static void EnsureApartment()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode before editing.");
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Apartment.unity")
        {var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.isDirty)EditorSceneManager.SaveScene(scene);EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");}
    }
    static void SaveApartment(){AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());}
    public static string LockStairwells()
    {
        EnsureApartment();int count=0;
        foreach(var floor in GameObject.Find("Apartment").transform.Cast<Transform>().Where(t=>t.name=="Ground Floor"||t.name.StartsWith("Floor ")))
        {
            var stairs=floor.Find("Stairwell");if(stairs.Find("Stairwell Door")!=null)continue;
            var door=G("Stairwell Door",stairs);door.localPosition=new Vector3(-2.09f,0,9.5f);door.localRotation=Quaternion.Euler(0,-90,0);
            B("Door Leaf",door,new Vector3(0,1.08f,0),new Vector3(.94f,2.12f,.06f),"Green Steel");
            foreach(float x in new[]{-.50f,.50f})B("Door Jamb",door,new Vector3(x,1.11f,0),new Vector3(.065f,2.22f,.18f),"Stainless Steel");
            B("Door Header",door,new Vector3(0,2.18f,0),new Vector3(1.065f,.075f,.18f),"Stainless Steel");
            B("Header Infill",door,new Vector3(0,2.44f,.015f),new Vector3(1.12f,.45f,.18f),"Yellowed Plaster");
            B("Door Threshold",door,new Vector3(0,.009f,0),new Vector3(1.0f,.018f,.22f),"Stainless Steel",false);
            B("Kick Plate",door,new Vector3(0,.22f,-.035f),new Vector3(.78f,.26f,.009f),"Stainless Steel",false);
            B("Lock Plate",door,new Vector3(.34f,1.02f,-.038f),new Vector3(.065f,.17f,.016f),"Stainless Steel",false);
            Rod("Handle Spindle",door,new Vector3(.34f,1.04f,-.045f),new Vector3(.34f,1.04f,-.09f),.013f,"Stainless Steel");
            Rod("Lever Handle",door,new Vector3(.34f,1.04f,-.09f),new Vector3(.23f,1.04f,-.09f),.012f,"Stainless Steel");
            Rod("Keyhole",door,new Vector3(.34f,.96f,-.048f),new Vector3(.34f,.96f,-.052f),.008f,"Black Rubber");
            foreach(float y in new[]{.28f,1.08f,1.86f})Rod("Door Hinge",door,new Vector3(-.465f,y-.055f,-.035f),new Vector3(-.465f,y+.055f,-.035f),.012f,"Stainless Steel");
            B("Door Closer",door,new Vector3(-.23f,2.04f,-.055f),new Vector3(.21f,.055f,.055f),"Stainless Steel",false);
            var plate=B("Stair Sign",door,new Vector3(0,1.57f,-.037f),new Vector3(.27f,.14f,.012f),"Aged Plastic",false);
            Text("STAIRS",plate,new Vector3(0,.025f,-.008f),.048f,Quaternion.identity,new Color(.08f,.09f,.07f));
            Text("LOCKED",plate,new Vector3(0,-.035f,-.008f),.033f,Quaternion.identity,new Color(.36f,.11f,.06f));
            var labels=AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/World Labels.mat");foreach(var text in door.GetComponentsInChildren<TextMesh>())text.GetComponent<Renderer>().sharedMaterial=labels;
            door.gameObject.AddComponent<LockedDoor>();
            var clone=UnityEngine.Object.Instantiate(door.gameObject);clone.name="Stairwell Door";clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);if(count==0)PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/Stairwell Door.prefab");UnityEngine.Object.DestroyImmediate(clone);count++;
        }
        SaveApartment();return "Locked stairwell doors installed on "+count+" floors.";
    }
    public static string CheckStairLocks()
    {
        EnsureApartment();var walker=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var cc=walker.GetComponent<CharacterController>();var saved=walker.transform.position;var rotation=walker.transform.rotation;var report=new System.Text.StringBuilder();int pass=0,total=0;
        try
        {
            for(int floor=0;floor<6;floor++)foreach(float z in new[]{9.1f,9.3f,9.5f,9.7f,9.9f,10.3f,10.7f})
            {cc.enabled=false;walker.transform.SetPositionAndRotation(new Vector3(0,floor*2.9f+.10f,z),Quaternion.Euler(0,-90,0));cc.enabled=true;Physics.SyncTransforms();for(int i=0;i<110;i++)walker.Step(Vector2.up,.02f,false);bool blocked=walker.transform.position.x>-2;total++;if(blocked)pass++;report.AppendLine("Floor "+(floor+1)+" z="+z+" blocked="+blocked+" reached="+walker.transform.position);}
        }
        finally{cc.enabled=false;walker.transform.SetPositionAndRotation(saved,rotation);cc.enabled=true;Physics.SyncTransforms();}
        Directory.CreateDirectory("References/Apartment/Office Polish");File.WriteAllText("References/Apartment/Office Polish/Stair Lock Checks.txt",report.ToString());return pass+"/"+total+" CharacterController approaches blocked.";
    }
}
