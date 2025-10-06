using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnomalyUIManager : MonoBehaviour
{
    public static AnomalyUIManager Instance;

    [Header("UI References")]
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image anomalyImage;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowAnomaly(AnomalyDataStructure data)
    {
        titleText.text = data.anomalyName;
        descriptionText.text = data.description;
        anomalyImage.sprite = data.anomalyImage;
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }
}
