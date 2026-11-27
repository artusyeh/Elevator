using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string FitBasementPanelRows()
    {
        EnsureApartment();var apt=GameObject.Find("Apartment").transform;
        foreach(var panel in new[]{apt.Find("B1/Main Electrical Room/Distribution Panel"),apt.Find("Ground Floor/Electrical Room/Secondary Distribution Panel")})
        {
            var controls=panel.GetComponentsInChildren<PowerSwitch>(true);int index=0;
            foreach(var control in controls.OrderBy(s=>s.mainBreaker?-1:0))
            {
                float x,y;if(control.mainBreaker){x=-.42f;y=2.0f;}else{int col=controls.Length>8?index/8:0,row=controls.Length>8?index%8:index;x=-.42f+col*.64f;y=1.77f-row*(controls.Length>8?.15f:.27f);index++;}
                control.transform.localPosition=new Vector3(x,y,-.16f);
                var label=control.transform.Find("Label");if(label==null)continue;
                label.localPosition=new Vector3(.23f,0,.0194f);label.localRotation=Quaternion.identity;label.localScale=new Vector3(.25f,.055f,1);
                string name=control.circuit==null?"":control.circuit.id=="Basement"?"B1 SERVICES":control.circuit.id=="Security Room"?"SECURITY OFFICE":"";
                if(name!="")
                {
                    string path=Base+"Materials/"+name+" Breaker Paper.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(mat==null){mat=new Material(M("Maintenance Aged Paper"));AssetDatabase.CreateAsset(mat,path);}
                    mat.SetTexture("_BaseMap",DetailMap(BuildingBase+"Textures/Key Tags/"+name+".png"));mat.DisableKeyword("_EMISSION");mat.SetColor("_EmissionColor",Color.black);label.GetComponent<Renderer>().sharedMaterial=mat;EditorUtility.SetDirty(mat);
                }
            }
        }
        SaveBasementUpdates();return "Both panels aligned; B1 and Security Office labels match the existing aged paper.";
    }
}
