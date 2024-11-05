using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartGameManager : MonoBehaviour
{
    public Button PlayButton;
    public Button RegisterButton;
    public Button LoginButton;

    // Audio
    private AudioSource BackgroundMusic;
    public Animator CarAnimator;
    private bool isTransitioning = false;

    void Start()
    {
        // Initially hide Register and Login buttons
        RegisterButton.gameObject.SetActive(false);
        LoginButton.gameObject.SetActive(false);

        // Audio
        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();

        // Add listeners to all the buttons
        PlayButton.onClick.AddListener(OnPlayButtonClicked);
        RegisterButton.onClick.AddListener(() => OnButtonClicked("RegisterScene"));
        LoginButton.onClick.AddListener(() => OnButtonClicked("LoginScene"));
    }

    void OnPlayButtonClicked()
    {
        if (isTransitioning) return;
        // Hide the Play button
        PlayButton.gameObject.SetActive(false);

        // Show the Register and Login buttons
        RegisterButton.gameObject.SetActive(true);
        LoginButton.gameObject.SetActive(true);
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
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneName);

        // Stop the background music
        BackgroundMusic.Stop();
    }
}
