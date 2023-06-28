using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Communicative))]
public class CommunicativeEditor : Editor
{
    private SerializedProperty emotionStateProp;
    private SerializedProperty personalityProp;
    private SerializedProperty memorySizeProp;
    private SerializedProperty shortTermMemoryProp;
    private SerializedProperty decisionStateMachineFileProp;

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

    public void Start()
    {
        this.emotionState = new EmotionState(this.emotionStateData);
    }
}
