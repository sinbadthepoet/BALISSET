using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StringReference
{
    public bool UseConstant;
    public float ConstantValue;
    public StringVariable Variable;

    public float Value
    {
        get { return UseConstant ? ConstantValue : Variable.value; }
    }
}
