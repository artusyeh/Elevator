using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-200)]
public sealed class LightingMoodController : MonoBehaviour
{
    public enum Mood { Normal,Horror }
    public Mood startingMood=Mood.Normal;
    public bool enableKeyboardToggle=true;
    public Key toggleKey=Key.L;
    public LightingMoodPreset normal,horror;
    public Volume interiorVolume;
    public Light[] lamps;
    public Renderer[] covers;
    public MeshRenderer[] surfaces;
    public LightCircuit[] circuits;
    public Mood CurrentMood {get;private set;}
    static bool hasSessionMood;
    static Mood sessionMood;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession(){hasSessionMood=false;sessionMood=Mood.Normal;}
    void OnEnable(){SetMood(Application.isPlaying&&hasSessionMood?sessionMood:startingMood);}
    void Update()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        return;
#else
        var keyboard=Keyboard.current;var scanner=InteractionScanner.Active;
        if(!enableKeyboardToggle||keyboard==null||!keyboard[toggleKey].wasPressedThisFrame||scanner==null||!scanner.UiVisible)return;
        ToggleMood();
#endif
    }
    public void ToggleMood()=>SetMood(CurrentMood==Mood.Normal?Mood.Horror:Mood.Normal);
    public void SetHorror(bool value)=>SetMood(value?Mood.Horror:Mood.Normal);
    public void SetMood(Mood mood)
    {
        var preset=mood==Mood.Normal?normal:horror;if(preset==null)return;
        for(int i=0;i<lamps.Length&&i<preset.lamps.Length;i++)if(lamps[i]!=null)preset.lamps[i].Apply(lamps[i]);
        for(int i=0;i<covers.Length&&i<preset.covers.Length;i++)if(covers[i]!=null)covers[i].sharedMaterials=preset.covers[i].materials;
        for(int i=0;i<surfaces.Length&&i<preset.surfaces.Length;i++)if(surfaces[i]!=null)preset.surfaces[i].Apply(surfaces[i]);
        RenderSettings.ambientMode=preset.ambientMode;RenderSettings.ambientSkyColor=preset.ambientSky;RenderSettings.ambientEquatorColor=preset.ambientEquator;RenderSettings.ambientGroundColor=preset.ambientGround;RenderSettings.ambientIntensity=preset.ambientIntensity;RenderSettings.reflectionIntensity=preset.reflectionIntensity;
        if(interiorVolume!=null)interiorVolume.sharedProfile=preset.postProcessing;
        // Re-read the new covers' emission values, then reapply the existing breakers.
        if(Application.isPlaying&&circuits!=null)foreach(var circuit in circuits)if(circuit!=null)circuit.RefreshEmissionSurfaces();
        CurrentMood=mood;if(Application.isPlaying){sessionMood=mood;hasSessionMood=true;}
    }
    [ContextMenu("Preview Normal Lighting")] public void PreviewNormal()=>SetMood(Mood.Normal);
    [ContextMenu("Preview Horror Lighting")] public void PreviewHorror()=>SetMood(Mood.Horror);
}
