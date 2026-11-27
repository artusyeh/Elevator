using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static class PhoneInstaller
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Start()
    {SceneManager.sceneLoaded-=OnLoaded;SceneManager.sceneLoaded+=OnLoaded;Install();}
    static void OnLoaded(Scene scene,LoadSceneMode mode){Install();}
    public static void Install()
    {
        foreach(var walker in Object.FindObjectsByType<AlleyStudy.AlleyWalker>(FindObjectsSortMode.None))
        {
            if(walker.eyes==null||walker.GetComponentInChildren<PhoneControls>(true)!=null)continue;
            var prefab=Resources.Load<GameObject>("Phone/Phone");if(prefab==null)continue;
            var phone=Object.Instantiate(prefab,walker.eyes.transform,false);phone.name="Player Phone";
            foreach(var canvas in phone.GetComponentsInChildren<Canvas>(true))canvas.worldCamera=walker.eyes;
        }
        if(Object.FindFirstObjectByType<EventSystem>()==null)
        {var events=new GameObject("Phone Event System",typeof(EventSystem),typeof(InputSystemUIInputModule));events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
    }
}
