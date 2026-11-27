using UnityEngine;

public sealed class ElevatorController : MonoBehaviour
{
    public Transform cabin;
    public Transform[] landings,leftDoors,rightDoors;
    public int[] landingFloors;
    public BasementAccess basementAccess;
    public Transform cabinLeft,cabinRight;
    public ElevatorFloorDisplay[] displays;
    public int currentFloor=1;
    public static event System.Action<int> Arrived;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetArrivalEvents(){Arrived=null;}
    public float metresPerSecond=1.45f;
    public bool vibration=true;
    public PowerCircuit powerCircuit;
    [Range(1,6)] public int emergencyFloor=1;
    public bool HasPower=>BuildingPower.IsPowered(powerCircuit);
    public bool Moving { get; private set; }
    public bool CanOperateDoors=>!Moving&&settle<=0;
    public int Direction { get; private set; }
    public int TargetFloor { get; private set; }=1;
    public int DisplayedFloor { get; private set; }=1;
    public float DoorAmount { get; private set; }=1;
    readonly bool[] requests=new bool[7];
    bool wantOpen=true;
    float baseHeight,startHeight,travelTime,elapsed,settle,dwell;
    CharacterController rider;
    Transform eyes;
    Vector3 eyeRest;
    bool shook;
    float shakeClock;
    bool previousPower=true;
    AlleyStudy.AlleyWalker cachedPlayer;
    AlleyStudy.AlleyWalker Player=>cachedPlayer!=null?cachedPlayer:(cachedPlayer=Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>());
    void Awake(){baseHeight=cabin.position.y-(currentFloor-1)*2.9f;TargetFloor=currentFloor;RefreshDisplay();}
    public bool IsRequested(int floor){return floor>=0&&floor<=6&&requests[floor];}
    public void RequestFloor(int floor)
    {
        if(floor<0||floor>6||!HasPower||floor==0&&(basementAccess==null||!basementAccess.CanEnter))return;
        if(!Moving&&floor==currentFloor){wantOpen=true;return;}
        requests[floor]=true;wantOpen=false;
    }
    public void OpenDoors(){if(!Moving&&settle<=0){wantOpen=true;dwell=2;}}
    public void CloseDoors(){if(!Moving&&settle<=0&&HasPower){wantOpen=false;dwell=0;}}
    public void RestoreAtFloor(int floor)
    {
        RemoveShake();rider=null;Moving=false;Direction=0;currentFloor=Mathf.Clamp(floor,0,6);TargetFloor=DisplayedFloor=currentFloor;System.Array.Clear(requests,0,requests.Length);settle=dwell=0;DoorAmount=1;wantOpen=true;
        var position=cabin.position;position.y=baseHeight+(currentFloor-1)*2.9f;cabin.position=position;ApplyDoors();RefreshDisplay();Physics.SyncTransforms();
    }
    void Update()
    {
        RemoveShake();
        Tick(Time.deltaTime);
    }
    bool IsInside(Transform player)
    {var p=cabin.InverseTransformPoint(player.position);return Mathf.Abs(p.x)<.73f&&p.z>.10f&&p.z<1.60f&&p.y>-.2f&&p.y<2.25f;}
    bool DoorwayOccupied()
    {
        var player=Player;
        if(player==null)return false;
        var p=cabin.InverseTransformPoint(player.transform.position);
        return Mathf.Abs(p.x)<.82f&&Mathf.Abs(p.z-.075f)<.38f&&p.y>-.2f&&p.y<.4f;
    }
    public void Tick(float dt)
    {
        dt=Mathf.Clamp(dt,0,.1f);shakeClock+=dt;
        bool power=HasPower;
        if(!power&&previousPower)
        {
            System.Array.Clear(requests,0,requests.Length);
            if(Moving)
            {
                // Ground-floor rescue keeps the maintenance panel reachable while stairs are locked.
                TargetFloor=Mathf.Clamp(emergencyFloor,1,6);
                startHeight=cabin.position.y;float destination=baseHeight+(TargetFloor-1)*2.9f;
                Direction=destination>startHeight?1:-1;elapsed=0;travelTime=Mathf.Max(2.5f,Mathf.Abs(destination-startHeight)/.6f);
            }
            else{wantOpen=true;dwell=2;}
        }
        previousPower=power;
        if(!power&&!Moving)wantOpen=true;
        if(!Moving)
        {
            if(settle>0){settle-=dt;if(settle<=0)wantOpen=true;}
            bool pending=false;foreach(bool request in requests)pending|=request;
            if(settle<=0)dwell=Mathf.Max(0,dwell-dt);
            if(pending&&settle<=0&&dwell<=0)wantOpen=false;
            if(!wantOpen&&DoorwayOccupied()){wantOpen=true;dwell=1;}
            DoorAmount=Mathf.MoveTowards(DoorAmount,wantOpen?1:0,dt*.8f);ApplyDoors();
            if(pending&&DoorAmount<=.0001f&&settle<=0&&dwell<=0)
            {
                int best=-1;for(int i=0;i<requests.Length;i++)if(requests[i]&&(best<0||Mathf.Abs(i-currentFloor)<Mathf.Abs(best-currentFloor)))best=i;
                TargetFloor=best;Direction=TargetFloor>currentFloor?1:-1;startHeight=cabin.position.y;
                travelTime=Mathf.Max(2.5f,Mathf.Abs((TargetFloor-1)*2.9f+baseHeight-startHeight)/metresPerSecond);elapsed=0;Moving=true;
                var player=Player;rider=player!=null&&IsInside(player.transform)?player.GetComponent<CharacterController>():null;
                if(rider!=null){eyes=player.eyes.transform;eyeRest=eyes.localPosition;}
            }
        }
        else
        {
            elapsed+=dt;float t=Mathf.Clamp01(elapsed/travelTime);float eased=t*t*(3-2*t);float old=cabin.position.y;
            var p=cabin.position;p.y=Mathf.Lerp(startHeight,baseHeight+(TargetFloor-1)*2.9f,eased);cabin.position=p;
            Physics.SyncTransforms();if(rider!=null&&rider.enabled)rider.Move(Vector3.up*(p.y-old));
            DisplayedFloor=Mathf.Clamp(Mathf.RoundToInt((p.y-baseHeight)/2.9f)+1,0,6);
            if(t>=1){currentFloor=TargetFloor;DisplayedFloor=currentFloor;requests[currentFloor]=false;Moving=false;Direction=0;settle=.35f;dwell=2.3f;RemoveShake();rider=null;Arrived?.Invoke(currentFloor);}
        }
        RefreshDisplay();
    }
    void ApplyDoors()
    {
        float offset=Mathf.Lerp(.275f,.96f,DoorAmount);
        for(int i=0;i<leftDoors.Length;i++){int floor=landingFloors!=null&&i<landingFloors.Length?landingFloors[i]:i+1;float v=floor==currentFloor&&!Moving?offset:.275f;var a=leftDoors[i].localPosition;var b=rightDoors[i].localPosition;a.x=-v;b.x=v;leftDoors[i].localPosition=a;rightDoors[i].localPosition=b;}
        var l=cabinLeft.localPosition;var r=cabinRight.localPosition;l.x=-offset;r.x=offset;cabinLeft.localPosition=l;cabinRight.localPosition=r;
    }
    void RefreshDisplay(){if(displays!=null)foreach(var display in displays)if(display!=null)display.Show(DisplayedFloor,Direction);}
    void LateUpdate()
    {
        if(!Moving||!vibration||rider==null||eyes==null)return;
        float envelope=Mathf.Clamp01(elapsed/.5f)*Mathf.Clamp01((travelTime-elapsed)/.5f);
        var offset=new Vector3(Mathf.Sin(shakeClock*39)*.0035f+Mathf.Sin(shakeClock*63)*.0015f,Mathf.Sin(shakeClock*47)*.0025f,0)*envelope;
        eyes.localPosition=eyeRest+offset;shook=true;
    }
    void RemoveShake(){if(shook&&eyes!=null)eyes.localPosition=eyeRest;shook=false;}
    void OnDisable(){RemoveShake();}
}


