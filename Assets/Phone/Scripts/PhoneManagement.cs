using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public sealed class PhoneManagement : MonoBehaviour
{
    public PhoneManagementData data;
    public Text[] taskStatuses,requestStatuses;
    public GameObject taskList,taskDetail;
    public Text detailTitle,detailBody;
    static readonly Dictionary<string,PhoneManagementData.TaskStatus> taskState=new Dictionary<string,PhoneManagementData.TaskStatus>();
    static readonly Dictionary<string,string> requestState=new Dictionary<string,string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetSession(){taskState.Clear();requestState.Clear();}
    public PhoneManagementData.TaskStatus GetTaskStatus(string id){PhoneManagementData.TaskStatus value;var story=StoryDirector.Active;if(story!=null&&story.state!=null&&System.Enum.TryParse(StoryDirector.Value(story.state.tasks,id,"Assigned"),out value))return value;if(taskState.TryGetValue(id,out value))return value;foreach(var task in data.assignments)if(task.id==id)return task.status;return PhoneManagementData.TaskStatus.Assigned;}
    public bool SetTaskStatus(string id,PhoneManagementData.TaskStatus value){foreach(var task in data.assignments)if(task.id==id){if(StoryDirector.Active!=null&&StoryDirector.Active.state!=null)StoryDirector.Active.SetTask(id,value);else taskState[id]=value;Refresh();return true;}return false;}
    public bool SetMaintenanceStatus(string reference,string status){foreach(var request in data.requests)if(request.reference==reference){if(StoryDirector.Active!=null&&StoryDirector.Active.state!=null)StoryDirector.Active.SetMaintenance(reference,status);else requestState[reference]=status;Refresh();return true;}return false;}
    public static string StatusLabel(PhoneManagementData.TaskStatus status)=>status==PhoneManagementData.TaskStatus.InProgress?"In Progress":status.ToString();
    public void Refresh()
    {
        var bridge=GetComponent<PhoneNarrativeBridge>();if(bridge!=null&&bridge.StoryActive){bridge.RefreshManagement();return;}
        if(data==null)return;
        for(int i=0;i<data.assignments.Length&&i<taskStatuses.Length;i++)taskStatuses[i].text=StatusLabel(GetTaskStatus(data.assignments[i].id));
        for(int i=0;i<data.requests.Length&&i<requestStatuses.Length;i++){string value;requestStatuses[i].text=requestState.TryGetValue(data.requests[i].reference,out value)?value:data.requests[i].status;}
    }
    public void ShowTask(int index)
    {
        if(index<0||index>=data.assignments.Length)return;var task=data.assignments[index];detailTitle.text=task.title;detailBody.text=task.location+"\n\n"+StatusLabel(GetTaskStatus(task.id))+"\n\n"+task.description;taskList.SetActive(false);taskDetail.SetActive(true);
    }
    public void ShowList(){if(taskList!=null)taskList.SetActive(true);if(taskDetail!=null)taskDetail.SetActive(false);Refresh();}
}
