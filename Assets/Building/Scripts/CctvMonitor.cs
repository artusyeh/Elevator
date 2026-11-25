using UnityEngine;

public sealed class CctvMonitor : MonoBehaviour,IInteractionTarget
{
    public string monitorId;
    public MeshRenderer screen;
    public PowerCircuit circuit;
    public CctvSystem system;
    public int initialChannel;
    public int ChannelIndex { get; private set; }
    public bool Powered=>isActiveAndEnabled&&BuildingPower.IsPowered(circuit);
    public string InteractionPrompt=>"Switch Camera";
    public float InteractionRange=>2;
    Material display;
    public bool CanInteract(InteractionScanner source)=>Powered&&system!=null;
    public void Interact(InteractionScanner source){if(CanInteract(source))system.NextChannel(this);}
    void Awake(){ChannelIndex=CctvSystem.ReadChannel(monitorId,initialChannel);display=new Material(screen.sharedMaterial);screen.sharedMaterial=display;}
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
