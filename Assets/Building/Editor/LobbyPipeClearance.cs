using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static partial class ApartmentBuilder
{
    static void SeatLobbyWaterMains(Transform lobby)
    {
        foreach(Transform pipe in lobby)
        {
            if(pipe.name!="Service Pipe"||Mathf.Abs(pipe.localPosition.y-2.58f)>.03f)continue;
            var renderer=pipe.GetComponent<Renderer>();if(renderer==null||renderer.bounds.size.z<10)continue;
            float end=pipe.localPosition.z+pipe.localScale.y;float extend=12.53f-end;
            var position=pipe.localPosition;position.z+=extend*.5f;pipe.localPosition=position;var scale=pipe.localScale;scale.y+=extend*.5f;pipe.localScale=scale;EditorUtility.SetDirty(pipe);
        }
    }
    static void RaiseLobbyDrainReturn(Transform services)
    {
        var original=services.Find("Lobby Drain Collector");if(original==null)return;
        var replacement=CommonRoute("Lobby Drain Collector",services,.055f,"Service Drain Enamel",new Vector3(-1.94f,2.54f,.34f),new Vector3(1.47f,2.54f,.34f),new Vector3(1.47f,2.58f,11.88f),new Vector3(1.47f,2.70f,12.18f),new Vector3(1.47f,2.70f,12.38f),new Vector3(2.15f,2.70f,12.38f),new Vector3(2.15f,2.70f,12.67f));
        if(services.Find("Drain Wall Socket")==null)Rod("Drain Wall Socket",services,new Vector3(2.15f,2.70f,12.47f),new Vector3(2.15f,2.70f,12.51f),.068f,"Service Drain Enamel");
        original.GetComponent<MeshFilter>().sharedMesh=replacement.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.DestroyImmediate(replacement.gameObject);EditorUtility.SetDirty(original.GetComponent<MeshFilter>());
    }
    public static string FixLobbyPipeCrossing()
    {
        var lobby=GameObject.Find("Ground Floor").transform.Find("Lobby");RaiseLobbyDrainReturn(lobby.Find("Ceiling Services"));SeatLobbyWaterMains(lobby);
        var path=CommonPrefabs+"Lobby Services.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{RaiseLobbyDrainReturn(root.transform);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        path="Assets/Apartment/Prefabs/Entrance Interior.prefab";root=PrefabUtility.LoadPrefabContents(path);try{foreach(var part in root.GetComponentsInChildren<Transform>(true))if(part.name=="Lobby"){SeatLobbyWaterMains(part);var services=part.Find("Ceiling Services");if(services!=null)RaiseLobbyDrainReturn(services);}PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Drain return rerouted above the three water pipes with separation at the ceiling corner.";
    }
}
