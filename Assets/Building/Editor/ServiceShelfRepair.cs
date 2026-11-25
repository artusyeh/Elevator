using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    const string ShelfReview="References/Service Shelf/";
    static void PrepareServiceTools()
    {
        foreach(string name in new[]{"Screwdriver","Pliers"})
        {
            string id=name.ToLowerInvariant(),folder=BuildingBase+"Models/"+name+"/";var material=Plain("Worn "+name,Color.white,1,1);
            material.SetTexture("_BaseMap",DetailMap(folder+"textures/"+id+"_diff_2k.jpg"));material.SetTexture("_BumpMap",DetailMap(folder+"textures/"+id+"_nor_gl_2k.jpg",true));material.SetFloat("_BumpScale",.65f);material.EnableKeyword("_NORMALMAP");
            var mask=DetailMask(folder+"textures/"+id+"_arm_2k.jpg",folder+"textures/Unity Mask.png");material.SetTexture("_MetallicGlossMap",mask);material.SetTexture("_OcclusionMap",mask);material.SetFloat("_OcclusionStrength",.85f);material.EnableKeyword("_METALLICSPECGLOSSMAP");material.EnableKeyword("_OCCLUSIONMAP");EditorUtility.SetDirty(material);
            var root=new GameObject(name);try
            {
                var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(folder+name+".fbx"),root.transform);model.name=name;
                foreach(var r in model.GetComponentsInChildren<Renderer>()){r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();r.lightmapIndex=-1;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
                var bounds=BoundsOf(root.transform);var collider=root.AddComponent<BoxCollider>();collider.center=bounds.center;collider.size=bounds.size;PrefabUtility.SaveAsPrefabAsset(root,BuildingBase+"Prefabs/"+name+".prefab");
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
    }
    static void FitServiceShelf(Transform room)
    {
        var shelf=room.Find("Service Shelf");shelf.localPosition=new Vector3(-3.15f,0,8.01f);
        foreach(var old in shelf.Cast<Transform>().Where(t=>new[]{"Ceramic Fuse","Fuse End","Screwdriver","Tool Handle","Tools"}.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        // Retain the existing plank, close the rear gap and keep its original front edge.
        LiftResize(shelf.Find("Shelf"),new Vector3(0,.65f,-.0575f),new Vector3(.42f,.035f,.295f));
        foreach(var bracket in shelf.Cast<Transform>().Where(t=>t.name=="Shelf Bracket"))
        {
            var plate=bracket.Find("Wall Plate");var at=plate.localPosition;at.z=.080f;plate.localPosition=at;
            if(bracket.Find("Mounting Screw")==null)foreach(float y in new[]{.475f,.605f})Rod("Mounting Screw",bracket,new Vector3(0,y,.071f),new Vector3(0,y,.067f),.0035f,"Cabin Aged Steel");
        }
        var tools=G("Tools",shelf);
        foreach(string name in new[]{"Screwdriver","Pliers"})
        {
            var tool=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildingBase+"Prefabs/"+name+".prefab"),tools);
            tool.name=name;tool.transform.localPosition=new Vector3(name=="Screwdriver"?.012f:-.010f,.6682f,name=="Screwdriver"?-.137f:-.030f);tool.transform.localRotation=Quaternion.Euler(0,name=="Screwdriver"?-9:13,0);
            var position=tool.transform.position;position.y+=shelf.Find("Shelf").GetComponent<Renderer>().bounds.max.y+.0007f-BoundsOf(tool.transform).min.y;tool.transform.position=position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(tool.transform);
            foreach(var t in tool.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
        }
        foreach(var r in shelf.GetComponentsInChildren<Renderer>())r.lightmapIndex=-1;
    }
    public static string CaptureServiceShelf(string suffix)
    {
        Capture("Service Shelf "+suffix,new Vector3(-3.16f,1.05f,7.16f),new Vector3(-3.15f,.64f,7.98f));
        Capture("Electrical Room Shelf "+suffix,new Vector3(-3.55f,1.65f,6.96f),new Vector3(-3.94f,1.09f,7.96f));
        foreach(string name in new[]{"Service Shelf","Electrical Room Shelf"})File.Copy("References/Apartment/Captures/"+name+" "+suffix+".png",ShelfReview+name+" "+suffix+".png",true);
        return "Shelf close-up and player-height room view captured";
    }
    public static string RepairServiceShelf()
    {
        EnsureApartment();Directory.CreateDirectory(ShelfReview);if(!File.Exists(ShelfReview+"Apartment.before.unity"))File.Copy("Assets/Scenes/Apartment.unity",ShelfReview+"Apartment.before.unity");
        if(!File.Exists(ShelfReview+"Service Shelf Before.png"))CaptureServiceShelf("Before");PrepareServiceTools();var room=GameObject.Find("Ground Floor").transform.Find("Electrical Room");FitServiceShelf(room);
        foreach(string path in new[]{BuildingBase+"Prefabs/Electrical Room.prefab",Base+"Prefabs/Entrance Interior.prefab"})
        {var prefab=PrefabUtility.LoadPrefabContents(path);try{var target=path.EndsWith("Entrance Interior.prefab")?prefab.transform.Find("Electrical Room"):prefab.transform;FitServiceShelf(target);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}}
        SaveServicePrefab(room.Find("Service Shelf"),"Service Shelf");SaveApartment();CaptureServiceShelf("After");return "Service shelf moved 35 cm to the right, brackets seated on wall, realistic CC0 screwdriver and pliers grounded on shelf; scene and shared prefabs saved.";
    }
}
