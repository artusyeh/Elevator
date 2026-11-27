using UnityEngine;

public sealed class CctvMonitor : MonoBehaviour,IInteractionTarget
{
    public string monitorId;
    public MeshRenderer screen;
    public PowerCircuit circuit;
    public CctvSystem system;
    public int initialChannel;
    public int ChannelIndex { get; private set; }
    public bool Powered=>isActiveAndEnabled&&screen!=null&&BuildingPower.IsPowered(circuit);
    public string InteractionPrompt=>"Switch Camera";
    public float InteractionRange=>2;
    Material display;
    Vector3 localFaceNormal=Vector3.forward;
    public Vector3 FaceNormal=>screen!=null?screen.transform.TransformDirection(localFaceNormal):Vector3.forward;
    public bool CanInteract(InteractionScanner source)=>Powered&&system!=null;
    public void Interact(InteractionScanner source){if(CanInteract(source))system.NextChannel(this);}
    void Awake(){ChannelIndex=CctvSystem.ReadChannel(monitorId,initialChannel);if(screen==null||screen.sharedMaterial==null)return;display=new Material(screen.sharedMaterial);screen.sharedMaterial=display;var mesh=screen.GetComponent<MeshFilter>();if(mesh!=null&&mesh.sharedMesh!=null&&mesh.sharedMesh.isReadable){var normals=mesh.sharedMesh.normals;if(normals.Length>0)localFaceNormal=normals[0];}}
    public void Select(int index){ChannelIndex=index;CctvSystem.RememberChannel(monitorId,index);}
    public void Refresh()
    {
        if(display==null||system==null)return;var channel=system.channels[Mathf.Clamp(ChannelIndex,0,system.channels.Length-1)];
        display.SetFloat("_Power",Powered?1:0);display.SetFloat("_Signal",channel.Powered&&channel.Texture!=null?1:0);
        display.SetTexture("_MainTex",channel.Texture);display.SetTexture("_LabelTex",channel.label);
        var clock=System.DateTime.UtcNow.AddHours(9);display.SetVector("_Clock",new Vector4(clock.Hour,clock.Minute,clock.Second,0));
    }
    void OnDestroy(){if(display!=null)Destroy(display);}
}
