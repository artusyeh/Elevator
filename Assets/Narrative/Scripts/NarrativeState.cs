using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]public class NarrativeValue{public string id,value;}
[Serializable]public class NarrativeWorldValue{public string binding;public NarrativeActionKind kind;public string text;public float number;public bool value;}
[Serializable]public class NarrativeDoorState{public string binding;public bool locked,open;}
[Serializable]public class NarrativeActorState{public string binding;public Vector3 position;public int waypoint;public bool walking;}
[Serializable]public class NightState
{
    public int version=1,night=1,index;
    public float seconds,beatSeconds,battery=100;
    public bool completed,mainPower=true;
    public int elevatorFloor=1;
    public bool playerInElevator;
    public Vector3 playerCabinLocal;
    public string scene="Alley",report="",dockId;
    public Vector3 playerPosition,playerRotation,eyeRotation;
    public List<string> signals=new List<string>(),keys=new List<string>();
    public List<string> stepSignals=new List<string>(),completedSteps=new List<string>();
    public List<NarrativeValue> tasks=new List<NarrativeValue>(),requests=new List<NarrativeValue>(),circuits=new List<NarrativeValue>(),cctv=new List<NarrativeValue>();
    public List<PhoneStoryMessage> messages=new List<PhoneStoryMessage>();
    public List<PhoneManagementData.Record> records=new List<PhoneManagementData.Record>();
    public List<NarrativeWorldValue> world=new List<NarrativeWorldValue>();
    public List<NarrativeDoorState> doors=new List<NarrativeDoorState>();
    public List<NarrativeActorState> actors=new List<NarrativeActorState>();
}
