using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RegisterManager : MonoBehaviour
{
    // UI Components
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Text usernameInvalidMessage;
    public Text passwordInvalidMessage;
    public Button submitButton;
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
    private bool isUsernameExistsInDB = false;
    private string username;
    private string password;

    // Messages
    private const string UsernameInvalidMessage = "Username already exists. Please choose a different username.";
    private const string PasswordInvalidMessage = "Password must contain digits, letters and more than 5 characters.";
    private const string UsernameEmptyMessage = "Username cannot be empty.";
    private const string PasswordEmptyMessage = "Password cannot be empty.";
    private const string EmptyMessage = "";

    void Start()
    {
        GreenTrafficLight.SetActive(false);
        usernameInvalidMessage.text = EmptyMessage;
        passwordInvalidMessage.text = EmptyMessage;

        submitButton.onClick.AddListener(OnSubmitButtonClick);

        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();
    }

    void Update()
    {
        username = usernameInputField.text;
        password = passwordInputField.text;

        // Username detects
        if (string.IsNullOrEmpty(username))
        {
            usernameInvalidMessage.text = UsernameEmptyMessage;
        }
        else if (isUsernameExistsInDB)
        {
            usernameInvalidMessage.text = UsernameInvalidMessage;
        }
        else
        {
            usernameInvalidMessage.text = EmptyMessage;
        }

        // Password detects
        if (string.IsNullOrEmpty(password))
        {
            passwordInvalidMessage.text = PasswordEmptyMessage;
        }
        else if (!IsValidPassword(password))
        {
            passwordInvalidMessage.text = PasswordInvalidMessage;
        }
        else
        {
            passwordInvalidMessage.text = EmptyMessage;
        }
    }

    void OnSubmitButtonClick()
    {
        username = usernameInputField.text;
        password = passwordInputField.text;

        if (!string.IsNullOrEmpty(username) && IsValidPassword(password))
        {
            StartCoroutine(dbManager.CheckUsernameExists(username, usernameExists =>
            {
                if (!usernameExists)
                {
                    isUsernameExistsInDB = false;
                    usernameInvalidMessage.text = EmptyMessage;

                    // Initialize status to false
                    dbManager.CreateUser(username, password, false);

                    if (isTransitioning) return;
                    isTransitioning = true;

                    LoadAnimation("LoginScene");
                }
                else
                {
                    isUsernameExistsInDB = true;
                }
            }));
        }
    }

    bool IsValidPassword(string password)
    {
        if (password.Length <= 5) return false;

        bool hasLetter = false;
        bool hasDigit = false;

        foreach (char c in password)
        {
            if (char.IsLetter(c)) hasLetter = true;
            if (char.IsDigit(c)) hasDigit = true;

            if (hasLetter && hasDigit) return true;
        }

        return false;
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