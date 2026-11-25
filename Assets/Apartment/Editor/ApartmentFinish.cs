using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
public static partial class ApartmentBuilder
{
    public static string Finish()
    {
        foreach(var file in Directory.GetFiles(Base+"Textures/peeling_painted_wall").Concat(Directory.GetFiles(Base+"Textures/dirty_tiles")).Where(f=>f.EndsWith(".png")||f.EndsWith(".jpg")))
        {var imp=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));if(Path.GetFileName(file)=="nor_gl.jpg")imp.textureType=TextureImporterType.NormalMap;else imp.sRGBTexture=Path.GetFileName(file)=="Diffuse.jpg"||Path.GetFileName(file)=="Green Albedo.png";imp.anisoLevel=8;imp.SaveAndReimport();}
        var green=M("Green Tile");green.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"Textures/dirty_tiles/Green Albedo.png"));green.SetColor("_BaseColor",Color.white);green.SetTextureScale("_BaseMap",Vector2.one/.95f);green.SetFloat("_Smoothness",.7f);
        var plaster=M("Yellowed Plaster");string folder=Base+"Textures/peeling_painted_wall/";plaster.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Diffuse.jpg"));plaster.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"nor_gl.jpg"));plaster.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Unity Mask.png"));plaster.SetTexture("_OcclusionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Unity Mask.png"));plaster.SetColor("_BaseColor",new Color(.79f,.81f,.72f));plaster.SetTextureScale("_BaseMap",Vector2.one*.5f);
        var ground=GameObject.Find("Ground Floor").transform;var office=ground.Find("Management Room");var lobby=ground.Find("Lobby");
        if(!office.GetComponentsInChildren<Transform>().Any(t=>t.name=="Clock Dial"))
        {
            var clock=G("Clock Dial",office);clock.position=new Vector3(-5.18f,2.335f,6.60f);
            for(int i=1;i<=12;i++){float a=i*Mathf.PI/6;Text(i.ToString(),clock,new Vector3(Mathf.Sin(a)*.15f,Mathf.Cos(a)*.15f,-.035f),.032f,Quaternion.identity,new Color(.06f,.065f,.055f));}
            Rod("Hour Hand",clock,new Vector3(0,0,-.04f),new Vector3(-.08f,.055f,-.04f),.004f,"Black Rubber");Rod("Minute Hand",clock,new Vector3(0,0,-.045f),new Vector3(.035f,.13f,-.045f),.003f,"Black Rubber");
            var labels=AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/World Labels.mat");foreach(var text in clock.GetComponentsInChildren<TextMesh>())text.GetComponent<Renderer>().sharedMaterial=labels;
        }
        if(lobby.Find("Floor Wear")==null)
        {
            var wear=G("Floor Wear",lobby);
            for(int i=0;i<7;i++){Weather(wear,"Ground Stain",new Vector3((i%3-1)*.6f,.003f,1.4f+i*1.43f),new Vector2(.3f+i*.06f,.7f),Quaternion.Euler(90,0,i*37));}
            Weather(wear,"Water Stain",new Vector3(1.992f,.32f,8.4f),new Vector2(.5f,.6f),Quaternion.Euler(0,90,0));
        }
        foreach(var floor in GameObject.Find("Apartment").transform.Cast<Transform>().Where(t=>t.name.StartsWith("Floor ")))
        {
            int level=int.Parse(floor.name.Substring(6))-1;float offset=level*2.9f;
            foreach(Transform group in floor)if(Mathf.Abs(group.localPosition.y)<.001f){foreach(Transform child in group)child.localPosition-=Vector3.up*offset;group.localPosition=Vector3.up*offset;}
        }
        var root=GameObject.Find("Apartment").transform;
        foreach(var name in new[]{"Mailboxes","Telephone","Desk Fan","Elevator Cabin","Elevator Landing","Residential Corridor"})
        {
            var original=root.GetComponentsInChildren<Transform>().First(t=>t.name==name);var clone=UnityEngine.Object.Instantiate(original.gameObject);clone.name=name;clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);PrefabUtility.SaveAsPrefabAsset(clone,Base+"Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(clone);
        }
        foreach(var light in root.GetComponentsInChildren<Light>().Where(l=>l.transform.parent.name=="Fluorescent Light")){light.colorTemperature=4400;light.intensity=light.transform.position.y<3?7:5.2f;light.range=5.5f;}
        foreach(var filter in root.GetComponentsInChildren<MeshFilter>())if(filter.sharedMesh!=null&&filter.sharedMesh.uv2.Length==0)GameObjectUtility.SetStaticEditorFlags(filter.gameObject,GameObjectUtility.GetStaticEditorFlags(filter.gameObject)&~StaticEditorFlags.ContributeGI);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Green ceramic albedo, peeling paint, floor wear, clock dial and reusable module origins finalized.";
    }
}
