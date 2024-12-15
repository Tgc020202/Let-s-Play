// using System.Collections;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;

// public class ButtonBehaviour : MonoBehaviour
// {
//     // UI Components
//     public Button editModeButton;
//     public Button playButton;

//     // Animations
//     public Animator CarAnimator;

//     // Defines
//     private bool isTransitioning = false;

//     void Start()
//     {
//         editModeButton.onClick.AddListener(() => OnButtonClicked("RoomSetup"));
//         playButton.onClick.AddListener(() => OnButtonClicked($"Game-{VariableHolder.mapCode}-{VariableHolder.modeCode}"));
//     }

//     void OnButtonClicked(string sceneName)
//     {
//         if (isTransitioning) return;
//         isTransitioning = true;
//         CarAnimator.SetBool("isTurningToNextScene", true);

//         StartCoroutine(DelayedSceneTransition(sceneName));
//     }

//     IEnumerator DelayedSceneTransition(string sceneName)
//     {
//         yield return new WaitForSeconds(2f);
//         SceneManager.LoadScene(sceneName);
//     }
// }
