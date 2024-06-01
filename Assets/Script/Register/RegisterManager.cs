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

        if (!string.IsNullOrEmpty(username) && IsValidPassword(password))
        {
            if (isTransitioning) return;
            isTransitioning = true;
            CarAnimator.SetBool("isTurningToNextScene", true);

            // Start a coroutine to delay the scene transition
            Debug.Log("Register Successfully!");
            StartCoroutine(DelayedSceneTransition("LoginScene"));
        }
        else
        {
            Debug.Log("Register Failed!");
        }
    }

    bool IsValidPassword(string password)
    {
        if (password.Length <= 5)
        {
            return false;
        }

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
