using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class DecisionStateMachineJSON
{
    public class StateJSON
    {
        public string Name { get; set; }
        public List<(string Name, JObject Condition)> NextStates { get; set; }
    }

    public List<StateJSON> States { get; set; }
}

public class DecisionStateMachine
{
    public delegate bool Condition(Dictionary<string, byte> data);

    public class State
    {
        public string Name { get; private set; }
        public (string Name, Condition)[] NextStates { get; private set; }

        public State(string name, (string Name, Condition)[] nextStates)
        {
            Name = name;
            NextStates = nextStates;
        }

        public State[] PotentialNextStates(Dictionary<string, byte> data, Dictionary<string, State> states)
        {
            List<State> potentialNextStates = new List<State>();

            foreach ((string name, Condition cond) in NextStates)
            {
                if (cond(data))
                {
                    potentialNextStates.Add(states[name]);
                }
            }

            return potentialNextStates.ToArray();
        }

        public static Condition CreateOrCondition(params Condition[] conds)
        {
            return (Dictionary<string, byte> data) =>
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
            return (Dictionary<string, byte> data) =>
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
            return (Dictionary<string, byte> data) =>
            {
                return data[key] < value;
            };
        }
        public static Condition CreateGreaterThanCondition(string key, int value)
        {
            return (Dictionary<string, byte> data) =>
            {
                return value < data[key];
            };
        }
        public static Condition CreateEqualToCondition(string key, int value)
        {
            return (Dictionary<string, byte> data) =>
            {
                return value == data[key];
            };
        }
        public static Condition CreateComplimentCondition(Condition cond)
        {
            return (Dictionary<string, byte> data) =>
            {
                return !cond(data);
            };
        }
    }

    private Dictionary<string, DecisionStateMachine.State> states;

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

    private Condition ParseCondition(JObject conditionJson)
    {
        string conditionType = conditionJson["type"].ToString();

        switch (conditionType)
        {
            case "<":
                string keyLessThan = conditionJson["key"].ToString();
                int valueLessThan = Convert.ToInt32(conditionJson["value"]);
                return State.CreateLessThanCondition(keyLessThan, valueLessThan);

            case ">":
                string keyGreaterThan = conditionJson["key"].ToString();
                int valueGreaterThan = Convert.ToInt32(conditionJson["value"]);
                return State.CreateGreaterThanCondition(keyGreaterThan, valueGreaterThan);

            case "=":
                string keyEqualTo = conditionJson["key"].ToString();
                int valueEqualTo = Convert.ToInt32(conditionJson["value"]);
                return State.CreateEqualToCondition(keyEqualTo, valueEqualTo);

            case "and":
                JObject comparison1 = conditionJson["comparison_1"].ToObject<JObject>();
                JObject comparison2 = conditionJson["comparison_2"].ToObject<JObject>();
                Condition condition1 = ParseCondition(comparison1);
                Condition condition2 = ParseCondition(comparison2);
                return State.CreateAndCondition(condition1, condition2);

            case "or":
                JObject comparison3 = conditionJson["comparison_1"].ToObject<JObject>();
                JObject comparison4 = conditionJson["comparison_2"].ToObject<JObject>();
                Condition condition3 = ParseCondition(comparison3);
                Condition condition4 = ParseCondition(comparison4);
                return State.CreateOrCondition(condition3, condition4);

            case "compliment":
                JObject complimentCondition = conditionJson["condition"].ToObject<JObject>();
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
