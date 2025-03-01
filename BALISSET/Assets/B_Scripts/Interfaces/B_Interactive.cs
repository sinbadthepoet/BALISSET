using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class B_Interactive : MonoBehaviour
{
    /// <summary>
    /// Get the string that describes interaction.
    /// </summary>
    public abstract string GetInteractionString();

    /// <summary>
    /// Triggers the interation.
    /// </summary>
    /// <param name="Interactor">The entity that interacts with the interactive object.</param>
    public abstract void Interact(B_Biped Interactor);

    protected virtual void Reset()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactive");
    }
}
