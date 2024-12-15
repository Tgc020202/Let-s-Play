using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    // UI Components
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Text usernameInvalidMessage;
    public Text passwordInvalidMessage;
    public Button playButton;
    public Button backToRegisterButton;

    // Scripts
    public DatabaseManager dbManager;

    // Audio
    private AudioSource BackgroundMusic;

    // Animations
    public Animator CarAnimator;

    // GameObjects
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;

    // Defines
    private bool isTransitioning = false;
    private string username;
    private string password;

    // Messages
    private const string UsernameNotExistMessage = "Username does not exist.";
    private const string IncorrectPasswordMessage = "Incorrect password.";
    private const string StatusIsActiveMessage = "Username is active.";
    private const string EmptyMessage = "";

    void Start()
    {
        GreenTrafficLight.SetActive(false);
        usernameInvalidMessage.text = EmptyMessage;
        passwordInvalidMessage.text = EmptyMessage;

        playButton.onClick.AddListener(OnPlayButtonClick);
        backToRegisterButton.onClick.AddListener(OnBackToRegisterButton);

        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();
    }

    void OnPlayButtonClick()
    {
        username = usernameInputField.text;
        password = passwordInputField.text;

        StartCoroutine(dbManager.GetUser(username, user =>
        {
            if (user != null && password == user.password && user.status == false)
            {
                usernameInvalidMessage.text = EmptyMessage;
                passwordInvalidMessage.text = EmptyMessage;

                dbManager.UpdateUserStatus(username, true);

                if (SessionManager.Instance != null) SessionManager.Instance.username = username;

                if (isTransitioning) return;
                isTransitioning = true;
                LoadAnimation("LobbyScene");
            }
            else
            {
                if (user == null)
                {
                    usernameInvalidMessage.text = UsernameNotExistMessage;
                }
                else if (password != user.password)
                {
                    passwordInvalidMessage.text = IncorrectPasswordMessage;
                }
                else if (user.status != null && user.status == true)
                {
                    usernameInvalidMessage.text = StatusIsActiveMessage;
                }
                else
                {
                    Debug.LogWarning("Something wrong.");
                }
            }
        }));
    }

    void OnBackToRegisterButton()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        LoadAnimation("RegisterScene");
    }

    void LoadAnimation(string sceneName)
    {
        RedTrafficLight.SetActive(false);
        GreenTrafficLight.SetActive(true);
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