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

    void Start()
    {
        editModeButton.onClick.AddListener(() => OnButtonClicked("RoomSetup"));
        // Example: Game-Map1-Mode1
        playButton.onClick.AddListener(() => OnButtonClicked($"Game-{VariableHolder.mapCode}-{VariableHolder.modeCode}"));
    }

    void OnButtonClicked(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        CarAnimator.SetBool("isTurningToNextScene", true);

        StartCoroutine(DelayedSceneTransition(sceneName));
    }

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneName);
    }
}
