using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AlertStringVariable : GameEvent
{
    // Similar to StringVariable, triggers a game event when the value is changed.
    private string _value;
    public string Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (value != _value)
            {
                _value = value;
                Raise();
            }
        }
    }
}
