using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject infoPanel;
    public Button infoButton;
    public TextMeshProUGUI buttonText;

    [Header("Settings")]
    public bool showOnStart = true;

    private bool isInfoVisible;

    void Start()
    {
        if (infoButton != null)
        {
            infoButton.onClick.AddListener(ToggleInfo);
        }

        if (showOnStart)
        {
            ShowInfo();
        }
        else
        {
            HideInfo();
        }
    }

    public void ToggleInfo()
    {
        if (isInfoVisible)
        {
            HideInfo();
        }
        else
        {
            ShowInfo();
        }
    }

    public void ShowInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        isInfoVisible = true;

        if (buttonText != null)
        {
            buttonText.text = "HIDE INFO";
        }
    }

    public void HideInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        isInfoVisible = false;

        if (buttonText != null)
        {
            buttonText.text = "SHOW INFO";
        }
    }
}