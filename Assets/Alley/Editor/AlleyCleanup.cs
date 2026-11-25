using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class AlleyCleanup
{
    const string Base="Assets/Alley/";
    static readonly List<string> notes=new List<string>();
    static Transform Group(string name,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    static Bounds BoundsOf(Transform t){var r=t.GetComponentsInChildren<Renderer>();var b=r[0].bounds;foreach(var a in r)b.Encapsulate(a.bounds);return b;}
    static Bounds LocalBounds(Transform t,Transform parent)
    {
        Bounds b=new Bounds();bool first=true;foreach(var f in t.GetComponentsInChildren<MeshFilter>())foreach(var v in f.sharedMesh.vertices){var p=parent.InverseTransformPoint(f.transform.TransformPoint(v));if(first){b=new Bounds(p,Vector3.zero);first=false;}else b.Encapsulate(p);}return b;
    }
    static void Rod(Transform t,Vector3 a,Vector3 b,float radius){t.localPosition=(a+b)*.5f;t.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);t.localScale=new Vector3(radius*2,(b-a).magnitude*.5f,radius*2);}
    static void StretchBack(Transform t,float back){if(Quaternion.Angle(t.localRotation,Quaternion.identity)>.1f)return;float front=t.localPosition.z-t.localScale.z*.5f;if(back<=front)return;var p=t.localPosition;p.z=(front+back)*.5f;var s=t.localScale;s.z=back-front;t.localPosition=p;t.localScale=s;}
    static void Cable(Transform p,Vector3 a,Vector3 b,Material mat)
    {
        var g=Group("Power Cable",p);var l=g.gameObject.AddComponent<LineRenderer>();l.positionCount=49;l.startWidth=l.endWidth=.014f;l.sharedMaterial=mat;l.generateLightingData=true;l.shadowCastingMode=ShadowCastingMode.Off;l.useWorldSpace=true;
        for(int i=0;i<49;i++){float t=i/48f;l.SetPosition(i,Vector3.Lerp(a,b,t)-Vector3.up*(.65f*4*t*(1-t)));}
    }
    static void SetZ(Transform t,float z){var p=t.localPosition;p.z=z;t.localPosition=p;}
    static void Move(Transform t,Vector3 p){notes.Add(t.name+": "+t.position+" -> "+p);t.position=p;}
    public static string Geometry()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");if(GameObject.Find("Pole Lines")!=null)throw new Exception("Cleanup already applied.");notes.Clear();
        var utilities=GameObject.Find("Utilities").transform;var groups=new List<List<Transform>>();
        foreach(Transform t in utilities){if(t.name=="Utility Pole")groups.Add(new List<Transform>());if(groups.Count>0)groups[groups.Count-1].Add(t);}
        Vector3[] positions={new Vector3(-2.87f,0,2.35f),new Vector3(-2.79f,0,19.65f),new Vector3(-3.08f,0,41),new Vector3(3.05f,0,8.2f),new Vector3(3.70f,0,27.35f),new Vector3(2.70f,0,47.15f)};
        var poles=new List<Transform>();
        for(int i=0;i<groups.Count;i++)
        {
            var old=groups[i][0].position;var delta=positions[i]-new Vector3(old.x,0,old.z);var assembly=Group("Power Pole",utilities);assembly.position=positions[i];poles.Add(assembly);int insulator=0;
            foreach(var t in groups[i])
            {
                if(t.name=="Overhead Service Cable"){UnityEngine.Object.DestroyImmediate(t.gameObject);continue;}t.SetParent(assembly,true);t.position+=delta;
                if(t.name=="Utility Pole"){t.name="Concrete Pole";Rod(t,new Vector3(0,-.12f,0),new Vector3(0,12.05f,0),.11f);}
                else if(t.name=="Crossarm")Rod(t,new Vector3(0,11.4f,-.75f),new Vector3(0,11.4f,.75f),.045f);
                else if(t.name=="Insulator"){float z=(insulator++-1)*.55f;Rod(t,new Vector3(0,11.4f,z),new Vector3(0,11.65f,z),.055f);}
            }
            notes.Add("Power pole moved from "+old+" to "+positions[i]+"; mast and crossarm clear the balconies.");
        }
        var lines=Group("Pole Lines",utilities);var rubber=AssetDatabase.LoadAssetAtPath<Material>(Base+"Materials/Charcoal rubber.mat");var ordered=poles.OrderBy(p=>p.position.z).ToArray();
        for(int i=0;i<ordered.Length-1;i++)for(int k=-1;k<=1;k++)Cable(lines,ordered[i].position+new Vector3(0,11.65f,k*.55f),ordered[i+1].position+new Vector3(0,11.65f,k*.55f),rubber);
        int slabs=0,brackets=0,hardware=0;var buildings=GameObject.Find("Buildings").transform;
        foreach(Transform building in buildings)
        {
            var facade=building.Find("Facade Volume");if(facade==null)continue;float width=facade.localScale.x;
            bool lowBalcony=building.Cast<Transform>().Any(t=>t.name=="Balcony Slab"&&t.localPosition.y<3.2f);
            if(lowBalcony){var eave=building.Find("Rain Eave");if(eave!=null){UnityEngine.Object.DestroyImmediate(eave.gameObject);notes.Add("Removed overlapping rain eave beneath balcony at "+building.position);}}
            foreach(Transform t in building.Cast<Transform>().ToArray())
            {
                if(t.name=="Balcony Slab"){StretchBack(t,-2.975f);slabs++;}
                if(t.name=="Floor Division"||t.name=="Recessed Residential Door"||t.name=="Window Recess")StretchBack(t,-2.995f);
                if(t.name=="Rainwater Downpipe"){var p=t.localPosition;p.x=width*.46f;p.z=-3.07f;t.localPosition=p;}
                if(t.name=="Pipe Strap"){var p=t.localPosition;p.x=width*.46f;p.z=-3.04f;t.localPosition=p;var s=t.localScale;s.z=.09f;t.localScale=s;}
                if(t.name=="Rain Eave"){var s=t.localScale;s.x=width*.88f;t.localScale=s;}
                if(t.name=="Electrical Meter")SetZ(t,-3.085f);
                if(t.name=="Meter Conduit")SetZ(t,-3.022f);
                if(t.name=="Meter Cover")SetZ(t,-3.20f);
            }
            foreach(var t in building.GetComponentsInChildren<Transform>())if(t.name=="Door Jamb"||t.name=="Door Header"||t.name=="Window Reveal"||t.name=="Window Lintel")StretchBack(t,-2.995f);
            foreach(var ac in building.GetComponentsInChildren<Transform>().Where(t=>t.name=="Outdoor AC Condenser"))
            {
                foreach(Transform t in ac){if(t.name=="Support Bracket"){var p=t.localPosition;p.y=-.2625f;p.z=.07f;t.localPosition=p;var s=t.localScale;s.z=.55f;t.localScale=s;brackets++;}else if(t.name=="Mounting Plate"){SetZ(t,.335f);}else if(t.name=="Coolant Pipe")Rod(t,new Vector3(.4f,-.04f,.03f),new Vector3(.46f,-.24f,.35f),.028f);}
            }
            var fittings=building.Find("Entrance Details");if(fittings!=null)foreach(Transform t in fittings)
            {
                if(t.name=="Wall Mailbox"||t.name=="Entrance Intercom"||t.name=="Security Camera"||t.name=="Utility Cabinet")
                {if(t.name!="Utility Cabinet")t.localRotation=Quaternion.Euler(t.name=="Security Camera"?20:0,180,0);var b=LocalBounds(t,building);SetZ(t,t.localPosition.z-3.005f-b.max.z);hardware++;}
            }
        }
        // The shop canopy originally passed through the adjacent first-floor balcony.
        var shop=GameObject.Find("Minato Sushi").transform;var awning=shop.Find("Deep Metal Awning");var ap=awning.localPosition;ap.x=-1;awning.localPosition=ap;var az=awning.localScale;az.x=3.9f;awning.localScale=az;
        foreach(var seam in shop.Cast<Transform>().Where(t=>t.name=="Awning Standing Seam").ToArray())
        {float x=seam.localPosition.x;if(x>.93f){UnityEngine.Object.DestroyImmediate(seam.gameObject);continue;}seam.localRotation=awning.localRotation;seam.localPosition=awning.localPosition+awning.localRotation*new Vector3(x+1,.0575f,0);}
        var backing=shop.Find("Shop Sign Backing");var bp=backing.localPosition;bp.x=-1;backing.localPosition=bp;var bs=backing.localScale;bs.x=3.8f;backing.localScale=bs;
        var sign=shop.Cast<Transform>().FirstOrDefault(t=>t.name=="Minato Sushi"&&t.GetComponent<Renderer>()!=null);if(sign!=null){var p=sign.localPosition;p.x=-1;sign.localPosition=p;var s=sign.localScale;s.x=3.4f;sign.localScale=s;}
        var lantern=shop.Find("Lantern Bracket");var lp=lantern.localPosition;lp.y=2.66f;lantern.localPosition=lp;
        // Remove the original residential door fittings that were showing through the shop overlay.
        foreach(Transform building in buildings)
        {
            var facade=building.Find("Facade Volume");if(facade==null||building.position.x>0)continue;
            foreach(Transform t in building.Cast<Transform>().ToArray())
            {if(!new[]{"Recessed Residential Door","Frosted Door Panel","Door Pull","Porch Fixture","Practical Light"}.Contains(t.name)||t.position.z<3.3f||t.position.z>9.1f)continue;UnityEngine.Object.DestroyImmediate(t.gameObject);}
            var details=building.Find("Entrance Details");if(details!=null)foreach(Transform t in details.Cast<Transform>().ToArray())if(t.position.z>3.3f&&t.position.z<9.1f)UnityEngine.Object.DestroyImmediate(t.gameObject);
            foreach(var t in building.GetComponentsInChildren<Transform>().Where(t=>t.name=="Door Jamb"||t.name=="Door Header"||t.name=="Mail Slot").ToArray())if(t.position.z>3.3f&&t.position.z<9.1f)UnityEngine.Object.DestroyImmediate(t.gameObject);
            var step=building.Find("Door Step");if(step!=null&&step.position.z>3.3f&&step.position.z<9.1f){step.SetParent(shop,true);var p=step.localPosition;p.x=1.88f;step.localPosition=p;step.name="Entrance Step";}
        }
        var homes=GameObject.Find("Residential Entrances").transform;
        foreach(Transform t in homes)
        {
            var p=t.position;
            if(t.name=="Potted Foliage"&&p.z>6&&p.z<7.5f)p.z=5.15f;
            else if(t.name=="Potted Succulent"&&p.z>10.6f&&p.z<11.3f){p.z=11.7f;p.x=-3;}
            else if(t.name=="Potted Foliage"&&p.z>17&&p.z<18)p.z=16.5f;
            else if(t.name=="Potted Fern"&&p.z>18&&p.z<18.6f)p.z=16.95f;
            else if(t.name=="Basket Bicycle"&&p.x>0&&p.z>19)p.z+=.55f;
            else if(t.name=="Potted Foliage"&&p.x<0&&p.z>34&&p.z<36)p.z=34.55f;
            else if(t.name=="Potted Succulent"&&p.x<0&&p.z>33&&p.z<35)p.z=33.7f;
            t.position=p;
        }
        Physics.SyncTransforms();var stackNames=new HashSet<string>{"Basket Bicycle","Commuter Bicycle","Delivery Bicycle","Potted Fern","Potted Foliage","Potted Succulent","Herb Planter","Watering Can","Folded Umbrella","Red Crate","Storage Crate","Drink Crates","Delivery Carton","Cleaning Bucket","Cleaning Bottle","Broom","Menu Stand","Waste Bin","Trash Bag","Traffic Cone"};
        var placed=GameObject.Find("Alley").GetComponentsInChildren<Transform>().Where(t=>stackNames.Contains(t.name)).ToArray();var stacking=new List<Transform>();int settled=0;
        foreach(var t in placed)
        {
            if((t.name=="Storage Crate"&&t.position.y>.3f)||(t.name=="Delivery Carton"&&t.position.y>.3f)){stacking.Add(t);continue;}
            var b=BoundsOf(t);float wall=float.PositiveInfinity;foreach(Transform building in buildings){var f=building.Find("Facade Volume");if(f==null||Mathf.Sign(building.position.x)!=Mathf.Sign(t.position.x)||Mathf.Abs(t.position.z-building.position.z)>f.localScale.x*.5f+.1f)continue;wall=Mathf.Min(wall,Mathf.Abs(building.position.x)-3);}
            if(t.position.x<0&&t.position.z>3.3f&&t.position.z<9.1f)wall=Mathf.Min(wall,3.04f);
            if(!float.IsPositiveInfinity(wall)){float delta=t.position.x>0?Mathf.Min(0,wall-.035f-b.max.x):Mathf.Max(0,-wall+.035f-b.min.x);t.position+=Vector3.right*delta;}
            b=BoundsOf(t);var hits=Physics.RaycastAll(new Vector3(t.position.x,.45f,t.position.z),Vector3.down,1);var floor=hits.Where(h=>h.normal.y>.7f&&h.point.y<.25f&&!h.collider.transform.IsChildOf(t)).OrderByDescending(h=>h.point.y).FirstOrDefault();if(floor.collider!=null){t.position+=Vector3.up*(floor.point.y+.003f-b.min.y);settled++;}
        }
        foreach(var t in stacking){var lower=placed.Where(p=>p!=t&&!stacking.Contains(p)&&Mathf.Abs(p.position.x-t.position.x)<.4f&&Mathf.Abs(p.position.z-t.position.z)<.35f).OrderBy(p=>Vector3.Distance(p.position,t.position)).FirstOrDefault();if(lower!=null)t.position+=Vector3.up*(BoundsOf(lower).max.y+.002f-BoundsOf(t).min.y);}
        foreach(var t in GameObject.Find("Alley").GetComponentsInChildren<Transform>().Where(t=>t.name=="Fallen Leaf")){RaycastHit h;if(Physics.Raycast(t.position+Vector3.up*.3f,Vector3.down,out h,1))t.position=new Vector3(t.position.x,h.point.y+.002f,t.position.z);}
        notes.Add("Anchored cantilever slabs="+slabs+"; corrected AC brackets="+brackets+"; mounted wall hardware="+hardware+"; settled props="+settled);
        Directory.CreateDirectory("References/Cleanup");File.WriteAllLines("References/Cleanup/Geometry Changes.txt",notes);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return notes.Last();
    }
    static Material FixtureMaterial(string name,Color color,float metal,float smooth,Color emission)
    {
        string path=Base+"Materials/Lighting/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);
        if(emission.maxColorComponent>0){m.SetColor("_EmissionColor",emission);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;m.EnableKeyword("_EMISSION");}EditorUtility.SetDirty(m);return m;
    }
    static GameObject Fixture(string name)
    {
        string fbx=Base+"Models/Lighting/"+name+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(fbx);importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.generateSecondaryUV=true;importer.SaveAndReimport();
        var housing=FixtureMaterial("Painted Housing",new Color(.13f,.15f,.14f),.45f,.36f,Color.black);var hardware=FixtureMaterial("Stainless Hardware",new Color(.4f,.43f,.42f),.85f,.42f,Color.black);var gasket=FixtureMaterial("Rubber Gasket",new Color(.018f,.022f,.02f),0,.2f,Color.black);
        var lens=FixtureMaterial(name+" Diffuser",new Color(.72f,.72f,.65f),0,.35f,name=="Alley Light"?new Color(.75f,.72f,.63f):new Color(.5f,.4f,.27f));
        var root=Group(name,null);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(fbx));model.name="Model";model.transform.SetParent(root,false);
        foreach(var r in model.GetComponentsInChildren<Renderer>()){var slots=r.sharedMaterials;for(int i=0;i<slots.Length;i++){string n=slots[i].name;slots[i]=n.Contains("Opal")?lens:n.Contains("Stainless")?hardware:n.Contains("Gasket")?gasket:housing;}r.sharedMaterials=slots;if(slots.Contains(lens))r.shadowCastingMode=ShadowCastingMode.Off;}
        var lamp=Group("Light",root);var l=lamp.gameObject.AddComponent<Light>();l.type=LightType.Spot;l.useColorTemperature=true;l.color=Color.white;l.shadows=LightShadows.Soft;l.shadowBias=.015f;l.shadowNormalBias=.08f;l.shadowCustomResolution=512;l.lightmapBakeType=LightmapBakeType.Mixed;
        if(name=="Residential Light"){lamp.localPosition=new Vector3(0,-.055f,-.105f);lamp.localRotation=Quaternion.Euler(65,0,0);l.spotAngle=115;l.innerSpotAngle=75;l.intensity=1.45f;l.range=3.6f;l.colorTemperature=3200;}
        else if(name=="Shop Entrance Light"){lamp.localPosition=new Vector3(0,-.095f,-.31f);lamp.localRotation=Quaternion.Euler(90,0,0);l.spotAngle=100;l.innerSpotAngle=65;l.intensity=3.2f;l.range=3.8f;l.colorTemperature=3300;}
        else{lamp.localPosition=new Vector3(0,-.07f,0);lamp.localRotation=Quaternion.Euler(90,0,0);l.spotAngle=105;l.innerSpotAngle=65;l.intensity=32;l.range=9.5f;l.colorTemperature=3900;}
        var data=lamp.gameObject.AddComponent<UniversalAdditionalLightData>();var so=new SerializedObject(data);so.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;so.ApplyModifiedPropertiesWithoutUndo();
        var result=PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base+"Prefabs/Lighting/"+name+".prefab");UnityEngine.Object.DestroyImmediate(root.gameObject);return result;
    }
    static Transform PlaceFixture(GameObject prefab,Transform parent,Vector3 local,Quaternion rotation)
    {var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.transform.SetParent(parent,false);g.transform.localPosition=local;g.transform.localRotation=rotation;return g.transform;}
    public static string Lighting()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");if(GameObject.Find("Residential Light")!=null)throw new Exception("Fixtures already installed.");
        Directory.CreateDirectory(Base+"Materials/Lighting");Directory.CreateDirectory(Base+"Prefabs/Lighting");AssetDatabase.Refresh();var wall=Fixture("Residential Light");var shopLamp=Fixture("Shop Entrance Light");var street=Fixture("Alley Light");int homes=0;
        foreach(Transform building in GameObject.Find("Buildings").transform)
        {
            var facade=building.Find("Facade Volume");if(facade==null)continue;float width=facade.localScale.x;
            foreach(Transform t in building){if(t.name=="Rainwater Downpipe"||t.name=="Pipe Strap"){var p=t.localPosition;p.x=width*.46f;t.localPosition=p;}else if(t.name=="Rain Eave"){var s=t.localScale;s.x=width*.88f;t.localScale=s;}}
            var old=building.Find("Porch Fixture");if(old==null||old.position.z>33)continue;var at=old.localPosition;at.z=-3.01f;UnityEngine.Object.DestroyImmediate(old.gameObject);
            foreach(Transform t in building.Cast<Transform>().ToArray())if(t.name=="Practical Light")UnityEngine.Object.DestroyImmediate(t.gameObject);
            PlaceFixture(wall,building,at,Quaternion.identity);homes++;
        }
        var shop=GameObject.Find("Minato Sushi").transform;
        foreach(var t in shop.Cast<Transform>().Where(t=>t.GetComponent<Light>()!=null).ToArray())
        {
            if(t.localPosition.x<0){UnityEngine.Object.DestroyImmediate(t.gameObject);continue;}
            t.name="Lantern Glow";t.localPosition=new Vector3(2.5f,2.3f,-.86f);var l=t.GetComponent<Light>();l.intensity=1.1f;l.range=3;l.colorTemperature=3000;l.shadows=LightShadows.Soft;
        }
        var lanternMaterial=FixtureMaterial("Lantern Paper",new Color(.67f,.56f,.37f),0,.15f,new Color(.65f,.46f,.26f));foreach(var r in shop.GetComponentsInChildren<Renderer>())if(r.name=="Paper Lantern"){r.sharedMaterial=lanternMaterial;r.shadowCastingMode=ShadowCastingMode.Off;}else if(r.name=="Lantern Rib")r.shadowCastingMode=ShadowCastingMode.Off;
        PlaceFixture(shopLamp,shop,new Vector3(1.88f,2.57f,-.095f),Quaternion.identity);
        int replaced=0;foreach(var pole in GameObject.Find("Utilities").transform.Cast<Transform>().Where(t=>t.name=="Power Pole").OrderBy(t=>t.position.z).Take(3))
        {
            var old=pole.Find("Streetlamp Diffuser");var light=pole.Find("Streetlight");float temperature=light.GetComponent<Light>().colorTemperature;float side=Mathf.Sign(pole.position.x);UnityEngine.Object.DestroyImmediate(old.gameObject);UnityEngine.Object.DestroyImmediate(light.gameObject);
            var fixture=PlaceFixture(street,pole,new Vector3(-side*.8f,5.295f,0),Quaternion.Euler(0,90,0));fixture.GetComponentInChildren<Light>().colorTemperature=temperature;replaced++;
        }
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());string result="Three original fixture prefabs; residential replacements="+homes+"; shop entrance=1; alley lamps="+replaced+". Farther fixtures retained.";File.WriteAllText("References/Cleanup/Lighting Changes.txt",result);return result;
    }
}
