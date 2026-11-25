using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class InteractionSetup
{
    const string UiPath="Assets/Interaction/Prefabs/Interaction UI.prefab";
    public static string Capture(string path)
    {
        var scanner=UnityEngine.Object.FindFirstObjectByType<InteractionScanner>();var camera=scanner.eyes;var view=scanner.GetComponentInChildren<InteractionReticle>();var canvas=view.GetComponent<Canvas>();
        var mode=canvas.renderMode;var uiCamera=canvas.worldCamera;float distance=canvas.planeDistance;
        var old=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1920,1080,24);var texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=camera.nearClipPlane+.01f;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1920,1080),0,0);texture.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,texture.EncodeToPNG());}
        finally{camera.targetTexture=old;canvas.renderMode=mode;canvas.worldCamera=uiCamera;canvas.planeDistance=distance;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);Canvas.ForceUpdateCanvases();}
        return path;
    }
    static RectTransform Element(string name,Transform parent,Vector2 size,Vector2 at)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.sizeDelta=size;rect.anchoredPosition=at;return rect;
    }
    static void MakePrefab()
    {
        if(!AssetDatabase.IsValidFolder("Assets/Interaction/Prefabs"))AssetDatabase.CreateFolder("Assets/Interaction","Prefabs");
        if(AssetDatabase.LoadAssetAtPath<GameObject>(UiPath)!=null)return;
        var root=new GameObject("Interaction UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(CanvasGroup),typeof(InteractionReticle));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=50;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=1;
        var group=root.GetComponent<CanvasGroup>();group.interactable=false;group.blocksRaycasts=false;
        var graphic=Element("Reticle",root.transform,new Vector2(20,20),Vector2.zero).gameObject.AddComponent<ReticleGraphic>();graphic.raycastTarget=false;graphic.color=new Color(1,1,1,.4f);
        var text=Element("Interaction Prompt",root.transform,new Vector2(320,22),new Vector2(0,-24)).gameObject.AddComponent<Text>();
        text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=14;text.fontStyle=FontStyle.Normal;text.alignment=TextAnchor.MiddleCenter;text.horizontalOverflow=HorizontalWrapMode.Overflow;text.verticalOverflow=VerticalWrapMode.Overflow;text.raycastTarget=false;text.text="";
        var view=root.GetComponent<InteractionReticle>();view.mark=graphic;view.prompt=text;view.visibility=group;
        PrefabUtility.SaveAsPrefabAsset(root,UiPath);UnityEngine.Object.DestroyImmediate(root);
    }
    static int BlankNotices()
    {
        const string path="Assets/Apartment/Materials/Blank Paper.mat";var paper=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(paper==null){paper=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(paper,path);}
        paper.SetColor("_BaseColor",new Color(.79f,.78f,.72f));paper.SetFloat("_Smoothness",.08f);
        int count=0;foreach(var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>new[]{"Building Notices","Building Plan","Calendar","Key Return"}.Contains(r.name)))
        {renderer.sharedMaterial=paper;PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);count++;}
        return count;
    }
    public static string Install()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode before installing.");
        Directory.CreateDirectory("References/Interaction");var initial=UnityEngine.SceneManagement.SceneManager.GetActiveScene();string original=initial.path;
        if(initial.isDirty)EditorSceneManager.SaveScene(initial);MakePrefab();var report=new System.Text.StringBuilder();
        foreach(var name in new[]{"Alley","Apartment"})
        {
            string path="Assets/Scenes/"+name+".unity";string backup="References/Interaction/"+name+".before.unity";if(!File.Exists(backup))File.Copy(path,backup);
            var scene=EditorSceneManager.OpenScene(path);var player=UnityEngine.Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();
            var scanner=player.GetComponent<InteractionScanner>();if(scanner==null)scanner=player.gameObject.AddComponent<InteractionScanner>();scanner.eyes=player.eyes;
            var views=player.GetComponentsInChildren<InteractionReticle>(true);InteractionReticle view;
            if(views.Length==0)view=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(UiPath),player.transform)).GetComponent<InteractionReticle>();
            else view=views[0];
            for(int i=1;i<views.Length;i++)UnityEngine.Object.DestroyImmediate(views[i].gameObject);
            view.scanner=scanner;PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            int notices=BlankNotices();EditorSceneManager.SaveScene(scene);report.AppendLine(name+": one scanner, one reticle; blank notice surfaces="+notices);
        }
        // The preview shares the lobby notice with the Apartment; keep its surface blank as well.
        string previewPath="Assets/Apartment/Prefabs/Entrance Interior.prefab";var preview=PrefabUtility.LoadPrefabContents(previewPath);
        try{foreach(var r in preview.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Building Notices"))r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Apartment/Materials/Blank Paper.mat");PrefabUtility.SaveAsPrefabAsset(preview,previewPath);}
        finally{PrefabUtility.UnloadPrefabContents(preview);}
        if(!string.IsNullOrEmpty(original))EditorSceneManager.OpenScene(original);AssetDatabase.SaveAssets();return report.ToString();
    }
}
