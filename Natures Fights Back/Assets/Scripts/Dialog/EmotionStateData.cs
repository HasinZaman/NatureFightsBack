using UnityEngine;
using UnityEditor;

[System.Serializable]
[CreateAssetMenu(fileName = "EmotionStateData", menuName = "Scriptable Object/Emotion State Data")]
public class EmotionStateData : ScriptableObject
{
    [SerializeField]
    private EmotionData[] emotions;

    [System.Serializable]
    public struct EmotionData
    {
        public string emotion;
        public IntRange range;
    }

    [System.Serializable]
    public struct IntRange
    {
        public int min;
        public int max;
    }

    public (string emotion, (int min, int max) range)[] Emotions
    {
        get
        {
            (string emotion, (int min, int max) range)[] emotionData = new (string emotion, (int min, int max) range)[emotions.Length];
            for (int i = 0; i < emotions.Length; i++)
            {
                emotionData[i].emotion = emotions[i].emotion;
                emotionData[i].range.min = emotions[i].range.min;
                emotionData[i].range.max = emotions[i].range.max;
            }
            return emotionData;
        }
        set
        {
            emotions = new EmotionData[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                emotions[i].emotion = value[i].emotion;
                emotions[i].range.min = value[i].range.min;
                emotions[i].range.max = value[i].range.max;
            }
        }
    }

    public static string ToJson(EmotionStateData data)
    {
        return JsonUtility.ToJson(data);
    }

    public static EmotionStateData FromJson(string json)
    {
        return JsonUtility.FromJson<EmotionStateData>(json);
    }
}

[CustomEditor(typeof(EmotionStateData))]
public class EmotionStateDataEditor : Editor
{
    private SerializedProperty emotionsProp;

    private void OnEnable()
    {
        emotionsProp = serializedObject.FindProperty("emotions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(emotionsProp, true);

        serializedObject.ApplyModifiedProperties();
    }
}