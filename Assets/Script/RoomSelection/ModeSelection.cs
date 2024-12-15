using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ModeSelection : MonoBehaviour
{
    // UI Components
    public Button previousButton;
    public Button nextButton;
    public Button modeButton;
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
        modeButton.onClick.AddListener(OnModeButtonClicked);
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
        if (currentModeIndex == 1)
        {
            modeText.text = "Game Mode 1\n(Secret Murder)";
            VariableHolder.modeCode = "Mode1";
        }
        else if (currentModeIndex == 2)
        {
            modeText.text = "Game Mode 2\n(Vote Murder)";
            VariableHolder.modeCode = "Mode2";
        }
        else if (currentModeIndex == 3)
        {
            modeText.text = "Game Mode 3\n(Zombie Murder)";
            VariableHolder.modeCode = "Mode3";
        }
    }

    void OnModeButtonClicked()
    {
        Debug.Log("Mode: " + currentModeIndex);
    }

    void OnBackButtonClicked()
    {
        TotalNumberUI.SetActive(true);
        MapUI.SetActive(false);
        ModeUI.SetActive(false);
    }
}
