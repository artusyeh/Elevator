using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static partial class ApartmentBuilder
{
    static void ConnectionCapture(string name,Vector3 p,Vector3 target){Capture(name,p,target);File.Copy("References/Apartment/Captures/"+name+".png",ConnectionFolder+name+".png",true);}
    public static string ConnectionViews()
    {
        EnsureApartment();ConnectionCapture("From Inside",new Vector3(0,1.65f,2.3f),new Vector3(0,1.3f,-6));ConnectionCapture("Apartment Entrance Match",new Vector3(0,1.65f,-3),new Vector3(0,1.45f,2));SaveApartment();
        EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");
        ConnectionCapture("Main Alley",new Vector3(0,1.65f,43),new Vector3(0,1.5f,60));ConnectionCapture("Junction",new Vector3(0,1.65f,55),new Vector3(3,1.5f,59));
        ConnectionCapture("Looking Left",new Vector3(0,1.65f,58),new Vector3(-18,1.5f,58));ConnectionCapture("Looking Right",new Vector3(0,1.65f,58),new Vector3(24,1.5f,58));
        ConnectionCapture("Apartment Exterior",new Vector3(22,1.65f,55.4f),new Vector3(22,5,64));ConnectionCapture("Entrance Outside",new Vector3(22,1.65f,59),new Vector3(22,1.5f,65));
        ConnectionCapture("Into Apartment",new Vector3(22,1.65f,62.8f),new Vector3(22,1.4f,69));ConnectionCapture("Alley Entrance Match",Address+new Vector3(0,1.65f,-3),Address+new Vector3(0,1.45f,2));
        ConnectionCapture("Full Building",new Vector3(22,2.5f,55.2f),new Vector3(22,9,64));ConnectionCapture("Street Layout",new Vector3(8,75,48),new Vector3(8,0,48.01f));SaveApartment();return "Eleven route views plus matching entrance views captured.";
    }
    public static string CheckConnectionRoute()
    {
        if(SceneManager.GetActiveScene().name!="Alley")EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");var walker=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var cc=walker.GetComponent<CharacterController>();var p=walker.transform.position;var q=walker.transform.rotation;var log=new System.Text.StringBuilder();
        try{cc.enabled=false;walker.transform.position=p;cc.enabled=true;Physics.SyncTransforms();
        foreach(var target in new[]{new Vector3(0,0,12),new Vector3(0,0,28),new Vector3(0,0,44),new Vector3(0,0,58),new Vector3(12,0,58),new Vector3(22,0,58),new Vector3(22,0,61.5f),new Vector3(22,0,63.3f)})
        {bool reached=false;for(int i=0;i<1200;i++){var delta=target-walker.transform.position;delta.y=0;if(delta.magnitude<.09f){reached=true;break;}walker.transform.rotation=Quaternion.LookRotation(delta);walker.Step(new Vector2(0,Mathf.Min(1,delta.magnitude/(walker.speed*.02f))),.02f,false);}log.AppendLine(target+": "+reached+" at "+walker.transform.position);}
        }
        finally{cc.enabled=false;walker.transform.SetPositionAndRotation(p,q);cc.enabled=true;Physics.SyncTransforms();}
        File.WriteAllText(ConnectionFolder+"Walk Checks.txt",log.ToString());return log.ToString();
    }
}

