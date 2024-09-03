using System;
using System.Collections.Generic;
namespace MyGoap
{
    public class GoapPlanner
    {
        readonly List<ActionNode> _actions;
        readonly Dictionary<Enum, int> _initialState;
        readonly Dictionary<Enum, int> _goalState;

        public GoapPlanner(List<ActionNode> actions, Dictionary<Enum, int> initialState, Dictionary<Enum, int> goalState)
        {
            _actions = actions;
            _initialState = initialState;
            _goalState = goalState;
        }
        public List<ActionNode> StartPlanner()
        {
            List<ActionNode> plan = Plan();
            return plan;
        }

        List<ActionNode> Plan()
        {
            int iterations = 0;
            int maxIterations = 10000000;
            var openList = new PriorityQueue<Node>();
            var closedList = new HashSet<Dictionary<Enum, int>>();
            var startNode = new Node
            {
                State = new Dictionary<Enum, int>(_initialState),
                Actions = new List<ActionNode>(),
                Cost = 0,
                Heuristic = Heuristic(new Dictionary<Enum, int>(_initialState))
            };

            openList.Enqueue(startNode, startNode.Cost + startNode.Heuristic);

            while (openList.Count > 0)
            {
                var currentNode = openList.Dequeue();
                
                if (IsGoalState(currentNode.State))
                {
                    return currentNode.Actions;
                }

                closedList.Add(DictionaryClone(currentNode.State));

                foreach (var action in _actions)
                {
                    if (action.CanExecute(currentNode.State))
                    {
                        var newState = new Dictionary<Enum, int>(currentNode.State);
                        action.ApplyEffects(newState);

                        if (closedList.Contains(DictionaryClone(newState)))
                            continue;

                        var newActions = new List<ActionNode>(currentNode.Actions) { action };
                        var newCost = currentNode.Cost + action.Cost;
                        var newNode = new Node
                        {
                            State = newState,
                            Actions = newActions,
                            Cost = newCost,
                            Heuristic = Heuristic(newState)
                        };

                        openList.Enqueue(newNode, newNode.Cost + newNode.Heuristic);
                    }
                }
                iterations++;
                if (iterations > maxIterations)
                {
                    
                }
            }
            return null;
        }

        bool IsGoalState(Dictionary<Enum, int> state)
        {
            foreach (var goal in _goalState)
            {
                if (!state.ContainsKey(goal.Key) || state[goal.Key] < goal.Value)
                    return false;
            }
            return true;
        }

        int Heuristic(Dictionary<Enum, int> state)
        {
            int heuristicValue = 0;
            foreach (var goal in _goalState)
            {
                if (!state.ContainsKey(goal.Key) || state[goal.Key] < goal.Value)
                    heuristicValue += goal.Value - (state.ContainsKey(goal.Key) ? state[goal.Key] : 0);
            }
            return heuristicValue;
        }

        Dictionary<Enum, int> DictionaryClone(Dictionary<Enum, int> dict)
        {
            return new Dictionary<Enum, int>(dict);
        }

        class Node
        {
            public Dictionary<Enum, int> State { get; set; }
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
        public Dictionary<Enum, int> Preconditions { get; private set; }
        public Dictionary<Enum, int> Effects { get; private set; }
        public float Cost { get; private set; }  // Cost to perform the action (e.g., time or resource cost)

        public ActionNode(string name, Dictionary<Enum, int> preconditions, Dictionary<Enum, int> effects, float cost)
        {
            Name = name;
            Preconditions = preconditions;
            Effects = effects;
            Cost = cost;
        }

        public bool CanExecute(Dictionary<Enum, int> state)
        {
            foreach (var precondition in Preconditions)
            {
                if (!state.ContainsKey(precondition.Key) || state[precondition.Key] < precondition.Value)
                    return false;
            }
            return true;
        }

        public void ApplyEffects(Dictionary<Enum, int> state)
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