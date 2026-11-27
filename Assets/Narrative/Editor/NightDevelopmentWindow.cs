using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public sealed class NightDevelopmentWindow : EditorWindow
{
    int night=1,index;
    string testMessage="Please check the entrance when you get a moment.";
    float battery=100;
    [MenuItem("Horror Game/Narrative/Story Development")]
    static void ShowWindow(){GetWindow<NightDevelopmentWindow>("Story Development");}
    void OnGUI()
    {
        EditorGUILayout.LabelField("Night authoring / development only",EditorStyles.boldLabel);night=EditorGUILayout.IntSlider("Night",night,1,7);var director=StoryDirector.Active;
        if(!Application.isPlaying){EditorGUILayout.HelpBox("Play Alley for the opening, or use Start Night 1 below. No later nights are authored.",MessageType.Info);if(GUILayout.Button("Start Night 1 from Alley")){EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/Alley.unity");EditorApplication.isPlaying=true;}return;}
        if(director==null){EditorGUILayout.HelpBox("Story runtime is not loaded.",MessageType.Warning);return;}
        EditorGUILayout.LabelField("Current",director.state==null?"Starting…":"Night "+director.state.night+" · "+director.CurrentId+" · "+director.ClockText);
        if(GUILayout.Button("Start / reset selected night"))director.StartNight(night);
        if(director.Opening&&GUILayout.Button("Skip opening (development)"))director.SkipOpening();
        if(director.definition==null||director.state==null)return;
        index=EditorGUILayout.Popup("Step",Mathf.Clamp(index,0,director.definition.beats.Length-1),director.definition.beats.Select(b=>b.id+" — "+b.objective).ToArray());
        if(GUILayout.Button("Jump to selected step"))director.JumpToBeat(index);
        if(GUILayout.Button("Complete current objective (TEST ONLY)"))CompleteTestStep();
        if(GUILayout.Button("Recall current guidance"))director.RecallObjective();
        EditorGUILayout.BeginHorizontal();if(GUILayout.Button("Clock +15 min"))director.AdvanceClock(15);if(GUILayout.Button("Clock +1 hour"))director.AdvanceClock(60);EditorGUILayout.EndHorizontal();
        testMessage=EditorGUILayout.TextField("Supervisor text",testMessage);if(GUILayout.Button("Send test message"))director.Send("Supervisor",testMessage);
        battery=EditorGUILayout.Slider("Battery %",battery,0,100);if(GUILayout.Button("Apply battery")){var phone=Object.FindFirstObjectByType<PhoneBattery>();if(phone!=null)phone.SetCharge(battery);}
        if(GUILayout.Button("Save checkpoint"))director.SaveCheckpoint();if(GUILayout.Button("Load checkpoint"))director.LoadCheckpoint();
        EditorGUILayout.Space();EditorGUILayout.LabelField("Night 1 encounters",EditorStyles.boldLabel);
        string[] labels={"1 · Guard's belongings","2 · CCTV interference","3 · Flickering light","4 · Door slam","5 · Silent call","6 · Empty elevator","7 · Distant voice"};string[] ids={"begin","cctv","patrol4","verify4","charge","sit","alley.trigger"};
        for(int i=0;i<labels.Length;i++)if(GUILayout.Button(labels[i]))director.JumpToBeat(System.Array.FindIndex(director.definition.beats,b=>b.id==ids[i]));
        if(GUILayout.Button("Replay silent call sequence"))director.TestSequence(Resources.Load<NarrativeSequence>("Narrative/Silent Call"));
        if(GUILayout.Button("Replay empty elevator sequence"))director.TestSequence(Resources.Load<NarrativeSequence>("Narrative/Empty Elevator"));
        if(GUILayout.Button("Test NPC waypoint movement"))SpawnTestActor();
    }
    public static void CompleteTestStep()
    {
        var director=StoryDirector.Active;if(director==null||!director.Running)return;var step=director.definition.beats[director.state.index];foreach(var condition in step.conditions.ToArray())director.Publish(condition.kind,condition.target,condition.value);foreach(var signal in step.waitForSignals.ToArray())director.Emit(signal);
    }
    public static void SpawnTestActor()
    {
        var player=Object.FindFirstObjectByType<AlleyStudy.AlleyWalker>();if(player==null)return;
        var prefab=Resources.Load<GameObject>("Narrative/Resident Placeholder");if(prefab==null)return;var root=Object.Instantiate(prefab);root.name="Development NPC (not a Night 1 character)";root.transform.position=player.transform.position+player.transform.forward*1.7f;root.GetComponentInChildren<NarrativeActor>().Begin();
    }
    void OnInspectorUpdate(){Repaint();}
}
