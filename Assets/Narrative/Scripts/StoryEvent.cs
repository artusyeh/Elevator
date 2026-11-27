using System;
[Serializable]public enum StoryEventKind { Signal,AreaEntered,AreaLeft,Interaction,ItemCollected,ItemReturned,DoorUnlocked,CircuitChanged,PhonePageViewed,MessageRead,Sitting,Standing,ElevatorArrived,CctvViewed,DialogueCompleted,AnimationCompleted,SequenceCompleted,AudioCompleted }
[Serializable]public class StoryCondition
{
    public StoryEventKind kind;
    public string target;
    public string value;
    public bool checkCurrentStateOnEnter;
    public bool Matches(StoryEventKind eventKind,string eventTarget,string eventValue)=>kind==eventKind&&target==eventTarget&&(string.IsNullOrEmpty(value)||value==eventValue);
}
