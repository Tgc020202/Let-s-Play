// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;
// using System.Collections;
// public class RoomSelectionButton : MonoBehaviour

// {
//     // UI Components
//     public Button createRoomButton;
//     public Button joinRoomButton;

//     // Animations
//     public Animator CarAnimator;

//     // GameObjects
//     public GameObject RedTrafficLight;
//     public GameObject GreenTrafficLight;

//     // Defines
//     private bool isTransitioning = false;

//     void Start()
//     {
//         GreenTrafficLight.SetActive(false);

//         createRoomButton.onClick.AddListener(() => OnButtonClicked("RoomSetup"));
//         joinRoomButton.onClick.AddListener(() => OnButtonClicked("LobbySelectionScene"));
//     }

//     void OnButtonClicked(string sceneName)
//     {
//         if (isTransitioning) return;
//         isTransitioning = true;

//         LoadAnimation(sceneName);
//     }

//     void LoadAnimation(string sceneName)
//     {
//         RedTrafficLight.SetActive(false);
//         GreenTrafficLight.SetActive(true);
//         CarAnimator.SetBool("isTurningToNextScene", true);

//         StartCoroutine(DelayedSceneTransition(sceneName));
//     }

//     IEnumerator DelayedSceneTransition(string sceneName)
//     {
//         yield return new WaitForSeconds(2f);
//         SceneManager.LoadScene(sceneName);
//         BackgroundMusic.Stop();
//     }
// }
