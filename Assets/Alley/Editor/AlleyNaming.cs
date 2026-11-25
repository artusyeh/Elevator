using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AlleyNaming
{
    static readonly Dictionary<string,string> Names = new Dictionary<string,string>
    {
        {"ALLEY — 52m environment study", "Alley"},
        {"01 Architecture / editable", "Buildings"},
        {"02 Street / collision", "Street"},
        {"03 Props / prefab instances", "Props"},
        {"04 Utilities / cables", "Utilities"},
        {"05 Lighting / panorama", "Lighting"},
        {"06 Player / WASD mouse Esc", "Player"},
        {"07 Review viewpoints / eye height 1.65 m", "Viewpoints"},
        {"01 Arrival", "Street Entrance"}, {"02 Sushi frontage", "Sushi Shop"},
        {"03 Vending corner", "Vending Corner"}, {"04 Apartment approach", "Apartment Entrance"},
        {"Minato Sushi / closed facade study", "Minato Sushi"},
        {"Aoba apartments / future entrance", "Aoba Apartments"},
        {"Garbage collection / reserved interaction area", "Garbage Collection"},
        {"Near neighborhood / south return facade", "Corner Building"},
        {"Street bed — 52 m", "Road"}, {"Glazing / curtain", "Window Glass"},
        {"Bicycle — city frame", "City Bicycle"}, {"Vending machine — unbranded", "Vending Machine"},
        {"Refuse bin — temporary", "Garbage Bin"}, {"Garbage bag placeholder", "Garbage Bag"},
        {"Plant stem", "Plant Stem"}, {"Leaf study", "Leaf"},
        {"Baked street reflection", "Street Reflections"},
        {"Night grade / editable volume", "Night Lighting"}, {"Cool night fill", "Moonlight"},
        {"Side-passage end / future extension", "Passage Gate"},
        {"VendingMachine_Study", "Vending Machine"}, {"CityBicycle_Study", "City Bicycle"},
        {"RefuseBin_Study", "Garbage Bin"},
        {"VendingMachine", "Vending Machine"}, {"CityBicycle", "City Bicycle"}, {"RefuseBin", "Garbage Bin"}
    };
    public static string Clean(string name)
    {
        string clean;
        if(Names.TryGetValue(name,out clean))return clean;
        var building=Regex.Match(name,@"^(West|East) / \d+ — (\d+) storeys$");
        if(building.Success) return building.Groups[2].Value=="2" ? "Townhouse" : "Apartment Building";
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name);
    }
    [MenuItem("Alley/Clean Up Names")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Exit Play Mode before renaming.");
        string[] prefabs=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Alley/Prefabs"});
        foreach(var guid in prefabs)
        {
            var path=AssetDatabase.GUIDToAssetPath(guid);
            var root=PrefabUtility.LoadPrefabContents(path);
            try {foreach(var t in root.GetComponentsInChildren<Transform>(true))t.name=Clean(t.name);PrefabUtility.SaveAsPrefabAsset(root,path);}
            finally {PrefabUtility.UnloadPrefabContents(root);}
            string name=Clean(System.IO.Path.GetFileNameWithoutExtension(path));
            string error=AssetDatabase.RenameAsset(path,name);
            if(!string.IsNullOrEmpty(error))throw new System.Exception(error);
        }
        var active=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach(var guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
            bool alreadyOpen=scene.IsValid() && scene.isLoaded;
            if(!alreadyOpen)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            bool changed=false;
            foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))
            {
                if(t==null)continue;
                if(t.name=="Review refinement applied" && t.childCount==0){Object.DestroyImmediate(t.gameObject);changed=true;continue;}
                string clean=Clean(t.name);if(t.name==clean)continue;t.name=clean;changed=true;
            }
            if(changed){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
            if(!alreadyOpen)EditorSceneManager.CloseScene(scene,true);
        }
        if(active.IsValid() && active.isLoaded)UnityEngine.SceneManagement.SceneManager.SetActiveScene(active);
        AssetDatabase.SaveAssets();
    }
}
