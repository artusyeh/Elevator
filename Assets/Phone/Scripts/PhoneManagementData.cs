using System;
using UnityEngine;
[CreateAssetMenu(menuName="Phone/Management Data")]
public sealed class PhoneManagementData : ScriptableObject
{
    public enum TaskStatus { Assigned,InProgress,Completed }
    [Serializable]public class Assignment{public string id,time,title,location;[TextArea]public string description;public TaskStatus status;}
    [Serializable]public class Request{public string reference,time,location,title,status;[TextArea]public string description;}
    [Serializable]public class Record{public string time,title,location;[TextArea]public string description;}
    public Assignment[] assignments;
    public Request[] requests;
    public Record[] records;
}
