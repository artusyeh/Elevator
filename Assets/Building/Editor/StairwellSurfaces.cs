using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    static void StairPaintStrip(Transform parent,string name,Vector3 a,Vector3 b)
    {
        var mesh=new Mesh{name=name};mesh.vertices=new[]{a,a+Vector3.up*1.22f,b+Vector3.up*1.22f,b};mesh.uv=new[]{Vector2.zero,new Vector2(0,1.22f),new Vector2(Vector3.Distance(a,b),1.22f),new Vector2(Vector3.Distance(a,b),0)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateNormals();mesh.RecalculateBounds();
        string path=BuildingBase+"Models/"+name+".asset";var asset=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(asset!=null){EditorUtility.CopySerialized(mesh,asset);UnityEngine.Object.DestroyImmediate(mesh);mesh=asset;}else AssetDatabase.CreateAsset(mesh,path);
        var t=G("Painted Finish",parent);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=M("Stair Green Paint");r.shadowCastingMode=ShadowCastingMode.Off;
    }
    public static string PolishStairwellSurfaces()
    {
        EnsureApartment();var enclosure=GameObject.Find("Apartment").transform.Find("Stairwell Enclosure");foreach(var r in enclosure.GetComponentsInChildren<Renderer>().Where(r=>r.name=="Painted Wall"))r.sharedMaterial=M("Stair Plaster");var old=enclosure.Find("Wall Paint");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);var paint=G("Wall Paint",enclosure);
        var green=M("Stair Green Paint");green.SetColor("_BaseColor",new Color(.42f,.50f,.40f));EditorUtility.SetDirty(green);var concrete=M("Stair Concrete");concrete.SetColor("_BaseColor",new Color(.76f,.77f,.73f));concrete.SetFloat("_BumpScale",.70f);EditorUtility.SetDirty(concrete);
        var windows=Enumerable.Range(0,5).Select(i=>new Bounds(new Vector3(-3.8f,i*2.9f+StairWindowHeight,13.98f),new Vector3(1.30f,.80f,.7f))).ToArray();var doors=Enumerable.Range(0,6).Select(i=>new Bounds(new Vector3(-2.21f,i*2.9f+1.10625f,9.56f),new Vector3(.7f,2.2125f,1.06f))).ToArray();
        for(int i=0;i<6;i++)
        {
            float y=i*2.9f;
            StairWall(paint,"Painted Finish",new Bounds(new Vector3(-3.8f,y+.61f,8.4805f),new Vector3(3.02f,1.22f,.001f)),new Bounds[0],"Stair Green Paint");
            StairWall(paint,"Painted Finish",new Bounds(new Vector3(-2.3005f,y+.61f,9.29f),new Vector3(.001f,1.22f,1.62f)),doors,"Stair Green Paint");
            StairPaintStrip(paint,"Left Landing Paint "+(i+1)+"F",new Vector3(-5.319f,y,8.48f),new Vector3(-5.319f,y,10));
            if(i==5)continue;
            StairPaintStrip(paint,"Left Flight Paint "+(i+1)+"F",new Vector3(-5.319f,y,10),new Vector3(-5.319f,y+1.45f,12.34f));
            StairPaintStrip(paint,"Left Turn Paint "+(i+1)+"F",new Vector3(-5.319f,y+1.45f,12.34f),new Vector3(-5.319f,y+1.45f,13.89f));
            StairPaintStrip(paint,"Right Flight Paint "+(i+1)+"F",new Vector3(-2.301f,y+1.45f,12.34f),new Vector3(-2.301f,y+2.838f,10.10f));
            StairPaintStrip(paint,"Right Turn Paint "+(i+1)+"F",new Vector3(-2.301f,y+1.45f,13.89f),new Vector3(-2.301f,y+1.45f,12.34f));
            StairWall(paint,"Painted Finish",new Bounds(new Vector3(-3.8f,y+2.06f,13.8895f),new Vector3(3.02f,1.22f,.001f)),windows,"Stair Green Paint");
        }
        foreach(var c in paint.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);foreach(var r in paint.GetComponentsInChildren<Renderer>()){r.lightmapIndex=-1;r.shadowCastingMode=ShadowCastingMode.Off;GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);}
        foreach(var stairs in GameObject.Find("Apartment").GetComponentsInChildren<Transform>().Where(t=>t.name=="Stairwell"&&t.Find("Structure")!=null))
        {
            int floor=Mathf.RoundToInt(stairs.position.y/2.9f)+1;string maps=BuildingBase+"Textures/Elevator/blue_metal_plate/";var doorPaint=Plain("Stair Fire Door "+floor,new Color(.60f,.77f,.62f),.50f,1);doorPaint.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(maps+"Diffuse.jpg"));doorPaint.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(maps+"nor_gl.jpg"));doorPaint.SetFloat("_BumpScale",.22f);doorPaint.EnableKeyword("_NORMALMAP");doorPaint.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(maps+"Unity Mask.png"));doorPaint.EnableKeyword("_METALLICSPECGLOSSMAP");foreach(string property in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap"})doorPaint.SetTextureOffset(property,new Vector2(floor*.113f,floor*.177f));EditorUtility.SetDirty(doorPaint);
            foreach(var r in stairs.Find("Stairwell Door").GetComponentsInChildren<Renderer>().Where(r=>new[]{"Door Leaf","Door Jamb","Door Header"}.Contains(r.name)))r.sharedMaterial=doorPaint;
            foreach(var lamp in stairs.Cast<Transform>().Where(t=>t.name=="Wall Light"))
            {
                var primary=lamp.Find("Light").GetComponent<Light>();primary.intensity=lamp.localPosition.z>12?4.8f:3.8f;
                if(lamp.Find("Soft Fill")==null){Lamp(lamp,new Vector3(0,-.15f,0),.30f,3.2f,5200,Quaternion.identity,LightType.Point);var fill=lamp.GetComponentsInChildren<Light>().First(l=>l!=primary);fill.name="Soft Fill";fill.lightmapBakeType=LightmapBakeType.Realtime;fill.color=new Color(.78f,.88f,1);}
            }
            var circuit=stairs.GetComponent<LightCircuit>();circuit.lights=stairs.GetComponentsInChildren<Light>().Where(l=>l.name!="Night Light"&&!l.GetComponentsInParent<Transform>().Any(t=>t.name=="Emergency Light")).ToArray();EditorUtility.SetDirty(circuit);FitShadowPolicy(stairs);
        }
        SaveApartment();return "Green paint follows the actual flights and landing heights; concrete detail and gentle landing fill improved.";
    }
}
