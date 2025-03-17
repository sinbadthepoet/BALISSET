using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAction : MonoBehaviour
{
    public string Name { get; }
    public float Cost { get; private set; }

    public HashSet<AgentBelief> Preconditions { get; } = new();
    public HashSet<AgentBelief> Effects { get; } = new();

    IActionStrategy strategy;
    public bool Complete => strategy.Complete;

    public void Start() => strategy.Start();
    public void Update(float deltaTime)
    {
        if (strategy.CanPerform)
        {
            strategy.Update(deltaTime);
        }
    }

    public void Stop() => strategy.Stop();
}
