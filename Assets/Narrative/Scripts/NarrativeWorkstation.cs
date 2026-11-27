using UnityEngine;
public sealed class NarrativeWorkstation : MonoBehaviour,IInteractionTarget
{
    public string InteractionPrompt=>StoryDirector.Active!=null&&StoryDirector.Active.CurrentId=="begin"?"Begin Shift":StoryDirector.Active!=null&&StoryDirector.Active.CurrentId=="report"?"Submit Nightly Report":"Read Regulations";
    public float InteractionRange=>2.2f;
    public bool CanInteract(InteractionScanner source)=>isActiveAndEnabled&&StoryDirector.Active!=null&&!StoryDirector.Active.Opening;
    public void Interact(InteractionScanner source){if(!CanInteract(source))return;var director=StoryDirector.Active;if(director.CurrentId=="begin")StoryDirector.Signal("shift.begin");else if(director.CurrentId=="report")director.Presentation.Report();else director.Presentation.Rules();}
}
