using UnityEngine;
[CreateAssetMenu(menuName="Phone/Prototype Content")]
public sealed class PhoneContent : ScriptableObject
{
    [System.Serializable]public class Contact {public string name,preview,time;public bool unread;[TextArea]public string incoming,outgoing;}
    public Contact[] contacts;
    public string[] tasks;
    [TextArea]public string[] regulations;
    [TextArea]public string[] maintenance;
    public BuildingDirectory directory;
}
