using UnityEngine;
public sealed class NarrativeAnalogClock : MonoBehaviour
{
    public Transform hourPivot,minutePivot;
    public Vector3 axis=Vector3.forward;
    void LateUpdate(){var director=StoryDirector.Active;if(director==null||director.state==null)return;var time=director.Clock;if(hourPivot!=null)hourPivot.localRotation=Quaternion.AngleAxis(-((time.Hour%12)*30+time.Minute*.5f),axis);if(minutePivot!=null)minutePivot.localRotation=Quaternion.AngleAxis(-time.Minute*6,axis);}
}
