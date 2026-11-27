using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class PlaytestRoomAccess : MonoBehaviour
{
    public bool enableShortcut=true;
    int lastToggleFrame=-1;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Initialize()
    {SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;Attach();}
    static void Loaded(Scene scene,LoadSceneMode mode){Attach();}
    static void Attach(){foreach(var player in Object.FindObjectsByType<AlleyStudy.AlleyWalker>(FindObjectsSortMode.None))if(player.GetComponent<PlaytestRoomAccess>()==null)player.gameObject.AddComponent<PlaytestRoomAccess>();}
    void Update()
    {
        var keys=Keyboard.current;if(!enableShortcut||keys==null)return;
        var scan=GetComponent<InteractionScanner>();if(scan!=null&&(scan.phoneOpen||scan.menuOpen||scan.documentOpen||scan.cutscenePlaying))return;
        if(keys.digit1Key.wasPressedThisFrame&&lastToggleFrame!=Time.frameCount){lastToggleFrame=Time.frameCount;BuildingKeys.TogglePlaytestAccess();}
    }
}
