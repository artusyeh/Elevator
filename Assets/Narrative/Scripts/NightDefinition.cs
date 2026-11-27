using System;
using UnityEngine;

public enum NarrativeActionKind { Message,Subtitle,SetTime,TaskStatus,MaintenanceStatus,Record,SetFlag,SetCircuit,LightingMood,DoorLock,DoorOpen,Elevator,CctvChannel,MoveActor,Sound,SetActive,LightIntensity,Weather,InvokeBinding,Sequence,Battery,AdvanceTime,Animation,RestrictPlayer,Scene,Fade,PhoneCall,CameraFault,LightFault,RingTelephone,SeatLock,ElevatorDoors,CloseFloorDoors }
[Serializable]public class NarrativeAction
{
    public NarrativeActionKind kind;
    public string target;
    public string completionId;
    [TextArea(2,6)]public string text;
    public float number;
    public bool value;
    public PowerCircuit circuit;
    public AudioClip audio;
    public NarrativeSequence sequence;
}
[Serializable]public class NarrativeBeat
{
    public string id,title;
    [TextArea]public string objective,guidance,supervisorMessage,protagonistDialogue;
    public string nextStepId;
    public StoryCondition[] conditions=new StoryCondition[0];
    public bool checkpointOnBegin=true,checkpointOnComplete=true;
    public NarrativeSequence scriptedSequence;
    public string scene="Apartment";
    public string spawnBinding;
    public string[] waitForSignals=new string[0];
    public bool freshSignals;
    public NarrativeAction[] enter=new NarrativeAction[0],exit=new NarrativeAction[0];
}
[Serializable]public class PhoneStoryMessage
{
    public string contact="Supervisor",time,body;
    public string id;
    public bool outgoing,read;
}
[CreateAssetMenu(menuName="Narrative/Night Definition")]
public sealed class NightDefinition : ScriptableObject
{
    [Range(1,7)]public int nightNumber=1;
    public string title="The New Hire";
    public string protagonist="Aki Mori";
    public string date="2026-11-01";
    public float startSeconds=21*3600+50*60;
    [Range(0,100)]public float startingBattery=100;
    [TextArea]public string[] openingLines;
    [Min(.1f)]public float openingLineSeconds=3.1f,fadeSeconds=1.4f;
    public PhoneStoryMessage[] history;
    public string approachSignal="approach.apartment";
    public PhoneStoryMessage[] approachMessages=new PhoneStoryMessage[0];
    public PhoneManagementData management;
    public NarrativeBeat[] beats;
}
