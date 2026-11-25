using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string StartCabinVisualChecks()
    {
        if(EditorApplication.isPlaying)return "Already playing";
        SaveApartment();EditorApplication.isPlaying=true;return "Play Mode requested";
    }
    public static string StopCabinVisualChecks(){EditorApplication.isPlaying=false;return "Play Mode exit requested";}
    public static string ClearCabinReviewLogs()
    {
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.LogEntries")).First(t=>t!=null);
        type.GetMethod("Clear",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic,null,Type.EmptyTypes,null).Invoke(null,null);return "Cleared resolved authoring logs before final validation";
    }
    public static string ReviewCabinConsole()
    {
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.LogEntries")).First(t=>t!=null);
        var method=type.GetMethod("GetCountsByType",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);object[] values={0,0,0};method.Invoke(null,values);
        string result=string.Join("; ",method.GetParameters().Select((p,i)=>p.Name+"="+values[i]).ToArray());File.WriteAllText(CabinReview+"Final Console.txt",result);return result;
    }
    public static string FinishCabinReview()
    {
        if(EditorApplication.isPlaying)return "Exit Play Mode first";
        SaveApartment();CaptureCabinReview("After");var root=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin;
        var rs=root.Find("Cabin Finishes").GetComponentsInChildren<Renderer>();int outside=0;
        foreach(var r in rs){var b=LiftLocalBounds(r,root);if(b.min.x<-.779f||b.max.x>.779f||b.min.y<-.001f||b.max.y>2.271f||b.min.z<.149f||b.max.z>1.611f)outside++;}
        File.AppendAllText(CabinReview+"Visual Checks.txt","\nNew finish pieces outside the cabin envelope="+outside+"/"+rs.Length);
        string scene="Assets/Scenes/Apartment.unity";UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");
        var preview=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).First(t=>t.name=="Elevator Cabin");string result="Alley preview finishes="+(preview.Find("Cabin Finishes")!=null)+"; mirror="+(preview.GetComponentInChildren<ApartmentMirror>()!=null)+"; CCTV="+(preview.GetComponentInChildren<CctvChannel>()!=null);
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);File.AppendAllText(CabinReview+"Visual Checks.txt","\n"+result);SaveApartment();return result+"; saved Apartment.";
    }
    public static string CheckCabinVisuals()
    {
        var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();var cabin=lift.cabin;var sb=new StringBuilder();
        foreach(string name in new[]{"Building/Cabin Surface","Building/Aged Mirror"}){var shader=Shader.Find(name);sb.AppendLine(name+": supported="+shader.isSupported+" shader errors="+ShaderUtil.ShaderHasError(shader));foreach(var e in ShaderUtil.GetShaderMessages(shader))sb.AppendLine(e.message);}
        sb.AppendLine("Six floor buttons="+(cabin.GetComponentsInChildren<ElevatorButton>().Count(b=>b.action==ElevatorButton.Action.Floor)==6));
        sb.AppendLine("Button references intact="+cabin.GetComponentsInChildren<ElevatorButton>().All(b=>b.cap!=null&&b.lamp!=null&&b.GetComponent<Collider>()!=null));
        sb.AppendLine("Functional mirror="+(cabin.GetComponentInChildren<ApartmentMirror>().reflectionCamera!=null));
        var receiver=cabin.GetComponent<LightCircuit>();sb.AppendLine("Elevator lighting circuit="+receiver.circuit.name+" lights="+receiver.lights.Length+" emitters="+receiver.emitters.Length);
        var camera=cabin.GetComponentInChildren<CctvChannel>();sb.AppendLine("Cabin CCTV source intact="+(camera!=null&&camera.cameraSource!=null));
        int mounts=0;foreach(var paper in cabin.Find("Cabin Finishes/Certificates").Cast<Transform>())
        {RaycastHit hit;Physics.SyncTransforms();if(Physics.Raycast(paper.position+paper.rotation*Vector3.back*.04f,paper.rotation*Vector3.forward,out hit,.06f)&&hit.collider.name=="Side Panel")mounts++;}
        sb.AppendLine("Wall-backed framed signs="+mounts+"/4");
        File.WriteAllText(CabinReview+"Visual Checks.txt",sb.ToString());return sb.ToString();
    }
    public static string RunCabinVisualChecks()
    {
        if(!EditorApplication.isPlaying)return "Enter Play Mode first";
        var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();var c=lift.cabin;var w=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();var scanner=w.GetComponent<InteractionScanner>();var cc=w.GetComponent<CharacterController>();
        cc.enabled=false;w.transform.position=c.TransformPoint(new Vector3(-.20f,.08f,.80f));cc.enabled=true;Cursor.lockState=CursorLockMode.Locked;Physics.SyncTransforms();var sb=new StringBuilder();int rayHits=0;
        foreach(var button in c.GetComponentsInChildren<ElevatorButton>())
        {w.eyes.transform.LookAt(button.transform.position);w.SyncLookPitch();scanner.RefreshFocus();if(object.ReferenceEquals(scanner.FocusedTarget,button))rayHits++;}
        sb.AppendLine("Unobstructed camera-centred button rays="+rayHits+"/9");
        var top=c.GetComponentsInChildren<ElevatorButton>().First(b=>b.action==ElevatorButton.Action.Floor&&b.floor==6);w.eyes.transform.LookAt(top.transform.position);w.SyncLookPitch();bool pressed=scanner.InteractFocused();var block=new MaterialPropertyBlock();top.lamp.GetPropertyBlock(block);sb.AppendLine("Floor-six interaction="+pressed+" request="+lift.IsRequested(6)+" illuminated ring="+(block.GetColor("_EmissionColor").maxColorComponent>.1f));
        foreach(int floor in new[]{6,1})
        {
            lift.RequestFloor(floor);int ticks=0;while(ticks++<2000){lift.Tick(.05f);if(lift.currentFloor==floor&&!lift.Moving&&lift.DoorAmount>.999f)break;}Physics.SyncTransforms();
            w.transform.rotation=Quaternion.Euler(0,-90,0);int steps=0;while(w.transform.position.x>1.65f&&steps++<100)w.Step(Vector2.up,.02f,false);bool exited=w.transform.position.x<1.72f;
            w.transform.rotation=Quaternion.Euler(0,90,0);steps=0;while(w.transform.position.x<4.08f&&steps++<100)w.Step(Vector2.up,.02f,false);bool inside=c.InverseTransformPoint(w.transform.position).z>.65f;
            sb.AppendLine("Ride to "+floor+": arrival="+(ticks<2000)+" doors fully open="+(lift.DoorAmount>.999f)+" exit="+exited+" reentry="+inside);
        }
        var camera=c.GetComponentInChildren<CctvChannel>();int before=camera.RenderCount;camera.RenderFeed();sb.AppendLine("Live cabin CCTV render="+(camera.RenderCount>before&&camera.Texture!=null));
        var mirror=c.GetComponentInChildren<ApartmentMirror>();mirror.RenderFrom(w.eyes);sb.AppendLine("Mirror live RT bound="+(mirror.mirrorSurface.sharedMaterial.GetTexture("_BaseMap")==mirror.texture));
        var receiver=c.GetComponent<LightCircuit>();BuildingPower.SetCircuit(receiver.circuit,false);var emergency=c.Find("Emergency Light").GetComponent<LightCircuit>();
        sb.AppendLine("Elevator circuit off: normal lights off="+receiver.lights.All(l=>!l.enabled)+" emergency on="+emergency.lights.Any(l=>l.enabled));
        var emission=new MaterialPropertyBlock();var tube=receiver.emitters.First(r=>r.sharedMaterials.Any(m=>m!=null&&m.name=="Weathered Fluorescent"));tube.GetPropertyBlock(emission,0);sb.AppendLine("Fixture emission switches off="+(emission.GetColor("_EmissionColor").maxColorComponent<.001f));
        BuildingPower.SetCircuit(receiver.circuit,true);sb.AppendLine("Normal lighting restored="+receiver.lights.All(l=>l.enabled));
        CaptureCabinReview("After");File.WriteAllText(CabinReview+"Runtime Checks.txt",sb.ToString());return sb.ToString();
    }
}
