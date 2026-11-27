using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class SceneEntrance : MonoBehaviour, IInteractionTarget
{
    public string destination;
    public Vector3 arrivalPosition;
    public Vector3 arrivalRotation;
    public string prompt="Enter";
    public string interactionVerb="Open";
    [Min(.1f)] public float interactionRange=2.7f;
    public string InteractionPrompt=>interactionVerb;
    public float InteractionRange=>interactionRange;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&!string.IsNullOrEmpty(destination)&&usedFrame!=Time.frameCount;
    public void Interact(InteractionScanner source){if(CanInteract(source)&&usedFrame!=Time.frameCount)Travel();}
    public bool preservePose;
    public Vector3 sceneOffset;
    public float doorwaySide=1;
    static int usedFrame=-1;
    bool near;
    void Update()
    {
        if(InteractionScanner.Active!=null)return;
        var camera=Camera.main;if(camera==null)return;
        var direction=transform.position-camera.transform.position;
        near=direction.magnitude<2.7f&&Vector3.Dot(camera.transform.forward,direction.normalized)>.65f;
        if(!near||Keyboard.current==null||!Keyboard.current.eKey.wasPressedThisFrame||usedFrame==Time.frameCount)return;
        Travel();
    }
    public void Travel()
    {
        if(StoryDirector.Active!=null&&!StoryDirector.Active.CanTravel(destination))return;
        if(StoryDirector.Active!=null)StoryDirector.Active.SaveCheckpoint();
        usedFrame=Time.frameCount;
        var position=arrivalPosition;var rotation=arrivalRotation;var eyeRotation=Quaternion.identity;
        var current=Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();
        if(preservePose&&current!=null){position=current.transform.position+sceneOffset;rotation=current.transform.eulerAngles;eyeRotation=current.eyes.transform.localRotation;position.z=transform.position.z+sceneOffset.z+doorwaySide*.48f;}
        SceneManager.sceneLoaded+=Arrive;
        void Arrive(Scene scene,LoadSceneMode mode)
        {
            SceneManager.sceneLoaded-=Arrive;
            var player=Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();if(player==null)return;
            var controller=player.GetComponent<CharacterController>();controller.enabled=false;
            player.transform.SetPositionAndRotation(position,Quaternion.Euler(rotation));controller.enabled=true;
            if(preservePose){player.eyes.transform.localRotation=eyeRotation;player.SyncLookPitch();}
        }
        SceneManager.LoadScene(destination);
    }
}
