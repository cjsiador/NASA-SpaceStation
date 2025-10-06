using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnomalyUIManager : MonoBehaviour
{
    // public AnomalyIcon anomalyIcon;

    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image anomalyImage;

    public void UpdateAnomaly(AnomalyIcon data)
    {
        titleText.text = data.title;
        descriptionText.text = data.description;
        anomalyImage.sprite = data.anomalyImgs[0];
    }
}
