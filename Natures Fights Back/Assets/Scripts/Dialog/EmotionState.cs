using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmotionState : ScriptableObject
{
    /// <summary>
    /// Represents an emotion with a name and intensity.
    /// </summary>
    [System.Serializable]
    public class Emotion : ICloneable
    {
        /// <summary>
         /// Comparer for sorting emotions based on intensity.
         /// </summary>
        public class IntensityComparer : IComparer<Emotion>
        {
            public int Compare(Emotion x, Emotion y)
            {
                // Compare the intensity values of the two emotions
                if (x.Intensity < y.Intensity)
                    return -1;
                if (x.Intensity > y.Intensity)
                    return 1;
                return 0;
            }
        }
        //todo!() replace String Name with Int name, where the int maps to the emotions array therfore less (less heap allocation)

        /// <summary>
        /// Gets the name of the emotion.
        /// </summary>
        [SerializeField]
        private string _name;
        public string Name
        {
            get => this._name;
            private set{this._name = value;}
        }

        /// <summary>
        /// Gets or sets the intensity of the emotion.
        /// </summary>
        [SerializeField]
        private int _intensity;
        public int Intensity
        {
            get => this._intensity;
            set { this._intensity = value; }
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
        public Emotion(string name, int intensity)
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

        public override string ToString()
        {
            return $"{EmotionState.getIntensityAdjective(this.Intensity)} {this.Name}";
        }
    }
    public static string[] adjectives
    {
        get
        {
            return new string[]
            {
                "serene",
                "moderate",
                "subdued",
                "heightened",
                "overwhelm"
            };
        }
    } 
    public static string[] emotions
    {
        get
        {
            return new string[]
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
        }
    }

    [SerializeField]
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

    [SerializeField]
    private Dictionary<string, Emotion> emotionDict;
    public string[] allEmotions
    {
        get
        {
            string[] emotions = new string[this.emotionDict.Count];

            int i1 = 0;
            foreach(string emotion in this.emotionDict.Keys)
            {
                emotions[i1++] = emotion;
            }

            return emotions;
        }
    }
    public Emotion this[string key]
    {
        get
        {
            return emotionDict[key].Clone() as Emotion;
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

            this._orderedEmotions[pointer].Intensity = value.Intensity;

            Func<int, int> compareIndex = (index) => Math.Clamp(index, 0, this._orderedEmotions.Length - 1);

            //shift left (larger value)
            while (this._orderedEmotions[pointer].Intensity < this._orderedEmotions[compareIndex(pointer - 1)].Intensity)
            {
                Emotion tmp = this._orderedEmotions[pointer];

                this._orderedEmotions[pointer] = this._orderedEmotions[compareIndex(pointer - 1)];
                this._orderedEmotions[compareIndex(pointer - 1)] = tmp;

                pointer--;
            }

            //shift right (smaller value)
            while (this._orderedEmotions[compareIndex(pointer + 1)].Intensity < this._orderedEmotions[pointer].Intensity)
            {
                 Emotion tmp = this._orderedEmotions[pointer];

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
        this.emotionDict = new Dictionary<string, Emotion>();
        this._orderedEmotions = new Emotion[EmotionState.emotions.Length];

        for (int i1 = 0; i1 < EmotionState.emotions.Length; i1++)
        {
            this._orderedEmotions[i1] = new Emotion(EmotionState.emotions[i1], 0);
            this.emotionDict.Add(EmotionState.emotions[i1], this._orderedEmotions[i1]);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmotionState"/> class with the specified initial values.
    /// </summary>
    /// <param name="initialValue">The initial values as a collection of emotion names and intensities.</param>
    public EmotionState(params (string, int)[] initalValue) : this()
    {
        foreach ((string emotion, int intensity) in initalValue)
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
    public EmotionState(params (string, (int min, int max))[] initalValue) : this()
    {
        foreach ((string emotion, (int min, int max)) in initalValue)
        {
            Emotion tmp = this[emotion];
            tmp.Intensity = new System.Random().Next(min, max + 1);

            this[emotion] = tmp;
        }
    }

    public EmotionState(EmotionStateData data) : this(data.Emotions)
    {

    }

    /// <summary>
    /// Gets the adjective associated with the specified intensity.
    /// </summary>
    /// <param name="intensity">The intensity value.</param>
    /// <returns>The adjective associated with the intensity.</returns>
    public static string getIntensityAdjective(int intensity)
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

    public Emotion[] consciousEmotions(int threshold)
    {
        List<Emotion> consciousEmotions = new List<Emotion>();

        // Perform a binary search to find the index of the first emotion with intensity greater than the threshold
        int startIndex = Array.BinarySearch(this._orderedEmotions, new Emotion("", threshold), new Emotion.IntensityComparer());
        if (startIndex < 0)
        {
            // If the exact threshold is not found, convert the index to a valid insertion point
            startIndex = ~startIndex;
        }

        // Add all emotions with intensity greater than the threshold
        for (int i = startIndex; i < this._orderedEmotions.Length; i++)
        {
            consciousEmotions.Add(this._orderedEmotions[i]);
        }

        if (consciousEmotions.Count == 0)
        {
            int last = this._orderedEmotions.Length - 1;
            consciousEmotions.Add(this._orderedEmotions[last]);
        }

        return consciousEmotions.ToArray();
    }

    public static (int, string)[] parseEmotionString(string rawString)
    {
        List<(int, string)> result = new List<(int, string)>();

        string[] emotions = rawString.Split(',');

        foreach (string rawEmotion in rawString.Split(','))
        {
            string[] components = rawEmotion.ToLower().Split(' ');

            if (components.Length != 3)
                continue;

            int delta;
            string emotion = components[2];

            switch (components[1])
            {
                case "serene":
                    delta = 1;
                    break;
                case "moderate":
                    delta = 2;
                    break;
                case "subdued":
                    delta = 4;
                    break;
                case "heightened":
                    delta = 8;
                    break;
                case "overwhelm":
                    delta = 16;
                    break;
                default:
                    delta = 0;
                    break;
            }

            if (delta == 0)
                continue;

            if (components[0] == "less")
            {
                delta *= -1;
            }

            result.Add((delta, emotion.Trim()));
        }

        return result.ToArray();
    }

}
