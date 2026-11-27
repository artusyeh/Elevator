using UnityEngine;

// Replace Model with final character art; authored waypoints and collision stay reusable.
[RequireComponent(typeof(CharacterController))]
public sealed class NarrativeActor : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed=.85f,turnSpeed=180;
    public string arrivedSignal="npc.resident.arrived";
    public GameObject model;
    public Animator animator;
    public int Waypoint {get;private set;}
    public bool Walking {get;private set;}
    CharacterController body;
    void Awake(){body=GetComponent<CharacterController>();}
    public void Begin(){Waypoint=0;Walking=waypoints!=null&&waypoints.Length>0;if(model!=null)model.SetActive(true);}
    public void Restore(Vector3 position,int waypoint,bool walking){if(body==null)body=GetComponent<CharacterController>();body.enabled=false;transform.position=position;body.enabled=true;Waypoint=waypoint;Walking=walking;if(model!=null)model.SetActive(walking||waypoint>0);}
    public void Tick(float seconds)
    {
        if(!Walking||body==null||Waypoint>=waypoints.Length)return;
        var delta=waypoints[Waypoint].position-transform.position;delta.y=0;
        if(delta.magnitude<.12f){Waypoint++;if(Waypoint>=waypoints.Length){Walking=false;StoryDirector.Signal(arrivedSignal);if(animator!=null)animator.SetFloat("Speed",0);}return;}
        transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(delta),turnSpeed*seconds);
        body.Move(delta.normalized*Mathf.Min(delta.magnitude,speed*seconds)+Vector3.down*2*seconds);
        if(animator!=null)animator.SetFloat("Speed",speed);
    }
    void Update(){Tick(Time.deltaTime);}
}
