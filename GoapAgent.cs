using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyGoap;
using UnityEngine;

public class GoapAgent
{
    GoapPlanner _goapPlanner;
    Task<List<ActionNode>> _planningTask;

    List<ActionNode> _actions = new List<ActionNode>();
    
    public void GetAllActions(Action<List<ActionNode>> actionCallback, List<ActionNode> actions, Dictionary<Enum, int> initialState, Dictionary<Enum, int> goalState)
    {
        
        // Cancel any existing planning task if needed
        if (_planningTask != null && !_planningTask.IsCompleted)
        {
            Debug.Log("uh oh");
            return;
        }

        // Start the planning task on a separate thread
        _planningTask = Task.Run(() => GetActions(actions, initialState, goalState));
        _planningTask.ContinueWith(task =>
        {
            if (task.Status != TaskStatus.RanToCompletion)
            {
                Debug.Log("uh oh");
                return;
            }
            _actions = task.Result;
            actionCallback.Invoke(_actions);
        });
    }

    public List<ActionNode> GetActions(List<ActionNode> actions, Dictionary<Enum, int> initialState, Dictionary<Enum, int> goalState)
    {
        //int coinCost = 5;

        //float timeCost = (float)coinCost / (float)coinsPerHour;

        //int timeCostToPurchaseWood = 1;
        //int timeCostToGatherWood = 1;

        //Debug.Log($"The cost is {coinCost}, it takes you {timeCost} hours to make that");

        // Initialize example actions
        

        _goapPlanner = new GoapPlanner(actions, initialState, goalState);

        return _goapPlanner.StartPlanner();
    }
}

