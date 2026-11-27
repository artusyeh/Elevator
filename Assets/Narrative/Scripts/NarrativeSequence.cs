using System;
using UnityEngine;
[CreateAssetMenu(menuName="Narrative/Timed Sequence")]
public sealed class NarrativeSequence : ScriptableObject
{
    public string sequenceId;
    [Serializable]public class Cue{[Min(0)]public float delay;public NarrativeAction[] actions;public bool waitForCompletion;public StoryCondition completion;[Min(1)]public float timeout=30;}
    public Cue[] cues;
}
