using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public static partial class ApartmentBuilder
{
    static readonly Vector3 Address=new Vector3(22,0,64);
    const string ConnectionFolder="References/Neighborhood Connection/";
    static Transform CopyPart(Transform source,Transform parent){var t=UnityEngine.Object.Instantiate(source.gameObject,parent).transform;t.name=source.name;t.SetPositionAndRotation(source.position,source.rotation);return t;}
    static void SharedSave(Transform t,string name){PrefabUtility.SaveAsPrefabAsset(t.gameObject,Base+"Prefabs/"+name+".prefab");}
    static Transform SharedInstance(string name,Vector3 at){var t=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/"+name+".prefab"))).transform;t.position=at;return t;}
    static void StreetBox(string name,Transform p,Vector3 at,Vector3 size,string mat){B(name,p,at,size,"Concrete").GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/"+mat+".mat");}
    static void ExteriorWindow(Transform p,Vector3 at,float width,float height,bool lit,bool grille=false)
    {
        B("Window Recess",p,at,new Vector3(width+.1f,height+.1f,.075f),"Black Rubber",false);
        B("Window Glass",p,at+Vector3.back*.044f,new Vector3(width-.07f,height-.07f,.012f),lit?"Occupied Window":"Dirty Glass",false);
        foreach(float x in new[]{-width*.5f,0,width*.5f})B("Window Frame",p,at+new Vector3(x,0,-.065f),new Vector3(.035f,height+.04f,.045f),"Green Steel",false);
        foreach(float y in new[]{-height*.5f,height*.5f})B("Window Frame",p,at+new Vector3(0,y,-.065f),new Vector3(width+.06f,.035f,.045f),"Green Steel",false);
        B("Window Sill",p,at+new Vector3(0,-height*.5f-.04f,-.09f),new Vector3(width+.18f,.07f,.20f),"Concrete",false);
        if(grille)for(int i=0;i<6;i++)Rod("Window Grille",p,at+new Vector3(-width*.43f+i*width*.172f,-height*.48f,-.13f),at+new Vector3(-width*.43f+i*width*.172f,height*.48f,-.13f),.007f,"Green Steel");
        Weather(p,"Water Stain",at+new Vector3(0,-height*.5f-.3f,.041f),new Vector2(width,.6f),Quaternion.identity);
    }
    static void Neighbor(Transform parent,Vector3 at,Vector3 size,int style)
    {
        var g=G("Neighboring House",parent);g.localPosition=at;
        B("Building",g,new Vector3(0,size.y*.5f,0),size,style%2==0?"Yellowed Plaster":"Concrete");
        B("Roof Edge",g,new Vector3(0,size.y,.0f),new Vector3(size.x+.18f,.16f,size.z+.18f),"Concrete");
        for(int floor=0;floor<Mathf.FloorToInt(size.y/2.8f);floor++)for(int bay=0;bay<2;bay++)ExteriorWindow(g,new Vector3((bay-.5f)*size.x*.48f,1.65f+floor*2.8f,-size.z*.5f-.045f),1.05f,1.15f,(floor+bay+style)%5==0,floor==0);
        PipeRun(g,new Vector3(-size.x*.43f,.1f,-size.z*.5f-.13f),new Vector3(-size.x*.43f,size.y-.2f,-size.z*.5f-.13f),.03f);
    }
    public static string ConnectNeighborhood()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");Directory.CreateDirectory(ConnectionFolder);
        var active=SceneManager.GetActiveScene();if(active.isDirty)EditorSceneManager.SaveScene(active);
        foreach(string n in new[]{"Alley","Apartment"})File.Copy("Assets/Scenes/"+n+".unity",ConnectionFolder+n+".before.unity",true);
        EnsureApartment();if(GameObject.Find("Aoba Exterior")!=null)throw new Exception("Shared environment already installed; edit the prefab rather than rebuilding.");
        var apt=GameObject.Find("Apartment").transform;var ground=apt.Find("Ground Floor");var lobby=ground.Find("Lobby");
        var exterior=G("Aoba Exterior",null);var court=ground.Find("Entrance Court");
        // Use the actual entrance assemblies as the shared source, rather than designing a second doorway.
        foreach(var t in lobby.Cast<Transform>().Where(t=>new[]{"Entrance Door","Entrance Stile","Security Gate","Entrance Mat"}.Contains(t.name)).ToArray())t.SetParent(exterior,true);
        foreach(var t in lobby.Cast<Transform>().Where(t=>(t.name=="Tiled Wall"||t.name=="Plaster Wall"||t.name=="Skirting")&&t.position.z<.1f).ToArray())t.SetParent(exterior,true);
        foreach(var t in court.Cast<Transform>().ToArray())
        {
            if(t.name=="Neighbor Door"||t.position.z<-5.9f)continue;
            if(t.name=="Pavement"||t.name=="Light"||t.name.Contains("Bicycle")||t.name.Contains("Foliage"))t.SetParent(exterior,true);
        }
        UnityEngine.Object.DestroyImmediate(court.gameObject);
        // Courtyard sides are open to the same side street in both scenes.
        foreach(float x in new[]{-3.05f,3.05f}){Wall(exterior,new Vector3(x,0,-2),4,2.6f,true);B("Court Coping",exterior,new Vector3(x,2.62f,-2),new Vector3(.26f,.10f,4.1f),"Concrete");}
        B("Court Apron",exterior,new Vector3(0,-.12f,-5),new Vector3(6,.24f,2),"Concrete");
        B("Drain Channel",exterior,new Vector3(0,.002f,-4.8f),new Vector3(2.8f,.012f,.14f),"Black Rubber",false);
        for(int i=0;i<24;i++)B("Drain Bar",exterior,new Vector3(-1.35f+i*.117f,.012f,-4.8f),new Vector3(.018f,.01f,.14f),"Stainless Steel",false);
        var glow=Plain("Occupied Window",new Color(.32f,.29f,.20f),.36f);glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(.20f,.135f,.064f));
        // Six occupied levels and parapet; front bays correspond to corridor floor elevations.
        var facade=G("Building Facade",exterior);
        for(int level=0;level<6;level++)
        {
            float y=level*2.9f;
            foreach(float x in new[]{-4.22f,3.75f})
            {
                float w=x<0?3.72f:2.7f;
                B("Facade Spandrel",facade,new Vector3(x,y+.47f,-.09f),new Vector3(w,.94f,.2f),"Concrete");
                B("Facade Lintel",facade,new Vector3(x,y+2.49f,-.09f),new Vector3(w,.82f,.2f),"Yellowed Plaster");
                foreach(float offset in new[]{-w*.5f+.18f,w*.5f-.18f})B("Facade Pier",facade,new Vector3(x+offset,y+1.51f,-.09f),new Vector3(.36f,1.14f,.2f),"Concrete");
                ExteriorWindow(facade,new Vector3(x,y+1.52f,-.205f),w-.85f,1.06f,(level+(int)Mathf.Abs(x))%4==0,level<2);
            }
            if(level>0){B("Landing Facade",facade,new Vector3(0,y+1.4f,-.09f),new Vector3(4.35f,2.8f,.20f),"Concrete");ExteriorWindow(facade,new Vector3(0,y+1.6f,-.205f),1.5f,1.05f,false,true);}
            foreach(float side in new[]{-1f,1f})
            {
                var wall=G("Side Facade",facade);wall.localPosition=new Vector3(side*(side<0?6.22f:5.25f),y,6.8f);wall.localRotation=Quaternion.Euler(0,side<0?90:-90,0);
                B("Side Wall",wall,new Vector3(0,1.4f,0),new Vector3(14,2.8f,.18f),"Concrete");
                for(int bay=0;bay<3;bay++)ExteriorWindow(wall,new Vector3(-4.7f+bay*3.5f,1.6f,-.12f),1.4f,1.05f,(bay+level)%4==0,level<2);
            }
            B("Rear Facade",facade,new Vector3(-.45f,y+1.4f,14.12f),new Vector3(11.7f,2.8f,.18f),"Concrete");
            // The actual barred stair window is exposed through a matching external grille.
            var stairWindow=G("Stair Window",facade);stairWindow.localPosition=new Vector3(-3.8f,y+2.08f,14.24f);stairWindow.localRotation=Quaternion.Euler(0,180,0);ExteriorWindow(stairWindow,Vector3.zero,1.35f,.85f,false,true);
            if(level>0&&level<5){B("Balcony Slab",facade,new Vector3(-4.2f,y-.05f,-.48f),new Vector3(3.5f,.13f,.8f),"Concrete");Rod("Balcony Handrail",facade,new Vector3(-5.9f,y+.9f,-.85f),new Vector3(-2.5f,y+.9f,-.85f),.018f,"Green Steel");for(int i=0;i<18;i++)Rod("Balcony Railing",facade,new Vector3(-5.9f+i*.2f,y,-.85f),new Vector3(-5.9f+i*.2f,y+.9f,-.85f),.009f,"Green Steel");}
        }
        foreach(float x in new[]{-5.8f,2.45f,4.85f})PipeRun(facade,new Vector3(x,.1f,-.29f),new Vector3(x,17.5f,-.29f),.043f);
        B("Roof Slab",facade,new Vector3(-.45f,17.48f,7),new Vector3(11.8f,.18f,14.4f),"Concrete");
        foreach(float x in new[]{-6.25f,5.35f})B("Roof Parapet",facade,new Vector3(x,17.93f,7),new Vector3(.18f,.85f,14.4f),"Concrete");
        foreach(float z in new[]{-.15f,14.2f})B("Roof Parapet",facade,new Vector3(-.45f,17.93f,z),new Vector3(11.8f,.85f,.18f),"Concrete");
        B("Stair Roof House",facade,new Vector3(-3.8f,18.15f,11.3f),new Vector3(3.1f,1.2f,4.5f),"Concrete");B("Elevator Overrun",facade,new Vector3(4.1f,18.1f,10.8f),new Vector3(2,1.1f,2.3f),"Concrete");
        var plaque=B("Building Nameplate",exterior,new Vector3(1.65f,1.65f,-.225f),new Vector3(.65f,.4f,.035f),"Aged Plastic",false);
        Q("Building Name",plaque,new Vector3(0,0,-.019f),new Vector2(.60f,.32f),AssetDatabase.LoadAssetAtPath<Material>("Assets/Alley/Materials/Aoba_residence_sign.mat"),Quaternion.identity);
        Prop("Security Camera",exterior,new Vector3(-1.5f,2.45f,-.32f),180,.23f,false);
        Fluorescent(exterior,new Vector3(0,2.56f,-.65f),2.1f,4100);
        Notice("Building Notices",exterior,new Vector3(2.22f,1.3f,-.21f),new Vector2(.42f,.55f),Quaternion.identity);
        SharedSave(exterior,"Aoba Exterior");
        // A shared context contains only the junction and nearby streets visible from the apartment.
        var context=G("Side Streets",null);context.position=-Address;
        StreetBox("Side Street",context,new Vector3(9,-.16f,58),new Vector3(66,.16f,4.5f),"Rain Damp Asphalt");
        StreetBox("Approach",context,new Vector3(0,-.16f,54.9f),new Vector3(7,.16f,1.7f),"Rain Damp Asphalt");
        B("Neighborhood Ground",context,new Vector3(8,-.38f,63),new Vector3(80,.25f,35),"Concrete");
        for(int i=0;i<7;i++){float x=-21+i*9; if(x>15&&x<30)continue;Neighbor(context,new Vector3(x,0,65.5f),new Vector3(8,7.5f+i%3*2.8f,9),i);}
        for(int i=0;i<4;i++){float x=13+i*8.5f;Neighbor(context,new Vector3(x,0,50.8f),new Vector3(7.8f,7+i%2*2.8f,8),i+2);}
        Neighbor(context,new Vector3(-19,0,50),new Vector3(14,8.5f,10),3);
        Neighbor(context,new Vector3(38,0,65),new Vector3(11,10,8),2);
        // Both branches turn behind buildings, hiding the ends rather than presenting a cut-off road.
        StreetBox("Left Bend",context,new Vector3(-23,-.16f,52),new Vector3(4,.16f,12),"Rain Damp Asphalt");
        StreetBox("Right Bend",context,new Vector3(40,-.16f,52),new Vector3(4,.16f,12),"Rain Damp Asphalt");
        B("Left Garden Wall",context,new Vector3(-25.3f,1.6f,54),new Vector3(.25f,3.2f,13),"Concrete");B("Right Garden Wall",context,new Vector3(42.3f,1.6f,54),new Vector3(.25f,3.2f,13),"Concrete");
        foreach(float x in new[]{-23f,40f}){var end=G("Service Gate",context);end.localPosition=new Vector3(x,0,46);Gate(end,Vector3.zero,3.9f,0);Neighbor(context,new Vector3(x,0,42),new Vector3(7,8,6),1);}
        foreach(float x in new[]{-15f,-4f,9f,30f,40f})
        {var pole=G("Side Street Lamp",context);pole.localPosition=new Vector3(x,0,60.1f);Rod("Lamp Post",pole,Vector3.zero,new Vector3(0,4.8f,0),.055f,"Green Steel");B("Lamp Housing",pole,new Vector3(0,4.75f,-.25f),new Vector3(.18f,.09f,.6f),"Stainless Steel",false);Lamp(pole,new Vector3(0,4.62f,-.3f),x>10?1.8f:2.6f,6,3900,Quaternion.Euler(90,0,0),LightType.Spot);}
        for(int i=0;i<3;i++)Rod("Overhead Cable",context,new Vector3(-24,6.1f+i*.18f,60.4f),new Vector3(42,5.9f+i*.18f,60.4f),.009f,"Black Rubber");
        // Rename context origin to entrance coordinates without changing geometry.
        foreach(Transform child in context)child.localPosition-=Address;context.position=Vector3.zero;
        SharedSave(context,"Side Streets");
        var entrance=exterior.GetComponentInChildren<SceneEntrance>();entrance.destination="Alley";entrance.preservePose=true;entrance.sceneOffset=Address;entrance.doorwaySide=-1;PrefabUtility.RecordPrefabInstancePropertyModifications(entrance);
        // Save source prefab without destination overrides, then use connected instances in both scenes.
        UnityEngine.Object.DestroyImmediate(exterior.gameObject);UnityEngine.Object.DestroyImmediate(context.gameObject);
        var ext=SharedInstance("Aoba Exterior",Vector3.zero);entrance=ext.GetComponentInChildren<SceneEntrance>();entrance.destination="Alley";entrance.preservePose=true;entrance.sceneOffset=Address;entrance.doorwaySide=-1;PrefabUtility.RecordPrefabInstancePropertyModifications(entrance);
        SharedInstance("Side Streets",Vector3.zero);
        var preview=G("Entrance Interior",null);foreach(var child in ground.Cast<Transform>())if(child.name!="Entrance Court")CopyPart(child,preview);
        foreach(var mirror in preview.GetComponentsInChildren<ApartmentMirror>())mirror.enabled=false;foreach(var light in preview.GetComponentsInChildren<Light>())light.lightmapBakeType=LightmapBakeType.Realtime;
        SharedSave(preview,"Entrance Interior");UnityEngine.Object.DestroyImmediate(preview.gameObject);
        SaveApartment();
        EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");var alley=GameObject.Find("Alley").transform;var aoba=alley.Find("Buildings/Aoba Apartments");aoba.gameObject.SetActive(false);
        var continuation=alley.Find("Street Continuation");foreach(var t in continuation.Cast<Transform>())
        {if(t.name=="North Neighbor"||t.name=="North Return Wall"||(t.name=="Neighboring Building"&&t.position.z>53))t.gameObject.SetActive(false);if(t.name=="Boundary Wall"){var c=t.GetComponent<BoxCollider>();if(c!=null){c.size=new Vector3(.5f,5,55);t.GetComponent<MeshFilter>().sharedMesh=Rounded(c.size,.006f);t.localScale=Vector3.one;t.position=new Vector3(t.position.x,2.5f,17);}}}
        var shared=SharedInstance("Aoba Exterior",Address);var travel=shared.GetComponentInChildren<SceneEntrance>();travel.destination="Apartment";travel.preservePose=true;travel.sceneOffset=-Address;travel.doorwaySide=1;PrefabUtility.RecordPrefabInstancePropertyModifications(travel);
        SharedInstance("Side Streets",Address);SharedInstance("Entrance Interior",Address);
        // Match sky and distant landmark context in the apartment without loading the original street.
        var sky=RenderSettings.skybox;var ambientSky=RenderSettings.ambientSkyColor;var ambientEquator=RenderSettings.ambientEquatorColor;var ambientGround=RenderSettings.ambientGroundColor;
        var backdrop=G("Neighborhood Backdrop",null);foreach(var renderer in alley.GetComponentsInChildren<Renderer>().Where(r=>r.sharedMaterial!=null&&(r.sharedMaterial.shader.name.Contains("Tokyo")||r.name.Contains("Tower"))).ToArray()){if(renderer.transform.parent!=null&&renderer.transform.parent.name=="Neighborhood Backdrop")continue;CopyPart(renderer.transform,backdrop);}
        foreach(Transform t in backdrop)t.position-=Address;SharedSave(backdrop,"Neighborhood Backdrop");UnityEngine.Object.DestroyImmediate(backdrop.gameObject);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=ambientSky;RenderSettings.ambientEquatorColor=ambientEquator;RenderSettings.ambientGroundColor=ambientGround;SharedInstance("Neighborhood Backdrop",Vector3.zero);SaveApartment();
        return "Shared six-floor exterior, entrance, side streets and interior preview installed; original main street retained.";
    }
}
