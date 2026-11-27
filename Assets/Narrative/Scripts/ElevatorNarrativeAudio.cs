using UnityEngine;
public sealed class ElevatorNarrativeAudio : MonoBehaviour
{
    public ElevatorController elevator;
    public AudioSource motor,sliding,bell;
    float previousDoor;
    bool moving,doorsMoving,wasOpened,wasClosed;
    public void Tick()
    {
        if(elevator==null)return;
        if(elevator.Moving&&!moving){if(motor!=null)motor.Play();}
        if(!elevator.Moving&&moving){if(motor!=null)motor.Stop();if(bell!=null)bell.Play();}
        moving=elevator.Moving;bool changing=Mathf.Abs(previousDoor-elevator.DoorAmount)>.0001f;
        if(changing&&!doorsMoving&&sliding!=null)sliding.Play();if(!changing&&doorsMoving&&sliding!=null)sliding.Stop();doorsMoving=changing;previousDoor=elevator.DoorAmount;
        bool opened=elevator.DoorAmount>.999f,closed=elevator.DoorAmount<.001f;
        if(opened&&!wasOpened)StoryDirector.Signal("elevator.opened");if(closed&&!wasClosed)StoryDirector.Signal("elevator.closed");wasOpened=opened;wasClosed=closed;
    }
    void Update(){Tick();}
}
