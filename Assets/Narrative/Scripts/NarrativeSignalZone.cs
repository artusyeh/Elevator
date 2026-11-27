using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public sealed class NarrativeSignalZone : MonoBehaviour
{
    public string signal;
    public bool emitWhileInside;
    bool inside;
    public void Check(Transform player)
    {
        if(player==null)return;var box=GetComponent<BoxCollider>();var p=transform.InverseTransformPoint(player.position)-box.center;var h=box.size*.5f;
        bool now=Mathf.Abs(p.x)<=h.x&&Mathf.Abs(p.y)<=h.y&&Mathf.Abs(p.z)<=h.z;
        if(now&&(!inside||emitWhileInside)){StoryDirector.Signal(signal);if(StoryDirector.Active!=null)StoryDirector.Active.Publish(StoryEventKind.AreaEntered,signal);}
        if(inside&&!now&&StoryDirector.Active!=null)StoryDirector.Active.Publish(StoryEventKind.AreaLeft,signal);inside=now;
    }
    void Update(){var scanner=InteractionScanner.Active;if(scanner!=null)Check(scanner.transform);}
}
