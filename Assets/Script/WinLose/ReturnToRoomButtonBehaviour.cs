using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnToRoomButtonBehaviour : MonoBehaviour
{
    public Button returnButton;
    private Text returnButtonText;

    public Animator CarAnimator;
    private bool isTransitioning = false;
    void Start()
    {
        returnButtonText = returnButton.GetComponentInChildren<Text>();

        returnButtonText.text = VariableHolder.isBossWin ? "Boss wins!!!\nAll of you back to work!!!" : "Workers win!!!\nBye Bye, Go home!!!";

        returnButton.onClick.AddListener(() =>
        {
            if (isTransitioning) return;
            isTransitioning = true;
            CarAnimator.SetBool("isTurningToNextScene", true);

            // Start a coroutine to delay the scene transition
            StartCoroutine(DelayedSceneTransition("RoomScene"));
        }
        );
    }

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Move to the specified scene
        SceneManager.LoadScene(sceneName);
    }
}
