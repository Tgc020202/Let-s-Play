using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class LoginManager : MonoBehaviour
{
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Text usernameInvalidMessage;
    public Text passwordInvalidMessage;
    public Button playButton;
    public Button backToRegisterButton;
    public DatabaseManager dbManager;

    // Audio
    private AudioSource BackgroundMusic;
    public Animator CarAnimator;
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;

    private bool isTransitioning = false;

    void Start()
    {
        GreenTrafficLight.SetActive(false);
        usernameInvalidMessage.text = "";
        passwordInvalidMessage.text = "";
        playButton.onClick.AddListener(OnPlayButtonClick);
        backToRegisterButton.onClick.AddListener(OnBackToRegisterButton);

        // Audio
        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();
    }

    void OnPlayButtonClick()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        StartCoroutine(dbManager.GetUser(username, user =>
        {
            if (user != null && password == user.password)
            {
                if (isTransitioning) return;
                isTransitioning = true;

                RedTrafficLight.SetActive(false);
                GreenTrafficLight.SetActive(true);
                usernameInvalidMessage.text = "";
                passwordInvalidMessage.text = "";


                CarAnimator.SetBool("isTurningToNextScene", true);

                Debug.Log("Login Successfully!");
                StartCoroutine(DelayedSceneTransition("LobbyScene"));
            }
            else
            {
                if (user == null)
                {
                    usernameInvalidMessage.text = "UserName is not exists!!!";
                    passwordInvalidMessage.text = "";
                }
                else if (password != user.password)
                {
                    usernameInvalidMessage.text = "";
                    passwordInvalidMessage.text = "Password is wrong!!!";
                }
                Debug.Log("Login Failed!");
            }
        }));
    }

    void OnBackToRegisterButton()
    {
        SceneManager.LoadScene("RegisterScene");

        // Stop the background music
        BackgroundMusic.Stop();
    }

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneName);

        // Stop the background music
        BackgroundMusic.Stop();
    }
}