using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class EmotionState
{
    /// <summary>
    /// Represents an emotion with a name and intensity.
    /// </summary>
    public class Emotion : ICloneable
    {
        //todo!() replace String Name with Int name, where the int maps to the emotions array therfore less (less heap allocation)

        /// <summary>
        /// Gets the name of the emotion.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the intensity of the emotion.
        /// </summary>
        public byte Intensity
        {
            get;
            set;
        }

        /// /// <summary>
        /// Gets the adjective associated with the emotion's intensity.
        /// </summary>
        public string Adjective
        {
            get => EmotionState.getIntensityAdjective(this.Intensity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Emotion"/> class with the specified name and intensity.
        /// </summary>
        /// <param name="name">The name of the emotion.</param>
        /// <param name="intensity">The intensity of the emotion.</param>
        public Emotion(string name, byte intensity)
        {
            this.Name = name;
            this.Intensity = intensity;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of the current instance.</returns>
        public object Clone()
        {
            return new Emotion((string)this.Name.Clone(), this.Intensity);
        }
    }
    private static string[] adjectives = new string[]
    {
        "serene",
        "moderate",
        "subdued",
        "heightened",
        "overwhelm"
    };
    private static string[] emotions = new string[]
    {
        "joy",
        "sadness",
        "anger",
        "fear",
        "surprise",
        "disgust",
        "love",
        "excitement",
        "guilt",
        "jealousy"
    };

    private Emotion[] _orderedEmotions;
    public Emotion[] orderedEmotions
    {
        get
        {
            // Perform a deep copy of the emotions array to preserve the order
            Emotion[] copy = new Emotion[this._orderedEmotions.Length];
            for (int i = 0; i < this._orderedEmotions.Length; i++)
            {
                copy[i] = (Emotion)this._orderedEmotions[i].Clone();
            }
            return copy;
        }
        private set
        {
            this._orderedEmotions = value;
        }
    }

    private Dictionary<string, Emotion> emotionDict;
    private Emotion this[string key]
    {
        get
        {
            //clone of data
            return (Emotion)emotionDict[key].Clone();
        }
        set
        {
            int pointer;
            //intialize pointer value
            for (pointer = 0; pointer < this._orderedEmotions.Length && this._orderedEmotions[pointer].Name != key; pointer++) ;

            if (pointer >= this._orderedEmotions.Length)
            {
                throw new KeyNotFoundException();
            }

            this._orderedEmotions[pointer] = value;

            Func<int, int> compareIndex = (index) => Math.Clamp(index, 0, this._orderedEmotions.Length - 1);

            //shift left (larger value)
            while (this._orderedEmotions[pointer].Intensity < this._orderedEmotions[compareIndex(pointer - 1)].Intensity)
            {
                ref Emotion tmp = ref this._orderedEmotions[pointer];

                this._orderedEmotions[pointer] = this._orderedEmotions[compareIndex(pointer - 1)];
                this._orderedEmotions[compareIndex(pointer - 1)] = tmp;

                pointer--;
            }

            //shift right (smaller value)
            while (this._orderedEmotions[compareIndex(pointer + 1)].Intensity < this._orderedEmotions[pointer].Intensity)
            {
                ref Emotion tmp = ref this._orderedEmotions[pointer];

                this._orderedEmotions[pointer] = this._orderedEmotions[compareIndex(pointer + 1)];
                this._orderedEmotions[compareIndex(pointer + 1)] = tmp;

                pointer++;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmotionState"/> class.
    /// </summary>
    public EmotionState()
    {
        this._orderedEmotions = new Emotion[EmotionState.emotions.Length];
        for (int i1 = 0; i1 < EmotionState.emotions.Length; i1++)
        {
            this._orderedEmotions[i1] = new Emotion(EmotionState.emotions[i1], 0);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmotionState"/> class with the specified initial values.
    /// </summary>
    /// <param name="initialValue">The initial values as a collection of emotion names and intensities.</param>
    public EmotionState(params (string, byte)[] initalValue) : this()
    {
        foreach ((string emotion, byte intensity) in initalValue)
        {
            Emotion tmp = this[emotion];
            tmp.Intensity = intensity;

            this[emotion] = tmp;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmotionState"/> class with the specified initial values and intensity range.
    /// </summary>
    /// <param name="initialValue">The initial values as a collection of emotion names and intensity ranges.</param>
    public EmotionState(params (string, (byte min, byte max))[] initalValue) : this()
    {
        foreach ((string emotion, (byte min, byte max)) in initalValue)
        {
            Emotion tmp = this[emotion];
            tmp.Intensity = (byte)new System.Random().Next(min, max + 1);

            this[emotion] = tmp;
        }
    }

    /// <summary>
    /// Gets the adjective associated with the specified intensity.
    /// </summary>
    /// <param name="intensity">The intensity value.</param>
    /// <returns>The adjective associated with the intensity.</returns>
    public static string getIntensityAdjective(byte intensity)
    {
        if (intensity < 20)
        {
            return EmotionState.adjectives[0];
        }
        else if (intensity < 40)
        {
            return EmotionState.adjectives[1];
        }
        else if (intensity < 60)
        {
            return EmotionState.adjectives[2];
        }
        else if (intensity < 80)
        {
            return adjectives[3];
        }

        return EmotionState.adjectives[4];
    }
}
