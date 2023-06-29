using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[System.Serializable]
[CreateAssetMenu(fileName = "EmotionStateData", menuName = "Scriptable Object/Personality")]
public class Personality : ScriptableObject
{
    /// <summary>
    /// Represents a trait of a personality.
    /// </summary>
    public enum Trait
    {
        Brave,
        Cautious,
        Optimistic,
        Pessimistic,
        Confident,
        Insecure,
        Loyal,
        Betraying,
        Adventurous,
        Timid,
        Wise,
        Foolish,
        Charismatic,
        Shy,
        Rebellious,
        Obedient,
        Ambitious,
        Content,
        Compassionate,
        ColdHearted
    }

    [SerializeField]
    private Trait[] _traits = new Trait[0];
    /// <summary>
    /// Gets or sets the traits of the personality.
    /// </summary>
    public Trait[] traits
    {
        get
        {
            Trait[] copy = new Trait[this._traits.Length];
            for (int i = 0; i < this._traits.Length; i++)
            {
                copy[i] = this._traits[i];
            }
            return copy;
        }
        private set
        {
            this._traits = value;
        }
    }

    [SerializeField]
    private string _description = "";

    /// <summary>
    /// Gets or sets the description of the personality.
    /// </summary>
    public string description
    {
        get => this._description;
        set
        {
            this._description = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Personality"/> struct with the specified traits and description.
    /// </summary>
    /// <param name="traits">The traits of the personality.</param>
    /// <param name="description">The description of the personality.</param>
    public Personality(Trait[] traits, string description)
    {
        this._traits = traits;
        this.description = description;
    }

    public override string ToString()
    {
        string traitList = string.Join(",", traits.Select(trait => trait.ToString()).ToArray());
        return $"description: {description}\ntraits: {traitList}";
    }
}

[CustomEditor(typeof(Personality))]
public class PersonalityEditor : Editor
{
    private SerializedProperty traitsProp;
    private SerializedProperty descriptionProp;
    private Personality.Trait nextTrait = Personality.Trait.Brave;

    private void OnEnable()
    {
        traitsProp = serializedObject.FindProperty("_traits");
        descriptionProp = serializedObject.FindProperty("_description");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(descriptionProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Traits", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        List<Personality.Trait> uniqueTraits = new List<Personality.Trait>();
        for (int i = 0; i < traitsProp.arraySize; i++)
        {
            SerializedProperty traitProp = traitsProp.GetArrayElementAtIndex(i);

            Personality.Trait trait = (Personality.Trait)traitProp.enumValueIndex;

            if (!uniqueTraits.Contains(trait))
            {
                uniqueTraits.Add(trait);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(traitProp);

                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    traitsProp.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                traitsProp.DeleteArrayElementAtIndex(i);
                i--;
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Trait"))
        {
            traitsProp.arraySize++;
            SerializedProperty newTraitProp = traitsProp.GetArrayElementAtIndex(traitsProp.arraySize - 1);
            newTraitProp.enumValueIndex = (int)nextTrait;
            nextTrait = GetNextTrait(nextTrait);
        }

        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }

    private Personality.Trait GetNextTrait(Personality.Trait currentTrait)
    {
        Personality.Trait[] allTraits = (Personality.Trait[])Enum.GetValues(typeof(Personality.Trait));
        int currentIndex = Array.IndexOf(allTraits, currentTrait);
        int nextIndex = (currentIndex + 1) % allTraits.Length;
        return allTraits[nextIndex];
    }

}