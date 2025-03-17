using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LayerMaskConfig : ScriptableObject
{
    public LayerMask interactive;
    public LayerMask groundCheck;
    public LayerMask damage;
}
