using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class ApartmentBuilder
{
    public static string PolishGuardUtilityRoom()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;var utility=apt.Find("Ground Floor/Electrical Room");
        var locker=utility.Find("Guard Locker");locker.localPosition=new Vector3(-5.70f,0,7.50f);locker.localRotation=Quaternion.Euler(0,-90,0);
        var warning=utility.Find("Meter Cabinet/Electrical Warning");warning.position=new Vector3(-5.899f,2.10f,7.50f);warning.rotation=Quaternion.Euler(0,-90,0);
        var panel=utility.Find("Secondary Distribution Panel");
        foreach(var breaker in panel.GetComponentsInChildren<PowerSwitch>(true).Where(s=>s.circuit!=null&&s.circuit.id=="Lobby").ToArray())UnityEngine.Object.DestroyImmediate(breaker.gameObject);
        var ordered=panel.GetComponentsInChildren<PowerSwitch>().OrderBy(s=>s.circuit.id=="Security Room"?0:s.circuit.id=="CCTV Cameras"?1:s.circuit.id=="CCTV Monitors"?2:3).ToArray();
        for(int i=0;i<ordered.Length;i++)
        {
            var t=ordered[i].transform;t.localPosition=new Vector3(-.40f,1.77f-i*.27f,-.16f);
            var label=t.Find("Label");if(label!=null){label.localPosition=new Vector3(.28f,0,.0194f);label.localRotation=Quaternion.identity;label.localScale=new Vector3(.30f,.065f,1);}
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);
        }
        var security=AssetDatabase.LoadAssetAtPath<PowerCircuit>(BuildingBase+"Circuits/Security Room.asset");security.label="Security Office";EditorUtility.SetDirty(security);
        var mat=Plain("Security Office Physical Nameplate",Color.white,.12f);mat.SetTexture("_BaseMap",DetailMap(BuildingBase+"Textures/Service Nameplates/SECURITY OFFICE.png"));
        mat.DisableKeyword("_EMISSION");mat.SetColor("_EmissionColor",Color.black);
        var room=apt.Find("Ground Floor/Management Room");room.Find("Management Door").GetComponentsInChildren<Transform>().First(t=>t.name=="Printed Nameplate").GetComponent<Renderer>().sharedMaterial=mat;
        var key=utility.GetComponentsInChildren<KeyPickup>().First(k=>k.keyId=="Management");key.label="Security Office key";key.gameObject.name="Security Office Key";
        foreach(var sw in apt.GetComponentsInChildren<PowerSwitch>(true).Where(s=>s.circuit==security))
        {var label=sw.transform.Find("Label");if(label!=null)label.GetComponent<Renderer>().sharedMaterial=mat;}
        SaveCommonModule(room.Find("Management Door"),BuildingBase+"Prefabs/Management Door.prefab");SaveCommonModule(key.transform,BuildingBase+"Prefabs/Management Room Key.prefab");
        SaveBasementUpdates();return "Security Office signs/key updated; locker backed against west wall, warning above; four aligned local controls; lobby control remains on B1 main panel.";
    }
}
