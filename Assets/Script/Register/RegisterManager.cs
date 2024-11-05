using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RegisterManager : MonoBehaviour
{
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Text usernameInvalidMessage;
    public Text passwordInvalidMessage;
    public Button submitButton;
    public DatabaseManager dbManager;

    // Audio
    private AudioSource BackgroundMusic;
    public Animator CarAnimator;
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;

    private bool isTransitioning = false;
    private bool isUsernameExists = false;

    void Start()
    {
        GreenTrafficLight.SetActive(false);
        usernameInvalidMessage.text = "";
        passwordInvalidMessage.text = "";
        submitButton.onClick.AddListener(OnSubmitButtonClick);

        // Audio
        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();
    }

    void Update()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(username))
        {
            usernameInvalidMessage.text = "Username cannot be empty!!!";
            isUsernameExists = false;
        }
        else if (isUsernameExists)
        {
            usernameInvalidMessage.text = "Username already exists. Please choose a different username.";
        }
        else
        {
            usernameInvalidMessage.text = "";
        }

        if (string.IsNullOrEmpty(password))
        {
            passwordInvalidMessage.text = "Password cannot be empty!!!";
        }
        else if (!IsValidPassword(password))
        {
            passwordInvalidMessage.text = "Password must contain digits and letters, and more than 5 characters";
        }
        else
        {
            passwordInvalidMessage.text = "";
        }
    }

    void OnSubmitButtonClick()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (!string.IsNullOrEmpty(username) && IsValidPassword(password))
        {
            if (isTransitioning) return;
            isTransitioning = true;

            StartCoroutine(dbManager.CheckUsernameExists(username, usernameExists =>
            {
                if (!usernameExists)
                {
                    isUsernameExists = false;
                    usernameInvalidMessage.text = "";
                    dbManager.CreateUser(username, password);

                    RedTrafficLight.SetActive(false);
                    GreenTrafficLight.SetActive(true);

                    CarAnimator.SetBool("isTurningToNextScene", true);

                    Debug.Log("Register Successfully!");
                    StartCoroutine(DelayedSceneTransition("LoginScene"));

                    // Stop the background music
                    BackgroundMusic.Stop();

                }
                else
                {
                    isTransitioning = false;
                    isUsernameExists = true;
                    Debug.Log("Username already exists. Please choose a different username.");
                }
            }));
        }
        else
        {
            Debug.Log("Register Failed! Invalid username or password.");
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

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneName);
    }
}