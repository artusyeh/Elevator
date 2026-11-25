using System.Collections.Generic;
using UnityEngine;

public sealed class LightCircuit : MonoBehaviour
{
    public PowerCircuit circuit;
    public Light[] lights;
    public Renderer[] emitters;
    struct Surface{public Renderer renderer;public int slot;public Color emission,color;}
    readonly List<Surface> surfaces=new List<Surface>();
    bool[] original;
    void OnEnable()
    {
        if(!Application.isPlaying)return;
        if(original==null)
        {
            original=new bool[lights.Length];for(int i=0;i<lights.Length;i++)original[i]=lights[i]!=null&&lights[i].enabled;
            foreach(var r in emitters)if(r!=null)for(int i=0;i<r.sharedMaterials.Length;i++)
            {var m=r.sharedMaterials[i];if(m!=null&&m.HasProperty("_EmissionColor")&&m.GetColor("_EmissionColor").maxColorComponent>.001f)surfaces.Add(new Surface{renderer=r,slot=i,emission=m.GetColor("_EmissionColor"),color=m.HasProperty("_BaseColor")?m.GetColor("_BaseColor"):Color.white});}
        }
        BuildingPower.Changed+=Apply;Apply();
    }
    void OnDisable(){BuildingPower.Changed-=Apply;}
    public void Apply()
    {
        if(original==null)return;bool on=BuildingPower.IsPowered(circuit);
        for(int i=0;i<lights.Length;i++)if(lights[i]!=null)lights[i].enabled=on&&original[i];
        var block=new MaterialPropertyBlock();foreach(var surface in surfaces)if(surface.renderer!=null)
        {surface.renderer.GetPropertyBlock(block,surface.slot);block.SetColor("_EmissionColor",on?surface.emission:Color.black);block.SetColor("_BaseColor",on?surface.color:surface.color*.28f);surface.renderer.SetPropertyBlock(block,surface.slot);block.Clear();}
    }
}
