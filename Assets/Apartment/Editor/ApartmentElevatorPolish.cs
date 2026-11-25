using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    public static string PolishElevatorEntrance()
    {
        EnsureApartment();
        var paint=Plain("Elevator Door Paint",new Color(.35f,.41f,.36f),.32f,.18f);
        paint.SetTexture("_BaseMap",M("Pale Green Panels").GetTexture("_BaseMap"));paint.SetTextureScale("_BaseMap",Vector2.one*.7f);
        paint.SetTexture("_BumpMap",M("Green Steel").GetTexture("_BumpMap"));paint.SetFloat("_BumpScale",.12f);paint.EnableKeyword("_NORMALMAP");
        Plain("Elevator Frame Metal",new Color(.34f,.36f,.32f),.39f,.65f);
        var lens=Plain("Elevator Button Lens",new Color(.48f,.41f,.24f),.35f,.05f);lens.EnableKeyword("_EMISSION");lens.SetColor("_EmissionColor",new Color(.12f,.074f,.018f));
        var diffuser=Plain("Cabin Diffuser",new Color(.58f,.58f,.45f),.22f);diffuser.EnableKeyword("_EMISSION");diffuser.SetColor("_EmissionColor",new Color(.8f,.76f,.58f));
        int count=0;
        foreach(var landing in GameObject.Find("Apartment").GetComponentsInChildren<Transform>().Where(t=>t.name=="Elevator Landing").ToArray())
        {
            var old=landing.Find("Entrance Details");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);var details=G("Entrance Details",landing);
            foreach(var r in landing.Cast<Transform>().Where(t=>new[]{"Door Jamb","Door Header","Threshold","Call Panel"}.Contains(t.name)).Select(t=>t.GetComponent<Renderer>()))r.sharedMaterial=M("Elevator Frame Metal");
            foreach(var rust in landing.Cast<Transform>().Where(t=>t.name=="Rust").ToArray())UnityEngine.Object.DestroyImmediate(rust.gameObject);
            foreach(string name in new[]{"Left Elevator Door","Right Elevator Door"})
            {
                var leaf=landing.Find(name);var c=leaf.GetComponent<BoxCollider>();c.size=new Vector3(.546f,2.12f,.05f);leaf.GetComponent<MeshFilter>().sharedMesh=Rounded(c.size,.002f);leaf.GetComponent<Renderer>().sharedMaterial=paint;
                var trim=leaf.Find("Door Details");if(trim!=null)UnityEngine.Object.DestroyImmediate(trim.gameObject);trim=G("Door Details",leaf);
                float edge=name.StartsWith("Left")?.269f:-.269f;
                B("Meeting Edge",trim,new Vector3(edge,0,-.0265f),new Vector3(.006f,2.11f,.006f),"Elevator Frame Metal",false,.001f);
                B("Lower Wear Strip",trim,new Vector3(0,-.93f,-.027f),new Vector3(.52f,.12f,.003f),"Elevator Frame Metal",false,.001f);
                Weather(trim,"Discoloration",new Vector3(-edge*.4f,-.70f,-.029f),new Vector2(.22f,.28f),Quaternion.identity);
            }
            foreach(float x in new[]{-.568f,.568f})
            {
                B("Door Reveal",details,new Vector3(x,1.075f,.016f),new Vector3(.018f,2.15f,.13f),"Elevator Frame Metal",false,.002f);
                B("Pocket Seal",details,new Vector3(x*1.025f,1.075f,.05f),new Vector3(.012f,2.15f,.02f),"Black Rubber",false,.001f);
            }
            B("Lintel Return",details,new Vector3(0,2.16f,.032f),new Vector3(1.14f,.055f,.16f),"Elevator Frame Metal",false,.002f);
            foreach(float x in new[]{-.915f,.915f})B("Door Pocket Cover",details,new Vector3(x,1.08f,-.046f),new Vector3(.55f,2.16f,.036f),"Elevator Door Paint",false,.003f);
            B("Entrance Light Housing",details,new Vector3(0,2.37f,-.19f),new Vector3(.45f,.075f,.20f),"Elevator Frame Metal",false,.005f);
            B("Entrance Light Cover",details,new Vector3(0,2.325f,-.19f),new Vector3(.36f,.025f,.14f),"Cabin Diffuser",false,.006f);
            Lamp(details,new Vector3(0,2.23f,-.31f),0.65f,2.7f,4200,Quaternion.identity);
            foreach(float z in new[]{-.064f,-.032f,.001f,.034f,.065f})B("Sill Track",details,new Vector3(0,.017f,z),new Vector3(1.1f,.002f,.006f),"Black Rubber",false,.0003f);
            foreach(float x in new[]{-.5f,.5f})foreach(float z in new[]{-.068f,.066f})Rod("Sill Screw",details,new Vector3(x,.016f,z),new Vector3(x,.019f,z),.004f,"Elevator Frame Metal");
            foreach(var text in landing.Cast<Transform>().Where(t=>t.GetComponent<TextMesh>()!=null&&t.GetComponent<TextMesh>().text.Contains("▲")).ToArray())UnityEngine.Object.DestroyImmediate(text.gameObject);
            var buttons=G("Call Buttons",details);buttons.localPosition=new Vector3(.94f,1.19f,-.148f);
            foreach(float y in new[]{-.065f,.065f})
            {
                Rod("Button Bezel",buttons,new Vector3(0,y,0),new Vector3(0,y,-.012f),.033f,"Elevator Frame Metal");
                Rod("Button Lens",buttons,new Vector3(0,y,-.012f),new Vector3(0,y,-.016f),.026f,"Elevator Button Lens");
                var m=new Mesh();m.vertices=new[]{new Vector3(-.011f,y+(y>0?-.006f:.006f),-.017f),new Vector3(.011f,y+(y>0?-.006f:.006f),-.017f),new Vector3(0,y+(y>0?.010f:-.010f),-.017f)};m.triangles=y>0?new[]{0,2,1}:new[]{0,1,2};m.RecalculateNormals();m.RecalculateBounds();
                var arrow=G("Button Arrow",buttons);arrow.gameObject.AddComponent<MeshFilter>().sharedMesh=m;arrow.gameObject.AddComponent<MeshRenderer>().sharedMaterial=M("Black Rubber");
                string path=Base+"Models/"+(y>0?"Call Up Arrow":"Call Down Arrow")+".asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(prior==null)AssetDatabase.CreateAsset(m,path);else{UnityEngine.Object.DestroyImmediate(m);arrow.GetComponent<MeshFilter>().sharedMesh=prior;}
            }
            foreach(float y in new[]{1.065f,1.315f})Rod("Panel Screw",details,new Vector3(.94f,y,-.144f),new Vector3(.94f,y,-.15f),.003f,"Elevator Frame Metal");
            count++;
        }
        var cabin=GameObject.Find("Elevator Cabin").transform;
        var fixture=cabin.Find("Fluorescent Light");foreach(var l in fixture.GetComponentsInChildren<Light>()){l.colorTemperature=4100;l.range=3.5f;l.bounceIntensity=2;l.lightmapBakeType=LightmapBakeType.Mixed;if(l.type==LightType.Spot){l.intensity=4.1f;l.spotAngle=145;l.innerSpotAngle=110;}else l.intensity=.45f;}
        var oldCabin=cabin.Find("Cabin Details");if(oldCabin!=null)UnityEngine.Object.DestroyImmediate(oldCabin.gameObject);var cdetails=G("Cabin Details",cabin);
        B("Light Housing",cdetails,new Vector3(0,2.245f,.70f),new Vector3(.38f,.035f,.78f),"Elevator Frame Metal",false,.01f);
        B("Frosted Light Cover",cdetails,new Vector3(0,2.218f,.70f),new Vector3(.31f,.028f,.68f),"Cabin Diffuser",false,.012f);
        foreach(var r in fixture.GetComponentsInChildren<MeshRenderer>())r.enabled=false;
        Lamp(cdetails,new Vector3(0,1.9f,.95f),.75f,2.7f,4100,Quaternion.identity);cdetails.GetComponentInChildren<Light>().lightmapBakeType=LightmapBakeType.Realtime;
        foreach(float side in new[]{-1f,1f})B("Cabin Skirting",cdetails,new Vector3(side*.738f,.07f,.85f),new Vector3(.015f,.13f,1.45f),"Elevator Frame Metal",false,.002f);
        B("Rear Skirting",cdetails,new Vector3(0,.07f,1.592f),new Vector3(1.46f,.13f,.015f),"Elevator Frame Metal",false,.002f);
        var floor=cabin.Find("Cabin Floor").GetComponent<Renderer>();var fm=Plain("Cabin Worn Floor",new Color(.10f,.13f,.105f),.27f,.03f);fm.SetTexture("_BumpMap",M("Green Steel").GetTexture("_BumpMap"));fm.SetFloat("_BumpScale",.08f);fm.EnableKeyword("_NORMALMAP");floor.sharedMaterial=fm;
        SaveApartment();return "Refined "+count+" elevator entrances, visible sliding seams, sill tracks, physical call buttons and softly lit cabin.";
    }
    public static string ElevatorRevisionViews()
    {
        EnsureApartment();var control=UnityEngine.Object.FindFirstObjectByType<SlidingDoors>();var a=control.left.localPosition;var b=control.right.localPosition;
        try{
            control.left.localPosition=new Vector3(-control.openOffset,a.y,a.z);control.right.localPosition=new Vector3(control.openOffset,b.y,b.z);
            Capture("Elevator Open",new Vector3(.8f,1.65f,10.55f),new Vector3(4.2f,1.3f,10.8f));Capture("Elevator Interior",new Vector3(3.55f,1.65f,10.95f),new Vector3(4.95f,1.3f,10.45f));
            control.left.localPosition=new Vector3(-control.closedOffset,a.y,a.z);control.right.localPosition=new Vector3(control.closedOffset,b.y,b.z);
            Capture("Elevator Closed",new Vector3(.8f,1.65f,10.55f),new Vector3(3.25f,1.3f,10.8f));
        }finally{control.left.localPosition=a;control.right.localPosition=b;}
        foreach(string name in new[]{"Elevator Open","Elevator Interior","Elevator Closed"})System.IO.File.Copy("References/Apartment/Captures/"+name+".png","References/Apartment/Elevator Revision/"+name+".png",true);SaveApartment();return "Captured open, closed and interior views.";
    }
}

