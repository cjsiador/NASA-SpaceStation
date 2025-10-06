using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GrabbedTools : MonoBehaviour
{
    [Header("Grab Items")]
    public XRGrabInteractable[] grabItems;  // List of grabbable objects

    [Header("Target to Activate")]
    public GameObject objectToActivate;     // Object to activate when any item is grabbed

    private bool activated = false;         // Prevent multiple activations

    void Start()
    {
        // Make sure target is off at start
        if (objectToActivate != null)
            objectToActivate.SetActive(false);

        // Subscribe to grab events
        foreach (var item in grabItems)
        {
            if (item != null)
                item.selectEntered.AddListener(OnItemGrabbed);
        }
    }

    private void OnItemGrabbed(SelectEnterEventArgs args)
    {
        if (activated) return; // only trigger once

        activated = true;
        if (objectToActivate != null)
            objectToActivate.SetActive(true);
    }

    void OnDestroy()
    {
        // Unsubscribe when destroyed (good practice)
        foreach (var item in grabItems)
        {
            if (item != null)
                item.selectEntered.RemoveListener(OnItemGrabbed);
        }
    }
}
