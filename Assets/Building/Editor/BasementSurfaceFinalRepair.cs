using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class ApartmentBuilder
{
    public static string FitDrainCeilingAndButtonBacks()
    {
        EnsureApartment();var b=GameObject.Find("Apartment").transform.Find("B1");var drain=b.Find("Water Pump Room/Recessed Service Drain");
        var cut=new Bounds(new Vector3(1.48f,-.26f,6),new Vector3(.32f,.60f,4.30f));
        foreach(var r in b.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&r.name=="Floor Around Drain").ToArray())LiftCutRenderer(r,b,new[]{cut},"Drain Floor Edge");
        var dark=Plain("Drain Interior Darkness",new Color(.009f,.011f,.009f),0);dark.SetFloat("_SpecularHighlights",0);dark.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        var bed=drain.Find("Recessed Channel Bed");bed.localPosition=new Vector3(1.48f,-.43f,6);bed.GetComponent<Renderer>().sharedMaterial=dark;
        if(drain.Find("Channel Side")==null)
        {
            foreach(float x in new[]{1.326f,1.634f})B("Channel Side",drain,new Vector3(x,-.23f,6),new Vector3(.012f,.43f,4.30f),"Drain Interior Darkness",false,.001f);
            foreach(float z in new[]{3.855f,8.145f})B("Channel End",drain,new Vector3(1.48f,-.23f,z),new Vector3(.32f,.43f,.012f),"Drain Interior Darkness",false,.001f);
        }
        var soffit=b.Find("Concrete Soffit").GetComponent<MeshRenderer>();var closure=b.Find("Water Pump Room/Pump Ceiling Closure");
        if(closure!=null)
        {
            var a=LiftLocalBounds(soffit,b);var c=LiftLocalBounds(closure.GetComponent<Renderer>(),b);float end=c.max.z,start=a.max.z;
            LiftResize(closure,new Vector3(c.center.x,c.center.y,(start+end)*.5f),new Vector3(c.size.x,c.size.y,end-start));
        }
        var joint=b.Find("Stairwell Enclosure/Ground Junction Front").GetComponent<MeshRenderer>();
        if(joint.enabled)LiftCutRenderer(joint,b,new[]{LiftLocalBounds(soffit,b)},"Nonoverlapping Header Return");
        var panel=UnityEngine.Object.FindFirstObjectByType<ElevatorController>().cabin.Find("Control Panel");
        // One continuous metal backing reaches the existing button bezels without moving their faces or hit targets.
        if(panel.Find("Button Mounting Backplate")==null)
        B("Button Mounting Backplate",panel,new Vector3(0,.94f,-.009f),new Vector3(.20f,1.22f,.060f),"Cabin Aged Steel",false,.002f);
        SaveBasementUpdates();return "Deepened drain with dark channel sides; removed ceiling overlap; added continuous physical button mounting backplate.";
    }
}
