using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class ApartmentBuilder
{
    [Serializable]class LeatherMaterials{public LeatherMaterial[] materials;}
    [Serializable]class LeatherMaterial{public string name,@base,normal,metalSmooth,occlusion;public float[] color;public float metallic,smoothness;}
    public static string ReplaceGuardJacket()
    {
        EnsureApartment();const string folder="Assets/Narrative/Models/Leather Jacket/";
        var descriptions=JsonUtility.FromJson<LeatherMaterials>("{\"materials\":"+System.IO.File.ReadAllText(folder+"Materials.json")+"}").materials;
        var materials=new System.Collections.Generic.Dictionary<string,Material>();
        foreach(var data in descriptions)
        {
            foreach(string filename in new[]{data.@base,data.normal,data.metalSmooth,data.occlusion})
            {
                if(string.IsNullOrEmpty(filename))continue;var importer=(TextureImporter)AssetImporter.GetAtPath(folder+filename);
                importer.maxTextureSize=1024;importer.sRGBTexture=filename==data.@base;importer.alphaSource=TextureImporterAlphaSource.FromInput;
                if(filename==data.normal)importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();
            }
            string path=folder+data.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.SetColor("_BaseColor",new Color(data.color[0],data.color[1],data.color[2],1));material.SetFloat("_Cull",0);
            if(!string.IsNullOrEmpty(data.@base))material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+data.@base));
            if(!string.IsNullOrEmpty(data.normal)){material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+data.normal));material.SetFloat("_BumpScale",1);material.EnableKeyword("_NORMALMAP");}
            if(!string.IsNullOrEmpty(data.metalSmooth)){material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+data.metalSmooth));material.SetFloat("_Smoothness",1);material.SetFloat("_Metallic",1);material.EnableKeyword("_METALLICSPECGLOSSMAP");}else{material.SetFloat("_Metallic",data.metallic);material.SetFloat("_Smoothness",data.smoothness);}
            if(!string.IsNullOrEmpty(data.occlusion)){material.SetTexture("_OcclusionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+data.occlusion));material.EnableKeyword("_OCCLUSIONMAP");}
            if(data.name=="Jacket_Material01"||data.name=="Jacket_Material04"){material.SetFloat("_AlphaClip",1);material.SetFloat("_Cutoff",.3f);material.EnableKeyword("_ALPHATEST_ON");material.renderQueue=2450;}
            materials[data.name]=material;EditorUtility.SetDirty(material);
        }
        var office=GameObject.Find("Management Room").transform;var cloth=office.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Abandoned Security Jacket");
        if(cloth.GetComponent<MeshRenderer>()!=null)UnityEngine.Object.DestroyImmediate(cloth.GetComponent<MeshRenderer>());if(cloth.GetComponent<MeshFilter>()!=null)UnityEngine.Object.DestroyImmediate(cloth.GetComponent<MeshFilter>());
        foreach(Transform child in cloth.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
        cloth.SetPositionAndRotation(Vector3.zero,Quaternion.identity);cloth.localScale=Vector3.one;
        var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(folder+"Black Leather Jacket.fbx"),cloth).transform;model.name="Leather Jacket Model";
        foreach(var renderer in model.GetComponentsInChildren<Renderer>()){string name=renderer.sharedMaterial.name;renderer.sharedMaterial=materials[name];}
        var bounds=BoundsOf(model);cloth.position=new Vector3(-3.20f,1.64f,1.63f+bounds.extents.z)-bounds.center;
        // A real hanger carries the shoulders; a short wall hook supports the hanger at its apex.
        bounds=BoundsOf(model);float z=bounds.min.z+.035f,top=bounds.max.y;
        var hanger=G("Coat Hanger",cloth);var apex=new Vector3(-3.20f,top+.045f,z);var left=new Vector3(-3.38f,top-.055f,z);var right=new Vector3(-3.02f,top-.055f,z);
        foreach(var segment in new[]{new[]{apex,left},new[]{left,right},new[]{right,apex}})Rod("Hanger Wire",hanger,hanger.InverseTransformPoint(segment[0]),hanger.InverseTransformPoint(segment[1]),.005f,"Cabin Aged Steel");
        var oldHook=office.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Jacket Hook");if(oldHook!=null)UnityEngine.Object.DestroyImmediate(oldHook.gameObject);
        Rod("Wall Hook",hanger,hanger.InverseTransformPoint(new Vector3(-3.20f,top+.045f,1.55f)),hanger.InverseTransformPoint(apex),.008f,"Cabin Aged Steel");
        var hit=cloth.GetComponent<BoxCollider>();hit.center=cloth.InverseTransformPoint(bounds.center);hit.size=bounds.size+Vector3.one*.025f;
        CommonDynamic(cloth);PrefabUtility.SaveAsPrefabAsset(cloth.gameObject,"Assets/Narrative/Abandoned Security Jacket.prefab");PrefabUtility.SaveAsPrefabAsset(cloth.parent.gameObject,"Assets/Narrative/Security Office Story Props.prefab");SaveApartment();AssetDatabase.SaveAssets();
        return "Black leather jacket fitted beside the personal shelf; physical hanger, PBR maps and existing inspection preserved.";
    }
    static NarrativeBinding StoryBind(GameObject owner,string id){var existing=owner.GetComponents<NarrativeBinding>().FirstOrDefault(b=>b.id==id);var binding=existing!=null?existing:owner.AddComponent<NarrativeBinding>();binding.id=id;binding.target=owner;return binding;}
    static AudioSource StorySound(Transform parent,string name,AudioClip clip=null,bool loop=false,float volume=.35f)
    {var root=G(name,parent);var sound=root.gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=1;sound.rolloffMode=AudioRolloffMode.Logarithmic;sound.minDistance=1.3f;sound.maxDistance=20;sound.volume=volume;sound.loop=loop;sound.clip=clip;return sound;}
    static Transform StoryZone(Transform parent,string name,string signal,Vector3 center,Vector3 size)
    {var root=G(name,parent);root.position=center;var box=root.gameObject.AddComponent<BoxCollider>();box.isTrigger=true;box.size=size;root.gameObject.AddComponent<NarrativeSignalZone>().signal=signal;return root;}
    static NarrativeInteractable Readable(Transform target,string file,string prompt="Inspect")
    {
        var reader=target.GetComponent<NarrativeInteractable>();if(reader==null)reader=target.gameObject.AddComponent<NarrativeInteractable>();reader.prompt=prompt;reader.document=AssetDatabase.LoadAssetAtPath<NarrativeDocument>("Assets/Narrative/Documents/"+file+".asset");
        if(target.GetComponent<Collider>()==null){var bounds=BoundsOf(target);var box=target.gameObject.AddComponent<BoxCollider>();var local=new Bounds(target.InverseTransformPoint(bounds.center),Vector3.one*.01f);for(int i=0;i<8;i++)local.Encapsulate(target.InverseTransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));box.center=local.center;box.size=local.size+Vector3.one*.006f;}return reader;
    }
    static void StoryOfficeProps(Transform office)
    {
        var log=office.GetComponentsInChildren<Transform>().First(t=>t.name=="Maintenance Logbook");Readable(log,"Guard Logbook","Read Shift Log");var mug=office.GetComponentsInChildren<Transform>().First(t=>t.name=="Caretaker Mug");Readable(mug,"Coffee Mug","Inspect Mug");
        if(office.Find("Security Office Story Props")!=null)return;var props=G("Security Office Story Props",office);
        var leather=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Narrative/Abandoned Security Jacket.prefab");var cloth=UnityEngine.Object.Instantiate(leather!=null?leather:AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Building/Prefabs/Interiors/Hanging Shirt.prefab"),props).transform;cloth.name="Abandoned Security Jacket";var bounds=BoundsOf(cloth);
        if(leather==null){cloth.localRotation=Quaternion.Euler(0,90,0)*cloth.localRotation;bounds=BoundsOf(cloth);cloth.localScale*=.8f/bounds.size.y;bounds=BoundsOf(cloth);cloth.position+=new Vector3(-5.88f,1.55f,5.05f)-bounds.center;var fabric=AssetDatabase.LoadAssetAtPath<Material>("Assets/Narrative/Guard Jacket Fabric.mat");foreach(var renderer in cloth.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=fabric;Rod("Jacket Hook",props,new Vector3(-5.995f,2.03f,5.05f),new Vector3(-5.85f,2.03f,5.05f),.012f,"Cabin Aged Steel");}Readable(cloth,"Guard Jacket","Inspect Jacket");
        var paper=AssetDatabase.LoadAssetAtPath<Material>("Assets/Apartment/Materials/Caretaker Notes Aged Paper.mat");
        var id=Q("Employee Identification Card",props,new Vector3(-3.45f,.788f,3.16f),new Vector2(.085f,.125f),paper,Quaternion.Euler(90,0,-8));Readable(id,"Employee ID","Read Employee ID");
        var note=Q("Unfinished Handwritten Note",props,new Vector3(-4.45f,.988f,1.78f),new Vector2(.18f,.12f),paper,Quaternion.Euler(90,0,4));Readable(note,"Unfinished Note","Read Note");
        var pad=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Apartment/Prefabs/Notebook.prefab"),props).transform;pad.name="Current Shift Sign In Pad";bounds=BoundsOf(pad);pad.localScale*=.13f/Mathf.Max(bounds.size.x,bounds.size.z);bounds=BoundsOf(pad);pad.position+=new Vector3(-2.72f-bounds.center.x,.778f-bounds.min.y,3.18f-bounds.center.z);var box=pad.gameObject.AddComponent<BoxCollider>();bounds=BoundsOf(pad);box.center=pad.InverseTransformPoint(bounds.center);box.size=new Vector3(bounds.size.x/pad.lossyScale.x,Mathf.Max(.01f,bounds.size.y)/pad.lossyScale.y,bounds.size.z/pad.lossyScale.z);pad.gameObject.AddComponent<NarrativeWorkstation>();
        PrefabUtility.SaveAsPrefabAsset(props.gameObject,"Assets/Narrative/Security Office Story Props.prefab");
    }
    static void StoryWallClock(Transform office)
    {
        var root=office.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Wall Clock");if(PrefabUtility.IsPartOfPrefabInstance(root.gameObject))PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetNearestPrefabInstanceRoot(root.gameObject),PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);var face=root.GetComponentsInChildren<Renderer>(true).First(r=>r.name=="wall_clock");var clock=root.GetComponent<NarrativeAnalogClock>();if(clock==null)clock=root.gameObject.AddComponent<NarrativeAnalogClock>();clock.axis=Vector3.back;
        foreach(var pair in new[]{new{part="wall_clock_hours_hand",pivot="Narrative Hour Pivot"},new{part="wall_clock_minute_hand",pivot="Narrative Minute Pivot"}})
        {
            var hand=root.GetComponentsInChildren<Transform>(true).First(t=>t.name==pair.part);var pivot=root.Find(pair.pivot);
            if(pivot==null||hand.parent!=pivot){var center=face.bounds.center;center.z=hand.GetComponent<Renderer>().bounds.center.z;var direction=root.InverseTransformDirection(hand.GetComponent<Renderer>().bounds.center-center);direction.z=0;if(pivot==null)pivot=G(pair.pivot,root);pivot.position=center;hand.SetParent(pivot,true);pivot.localRotation=Quaternion.FromToRotation(direction.normalized,Vector3.up);hand.SetParent(root,true);pivot.localRotation=Quaternion.identity;hand.SetParent(pivot,true);}
            if(pair.part.Contains("hours"))clock.hourPivot=pivot;else clock.minutePivot=pivot;
        }
        foreach(var renderer in root.GetComponentsInChildren<Renderer>(true).Where(r=>r.name=="wall_clock_second_hand"))renderer.enabled=false;
    }
    static void ConfigureApartmentNight()
    {
        EnsureApartment();if(GameObject.Find("Night 1 Gameplay Hooks")!=null)return;var root=new GameObject("Night 1 Gameplay Hooks").transform;var office=GameObject.Find("Management Room").transform;
        var officeZone=StoryZone(root,"Office Area","office.zone",new Vector3(-4.15f,1.0f,4.15f),new Vector3(3.6f,2.0f,5.0f));StoryBind(officeZone.gameObject,"office.zone");
        foreach(var spawn in new[]{new{name="spawn.office",at=new Vector3(-4.3f,.05f,4.6f)},new{name="spawn.utility",at=new Vector3(-3.7f,.05f,7.2f)},new{name="spawn.floor2",at=new Vector3(.4f,2.95f,7)},new{name="spawn.floor3",at=new Vector3(.4f,5.85f,6)},new{name="spawn.floor4",at=new Vector3(.4f,8.75f,9.6f)}}){var marker=G(spawn.name,root);marker.position=spawn.at;StoryBind(marker.gameObject,spawn.name);}
        var officeDoor=office.GetComponentInChildren<StairAccessDoor>();StoryBind(officeDoor.gameObject,"office.door").door=officeDoor;
        for(int floor=2;floor<=6;floor++)StoryZone(root,"Residential Area "+floor,"floor."+floor,new Vector3(0,(floor-1)*2.9f+.9f,6.2f),new Vector3(3.7f,1.8f,12.2f));
        var corridor2=GameObject.Find("Floor 2").transform.Find("Residential Corridor");var door203=corridor2.Cast<Transform>().Where(t=>t.name=="Apartment Door").OrderBy(t=>Vector3.Distance(t.position,new Vector3(-1.94f,2.9f,8.2f))).First();var inspect=door203.gameObject.AddComponent<NarrativeInteractable>();inspect.prompt="Inspect Lock 203";inspect.signal="inspect.door.203";inspect.observation="203 is locked. The latch feels secure.";
        foreach(int floor in new[]{2,4})
        {
            var corridor=GameObject.Find("Floor "+floor).transform.Find("Residential Corridor");var fixture=corridor.Cast<Transform>().Where(t=>t.name=="Fluorescent Light").OrderBy(t=>Mathf.Abs(t.position.z-6.5f)).First();
            var reader=fixture.gameObject.AddComponent<NarrativeInteractable>();reader.prompt="Inspect Light";reader.signal="inspect.light."+floor;reader.observation=floor==4?"Steady now. The buzzing has stopped.":"Both tubes are working.";var box=fixture.gameObject.AddComponent<BoxCollider>();box.center=fixture.InverseTransformPoint(fixture.position+new Vector3(0,-.035f,0));box.size=new Vector3(.27f,.13f,1.25f);
            if(floor==4){corridor.GetComponentInChildren<PowerSwitch>().transform.position=new Vector3(3.18f,9.73f,12.13f);var fault=fixture.gameObject.AddComponent<NarrativeLightFault>();fault.fixture=fixture.GetComponentInChildren<Light>();fault.emitters=fixture.GetComponentsInChildren<Renderer>().Where(r=>r.sharedMaterial!=null&&r.sharedMaterial.HasProperty("_EmissionColor")&&r.sharedMaterial.GetColor("_EmissionColor").maxColorComponent>.01f).ToArray();fault.circuit=corridor.GetComponentInChildren<PowerSwitch>().circuit;fault.buzz=StorySound(fixture,"Electrical Buzz",StoryAudio("Fluorescent Buzz"),true,.55f);StoryBind(fixture.gameObject,"light.fault").lightFault=fault;}
        }
        StoryBind(root.gameObject,"floor.doors");var slam=StorySound(root,"Distant Door Slam",null,false,.45f);var slamBinding=StoryBind(slam.gameObject,"door.slam");slamBinding.sound=slam;slamBinding.soundBehindPlayer=true;
        var system=UnityEngine.Object.FindFirstObjectByType<CctvSystem>();StoryBind(system.channels[3].gameObject,"camera.fault").cameraFault=system.channels[3];
        var lift=UnityEngine.Object.FindFirstObjectByType<ElevatorController>();StoryBind(lift.gameObject,"lift").elevator=lift;
        var feedRoot=G("Lift Landing CCTV",root);feedRoot.position=new Vector3(.8f,2.35f,9.2f);feedRoot.LookAt(new Vector3(3.12f,1.25f,10.8f));var camera=feedRoot.gameObject.AddComponent<Camera>();camera.enabled=false;camera.fieldOfView=75;camera.nearClipPlane=.05f;camera.farClipPlane=12;camera.cullingMask=~(1<<5);var channel=feedRoot.gameObject.AddComponent<CctvChannel>();channel.cameraSource=camera;channel.cameraNumber=system.channels.Length+1;channel.location="1F Lift Landing";channel.circuit=system.channels[1].circuit;channel.label=system.channels[1].label;system.channels=system.channels.Concat(new[]{channel}).ToArray();
        var monitor=system.monitors[0];StoryBind(monitor.gameObject,"lift.monitor").monitor=monitor;var empty=AssetDatabase.LoadAssetAtPath<NarrativeSequence>(StoryResources+"Empty Elevator.asset");empty.cues[0].actions.First(a=>a.kind==NarrativeActionKind.CctvChannel).number=system.channels.Length-1;EditorUtility.SetDirty(empty);
        var audio=lift.gameObject.AddComponent<ElevatorNarrativeAudio>();audio.elevator=lift;audio.motor=StorySound(lift.cabin,"Elevator Motor",StoryAudio("Elevator Motor"),true,.5f);audio.sliding=StorySound(lift.cabin,"Door Movement",StoryAudio("Elevator Doors"),true,.5f);audio.bell=StorySound(lift.cabin,"Arrival Bell",StoryAudio("Elevator Bell"),false,.65f);
        var chair=office.GetComponentInChildren<ChairSeat>();StoryBind(chair.gameObject,"security.chair").chair=chair;
        var phone=office.GetComponentsInChildren<Transform>().First(t=>t.name=="Telephone Station");var telephone=phone.gameObject.AddComponent<NarrativeTelephone>();telephone.ringSource=StorySound(phone,"Desk Telephone Ring",StoryAudio("Telephone Ring"),true,.6f);telephone.conversation=AssetDatabase.LoadAssetAtPath<NarrativeSequence>(StoryResources+"Silent Call.asset");var phoneBinding=StoryBind(phone.gameObject,"desk.telephone");phoneBinding.telephone=telephone;phoneBinding.sound=StorySound(phone,"Handset Audio",null,false,.5f);var hit=phone.gameObject.GetComponent<BoxCollider>();if(hit==null)hit=phone.gameObject.AddComponent<BoxCollider>();hit.center=Vector3.up*.08f;hit.size=new Vector3(.35f,.18f,.3f);
        var voice=StorySound(root,"Protagonist Voice",null,false,.7f);voice.spatialBlend=0;StoryBind(voice.gameObject,"protagonist.voice").sound=voice;
        StoryOfficeProps(office);StoryWallClock(office);SaveApartment();
    }
    public static string ConfigureNightOneScenes()
    {
        ConfigureApartmentNight();EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");
        if(GameObject.Find("Night 1 Alley Hooks")==null){var root=new GameObject("Night 1 Alley Hooks").transform;StoryZone(root,"Approach Apartment","approach.apartment",new Vector3(22,1,60),new Vector3(6,3,6));StoryZone(root,"Distant Voice Trigger","alley.voice.zone",new Vector3(12,1,58),new Vector3(5,3,5));StoryZone(root,"Walk Home Exit","alley.exit",new Vector3(0,1,-1),new Vector3(7,3,5));foreach(var spawn in new[]{new{name="spawn.alley",at=new Vector3(0,.05f,4)},new{name="spawn.voice",at=new Vector3(16,.05f,58)}}){var marker=G(spawn.name,root);marker.position=spawn.at;StoryBind(marker.gameObject,spawn.name);}var voice=StorySound(root,"Distant Alley Vocalization",null,false,.3f);voice.maxDistance=32;var binding=StoryBind(voice.gameObject,"alley.voice");binding.sound=voice;binding.soundBehindPlayer=true;SaveApartment();}
        EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");AssetDatabase.SaveAssets();return "Apartment and Alley hooks saved: optional guard belongings, physical inspections, zones, faults, telephone, chair, real elevator audio/CCTV, wall clock and exit triggers.";
    }
}
