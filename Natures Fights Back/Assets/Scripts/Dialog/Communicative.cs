using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmotionState
{
    //todo!() replace String Name with Int name, where the int maps to the emotions array therfore less (less heap allocation)
    public class Emotion : ICloneable
    {
        public string Name
        {
            get;
            private set;
        }
        public byte Intensity
        {
            get;
            private set;
        }

        public string Adjective
        {
            get => EmotionState.getIntensityAdjective(this.Intensity);
        }

        public Emotion(string name, byte intensity)
        {
            this.Name = name;
            this.Intensity = intensity;
        }

        public object Clone()
        {
            return new Emotion((string) this.Name.Clone(), this.Intensity);
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

    public Emotion[] orderedEmotions
    {
        get;
        private set;
    }
    private Dictionary<string, Emotion> emotionDict;

    private Emotion this[string key]
    {
        get
        {
            //clone of data
            return (Emotion) emotionDict[key].Clone();
        }
        set
        {
            int pointer;
            for (pointer = 0; pointer < this.orderedEmotions.Length && this.orderedEmotions[pointer].Name != key; pointer++);

            if (pointer >= this.orderedEmotions.Length)
            {
                throw new KeyNotFoundException();
            }

            this.orderedEmotions[pointer] = value;

            Func<int, int> compareIndex = (index) => Math.Clamp(index, 0, this.orderedEmotions.Length - 1);

            while (this.orderedEmotions[pointer].Intensity < this.orderedEmotions[compareIndex(pointer - 1)].Intensity)
            {
                ref Emotion tmp = ref this.orderedEmotions[pointer];

                this.orderedEmotions[pointer] = this.orderedEmotions[compareIndex(pointer - 1)];
                this.orderedEmotions[compareIndex(pointer - 1)] = tmp;

                pointer--;
            }

            while (this.orderedEmotions[compareIndex(pointer + 1)].Intensity < this.orderedEmotions[pointer].Intensity)
            {
                ref Emotion tmp = ref this.orderedEmotions[pointer];

                this.orderedEmotions[pointer] = this.orderedEmotions[compareIndex(pointer + 1)];
                this.orderedEmotions[compareIndex(pointer + 1)] = tmp;

                pointer--;
            }
        }
    }


    public EmotionState()
    {
        this.orderedEmotions = new Emotion[EmotionState.emotions.Length];
        for(int i1 = 0; i1 < EmotionState.emotions.Length; i1++)
        {
            this.orderedEmotions[i1] = new Emotion(EmotionState.emotions[i1], 0);
        }
    }

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

public class Communicative : MonoBehaviour
{
    // emotial state
    // decistion tree state machine
    // peronsality
    // speech patern
    // open ai api

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
