using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Communicative))]
public class CommunicativeEditor : Editor
{
    private SerializedProperty emotionStateProp;
    private SerializedProperty personalityProp;
    private SerializedProperty memorySizeProp;
    private SerializedProperty shortTermMemoryProp;
    private SerializedProperty decisionStateMachineFileProp;
    private SerializedProperty sourceProp;

    private bool showEmotionState = true;
    private bool showPersonalityState = true;
    private bool showShortTermMemoryState = true;

    private PersonalityEditor personalityEditor;

    private void OnEnable()
    {
        emotionStateProp = serializedObject.FindProperty("emotionState");
        personalityProp = serializedObject.FindProperty("personality");
        memorySizeProp = serializedObject.FindProperty("memorySize");
        shortTermMemoryProp = serializedObject.FindProperty("shortTermMemory");
        decisionStateMachineFileProp = serializedObject.FindProperty("decisionStateMachineFile");
        sourceProp = serializedObject.FindProperty("source");

        personalityEditor = Editor.CreateEditor(personalityProp.objectReferenceValue) as PersonalityEditor;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        showEmotionState = EditorGUILayout.Foldout(showEmotionState, "Emotion State");

        if (showEmotionState)
        {
            DrawEmotionStateGUI();
        }

        EditorGUILayout.Space();
        showPersonalityState = EditorGUILayout.Foldout(showPersonalityState, "Personality");
        if (showPersonalityState)
        {
            DrawPersonalityGUI();
        }

        EditorGUILayout.Space();
        showShortTermMemoryState = EditorGUILayout.Foldout(showShortTermMemoryState, "Short Term Memory");
        if (showPersonalityState)
        {
            EditorGUILayout.PropertyField(memorySizeProp);
            EditorGUILayout.PropertyField(shortTermMemoryProp, true);
        }

        EditorGUILayout.PropertyField(decisionStateMachineFileProp);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(sourceProp);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEmotionStateGUI()
    {
        Communicative communicative = (Communicative)target;

        if (!EditorApplication.isPlaying)
        {
            serializedObject.Update();

            SerializedProperty emotionStateDataProp = serializedObject.FindProperty("emotionStateData");
            EditorGUILayout.PropertyField(emotionStateDataProp);

            if (emotionStateDataProp.objectReferenceValue == null)
            {
                if (GUILayout.Button("Create Emotion State Data"))
                {
                    string assetPath = EditorUtility.SaveFilePanelInProject("Save Emotion State Data", "New Emotion State Data", "asset", "Specify a file name to save the emotion state data");
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        EmotionStateData emotionStateData = ScriptableObject.CreateInstance<EmotionStateData>();
                        AssetDatabase.CreateAsset(emotionStateData, assetPath);
                        emotionStateDataProp.objectReferenceValue = emotionStateData;

                        // Assign the newly created emotionStateData to the communicative.emotionStateData field
                        communicative.emotionStateData = emotionStateData;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            EmotionState emotionState;

            if (emotionStateProp.objectReferenceValue == null)
            {
                emotionState = new EmotionState();
            }
            else
            {
                emotionState = emotionStateProp.objectReferenceValue as EmotionState;
            }

            EditorGUI.indentLevel++;

            // Create a horizontal layout to display the emotions in order of intensity
            {
                EditorGUILayout.LabelField("Ordered Emotions", EditorStyles.boldLabel);

                string emotionList = "";

                foreach (EmotionState.Emotion emotion in emotionState.orderedEmotions)
                {
                    emotionList += $"{emotion.Name}({emotion.Intensity}),";
                }

                EditorGUILayout.LabelField(emotionList.TrimEnd(','));
            }

            EditorGUILayout.Space();
            foreach (string emotionName in emotionState.allEmotions)
            {
                EmotionState.Emotion emotion = emotionState[emotionName];

                int initial = emotion.Intensity;
                //Debug.Log(initial);
                //Debug.Log(emotionName);

                EditorGUILayout.LabelField(emotionName);
                emotion.Intensity = EditorGUILayout.IntSlider(emotion.Intensity, 0, 100);

                emotionState[emotionName] = emotion;
            }

            EditorGUI.indentLevel--;

            emotionStateProp.objectReferenceValue = emotionState;
        }
    }

    private void DrawPersonalityGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Personality", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(personalityProp);
        if (EditorGUI.EndChangeCheck())
        {
            if (personalityProp.objectReferenceValue != null)
            {
                personalityEditor = Editor.CreateEditor(personalityProp.objectReferenceValue) as PersonalityEditor;
            }
            else
            {
                personalityEditor = null;
            }
        }

        if (personalityEditor != null)
        {
            personalityEditor.OnInspectorGUI();
        }

        serializedObject.ApplyModifiedProperties();
    }
}

public class Communicative : MonoBehaviour
{
    [SerializeField]
    public EmotionStateData emotionStateData;
    [SerializeField]
    private EmotionState emotionState;
    [SerializeField]
    public Personality personality;

    [SerializeField]
    private int memorySize = 200;
    [SerializeField]
    private List<string> shortTermMemory = new List<string>();


    [SerializeField]
    private string decisionStateMachineFile;
    private DecisionStateMachine stateMachine;


    [SerializeField]
    private VerbalAudioSource source;

    public void Start()
    {
        this.emotionState = new EmotionState(this.emotionStateData);

        if(this.personality == null)
        {
            //create random personality
        }

        stateMachine = new DecisionStateMachine(decisionStateMachineFile);
    }

    public string tell(string say)
    {
        // format prompts
        string query;
        string response;

        this.shortTermMemory.Add($"Other:\"{say}\"");

        //get update emotion
        {
            var allEmotions = string.Join(',', this.emotionState.allEmotions);

            query = getPrompt($"What would your characters feel emotinally? answer in the format \"(direction) (modifier) (emotion),...\" - directions are (more or less) & modifier are ({string.Join(",", EmotionState.adjectives)}) & emotions are ({allEmotions})");
            response = OpenAI.prompt(query).Trim().Replace(", ", ",").Trim('.').Trim(',');

            foreach ((int delta, string key) in EmotionState.parseEmotionString(response))
            {
                try
                {
                    var newEmotion = this.emotionState[key];

                    newEmotion.Intensity += delta;

                    this.emotionState[key] = newEmotion;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                
            }
        }
        // get decision
        {
            Dictionary<string, int> emotionDictionary = this.emotionState.orderedEmotions.ToDictionary(e => e.Name, e => e.Intensity);

            string[] options = Array.ConvertAll
            (
                this.stateMachine.Current.PotentialNextStates
                (
                    emotionDictionary,
                    this.stateMachine.states
                ),
                e => $"\"{e.ToString()}\""
            );

            string optionsString = string.Join(',', options);

            query = getPrompt($"Pick one of the following actions (\"Nothing\",{optionsString}) - Do not justify answer");
            response = OpenAI.prompt(query);

            if (options.Contains(response))
            {
                this.stateMachine.setState(response);
            }
        }
        // get response
        {
            query = getPrompt("What would your character say - Do not justify answer");
            response = OpenAI.prompt(query).Trim('"');

            Debug.Log(response);

            source.addText(response);

            this.shortTermMemory.Add($"Me:\"{response}\"");

            if (this.memorySize < this.shortTermMemory.Count)
            {
                this.shortTermMemory.RemoveAt(0);
            }

            return response;
        }

    }
    private string getPrompt()
    {
        return  $"You are a character in a cyber punk world with the following state\n"+
                $"Conscious Emotions: { string.Join(',', Array.ConvertAll( this.emotionState.consciousEmotions(50), e => e.ToString() ) ) }\n" +
                $"{this.personality}\n" +
                $"Conversation History: {string.Join(", ", this.shortTermMemory)}\n" +
                $"Current Thought:{this.stateMachine.Current.Name}";
    }
    private string getPrompt(string query)
    {
        return $"{this.getPrompt()}\n{query}";
    }
}
