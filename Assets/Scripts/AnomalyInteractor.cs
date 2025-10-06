using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AnomalyIconInteractor : MonoBehaviour
{
    public AnomalyUIManager uiManager;
    private AnomalyIcon iconData;
    public XRSimpleInteractable simpleInteractable;

    void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        iconData = GetComponent<AnomalyIcon>();

        simpleInteractable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (uiManager != null && iconData != null)
        {
            uiManager.UpdateAnomaly(iconData);
        }

        // HAPTICS: Trigger vibration on controller
        var interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;
        if (interactor != null)
        {
            interactor.SendHapticImpulse(0.5f, 0.5f);
        }
    }

    private void OnDestroy()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }
}
