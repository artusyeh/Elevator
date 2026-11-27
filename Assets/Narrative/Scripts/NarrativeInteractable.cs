using UnityEngine;
public sealed class NarrativeInteractable : MonoBehaviour,IInteractionTarget
{
    public string prompt="Inspect",signal;
    [TextArea]public string observation;
    public NarrativeDocument document;
    public float range=2.2f;
    public string InteractionPrompt=>prompt;
    public float InteractionRange=>range;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&StoryDirector.Active!=null&&!StoryDirector.Active.Opening;
    public void Interact(InteractionScanner source)
    {
        if(!CanInteract(source))return;
        if(document!=null)StoryDirector.Active.Presentation.Read(document);else if(!string.IsNullOrEmpty(observation))StoryDirector.Active.Presentation.Say("Aki",observation,4);
        if(!string.IsNullOrEmpty(signal))StoryDirector.Signal(signal);
    }
}
