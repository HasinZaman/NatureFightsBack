using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public struct Personality
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


    private Trait[] _traits;
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

    /// <summary>
    /// Gets or sets the description of the personality.
    /// </summary>
    public string description
    {
        get;
        private set;
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
}
