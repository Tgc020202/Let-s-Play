using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ModeSelection : MonoBehaviour
{
    public Button previousButton;
    public Button nextButton;
    public Button modeButton;
    public Text modeText;
    public Animator CarAnimator;
    private bool isTransitioning = false;
    private int currentModeIndex = 1;
    private const int maxModes = 3;

    void Start()
    {
        // Assign listeners for map selection
        nextButton.onClick.AddListener(OnNextClicked);
        previousButton.onClick.AddListener(OnPreviousClicked);
        modeButton.onClick.AddListener(() => OnModeButtonClicked("RoomScene"));

        // Initialize the map name and button states
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

    void OnModeButtonClicked(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        CarAnimator.SetBool("isTurningToNextScene", true);

        // Start a coroutine to delay the scene transition
        StartCoroutine(DelayedSceneTransition(sceneName));
    }

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Move to the specified scene
        SceneManager.LoadScene(sceneName);
    }
}
