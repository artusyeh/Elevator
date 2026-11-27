using UnityEngine;
public sealed class NarrativeTelephone : MonoBehaviour,IInteractionTarget
{
    public AudioSource ringSource;
    public NarrativeSequence conversation;
    public bool Ringing {get;private set;}
    public string InteractionPrompt=>Ringing?"Answer Telephone":"Inspect Telephone";
    public float InteractionRange=>2.2f;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&StoryDirector.Active!=null&&!StoryDirector.Active.Opening;
    public void Ring(){if(Ringing)return;Ringing=true;if(ringSource!=null)ringSource.Play();}
    public void Interact(InteractionScanner source)
    {
        if(!CanInteract(source))return;
        if(!Ringing){StoryDirector.Active.Presentation.Say("Aki","The line is quiet.",2.5f);return;}
        Ringing=false;if(ringSource!=null)ringSource.Stop();StoryDirector.Signal("telephone.answered");StoryDirector.Active.TestSequence(conversation);
    }
}
