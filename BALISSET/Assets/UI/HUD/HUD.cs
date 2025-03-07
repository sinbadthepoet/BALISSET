using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    GameEventListener gameEventListener;
    Label InteractionLabel;

    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        gameEventListener = GetComponent<GameEventListener>();

        //THIS WILL NOT LAST.
        InteractionLabel = uiDocument.rootVisualElement.Q<Label>();
    }

    public void UpdateInteractionString()
    {
        var StringVar = gameEventListener.Event as AlertStringVariable;
        InteractionLabel.text = StringVar.Value;
    }
}
