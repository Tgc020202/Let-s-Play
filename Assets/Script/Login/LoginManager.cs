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
    public Button loginButton;
    public Button goToRegisterButton;
    public Animator CarAnimator;
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;
    public DatabaseManager dbManager;
    private bool isTransitioning = false;

    void Start()
    {
        GreenTrafficLight.SetActive(false);
        usernameInvalidMessage.text = "";
        passwordInvalidMessage.text = "";
        loginButton.onClick.AddListener(OnLoginButtonClick);
        goToRegisterButton.onClick.AddListener(OnGoToRegisterButton);
    }

    void OnLoginButtonClick()
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
                // StartCoroutine(DelayedSceneTransition("RoomSelectionsScene"));
                StartCoroutine(DelayedSceneTransition("GameView"));
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

    void OnGoToRegisterButton()
    {
        SceneManager.LoadScene("RegisterScene");
    }

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneName);
    }
}