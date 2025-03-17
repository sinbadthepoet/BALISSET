using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionStrategy
{
    bool CanPerform { get; }
    bool Complete { get; }

    void Start()
    {

    }

    void Update(float deltaTime)
    {

    }

    void Stop()
    {

    }
}