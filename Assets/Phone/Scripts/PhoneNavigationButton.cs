using UnityEngine;
using UnityEngine.UI;
public sealed class PhoneNavigationButton : MonoBehaviour
{
    public enum Action { Navigate,Conversation,Back,Call,Flashlight,Task,SendMessage,SubmitReport,Dial,ClearDial,Record,BeginShift,ShowReport,CancelReport }
    public Action action;
    public PhoneApplications.Page page;
    public int index;
    public string argument;
    void Awake(){GetComponent<Button>().onClick.AddListener(Activate);}
    public void Activate(){var phone=GetComponentInParent<PhoneApplications>();if(phone!=null)phone.Activate(this);}
}
