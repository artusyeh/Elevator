using System.Linq;
using UnityEngine;
using UnityEditor;
public static partial class ApartmentBuilder
{
    public static string BuildDevelopmentActor()
    {
        var root=new GameObject("Resident Waypoint Sample");var actorRoot=G("Replaceable Resident",root.transform);var body=actorRoot.gameObject.AddComponent<CharacterController>();body.height=1.72f;body.radius=.22f;body.center=Vector3.up*.86f;var actor=actorRoot.gameObject.AddComponent<NarrativeActor>();var model=G("Model (development placeholder)",actorRoot);actor.model=model.gameObject;
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Narrative/Guard Jacket Fabric.mat");
        foreach(bool head in new[]{false,true}){var part=GameObject.CreatePrimitive(head?PrimitiveType.Sphere:PrimitiveType.Capsule);part.name=head?"Head":"Body";part.transform.SetParent(model,false);part.transform.localPosition=Vector3.up*(head?1.6f:.84f);part.transform.localScale=head?Vector3.one*.22f:new Vector3(.38f,.65f,.33f);UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());part.GetComponent<Renderer>().sharedMaterial=material;}
        var a=G("Waypoint A",root.transform);var b=G("Waypoint B",root.transform);b.localPosition=new Vector3(0,0,2.5f);actor.waypoints=new[]{a,b};model.gameObject.SetActive(false);PrefabUtility.SaveAsPrefabAsset(root,StoryResources+"Resident Placeholder.prefab");UnityEngine.Object.DestroyImmediate(root);return "Development-only replaceable waypoint actor prefab saved; no NPC is placed in Night 1.";
    }
    public static string ValidateNightOne()
    {
        var definition=AssetDatabase.LoadAssetAtPath<NightDefinition>(StoryResources+"Night 1.asset");if(definition==null)throw new System.Exception("Missing Night 1 definition");
        if(definition.beats.Select(b=>b.id).Distinct().Count()!=definition.beats.Length)throw new System.Exception("Duplicate step IDs");
        for(int i=0;i<definition.beats.Length;i++){var beat=definition.beats[i];if(string.IsNullOrEmpty(beat.objective)||beat.conditions.Length==0&&beat.waitForSignals.Length==0)throw new System.Exception("Unguided/unconditional step: "+beat.id);if(i+1<definition.beats.Length&&beat.nextStepId!=definition.beats[i+1].id)throw new System.Exception("Non-linear next step: "+beat.id);}
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(StoryResources+"Narrative Runtime.prefab");if(prefab==null||prefab.GetComponentInChildren<NarrativePresentation>(true).regulations==null)throw new System.Exception("Missing narrative UI dependency");
        var phone=AssetDatabase.LoadAssetAtPath<GameObject>(PhonePath).GetComponent<PhoneNarrativeBridge>();if(phone==null||phone.taskTemplate==null||phone.messageContent==null||phone.reportInput==null)throw new System.Exception("Missing phone bridge fields");
        var scenes=EditorBuildSettings.scenes.Where(s=>s.path!="Assets/Scenes/Alley.unity"&&s.path!="Assets/Scenes/Apartment.unity").ToList();scenes.Insert(0,new EditorBuildSettingsScene("Assets/Scenes/Apartment.unity",true));scenes.Insert(0,new EditorBuildSettingsScene("Assets/Scenes/Alley.unity",true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
        return definition.beats.Length+" unique guided linear steps validated; Night 1 only; Alley is the build entry scene.";
    }
}
