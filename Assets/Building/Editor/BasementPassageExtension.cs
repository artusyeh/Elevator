using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string ExtendBasementServicePassage()
    {
        EnsureApartment();var b=GameObject.Find("Apartment").transform.Find("B1");if(b.Find("Extended Service Passage")!=null)return "Passage already extended";
        var wall=b.Find("South Foundation").GetComponent<MeshRenderer>();
        LiftCutRenderer(wall,b,new[]{new Bounds(new Vector3(0,1.3f,.3f),new Vector3(2.12f,2.65f,.4f))},"South Wall Return");
        var passage=G("Extended Service Passage",b);
        B("Passage Floor",passage,new Vector3(0,-.12f,-2.70f),new Vector3(2.12f,.24f,6.0f),"Basement Damp Concrete",true,.001f);
        B("Passage Ceiling",passage,new Vector3(0,2.69f,-2.70f),new Vector3(2.30f,.18f,6.0f),"Concrete",true,.001f);
        foreach(float x in new[]{-1.15f,1.15f})B("Passage Wall",passage,new Vector3(x,1.3f,-2.7f),new Vector3(.18f,2.60f,6.0f),"Concrete",true,.001f);
        B("Service End Wall",passage,new Vector3(0,1.3f,-5.79f),new Vector3(2.48f,2.60f,.18f),"Concrete",true,.001f);
        PipeRun(passage,new Vector3(.8f,2.25f,1),new Vector3(.8f,2.25f,-5.62f),.07f);
        PipeRun(passage,new Vector3(.8f,2.25f,-5.62f),new Vector3(.8f,.35f,-5.62f),.07f);
        for(float z=-.7f;z>-5.5f;z-=1.5f)Rod("Pipe Hanger",passage,new Vector3(.8f,2.3f,z),new Vector3(.8f,2.6f,z),.008f,"Stainless Steel");
        var circuit=AssetDatabase.LoadAssetAtPath<PowerCircuit>(BuildingBase+"Circuits/Basement.asset");
        BasementLight(passage,new Vector3(0,2.57f,-1.4f),circuit);BasementLight(passage,new Vector3(0,2.57f,-4.4f),circuit);
        Weather(passage,"Water Stain",new Vector3(0,1.1f,-5.695f),new Vector2(.9f,1.5f),Quaternion.identity);
        CommonDynamic(passage);SaveCommonModule(b,BuildingBase+"Prefabs/B1 Services.prefab");SaveApartment();return "Extended south service passage by 6m, with continuous floor, enclosed walls/ceiling, connected pipes and circuit-controlled lights.";
    }
}
