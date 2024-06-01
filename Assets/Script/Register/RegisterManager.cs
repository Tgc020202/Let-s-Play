using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RegisterManager : MonoBehaviour
{
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Button registerButton;
    public Animator CarAnimator;
    private bool isTransitioning = false;

    // Start is called before the first frame update
    void Start()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClick);
    }

    void OnRegisterButtonClick()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        // Perform login validation or send to a server, etc.
        Debug.Log("Username: " + username);
        Debug.Log("Password: " + password);

        // Example validation
        if (username == "admin" && password == "12345")
        {
            Debug.Log("Login Successful!");
            if (isTransitioning) return;
            isTransitioning = true;
            CarAnimator.SetBool("isTurningToNextScene", true);

            // Start a coroutine to delay the scene transition
            StartCoroutine(DelayedSceneTransition("LoginScene"));
        }
        else
        {
            Debug.Log("Login Failed!");
        }
    }

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(sceneName);
    }
}
