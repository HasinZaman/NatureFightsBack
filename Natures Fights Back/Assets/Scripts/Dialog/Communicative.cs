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

    private void OnEnable()
    {
        emotionStateProp = serializedObject.FindProperty("emotionState");
        personalityProp = serializedObject.FindProperty("personality");
        memorySizeProp = serializedObject.FindProperty("memorySize");
        shortTermMemoryProp = serializedObject.FindProperty("shortTermMemory");
        decisionStateMachineFileProp = serializedObject.FindProperty("decisionStateMachineFile");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(emotionStateProp);
        EditorGUILayout.PropertyField(personalityProp);
        EditorGUILayout.PropertyField(memorySizeProp);
        EditorGUILayout.PropertyField(shortTermMemoryProp, true);
        EditorGUILayout.PropertyField(decisionStateMachineFileProp);

        serializedObject.ApplyModifiedProperties();
    }
}

public class Communicative : MonoBehaviour
{
    

    [SerializeField]
    private EmotionState emotionState;
    [SerializeField]
    private Personality personality;

    [SerializeField]
    private int memorySize = 200;
    [SerializeField]
    private List<string> shortTermMemory = new List<string>();


    [SerializeField]
    private string decisionStateMachineFile;
    private DecisionStateMachine stateMachine;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
