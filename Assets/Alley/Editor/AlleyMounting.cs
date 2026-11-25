using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class AlleyMounting
{
    const string Folder="References/Sign Mounting/";
    static readonly HashSet<string> SignNames=new HashSet<string>{"Street Address","Missing Cat","Neighborhood Notice","Security Notice","No Parking","Shoe Repair","Apartment Nameplate","Festival","Closed Today","Business Hours","Menu","Collection Schedule","Minato Sushi","Aoba Residence","Collection Notice"};
    static bool IsSign(MeshRenderer r){var f=r.GetComponent<MeshFilter>();return f!=null&&f.sharedMesh!=null&&f.sharedMesh.vertexCount<=24&&SignNames.Contains(r.name);}
    static Transform Ancestor(Transform t,Func<Transform,bool> condition){for(var p=t;p!=null;p=p.parent)if(condition(p))return p;return null;}
    static Bounds BoundsOf(Transform t){var rs=t.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
    static void Change(Transform t,Vector3 local){t.localPosition=local;if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);}
    public static string Entrance()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");var b=GameObject.Find("Buildings").transform.Cast<Transform>().First(t=>Vector3.Distance(t.position,new Vector3(6.3f,0,11.4f))<.2f);var door=b.Find("Recessed Residential Door");if(door.localPosition.x>0)return "Entrance already clear of vending machines.";
        float original=door.localPosition.x,target=1.288f,delta=target-original;var windowPosition=b.Find("Window Glass").localPosition;
        // This is one facade adjustment: remove only the window occupying the new doorway.
        var remove=new List<Transform>();foreach(Transform t in b){if(t.localPosition.y<2.7f&&new[]{"Window Recess","Window Glass","Aluminium Mullion","Window Sill","Curtain Fold"}.Contains(t.name))remove.Add(t);}
        var fittings=b.Find("Facade Fittings");if(fittings!=null)foreach(Transform t in fittings)if(t.localPosition.y<2.7f&&new[]{"Window Pane","Window Reveal","Glazing Seal","Window Lintel","Water Stain"}.Contains(t.name)&&Mathf.Abs(t.localPosition.x-windowPosition.x)<1.25f)remove.Add(t);
        foreach(var t in remove)UnityEngine.Object.DestroyImmediate(t.gameObject);
        foreach(Transform t in b)if(new[]{"Recessed Residential Door","Frosted Door Panel","Door Pull","Door Step"}.Contains(t.name))Change(t,t.localPosition+Vector3.right*delta);
        if(fittings!=null)foreach(Transform t in fittings)if(new[]{"Door Jamb","Door Header","Mail Slot"}.Contains(t.name))Change(t,t.localPosition+Vector3.right*delta);
        foreach(Transform t in b.Find("Entrance Details"))if(t.name=="Wall Mailbox"||t.name=="Entrance Intercom")Change(t,t.localPosition+Vector3.right*delta);
        var ac=b.Find("Outdoor AC Condenser");Change(ac,new Vector3(-1.6f,2.45f,ac.localPosition.z));
        var lighting=GameObject.Find("Window Lighting");if(lighting!=null)foreach(Transform t in lighting.transform.Cast<Transform>().ToArray()){var p=b.InverseTransformPoint(t.position);if(p.y<2.7f&&Mathf.Abs(p.x-windowPosition.x)<1.05f&&Mathf.Abs(p.z+3.15f)<.8f)UnityEngine.Object.DestroyImmediate(t.gameObject);}
        var lamp=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Alley/Prefabs/Lighting/Residential Light.prefab"));lamp.transform.SetParent(b,false);lamp.transform.localPosition=new Vector3(target,2.5f,-3.01f);var light=lamp.GetComponentInChildren<Light>();light.transform.localRotation=Quaternion.Euler(105,0,0);light.intensity=1.25f;PrefabUtility.RecordPrefabInstancePropertyModifications(light);PrefabUtility.RecordPrefabInstancePropertyModifications(light.transform);PrefabUtility.RecordPrefabInstancePropertyModifications(lamp.transform);
        var plant=GameObject.Find("Herb Planter").transform;plant.position=new Vector3(2.83f,-.007f,11.3f);PrefabUtility.RecordPrefabInstancePropertyModifications(plant);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());File.WriteAllText(Folder+"Entrance.txt","Vending positions preserved. Entrance moved from local x="+original+" to "+target+" (street z="+door.position.z+"). Door leaf, glazing, handle, frame, step, intercom and mailbox moved together. AC relocated above vending machines with its brackets and pipe connection. Removed only the ground window occupying the new entrance; original facade volume closes the former doorway. Herb planter moved clear of the step.");return "Door, frame, step and entrance fittings moved to street z="+door.position.z+". Both vending machines retained.";
    }
    sealed class Surface : IDisposable
    {
        public Transform owner;public MeshCollider collider;GameObject go;Mesh mesh;readonly List<int> ends=new List<int>();readonly List<MeshRenderer> parts=new List<MeshRenderer>();
        public Surface(Transform root)
        {
            owner=root;var rs=root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled&&!IsSign(r)&&!new[]{"Peeling Paint","Rust","Dirt","Water Stain","Moss","Crack"}.Contains(r.name)).ToList();
            // Include machine cabinets as occluders, so notices cannot be mounted behind them.
            if(root.name=="Townhouse"||root.name=="Apartment Building")foreach(var r in GameObject.Find("Alley").GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Cabinet"&&Ancestor(r.transform,t=>t.name=="Vending Machine")!=null))if(Mathf.Abs(r.bounds.center.z-root.position.z)<5)rs.Add(r);
            var vs=new List<Vector3>();var ts=new List<int>();foreach(var r in rs){var f=r.GetComponent<MeshFilter>();if(f==null||f.sharedMesh==null)continue;int n=vs.Count;vs.AddRange(f.sharedMesh.vertices.Select(v=>f.transform.TransformPoint(v)));ts.AddRange(f.sharedMesh.triangles.Select(t=>n+t));parts.Add(r);ends.Add(ts.Count/3);}
            mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(vs);mesh.SetTriangles(ts,0);mesh.RecalculateBounds();go=new GameObject("Mounting Surface");collider=go.AddComponent<MeshCollider>();collider.sharedMesh=mesh;
        }
        public MeshRenderer Part(int triangle){for(int i=0;i<ends.Count;i++)if(triangle<ends[i])return parts[i];return null;}
        public void Dispose(){UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(mesh);}
    }
    static Transform Context(Transform sign)
    {
        var direct=Ancestor(sign.parent,t=>t.name=="Menu Stand"||t.name=="Minato Sushi"||t.name=="Aoba Apartments"||t.Find("Facade Volume")!=null);if(direct!=null)return direct;
        Vector3 normal=-sign.forward;var hits=Physics.RaycastAll(sign.position+normal, -normal,3).Where(h=>Vector3.Dot(h.normal,normal)>.95f).OrderBy(h=>h.distance).ToArray();foreach(var h in hits){var root=Ancestor(h.collider.transform,t=>t.name=="Aoba Apartments"||t.Find("Facade Volume")!=null);if(root!=null)return root;if(h.collider.name.Contains("Wall"))return h.collider.transform;}throw new Exception("No wall context for "+sign.name);
    }
    static bool Receiver(Transform context,MeshRenderer part)
    {if(part==null)return false;if(context.name=="Menu Stand")return true;if(context.name=="Minato Sushi")return part.name=="Timber Shop Frontage"||part.name=="Shop Sign Backing";if(context.name=="Aoba Apartments")return part.name=="Apartment Facade";if(context.name=="Service Alley"||context.name.Contains("Wall"))return part.name.Contains("Wall");return part.name=="Facade Volume"||part.name=="Concrete Plinth";}
    static bool TryPlace(Surface surface,Transform sign,Vector2 xy,float width,float height,out Vector3 at,out Quaternion rotation,out string reason)
    {
        at=Vector3.zero;rotation=Quaternion.identity;reason="";var owner=surface.owner;var local=owner.InverseTransformPoint(sign.position);Vector3 normal=owner.name=="Menu Stand"?-sign.forward:owner.TransformDirection(Vector3.back);Vector3 basePoint=owner.TransformPoint(new Vector3(xy.x,xy.y,local.z));RaycastHit centre;
        if(!surface.collider.Raycast(new Ray(basePoint+normal*2,-normal),out centre,4)){reason="no surface";return false;}var part=surface.Part(centre.triangleIndex);if(!Receiver(owner,part)){reason="blocked by "+(part==null?"mesh":part.name);return false;}if(Vector3.Dot(centre.normal,normal)<(owner.name=="Menu Stand"?.9f:.94f)){reason="surface angle "+centre.normal;return false;}
        rotation=Quaternion.LookRotation(-centre.normal,Vector3.up);Vector3 right=rotation*Vector3.right,up=rotation*Vector3.up;
        foreach(float x in new[]{-.5f,0,.5f})foreach(float y in new[]{-.5f,0,.5f})
        {Vector3 p=centre.point+right*(x*width+Mathf.Sign(x)*.012f)+up*(y*height+Mathf.Sign(y)*.012f);RaycastHit hit;if(!surface.collider.Raycast(new Ray(p+centre.normal*.5f,-centre.normal),out hit,1)){reason="unsupported edge";return false;}if(!Receiver(owner,surface.Part(hit.triangleIndex))||Vector3.Dot(hit.normal,centre.normal)<.997f||Mathf.Abs(Vector3.Dot(hit.point-centre.point,centre.normal))>.0025f){reason="uneven support at "+x+","+y+" on "+surface.Part(hit.triangleIndex).name+" depth="+Vector3.Dot(hit.point-centre.point,centre.normal);return false;}}
        at=centre.point+centre.normal*.0015f;return true;
    }
    static Vector2 Preferred(string name,Vector2 original)
    {
        if(name=="Street Address")return new Vector2(-.65f,2.05f);if(name=="Missing Cat")return new Vector2(-.65f,1.35f);if(name=="Neighborhood Notice")return new Vector2(-.50f,1.45f);if(name=="Security Notice")return new Vector2(-.25f,1.85f);if(name=="No Parking")return new Vector2(-.6f,.85f);if(name=="Shoe Repair")return new Vector2(-.1f,2.05f);return original;
    }
    public static string Signs()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");Physics.SyncTransforms();var signs=GameObject.Find("Alley").GetComponentsInChildren<MeshRenderer>().Where(IsSign).ToArray();var surfaces=new Dictionary<Transform,Surface>();var placed=new Dictionary<Transform,List<Rect>>();var log=new List<string>();int count=0,failures=0;
        try
        {
            foreach(var r in signs.OrderBy(r=>r.name=="Street Address"?0:1))
            {
                var t=r.transform;var context=Context(t);Surface surface;if(!surfaces.TryGetValue(context,out surface)){surface=new Surface(context);surfaces[context]=surface;placed[context]=new List<Rect>();}
                var original=context.InverseTransformPoint(t.position);Vector2 initial=new Vector2(original.x,original.y);float width=t.TransformVector(Vector3.right).magnitude,height=t.TransformVector(Vector3.up).magnitude;
                if(context.name=="Minato Sushi"&&t.name=="Business Hours")initial=new Vector2(2.64f,1.48f);
                if(context.name=="Minato Sushi"&&t.name=="Closed Today"){initial=new Vector2(2.64f,1.15f);width=.24f;height=.061f;var s=t.localScale;s.x=width;s.y=height;t.localScale=s;}
                if(context.name=="Minato Sushi"&&t.name=="Menu")initial=new Vector2(-2.64f,1.5f);
                if(context.name=="Aoba Apartments"&&t.name=="Aoba Residence")initial=new Vector2(-2.45f,2.65f);
                var candidate=new List<Vector2>{initial};if(context.Find("Facade Volume")!=null)candidate.Add(Preferred(t.name,initial));
                Vector2 preferred=Preferred(t.name,initial);if(context.Find("Facade Volume")!=null)for(float y=.75f;y<=2.65f;y+=.1f)for(float x=-2.6f;x<=2.6f;x+=.1f)candidate.Add(new Vector2(x,y));else if(context.name=="Menu Stand")for(float y=initial.y-.10f;y<=initial.y+.1f;y+=.02f)candidate.Add(new Vector2(initial.x,y));
                if(candidate.Count>2){var first=candidate.Take(2).ToArray();candidate=first.Concat(candidate.Skip(2).OrderBy(p=>(p-preferred).sqrMagnitude)).ToList();}
                bool done=false;string why="";foreach(var xy in candidate)
                {
                    float w=width/context.lossyScale.x,h=height/context.lossyScale.y;var rect=new Rect(xy.x-w*.5f-.025f,xy.y-h*.5f-.025f,w+.05f,h+.05f);if(placed[context].Any(p=>p.Overlaps(rect)))continue;Vector3 at;Quaternion rotation;if(!TryPlace(surface,t,xy,width,height,out at,out rotation,out why))continue;
                    var before=t.position;t.SetPositionAndRotation(at,rotation);r.shadowCastingMode=ShadowCastingMode.Off;PrefabUtility.RecordPrefabInstancePropertyModifications(t);PrefabUtility.RecordPrefabInstancePropertyModifications(r);placed[context].Add(rect);log.Add(t.name+" | "+context.name+" | "+before+" -> "+at+" | flush offset 1.5 mm");count++;done=true;break;
                }
                if(!done){failures++;log.Add("UNRESOLVED "+t.name+" | "+context.name+" | "+why);}
            }
        }
        finally{foreach(var surface in surfaces.Values)surface.Dispose();}
        File.WriteAllLines(Folder+"Mounting Changes.txt",log);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Mounted and checked="+count+"; unresolved="+failures;
    }
    public static void Capture(string file,Vector3 at,Vector3 target)
    {
        var cam=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>().eyes;var p=cam.transform.position;var q=cam.transform.rotation;var old=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1600,1000,24);var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);
        try{cam.transform.SetPositionAndRotation(at,Quaternion.LookRotation(target-at));cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes(Folder+file+".png",image.EncodeToPNG());}
        finally{cam.transform.SetPositionAndRotation(p,q);cam.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
    }
}
