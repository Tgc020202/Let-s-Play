using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnToRoomButtonBehaviour : MonoBehaviour
{
    public Button returnButton;
    private Text returnButtonText;

    // Audio
    private AudioSource BackgroundMusic;
    public Animator CarAnimator;

    private bool isTransitioning = false;

    void Start()
    {
        returnButtonText = returnButton.GetComponentInChildren<Text>();
        returnButtonText.text = VariableHolder.isBossWin ? "Boss wins!!!\nAll of you back to work!!!" : "Workers win!!!\nBye Bye, Go home!!!";

        // Audio setup
        BackgroundMusic = GameObject.Find("AudioManager/BackgroundWinMusic").GetComponent<AudioSource>();

        returnButton.onClick.AddListener(OnReturnButtonClick);
    }

    void OnReturnButtonClick()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        CarAnimator.SetBool("isTurningToNextScene", true);

        StartCoroutine(DelayedSceneTransition("LobbyScene"));
    }

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        yield return new WaitForSeconds(2f);
        VariableHolder.isFromEndGameToRoom = true;

        // Stop the background music
        BackgroundMusic.Stop();

        // Load the LobbyScene
        SceneManager.LoadScene(sceneName);
    }
}
