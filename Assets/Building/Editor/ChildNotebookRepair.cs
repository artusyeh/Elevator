using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    public static string ReplaceChildDrawingPad()
    {
        EnsureApartment();var room=GameObject.Find("Room 503").transform;var study=room.Find("Family Study Corner");
        var old=study.Find("Child Drawing Pad");if(old!=null)old.gameObject.SetActive(false);
        foreach(var pencil in study.Cast<Transform>().Where(t=>t.name=="Worn Pencil").ToArray())pencil.gameObject.SetActive(false);
        if(study.Find("Child Exercise Notebook")==null)
        {
            var template=study.Find("Worn Schoolbooks/Notebook");var notebook=UnityEngine.Object.Instantiate(template.gameObject,study).transform;notebook.name="Child Exercise Notebook";
            notebook.localScale=new Vector3(1.15f,.48f,1.15f);notebook.localRotation=Quaternion.Euler(0,-7,0);
            float top=study.Find("Scarred Desk Top").GetComponent<Renderer>().bounds.max.y-room.position.y;
            notebook.localPosition=new Vector3(-3.48f,top+.0008f,6.24f);
            var material=Plain("Used Pencil Lacquer",new Color(.49f,.33f,.12f),.16f);
            var tools=G("Desk Pencils",study);
            for(int i=0;i<2;i++)
            {
                float x=-3.29f-i*.025f;float y=top+.0034f;float z=6.19f+i*.025f;
                Rod("Wood Pencil",tools,new Vector3(x,y,z),new Vector3(x+.012f,y,z+.13f),.0027f,material.name);
                Rod("Exposed Pencil Wood",tools,new Vector3(x,y,z),new Vector3(x-.001f,y,z-.012f),.0018f,"Old Wood");
                Rod("Graphite Tip",tools,new Vector3(x-.001f,y,z-.012f),new Vector3(x-.0012f,y,z-.015f),.0007f,"Black Rubber");
            }
            CommonDynamic(notebook);CommonDynamic(tools);
        }
        SaveCommonModule(room,BuildingBase+"Prefabs/Interiors/Room 503.prefab");SaveApartment();return "Replaced blank drawing slab and three dark pencil bars with a bound worn school notebook and two separate wooden pencils.";
    }
}
