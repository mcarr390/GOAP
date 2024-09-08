using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGoap
{
    public class GoapPlanner
    {
        readonly List<ActionNode> _actions;
        readonly Dictionary<string, int> _initialState;
        readonly Dictionary<string, int> _goalState;

        public GoapPlanner(List<ActionNode> actions, Dictionary<string, int> initialState, Dictionary<string, int> goalState)
        {
            _actions = actions;
            _initialState = initialState;
            _goalState = goalState;
        }
        public List<ActionNode> StartPlanner(Vector3 startingPos)
        {
            List<ActionNode> plan = Plan(startingPos);
            return plan;
        }

List<ActionNode> Plan(Vector3 startingPos)
{
    int iterations = 0;
    int maxIterations = 100000;
    var openList = new PriorityQueue<Node>();
    var closedList = new HashSet<Dictionary<string, int>>();
    
    // Start node with initial state and starting position
    var startNode = new Node
    {
        State = new Dictionary<string, int>(_initialState),
        Actions = new List<ActionNode>(),
        Cost = 0,
        Position = startingPos,  // Set initial position
        Heuristic = Heuristic(new Dictionary<string, int>(_initialState))
    };

    openList.Enqueue(startNode, startNode.Cost + startNode.Heuristic);

    while (openList.Count > 0 && iterations < maxIterations)
    {
        var currentNode = openList.Dequeue();

        if (IsGoalState(currentNode.State))
        {
            // Goal reached, return the action plan
            return currentNode.Actions;
        }

        closedList.Add(DictionaryClone(currentNode.State));

        foreach (var action in _actions)
        {
            if (action.CanExecute(currentNode.State))
            {
                // Calculate new state after applying the action's effects
                var newState = new Dictionary<string, int>(currentNode.State);
                action.ApplyEffects(newState);

                if (closedList.Contains(DictionaryClone(newState)))
                    continue;

                // Update the position cost (travel distance between current node and action)
                var distance = Distance(currentNode.Position, action.StartingPos);
                if (float.IsNaN(distance) || float.IsInfinity(distance))
                {
                    Debug.LogError("Invalid distance calculation");
                    continue;
                }

                // Combine the action's intrinsic cost and the travel distance
                var travelCost = distance;
                var actionCost = action.Cost + travelCost;  // BaseCost is the intrinsic action cost

                var newCost = currentNode.Cost + actionCost;

                // Create a new node with the updated state, actions, position, and cost
                var newNode = new Node
                {
                    Position = action.StartingPos,  // Move NPC to the action's position
                    State = newState,
                    Actions = new List<ActionNode>(currentNode.Actions) { action },
                    Cost = newCost,
                    Heuristic = Heuristic(newState)
                };

                // Add new node to the open list
                openList.Enqueue(newNode, newNode.Cost + newNode.Heuristic);
            }
        }

        iterations++;
    }
    return null;  // No valid plan found
}
        bool IsGoalState(Dictionary<string, int> state)
        {
            foreach (var goal in _goalState)
            {
                if (!state.ContainsKey(goal.Key) || state[goal.Key] < goal.Value)
                    return false;
            }
            return true;
        }

        int Heuristic(Dictionary<string, int> state)
        {
            int heuristicValue = 0;
            foreach (var goal in _goalState)
            {
                if (!state.ContainsKey(goal.Key) || state[goal.Key] < goal.Value)
                    heuristicValue += goal.Value - (state.ContainsKey(goal.Key) ? state[goal.Key] : 0);
            }
            return heuristicValue;
        }

        Dictionary<string, int> DictionaryClone(Dictionary<string, int> dict)
        {
            return new Dictionary<string, int>(dict);
        }
        
        public static float Distance(Vector3 v1, Vector3 v2)
        {
            float diffX = v2.x - v1.x;
            float diffY = v2.y - v1.y;
            float diffZ = v2.z - v1.z;
        
            return (float)Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
        }

        class Node
        {
            public float TotalDistanceTraveled { get; set; }
            public Vector3 Position { get; set; }
            public Dictionary<string, int> State { get; set; }
            public List<ActionNode> Actions { get; set; }
            public float Cost { get; set; }
            public int Heuristic { get; set; }
        }
    }


    public class PriorityQueue<T>
    {
        private List<(T item, float priority)> elements = new List<(T, float)>();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            elements.Add((item, priority));
            Swim(elements.Count - 1);
        }

        public T Dequeue()
        {
            if (elements.Count == 0)
                throw new InvalidOperationException("The queue is empty.");

            var bestItem = elements[0].item;
            Swap(0, elements.Count - 1);
            elements.RemoveAt(elements.Count - 1);
            Sink(0);
            return bestItem;
        }

        private void Swim(int k)
        {
            while (k > 0 && elements[k].priority < elements[Parent(k)].priority)
            {
                Swap(k, Parent(k));
                k = Parent(k);
            }
        }

        private void Sink(int k)
        {
            int size = elements.Count;
            while (LeftChild(k) < size)
            {
                int j = LeftChild(k);
                if (j + 1 < size && elements[j].priority > elements[j + 1].priority)
                    j++;
                if (elements[k].priority <= elements[j].priority)
                    break;
                Swap(k, j);
                k = j;
            }
        }

        private void Swap(int i, int j)
        {
            var temp = elements[i];
            elements[i] = elements[j];
            elements[j] = temp;
        }

        private int Parent(int k) => (k - 1) / 2;
        private int LeftChild(int k) => 2 * k + 1;
        private int RightChild(int k) => 2 * k + 2;
    }
    
    public class ActionNode
    {
        public string Name { get; private set; }
        public Dictionary<string, int> Preconditions { get; private set; }
        public Dictionary<string, int> Effects { get; private set; }
        public float Cost { get; set; }  // Cost to perform the action (e.g., time or resource cost)

        public Vector3 StartingPos { get; set; }
        public int Id { get; set; }
        public ActionNode(string name, Dictionary<string, int> preconditions, Dictionary<string, int> effects, float cost, Vector3 startingPos)
        {
            Name = name;
            Preconditions = preconditions;
            Effects = effects;
            Cost = cost;
            StartingPos = startingPos;
        }

        public bool CanExecute(Dictionary<string, int> state)
        {
            foreach (var precondition in Preconditions)
            {
                if (!state.ContainsKey(precondition.Key) || state[precondition.Key] < precondition.Value)
                    return false;
            }
            return true;
        }

        public void ApplyEffects(Dictionary<string, int> state)
        {
            foreach (var effect in Effects)
            {
                if (state.ContainsKey(effect.Key))
                    state[effect.Key] += effect.Value;
                else
                    state.Add(effect.Key, effect.Value);
            }
        }
    }

}