using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    [SerializeField] StringReference InteractionString;
    Label InteractionLabel;
    [SerializeField] StringReference DebugStrings;
    Label DebugLabel;

    [SerializeField] IntReference AmmoCount;
    Label AmmoInMagazine;
    [SerializeField] IntReference ReserveCount;
    Label ReserveAmmo;

    // TODO: Separate Crosshair Visual Container and Crosshair object,
    // Set crosshair to 0.7 opacity

    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        InteractionLabel = uiDocument.rootVisualElement.Q<Label>("Highlight");
        AmmoInMagazine = uiDocument.rootVisualElement.Q<Label>("Loaded");
        ReserveAmmo = uiDocument.rootVisualElement.Q<Label>("Reserve");
        DebugLabel = uiDocument.rootVisualElement.Q<Label>("DebugStrings");

    }
    
    void Update()
    {
        InteractionLabel.text = InteractionString.Value;
        DebugLabel.text = DebugStrings.Value;
        AmmoInMagazine.text = AmmoCount.Value.ToString();
        ReserveAmmo.text = ReserveCount.Value.ToString();
    }
}
