using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string RebuildPumpDrain()
    {
        EnsureApartment();var b=GameObject.Find("Apartment").transform.Find("B1");var pump=b.Find("Water Pump Room");
        if(pump.Find("Recessed Service Drain")!=null)return "Drain already rebuilt";
        foreach(var old in pump.Cast<Transform>().Where(t=>t.name=="Drainage Grate"||t.name=="Drainage Channel").ToArray())old.gameObject.SetActive(false);
        var cut=new Bounds(new Vector3(1.48f,-.04f,6),new Vector3(.32f,.12f,4.30f));
        LiftCutRenderer(b.Find("Basement Slab").GetComponent<MeshRenderer>(),b,new[]{cut},"Floor Around Drain");
        var drain=G("Recessed Service Drain",pump);
        B("Recessed Channel Bed",drain,new Vector3(1.48f,-.086f,6),new Vector3(.30f,.025f,4.28f),"Black Rubber",false,.001f);
        var pieces=new List<CombineInstance>();
        System.Action<Vector3,Vector3> box=(at,size)=>pieces.Add(new CombineInstance{mesh=Rounded(size,.0015f),transform=Matrix4x4.TRS(at,Quaternion.identity,Vector3.one)});
        foreach(float x in new[]{-.145f,.145f})box(new Vector3(x,-.008f,0),new Vector3(.014f,.020f,1.04f));
        foreach(float z in new[]{-.513f,.513f})box(new Vector3(0,-.008f,z),new Vector3(.28f,.020f,.014f));
        for(int i=0;i<27;i++)box(new Vector3(0,-.006f,-.486f+i*.0374f),new Vector3(.277f,.016f,.018f));
        var mesh=new Mesh{name="Cast Iron Service Grating"};mesh.CombineMeshes(pieces.ToArray(),true,true);mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,BuildingBase+"Models/Cast Iron Service Grating.asset");
        for(int i=0;i<4;i++)
        {
            var module=G("Removable Grate "+(i+1),drain);module.localPosition=new Vector3(1.48f,0,4.425f+i*1.05f);
            module.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;module.gameObject.AddComponent<MeshRenderer>().sharedMaterial=M("Cabin Aged Steel");
            var collider=module.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(.304f,.02f,1.04f);collider.center=new Vector3(0,-.008f,0);
        }
        CommonDynamic(drain);SaveBasementUpdates();return "Rebuilt drain with a real recessed floor opening, dark channel bed and four framed cast-iron grates with narrow slots. Walking surface stays flush.";
    }
}
