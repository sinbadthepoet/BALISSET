using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StringReference
{
    public bool UseConstant;
    public string ConstantValue;
    public StringVariable Variable;

    public string Value
    {
        get { return UseConstant ? ConstantValue : Variable.Value; }
    }
}
