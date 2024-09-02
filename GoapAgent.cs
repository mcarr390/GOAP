using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

using System.Threading.Tasks;

namespace MyGoap
{
    public class GoapAgent : MonoBehaviour
    {
    public GoapPlanner goapPlanner;
    public Task<List<ActionNode>> planningTask;

    public List<ActionNode> actions = new List<ActionNode>();

    [ContextMenu("Get Actions")]
    public void GetIt()
    {
        StartCoroutine(OtherBoi());
    }

    public IEnumerator OtherBoi()
    {
        planningTask = Task.Run(() => GetActions(50));
        yield return StartCoroutine(WaitForActionsCoroutine());
        Debug.Log("Goap Done");
    }
    
    private IEnumerator WaitForActionsCoroutine()
    {
        // Wait until the planningTask is completed
        while (!planningTask.IsCompleted)
        {
            yield return null; // Wait for the next frame
        }

        // Check the task's status
        if (planningTask.Status == TaskStatus.RanToCompletion)
        {
            actions = planningTask.Result;
            Debug.Log("Planning completed successfully.");
            foreach (ActionNode action in actions)
            {
                Debug.Log(action.Name);
            }
            // You can now safely access the `actions` property here
        }
        else if (planningTask.Status == TaskStatus.Faulted)
        {
            Debug.LogError("Planning failed: " + planningTask.Exception);
        }
    }
    public void GetAllActions()
    {
        // Cancel any existing planning task if needed
        if (planningTask != null && !planningTask.IsCompleted)
        {
            Debug.Log("Previous planning task is still running. Please wait.");
            return;
        }

        // Start the planning task on a separate thread
        planningTask = Task.Run(() => GetActions(50));
        planningTask.ContinueWith(task =>
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                actions = task.Result;
            }
            else
            {
                Debug.LogError("Planning failed: " + task.Exception);
            }
        });
    }

    List<ActionNode> GetActions(int coinsPerHour)
    {
        int coinCost = 5;

        float timeCost = (float)coinCost / (float)coinsPerHour;

        int timeCostToPurchaseWood = 1;
        int timeCostToGatherWood = 1;

        Debug.Log($"The cost is {coinCost}, it takes you {timeCost} hours to make that");

        // Initialize example actions
        var actions = new List<ActionNode>
        {
            new ActionNode("GatherWood",
                new Dictionary<string, int> {  },
                new Dictionary<string, int> { { "Wood", 1 } },
                3),

            new ActionNode("GatherStone",
                new Dictionary<string, int> {  },
                new Dictionary<string, int> { { "Stone", 1 } },
                1),
            
            new ActionNode("CraftShield",
                new Dictionary<string, int> { { "Wood", 1 }, { "Stone", 1 } },
                new Dictionary<string, int> { { "Shield", 1 },
                    { "Wood", -1 }, { "Stone", -1 } },
                1),

            new ActionNode("GatherIron",
                new Dictionary<string, int> { { "Iron", 0 } },
                new Dictionary<string, int> { { "Iron", 5 } },
                8),

            new ActionNode("BuyWood",
                new Dictionary<string, int> { { "Wood", 0 }, {"Money", 20} },
                new Dictionary<string, int> { { "Wood", 5 }, { "Money", -20 } },
                1 + timeCost),

            new ActionNode("SellWood",
                new Dictionary<string, int> { { "Wood", 30 } },
                new Dictionary<string, int> { { "Money", 10 },
                    { "Wood", -30 } },
                1),
            
            new ActionNode("CraftSword",
                new Dictionary<string, int> { { "Steel", 10 }, { "Leather", 5 }, { "OakWood", 5 } },
                new Dictionary<string, int> { { "Sword", 1 }, { "Steel", -10 }, { "Leather", -5 }, { "OakWood", -5 } },
                1),

            new ActionNode("CraftBow",
                new Dictionary<string, int> { { "YewWood", 10 }, { "Linen", 5 } },
                new Dictionary<string, int> { { "Bow", 1 }, { "YewWood", -10 }, { "Linen", -5 } },
                1),

            new ActionNode("CraftCrossbow",
                new Dictionary<string, int> { { "OakWood", 10 }, { "Steel", 10 }, { "Leather", 5 } },
                new Dictionary<string, int> { { "Crossbow", 1 }, { "OakWood", -10 }, { "Steel", -10 }, { "Leather", -5 } },
                1),

            new ActionNode("CraftFrostDagger",
                new Dictionary<string, int> { { "Steel", 5 }, { "EssenceOfShadow", 5 }, { "Aetherweed", 3 } },
                new Dictionary<string, int> { { "FrostDagger", 1 }, { "Steel", -5 }, { "EssenceOfShadow", -5 }, { "Aetherweed", -3 } },
                1),
            new ActionNode("GatherLeather",
                new Dictionary<string, int> {  },
                new Dictionary<string, int> { { "Leather", 5 } },
                3),
            new ActionNode("GatherSteel",
                new Dictionary<string, int> {  },
                new Dictionary<string, int> { { "Steel", 5 } },
                1),
            new ActionNode("GatherOakWood",
                new Dictionary<string, int> {  },
                new Dictionary<string, int> { { "OakWood", 5 } },
                1),
        };

        // Initialize initial state
        var initialState = new Dictionary<string, int>
        {
            { "Wood", 0 },
            { "Stone", 0 },
            { "Money", 0 },
            { "Iron", 0 },
            { "Shield", 0 },
            { "Steel", 0 },
            { "Leather", 0 },
            { "OakWood", 0 },
            { "Sword", 0 },
            { "Hunger", 0 },
            
        };

        // Initialize goal state
        var goalState = new Dictionary<string, int>
        {
            { "Shield", 1 }
        };

        goapPlanner = new GoapPlanner(actions, initialState, goalState);

        return goapPlanner.StartPlanner();
    }
}


    public class GameAction
    {
        
    }
}

