using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ModeSelection : MonoBehaviour
{
    // UI Components
    public Button previousButton;
    public Button nextButton;
    public Button backButon;
    public Text modeText;

    // GameObjects
    public GameObject TotalNumberUI;
    public GameObject MapUI;
    public GameObject ModeUI;

    // Defines
    public int currentModeIndex = 1;
    private const int maxModes = 3;

    void Start()
    {
        nextButton.onClick.AddListener(OnNextClicked);
        previousButton.onClick.AddListener(OnPreviousClicked);
        backButon.onClick.AddListener(OnBackButtonClicked);

        UpdateModeText();
        previousButton.interactable = false;
    }

    void OnNextClicked()
    {
        if (currentModeIndex < maxModes)
        {
            currentModeIndex++;
            UpdateModeText();
        }
        previousButton.interactable = currentModeIndex > 1;
        nextButton.interactable = currentModeIndex < maxModes;
    }

    void OnPreviousClicked()
    {
        if (currentModeIndex > 1)
        {
            currentModeIndex--;
            UpdateModeText();
        }
        previousButton.interactable = currentModeIndex > 1;
        nextButton.interactable = currentModeIndex < maxModes;
    }

    void UpdateModeText()
    {
        switch (currentModeIndex)
        {
            case 1:
                modeText.text = "Game Mode 1\n(Secret Murder)";
                break;
            case 2:
                modeText.text = "Game Mode 2\n(Vote Murder)";
                break;
            case 3:
                modeText.text = "Game Mode 3\n(Random Mode)";
                break;
            default:
                modeText.text = "Unknown Mode";
                break;
        }
    }

    void OnBackButtonClicked()
    {
        TotalNumberUI.SetActive(true);
        MapUI.SetActive(false);
        ModeUI.SetActive(false);
    }
}
