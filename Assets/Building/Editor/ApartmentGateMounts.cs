using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    const string GateMountReview="References/Apartment Gate Mounts/";
    static void FitApartmentGateMount(Transform door)
    {
        // Set the entrance against the actual corridor wall, instead of leaving
        // both its timber jamb and security grille projecting into the passage.
        var position=door.localPosition;position.x=Mathf.Sign(position.x)*1.94f;door.localPosition=position;
        var gate=door.Find("Security Gate");gate.localPosition=new Vector3(0,0,-.095f);gate.localScale=new Vector3(.88f,1,1);
        var guides=CommonGroup(gate,"Gate Guides");
        foreach(var stile in gate.Cast<Transform>().Where(t=>t.name=="Gate Stile").ToArray())
        {
            float x=stile.localPosition.x;
            B("Upper Guide Shoe",guides,new Vector3(x,2.138f,0),new Vector3(.030f,.026f,.032f),"Cabin Aged Steel",false,.001f);
            B("Lower Guide Shoe",guides,new Vector3(x,.040f,0),new Vector3(.030f,.022f,.032f),"Cabin Aged Steel",false,.001f);
        }
        var old=door.Find("Gate Mounting Frame");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var frame=G("Gate Mounting Frame",door);
        // End faces meet the existing jamb and gate posts; they do not overlap
        // as coplanar cover plates, which would reintroduce flickering edges.
        foreach(float side in new[]{-1f,1f})
        {
            B("Jamb Return",frame,new Vector3(side*.47f,1.08f,-.0225f),new Vector3(.065f,2.16f,.085f),"Green Steel",true,.001f);
            foreach(float y in new[]{.22f,1.08f,1.98f})
                Rod("Frame Fixing",frame,new Vector3(side*.491f,y,-.066f),new Vector3(side*.491f,y,-.069f),.0035f,"Cabin Aged Steel");
        }
        B("Header Return",frame,new Vector3(0,2.18f,-.0175f),new Vector3(1f,.08f,.075f),"Green Steel",true,.001f);
        // A small continuous sill ties the two uprights to the doorway floor.
        B("Rebated Threshold",frame,new Vector3(0,.0045f,-.016f),new Vector3(.87f,.009f,.11f),"Cabin Aged Steel",false,.001f);
        B("Bottom Guide Channel",frame,new Vector3(0,.015f,-.105f),new Vector3(.91f,.030f,.080f),"Green Steel",false,.001f);
        var bell=door.Find("Doorbell");if(bell!=null){var p=bell.localPosition;p.z=.0415f;bell.localPosition=p;EditorUtility.SetDirty(bell);}
        CommonDynamic(frame);CommonDynamic(guides);EditorUtility.SetDirty(door);EditorUtility.SetDirty(gate);
    }
    public static string RepairApartmentGateMounts()
    {
        EnsureApartment();Directory.CreateDirectory(GateMountReview);
        if(!File.Exists(GateMountReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",GateMountReview+"Apartment.before.unity");
        var root=GameObject.Find("Apartment").transform;
        var doors=root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Apartment Door"&&t.parent.name=="Residential Corridor").ToArray();
        foreach(var door in doors)FitApartmentGateMount(door);
        string path=Base+"Prefabs/Residential Corridor.prefab";var prefab=PrefabUtility.LoadPrefabContents(path);int source=0;
        try{foreach(var door in prefab.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Apartment Door").ToArray()){FitApartmentGateMount(door);source++;}PrefabUtility.SaveAsPrefabAsset(prefab,path);}
        finally{PrefabUtility.UnloadPrefabContents(prefab);}
        SaveApartment();return "Fitted "+doors.Length+" apartment gates and "+source+" source-prefab entrances to their doorframes, with continuous returns and clear hardware spacing.";
    }
    public static string CaptureApartmentGateMounts()
    {
        var mood=UnityEngine.Object.FindFirstObjectByType<LightingMoodController>();var prior=mood.CurrentMood;mood.SetHorror(true);
        try
        {
            var names=new[]{"Side mounting","Complete entrance","Opposite side","Upper floor"};
            var at=new[]{new Vector3(-1.42f,4.63f,3.15f),new Vector3(.05f,4.63f,2.15f),new Vector3(1.42f,4.63f,3.15f),new Vector3(-.1f,16.23f,8.2f)};
            var target=new[]{new Vector3(-1.85f,4.18f,1.90f),new Vector3(-1.90f,4.05f,1.90f),new Vector3(1.85f,4.18f,1.90f),new Vector3(1.94f,15.7f,8.2f)};
            for(int i=0;i<names.Length;i++){string name="Mounted gate "+names[i];Capture(name,at[i],target[i]);File.Copy("References/Apartment/Captures/"+name+".png",GateMountReview+names[i]+".png",true);}
        }
        finally{mood.SetMood(prior);}
        return "Captured mounted gates at player height from both corridor sides and the upper floor.";
    }
}
