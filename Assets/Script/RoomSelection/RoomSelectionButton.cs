using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
public class RoomSelectionButton : MonoBehaviour

{
    public Button createRoomButton;
    public Button joinRoomButton;
    public Animator CarAnimator;
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;
    private bool isTransitioning = false;

    void Start()
    {
        GreenTrafficLight.SetActive(false);
        createRoomButton.onClick.AddListener(() => OnButtonClicked("RoomSetupScene"));
        joinRoomButton.onClick.AddListener(() => OnButtonClicked("LobbySelectionScene"));
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
