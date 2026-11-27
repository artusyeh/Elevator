using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string SealPumpRoomCeiling()
    {
        EnsureApartment();var b=GameObject.Find("Apartment").transform.Find("B1");var ceiling=b.Find("Concrete Soffit").GetComponent<Renderer>().bounds;
        float start=b.InverseTransformPoint(new Vector3(0,0,ceiling.max.z)).z;
        float end=8.89f;
        if(b.Find("Water Pump Room/Pump Ceiling Closure")==null)
        {
            var part=B("Pump Ceiling Closure",b.Find("Water Pump Room"),new Vector3(3.15f,2.69f,(start+end)*.5f),new Vector3(4.10f,.18f,end-start),"Concrete",true,.001f);
            CommonDynamic(part);
        }
        SaveCommonModule(b,BuildingBase+"Prefabs/B1 Services.prefab");SaveApartment();return "Closed rear pump-room ceiling strip, flush with the existing soffit.";
    }
}
