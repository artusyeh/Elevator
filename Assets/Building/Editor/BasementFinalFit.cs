using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string SealBasementAndMatchButton()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;var b=apt.Find("B1");
        if(b.Find("Stairwell Outer Backing")==null)
        {
            var shell=G("Stairwell Outer Backing",b);
            B("Continuous West Backing",shell,new Vector3(-5.55f,2.4f,11.2f),new Vector3(.10f,5.4f,5.90f),"Concrete");
            B("Continuous Rear Backing",shell,new Vector3(-3.8f,2.4f,14.12f),new Vector3(3.60f,5.4f,.10f),"Concrete");
            B("Continuous Front Backing",shell,new Vector3(-3.8f,2.4f,8.24f),new Vector3(3.60f,5.4f,.10f),"Concrete");
            B("East Front Backing",shell,new Vector3(-2.07f,2.4f,8.66f),new Vector3(.10f,5.4f,.74f),"Concrete");
            B("East Rear Backing",shell,new Vector3(-2.07f,2.4f,12.13f),new Vector3(.10f,5.4f,4.08f),"Concrete");
            B("Door Head Backing",shell,new Vector3(-2.07f,2.55f,9.56f),new Vector3(.10f,.70f,1.06f),"Concrete");CommonDynamic(shell);
        }
        var stairDoor=b.Find("Stairwell").GetComponentInChildren<StairAccessDoor>(true);
        var plaque=stairDoor.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Stairwell Plaque"&&t.GetComponent<MeshFilter>()!=null);
        string signPath=Base+"Materials/Basement Door Plaque.mat";var sign=AssetDatabase.LoadAssetAtPath<Material>(signPath);
        if(sign==null){sign=new Material(plaque.GetComponent<Renderer>().sharedMaterial);AssetDatabase.CreateAsset(sign,signPath);}sign.SetTexture("_BaseMap",DetailMap(BuildingBase+"Textures/Service Nameplates/BASEMENT.png"));plaque.GetComponent<Renderer>().sharedMaterial=sign;EditorUtility.SetDirty(sign);
        var assembly=plaque.parent;
        if(stairDoor.leaf.Find("Interior Basement Plaque")==null){var inside=UnityEngine.Object.Instantiate(assembly.gameObject,stairDoor.leaf).transform;inside.name="Interior Basement Plaque";var p=assembly.localPosition;p.z=-p.z;inside.localPosition=p;inside.localRotation=Quaternion.Euler(0,180,0);}
        var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();var panel=lift.cabin.Find("Control Panel");
        if(panel.Find("B1 Button")!=null)UnityEngine.Object.DestroyImmediate(panel.Find("B1 Button").gameObject);
        var source=lift.cabin.GetComponentsInChildren<ElevatorButton>().First(e=>e.action==ElevatorButton.Action.Floor&&e.floor==1);
        if(!lift.cabin.GetComponentsInChildren<ElevatorButton>().Any(e=>e.action==ElevatorButton.Action.Floor&&e.floor==0))
        {
            var cap=UnityEngine.Object.Instantiate(source.cap.gameObject,panel).transform;cap.name="B1 Floor Button";var p=source.cap.localPosition;p.y=.60f;cap.localPosition=p;
            foreach(var text in cap.GetComponentsInChildren<TextMesh>(true))UnityEngine.Object.DestroyImmediate(text.gameObject);
            var marking=cap.GetComponentsInChildren<Transform>().First(t=>t.name=="Button Marking");var label=new Material(marking.GetComponent<Renderer>().sharedMaterial);label.SetTexture("_BaseMap",DetailMap(BuildingBase+"Textures/Elevator/Labels/Floor B1.png"));AssetDatabase.CreateAsset(label,Base+"Materials/Cabin Floor B1.mat");marking.GetComponent<Renderer>().sharedMaterial=label;
            var detailRoot=source.lamp.transform.parent;Renderer lamp=null;
            foreach(var part in detailRoot.Cast<Transform>().Where(t=>(t.name=="Button Bezel"||t.name=="Button Indicator")&&Mathf.Abs(t.localPosition.y-source.cap.localPosition.y)<.01f).ToArray())
            {var copy=UnityEngine.Object.Instantiate(part.gameObject,detailRoot).transform;copy.name="B1 "+part.name;var at=copy.localPosition;at.y=.60f;copy.localPosition=at;if(part.name=="Button Indicator")lamp=copy.GetComponent<Renderer>();}
            var actor=UnityEngine.Object.Instantiate(source.gameObject,panel).transform;actor.name="B1 Button Interaction";var loc=actor.localPosition;loc.y=.60f;actor.localPosition=loc;var press=actor.GetComponent<ElevatorButton>();press.floor=0;press.cap=cap;press.lamp=lamp;
        }
        SaveBasementUpdates();return "Continuous outer stair backing fitted; BASEMENT plaques on both sides; B1 button cloned from the actual floor-button assembly.";
    }
}
