using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static readonly string[] AccessibleUnits={"201","306","404","503","602"};
    static void FunctionalKeyBoard(Transform board)
    {
        if(board.Find("Key Hooks")!=null)return;
        var important=new System.Collections.Generic.Dictionary<int,string>{{1,"503"},{6,"201"},{9,"602"},{12,"306"},{18,"404"},{15,"Stairwell"}};
        var empty=new System.Collections.Generic.HashSet<int>{0,5,10,16,20};
        var hanging=board.Find("Hanging Keys");var parts=hanging.Cast<Transform>().ToArray();
        var hooks=board.Cast<Transform>().Where(t=>t.name=="Key Hook").OrderByDescending(t=>t.localPosition.y).ThenBy(t=>t.localPosition.x).ToArray();var positions=hooks.Select(t=>t.localPosition).ToArray();
        var owners=parts.ToDictionary(part=>part,part=>Enumerable.Range(0,positions.Length).OrderBy(j=>Vector2.Distance(new Vector2(part.localPosition.x,part.localPosition.y),new Vector2(positions[j].x,positions[j].y))).First());
        var tags=board.Cast<Transform>().Where(t=>t.name=="Key Tag").ToList();var holders=G("Key Hooks",board);
        var colors=new[]{new Color(.38f,.35f,.25f),new Color(.29f,.34f,.34f),new Color(.56f,.53f,.42f)};
        var materials=colors.Select((c,i)=>{string name="Worn Key Tag "+i,path=Base+"Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(M("Key Tag Plastic"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",.14f);EditorUtility.SetDirty(m);return m;}).ToArray();
        for(int i=0;i<hooks.Length;i++)
        {
            var p=positions[i];var delta=new Vector3(Mathf.Sin(i*7.1f)*.007f,Mathf.Cos(i*4.9f)*.009f,0);hooks[i].localPosition+=delta;
            var actor=G(important.ContainsKey(i)?"Key "+important[i]:empty.Contains(i)?"Empty Hook":"Spare Keys",holders);actor.localPosition=p;
            var visual=G("Key and Tag",actor);
            foreach(var part in parts)
            {
                if(owners[part]!=i)continue;
                if(part.name=="Hook Tip"){part.localPosition+=delta;continue;}part.SetParent(visual,true);
            }
            var tag=tags.OrderBy(t=>Vector2.Distance(new Vector2(t.localPosition.x,t.localPosition.y),new Vector2(p.x+.036f,p.y-.079f))).First();tags.Remove(tag);tag.SetParent(visual,true);
            tag.localScale=new Vector3(.88f+i%4*.06f,.85f+i%3*.10f,1);tag.Rotate(0,0,Mathf.Sin(i*2.7f)*7);tag.GetComponent<Renderer>().sharedMaterial=materials[i%3];actor.localPosition+=delta;
            if(empty.Contains(i)){visual.gameObject.SetActive(false);continue;}
            if(important.ContainsKey(i))
            {
                string room=important[i];var pickup=actor.gameObject.AddComponent<KeyPickup>();pickup.keyId=room=="Stairwell"?room:"Room "+room;pickup.label=room+" key";pickup.allowReturn=true;pickup.visualRoot=visual.gameObject;
                var box=actor.gameObject.AddComponent<BoxCollider>();box.center=new Vector3(.015f,-.045f,-.044f);box.size=new Vector3(.080f,.14f,.080f);
                if(room!="Stairwell")Q("Worn Room Tag",tag,new Vector3(0,0,-.0037f),new Vector2(.038f,.025f),M("Mailbox "+room+" Insert"),Quaternion.identity);
                else{tag.localScale=new Vector3(1.65f,1.22f,1);pickup.label="Stairwell door key";Q("Stairwell Label",tag,new Vector3(0,0,tag.GetComponent<MeshFilter>().sharedMesh.bounds.min.z-.0005f),new Vector2(.041f,.029f),M("Stairwell Key Label"),Quaternion.identity);}
            }
        }
        var glass=board.Find("Key Cabinet Glass");if(glass!=null)glass.gameObject.SetActive(false);
    }
    public static string InstallFunctionalKeys()
    {
        EnsureApartment();var apartment=GameObject.Find("Apartment").transform;var board=apartment.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Key Cabinet");FunctionalKeyBoard(board);
        foreach(var old in apartment.GetComponentsInChildren<KeyPickup>(true).Where(k=>k.keyId=="Stairwell"&&!k.transform.IsChildOf(board)).ToArray())old.gameObject.SetActive(false);
        foreach(var id in AccessibleUnits)
        {
            var door=apartment.Find("Floor "+id[0]+"/Residential Corridor").Cast<Transform>().First(t=>t.name=="Apartment Door"&&t.GetComponentsInChildren<TextMesh>(true).Any(x=>x.text==id));
            var target=door.GetComponent<RoomAccessLock>()??door.gameObject.AddComponent<RoomAccessLock>();target.roomNumber=id;target.requiredKeyId="Room "+id;target.locked=true;EditorUtility.SetDirty(target);
        }
        foreach(string path in new[]{BuildingBase+"Prefabs/Key Cabinet.prefab",Base+"Prefabs/Entrance Interior.prefab"})
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try{foreach(var b in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Key Cabinet").ToArray())FunctionalKeyBoard(b);foreach(var old in root.GetComponentsInChildren<KeyPickup>(true).Where(k=>k.keyId=="Stairwell"&&!k.allowReturn).ToArray())old.gameObject.SetActive(false);PrefabUtility.SaveAsPrefabAsset(root,path);}
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        SaveApartment();return "Six usable board keys (201,306,404,503,602 and stairs), ten spare keys, five empty hooks; central pickup, return and matching locks configured.";
    }
}
