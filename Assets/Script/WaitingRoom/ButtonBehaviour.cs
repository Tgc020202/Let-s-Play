using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonBehaviour : MonoBehaviour
{
    public Button editModeButton;
    public Button playButton;
    public Animator CarAnimator;
    private bool isTransitioning = false;

    // Start is called before the first frame update
    void Start()
    {
        editModeButton.onClick.AddListener(() => OnButtonClicked("RoomSetup"));
        // Game-Map1-Mode1-Boss
        // playButton.onClick.AddListener(() => OnButtonClicked($"Game-{VariableHolder.mapCode}-{VariableHolder.modeCode}-Boss"));
        playButton.onClick.AddListener(() => OnButtonClicked($"Game-{VariableHolder.mapCode}-{VariableHolder.modeCode}-Worker"));
    }

    void OnButtonClicked(string sceneName)
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
