using UnityEngine;
[CreateAssetMenu(menuName="Narrative/Readable Document")]
public sealed class NarrativeDocument : ScriptableObject
{
    public string documentId,title;
    [TextArea(10,40)]public string body;
}
