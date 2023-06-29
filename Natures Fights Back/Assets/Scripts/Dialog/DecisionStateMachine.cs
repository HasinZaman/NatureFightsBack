using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DecisionStateMachineJSON
{
    public class StateJSON
    {
        public string Name { get; set; }
        public List<NextStateJSON> NextStates { get; set; }
    }

    public class NextStateJSON
    {
        public string Name { get; set; }
        public ConditionJSON Condition { get; set; }
    }

    public class ConditionJSON
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
        public ConditionJSON Comparison_1 { get; set; }
        public ConditionJSON Comparison_2 { get; set; }
        public ConditionJSON Condition { get; set; }
    }

    public List<StateJSON> States { get; set; }
}


public class DecisionStateMachine
{
    public delegate bool Condition(Dictionary<string, int> data);

    public class State
    {
        public string Name { get; private set; }
        public (string Name, Condition)[] NextStates { get; private set; }

        public State(string name, (string Name, Condition)[] nextStates)
        {
            Name = name;
            NextStates = nextStates;
        }

        public State[] PotentialNextStates(Dictionary<string, int> data, Dictionary<string, State> states)
        {
            List<State> potentialNextStates = new List<State>();

            foreach ((string name, Condition cond) in NextStates)
            {
                if (cond(data))
                {
                    potentialNextStates.Add(states[name]);
                }
            }

            if(potentialNextStates.Count == 0)
            {
                return new State[0];
            }

            return potentialNextStates.ToArray();
        }

        public static Condition CreateOrCondition(params Condition[] conds)
        {
            return (Dictionary<string, int> data) =>
            {
                foreach (Condition cond in conds)
                {
                    if (cond(data))
                    {
                        return true;
                    }
                }
                return false;
            };
        }
        public static Condition CreateAndCondition(params Condition[] conds)
        {
            return (Dictionary<string, int> data) =>
            {
                foreach (Condition cond in conds)
                {
                    if (!cond(data))
                    {
                        return false;
                    }
                }
                return true;
            };
        }
        public static Condition CreateLessThanCondition(string key, int value)
        {
            return (Dictionary<string, int> data) =>
            {
                return data[key] < value;
            };
        }
        public static Condition CreateGreaterThanCondition(string key, int value)
        {
            return (Dictionary<string, int> data) =>
            {
                return value < data[key];
            };
        }
        public static Condition CreateEqualToCondition(string key, int value)
        {
            return (Dictionary<string, int> data) =>
            {
                return value == data[key];
            };
        }
        public static Condition CreateComplimentCondition(Condition cond)
        {
            return (Dictionary<string, int> data) =>
            {
                return !cond(data);
            };
        }
    }

    public Dictionary<string, DecisionStateMachine.State> states;

    public DecisionStateMachine.State Current { get; private set; }

    public DecisionStateMachine(string jsonFilePath)
    {
        string json = System.IO.File.ReadAllText(jsonFilePath);
        DecisionStateMachineJSON decisionJson = JsonConvert.DeserializeObject<DecisionStateMachineJSON>(json);

        states = new Dictionary<string, State>();

        foreach (DecisionStateMachineJSON.StateJSON stateJson in decisionJson.States)
        {
            (string Name, Condition)[] nextStates = new (string Name, Condition)[stateJson.NextStates.Count];
            for (int i = 0; i < stateJson.NextStates.Count; i++)
            {
                string nextStateName = stateJson.NextStates[i].Name;

                Condition nextStateCondition = ParseCondition(stateJson.NextStates[i].Condition);
                nextStates[i] = (nextStateName, nextStateCondition);
            }

            State state = new State(stateJson.Name, nextStates);
            states.Add(stateJson.Name, state);
        }

        // Set initial state
        Current = states[decisionJson.States[0].Name];
    }

    private Condition ParseCondition(DecisionStateMachineJSON.ConditionJSON conditionJson)
    {
        if (conditionJson == null)
        {
            return (Dictionary<string, int> data) => true;
        }
        string conditionType = conditionJson.Type;

        switch (conditionType)
        {
            case "<":
                string keyLessThan = conditionJson.Key;
                int valueLessThan = conditionJson.Value;
                return State.CreateLessThanCondition(keyLessThan, valueLessThan);

            case ">":
                string keyGreaterThan = conditionJson.Key;
                int valueGreaterThan = conditionJson.Value;
                return State.CreateGreaterThanCondition(keyGreaterThan, valueGreaterThan);

            case "=":
                string keyEqualTo = conditionJson.Key;
                int valueEqualTo = conditionJson.Value;
                return State.CreateEqualToCondition(keyEqualTo, valueEqualTo);

            case "and":
                DecisionStateMachineJSON.ConditionJSON comparison1 = conditionJson.Comparison_1;
                DecisionStateMachineJSON.ConditionJSON comparison2 = conditionJson.Comparison_2;
                Condition condition1 = ParseCondition(comparison1);
                Condition condition2 = ParseCondition(comparison2);
                return State.CreateAndCondition(condition1, condition2);

            case "or":
                DecisionStateMachineJSON.ConditionJSON comparison3 = conditionJson.Comparison_1;
                DecisionStateMachineJSON.ConditionJSON comparison4 = conditionJson.Comparison_2;
                Condition condition3 = ParseCondition(comparison3);
                Condition condition4 = ParseCondition(comparison4);
                return State.CreateOrCondition(condition3, condition4);

            case "compliment":
                DecisionStateMachineJSON.ConditionJSON complimentCondition = conditionJson.Condition;
                Condition baseCondition = ParseCondition(complimentCondition);
                return State.CreateComplimentCondition(baseCondition);

            default:
                throw new ArgumentException("Invalid condition type: " + conditionType);
        }
    }

    public void setState(string name)
    {
        this.Current = states[name];
    }
}
