using UnityEngine;

[System.Serializable]
public class AnomalyDataStructure
{
    public string anomalyName;
    [TextArea] public string description;
    public Sprite anomalyImage;
}