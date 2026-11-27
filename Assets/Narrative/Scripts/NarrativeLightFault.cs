using System.Collections.Generic;
using UnityEngine;
public sealed class NarrativeLightFault : MonoBehaviour
{
    public Light fixture;
    public Renderer[] emitters;
    public PowerCircuit circuit;
    public AudioSource buzz;
    public float approachDistance=2.4f,briefBlackout=.7f;
    public float[] flickerDurations={.12f,.31f,.08f,.6f,.17f,.45f,.09f,.8f};
    public float[] levels={.8f,.35f,.9f,.6f,.2f,.85f,.4f,.75f};
    public bool Active {get;private set;}
    float intensity,remaining,blackout;
    int pattern;
    bool approached;
    readonly List<MaterialPropertyBlock> originals=new List<MaterialPropertyBlock>();
    public void SetFault(bool value)
    {
        if(value==Active)return;Active=value;
        if(value){intensity=fixture.intensity;originals.Clear();foreach(var renderer in emitters){var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);originals.Add(block);}pattern=0;remaining=0;approached=false;if(buzz!=null)buzz.Play();}
        else{fixture.intensity=BuildingPower.IsPowered(circuit)?intensity:0;for(int i=0;i<emitters.Length;i++)emitters[i].SetPropertyBlock(originals[i]);if(buzz!=null)buzz.Stop();}
    }
    public void Tick(float dt,Vector3 playerEye)
    {
        if(!Active||fixture==null)return;
        if(!approached&&StoryDirector.Active!=null&&StoryDirector.Active.CurrentId=="patrol4"&&Vector3.Distance(playerEye,transform.position)<=approachDistance){approached=true;blackout=briefBlackout;StoryDirector.Signal("light.fault.approached");}
        blackout=Mathf.Max(0,blackout-dt);remaining-=dt;if(remaining<=0){pattern=(pattern+1)%flickerDurations.Length;remaining=flickerDurations[pattern];}
        float level=blackout>0||!BuildingPower.IsPowered(circuit)?0:levels[pattern%levels.Length];fixture.intensity=intensity*level;
        for(int i=0;i<emitters.Length;i++){var renderer=emitters[i];var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);var material=renderer.sharedMaterial;if(material!=null&&material.HasProperty("_EmissionColor"))block.SetColor("_EmissionColor",material.GetColor("_EmissionColor")*level);renderer.SetPropertyBlock(block);}
    }
    void Update(){var player=InteractionScanner.Active;if(player!=null)Tick(Time.deltaTime,player.eyes.transform.position);}
    void OnDisable(){if(Active)SetFault(false);}
}
