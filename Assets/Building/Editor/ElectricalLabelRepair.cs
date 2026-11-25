using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string ElectricalLabelFolder="Assets/Building/Textures/Electrical Labels/";
    const string ElectricalLabelReview="References/Electrical Labels/";
    static Material ElectricalPaper(string name,string materialName=null)
    {
        var map=DetailMap(ElectricalLabelFolder+name+" Paper.png");
        var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(map));importer.mipmapEnabled=true;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Trilinear;importer.SaveAndReimport();
        var material=Plain(materialName??name+" Aged Paper",Color.white,.055f,0);material.SetTexture("_BaseMap",map);material.SetTexture("_BumpMap",DetailMap(ElectricalLabelFolder+"Paper Normal.png",true));material.SetFloat("_BumpScale",.12f);material.EnableKeyword("_NORMALMAP");material.SetFloat("_AlphaClip",1);material.SetFloat("_Cutoff",.5f);material.EnableKeyword("_ALPHATEST_ON");material.SetOverrideTag("RenderType","TransparentCutout");material.renderQueue=2450;material.SetFloat("_SpecularHighlights",0);material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");EditorUtility.SetDirty(material);return material;
    }
    static int FitElectricalLabels(Transform room)
    {
        int count=0;var panel=room.name=="Distribution Panel"?room:room.Find("Distribution Panel");
        if(panel!=null)
        {
            var face=panel.Find("Recessed Face").GetComponent<Renderer>();
            float front=panel.InverseTransformPoint(face.bounds.min).z;
            foreach(var control in panel.GetComponentsInChildren<PowerSwitch>(true))
            {
                var label=control.transform.Find("Label");if(label==null)continue;
                string name=control.mainBreaker?"Main Supply":control.circuit.label;
                var at=panel.InverseTransformPoint(label.position);at.z=front-.0006f;label.position=panel.TransformPoint(at);label.rotation=panel.rotation;
                var renderer=label.GetComponent<Renderer>();renderer.sharedMaterial=M(name+" Aged Paper");renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.lightmapIndex=-1;count++;
            }
        }
        if(room.name=="Distribution Panel")return count;
        var meters=room.Find("Meter Cabinet");var warning=meters==null?null:meters.Find("Electrical Warning");
        if(warning!=null)
        {
            // The west wall's inside face is the actual paper support, not the meter enclosure.
            var wall=room.Cast<Transform>().First(t=>t.name=="Plaster Wall"&&t.GetComponent<Renderer>().bounds.size.x<.3f);
            var p=warning.position;p.x=wall.GetComponent<Renderer>().bounds.max.x+.0006f;warning.position=p;warning.rotation=room.rotation*Quaternion.Euler(0,-90,0);
            warning.GetComponent<Renderer>().sharedMaterial=M("Electrical Warning Aged Paper");warning.GetComponent<Renderer>().lightmapIndex=-1;
            var backing=meters.Find("Warning Backing");if(backing!=null)UnityEngine.Object.DestroyImmediate(backing.gameObject);count++;
        }
        var notes=room.Find("Caretaker Notes");if(notes!=null)
        {
            var wall=room.Cast<Transform>().First(t=>t.name=="Plaster Wall"&&t.GetComponent<Renderer>().bounds.size.x>1);
            var p=notes.position;p.z=wall.GetComponent<Renderer>().bounds.min.z-.0006f;notes.position=p;notes.rotation=room.rotation;notes.GetComponent<Renderer>().sharedMaterial=M("Caretaker Notes Aged Paper");notes.GetComponent<Renderer>().lightmapIndex=-1;count++;
        }
        var door=room.Find("Electrical Room Door/Door Pivot/Stairwell Plaque/Stairwell Plaque");
        if(door!=null){door.GetComponent<Renderer>().sharedMaterial=M("Electrical Room Aged Paper");count++;}
        return count;
    }
    public static string CaptureElectricalLabels(string suffix)
    {
        foreach(string name in new[]{"Electrical Panel","Mounted Labels"})
        {
            var at=name=="Electrical Panel"?new Vector3(-4.30f,1.41f,6.88f):new Vector3(-4.69f,1.64f,7.08f);
            var target=name=="Electrical Panel"?new Vector3(-4.34f,1.40f,7.83f):new Vector3(-5.20f,1.51f,7.85f);
            var camera=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>().eyes;float fov=camera.fieldOfView;
            try{if(name=="Electrical Panel")camera.fieldOfView=80;Capture(name+" "+suffix,at,target);}finally{camera.fieldOfView=fov;}
            File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png",ElectricalLabelReview+name+" "+suffix+".png",true);
        }
        return "Electrical room screenshots captured";
    }
    public static string RepairElectricalLabels()
    {
        EnsureApartment();Directory.CreateDirectory(ElectricalLabelReview);if(!File.Exists(ElectricalLabelReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",ElectricalLabelReview+"Apartment.before.unity");
        if(!File.Exists(ElectricalLabelReview+"Electrical Panel Before.png"))CaptureElectricalLabels("Before");
        foreach(string name in new[]{"Main Supply","Lobby","Security Room","Second Floor","Third Floor","Fourth Floor","Fifth Floor","Sixth Floor","Stairwell","Elevator","CCTV Cameras","CCTV Monitors","Entrance","Emergency","Maintenance","Electrical Warning","Caretaker Notes","Electrical Room"})ElectricalPaper(name);
        var directory=AssetDatabase.LoadAssetAtPath<BuildingDirectory>(BuildingBase+"Apartments.asset");foreach(var unit in directory.units)ElectricalPaper("Mailbox "+unit.number,"Mailbox "+unit.number+" Insert");
        var room=GameObject.Find("Ground Floor").transform.Find("Electrical Room");int count=FitElectricalLabels(room);
        foreach(string path in new[]{BuildingBase+"Prefabs/Electrical Room.prefab",BuildingBase+"Prefabs/Distribution Panel.prefab",Base+"Prefabs/Entrance Interior.prefab"})
        {
            var prefab=PrefabUtility.LoadPrefabContents(path);try{var target=path.EndsWith("Entrance Interior.prefab")?prefab.transform.Find("Electrical Room"):prefab.transform;if(target!=null)FitElectricalLabels(target);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        }
        SaveApartment();CaptureElectricalLabels("After");return count+" electrical-room labels updated; paper mounted 0.6 mm from supporting surfaces; scene and shared prefabs saved.";
    }
}
