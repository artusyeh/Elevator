public interface IInteractionTarget
{
    string InteractionPrompt { get; }
    float InteractionRange { get; }
    bool CanInteract(InteractionScanner source);
    void Interact(InteractionScanner source);
}
