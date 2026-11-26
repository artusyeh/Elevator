using System;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName="Building/Lighting Mood")]
public sealed class LightingMoodPreset : ScriptableObject
{
    [Serializable] public struct Lamp
    {
        public float intensity,range,temperature,spotAngle,innerSpotAngle,bias,normalBias;
        public Color color;
        public bool useTemperature;
        public LightShadows shadows;
        public static Lamp Capture(Light source)=>new Lamp{intensity=source.intensity,range=source.range,temperature=source.colorTemperature,spotAngle=source.spotAngle,innerSpotAngle=source.innerSpotAngle,bias=source.shadowBias,normalBias=source.shadowNormalBias,color=source.color,useTemperature=source.useColorTemperature,shadows=source.shadows};
        public void Apply(Light target)
        {
            // Never change enabled: BuildingPower/LightCircuit own the electrical state.
            target.intensity=intensity;target.range=range;target.color=color;target.useColorTemperature=useTemperature;target.colorTemperature=temperature;target.spotAngle=spotAngle;target.innerSpotAngle=innerSpotAngle;target.shadows=shadows;target.shadowBias=bias;target.shadowNormalBias=normalBias;
        }
    }
    [Serializable] public struct Cover {public Material[] materials;}
    [Serializable] public struct Surface
    {
        public ReceiveGI receiveGI;
        public LightProbeUsage probes;
        public int lightmapIndex;
        public Vector4 lightmapScaleOffset;
        public static Surface Capture(MeshRenderer source)=>new Surface{receiveGI=source.receiveGI,probes=source.lightProbeUsage,lightmapIndex=source.lightmapIndex,lightmapScaleOffset=source.lightmapScaleOffset};
        public void Apply(MeshRenderer target){target.receiveGI=receiveGI;target.lightProbeUsage=probes;target.lightmapIndex=lightmapIndex;target.lightmapScaleOffset=lightmapScaleOffset;}
    }
    public VolumeProfile postProcessing;
    public AmbientMode ambientMode=AmbientMode.Trilight;
    public Color ambientSky,ambientEquator,ambientGround;
    public float ambientIntensity=1,reflectionIntensity=.65f;
    public Lamp[] lamps=Array.Empty<Lamp>();
    public Cover[] covers=Array.Empty<Cover>();
    public Surface[] surfaces=Array.Empty<Surface>();
}
