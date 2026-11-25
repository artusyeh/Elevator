using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    const string CabinReview="References/Elevator Redesign/";
    public static string InspectCabinHardware()
    {
        var cabin=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;var sb=new StringBuilder();
        foreach(var name in new[]{"Ventilation Fan","Cabin Camera","Emergency Light","Control Panel","Cabin Doors"})
        {var t=cabin.Find(name);sb.AppendLine(name+" bounds="+LiftLocalBounds(t.GetComponentInChildren<Renderer>(),cabin));foreach(Transform child in t)sb.AppendLine("  "+child.name+" pos="+child.localPosition+" scale="+child.localScale);}
        File.WriteAllText(CabinReview+"Hardware.txt",sb.ToString());return "Hardware inspected";
    }
    public static string InspectCabin()
    {
        var sb=new StringBuilder();sb.AppendLine("Playing="+EditorApplication.isPlaying+" scene="+UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();if(lift==null)return "No lift";
        foreach(var t in lift.cabin.GetComponentsInChildren<Transform>(true))
        {
            var r=t.GetComponent<Renderer>();var light=t.GetComponent<Light>();var button=t.GetComponent<ElevatorButton>();
            if(t.parent==lift.cabin||light!=null||button!=null||t.GetComponent<LightCircuit>()!=null||t.name.Contains("Panel")||t.name.Contains("Button")||t.name.Contains("Mirror"))
            {
                sb.Append(t.name+" parent="+t.parent.name+" local="+t.localPosition.ToString("F3")+" rot="+t.localEulerAngles+" scale="+t.localScale);
                if(r!=null)sb.Append(" bounds="+LiftLocalBounds(r,lift.cabin).ToString("F3")+" mats="+string.Join(",",r.sharedMaterials.Select(m=>m==null?"none":m.name).ToArray()));
                if(light!=null)sb.Append(" light="+light.type+" intensity="+light.intensity+" kelvin="+light.colorTemperature+" enabled="+light.enabled);
                if(button!=null)sb.Append(" button="+button.action+":"+button.floor+" cap="+(button.cap==null?"none":button.cap.name)+" lamp="+(button.lamp==null?"none":button.lamp.name));
                sb.AppendLine();
            }
        }
        foreach(var receiver in lift.cabin.GetComponentsInChildren<LightCircuit>(true))sb.AppendLine("Circuit="+receiver.name+" "+receiver.circuit.name+" lights="+receiver.lights.Length+" emitters="+receiver.emitters.Length);
        File.WriteAllText(CabinReview+"Cabin Inspection.txt",sb.ToString());
        if(!EditorApplication.isPlaying)
        {
            foreach(var file in new[]{"Assets/Scenes/Apartment.unity",Base+"Prefabs/Elevator Cabin.prefab",Base+"Prefabs/Elevator Landing.prefab",Base+"Prefabs/Entrance Interior.prefab"})
            {string target=CabinReview+Path.GetFileName(file)+".before";if(!File.Exists(target))File.Copy(file,target);}
            CaptureCabinReview("Before");
        }
        return "Inspection and before captures saved.";
    }
    static void CaptureCabinReview(string suffix)
    {
        var c=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;
        Action<string,Vector3,Vector3> view=(name,at,target)=>{string n=name+" "+suffix;Capture(n,c.TransformPoint(at),c.TransformPoint(target));File.Copy("References/Apartment/Captures/"+n+".png",CabinReview+n+".png",true);};
        view("Entrance",new Vector3(0,1.65f,-1.05f),new Vector3(0,1.25f,1.3f));
        view("Interior",new Vector3(-.22f,1.65f,.28f),new Vector3(.42f,1.30f,1.34f));
        view("Mirror",new Vector3(.23f,1.65f,.35f),new Vector3(-.15f,1.55f,1.57f));
        view("Controls",new Vector3(-.36f,1.65f,1.05f),new Vector3(.70f,1.30f,.40f));
        view("Ceiling",new Vector3(-.25f,1.65f,.36f),new Vector3(.12f,2.27f,1.05f));
        view("Floor",new Vector3(-.20f,1.65f,.26f),new Vector3(.22f,.01f,1.02f));
    }
}
