using UnityEngine;

public class AnomalyIcon : MonoBehaviour
{
    public AnomalyDataStructure anomalyInfo; // assigned in Inspector

    public void OnSelect()
    {
        // When clicked, tell the UI Manager to show details
        AnomalyUIManager.Instance.ShowAnomaly(anomalyInfo);
    }
}
