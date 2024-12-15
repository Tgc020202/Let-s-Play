using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartGameManager : MonoBehaviour
{
    // UI Components
    public Button PlayButton;
    public Button RegisterButton;
    public Button LoginButton;

    // Audio
    private AudioSource BackgroundMusic;

    // Animations
    public Animator CarAnimator;

    // Defines
    private bool isTransitioning = false;

    void Start()
    {
        RegisterButton.gameObject.SetActive(false);
        LoginButton.gameObject.SetActive(false);

        PlayButton.onClick.AddListener(OnPlayButtonClicked);
        RegisterButton.onClick.AddListener(() => OnButtonClicked("RegisterScene"));
        LoginButton.onClick.AddListener(() => OnButtonClicked("LoginScene"));

        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();
    }

    void OnPlayButtonClicked()
    {
        if (isTransitioning) return;

        PlayButton.gameObject.SetActive(false);
        RegisterButton.gameObject.SetActive(true);
        LoginButton.gameObject.SetActive(true);
    }

    void OnButtonClicked(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        LoadAnimation(sceneName);
    }

    void LoadAnimation(string sceneName)
    {
        // RedTrafficLight.SetActive(false);
        // GreenTrafficLight.SetActive(true);
        CarAnimator.SetBool("isTurningToNextScene", true);

        StartCoroutine(DelayedSceneTransition(sceneName));
    }

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneName);
        BackgroundMusic.Stop();
    }
}
