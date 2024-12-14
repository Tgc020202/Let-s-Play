using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndGameSceneBehaviour : MonoBehaviourPunCallbacks
{
    public Button winLoseButton;
    public Button returnButton;
    private AudioSource BackgroundMusic;
    public Animator CarAnimator;
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;
    private bool isTransitioning = false;

    private int numberOfPlayers;
    private int maxNumberOfBosses;
    private int maxNumberOfWorkers;
    private int currentMapIndex;
    private int currentModeIndex;
    private string roomName;

    void Start()
    {
        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic")?.GetComponent<AudioSource>();

        if (BackgroundMusic != null)
        {
            BackgroundMusic.Play();
        }
        else
        {
            Debug.LogError($"BackgroundMusic not found at AudioManager/BackgroundMusic. Ensure AudioManager is set up correctly.");
        }
        GreenTrafficLight.SetActive(false);

        if (RoomManager.Instance != null)
        {
            if (winLoseButton != null)
            {
                Text winLoseMessage = winLoseButton.GetComponentInChildren<Text>();
                if (winLoseMessage != null)
                {
                    winLoseMessage.text = RoomManager.Instance.isBossWin ? "Boss wins!!!\nAll of you back to work!!!" : "Staff wins!!!\nLet's go back home!!!";
                }
                else
                {
                    Debug.LogError("No Text component found in the button's children.");
                }
            }
        }
        else
        {
            Debug.LogError("RoomManager.Instance is null! Please initialize it.");
        }

        if (RoomManager.Instance != null)
        {
            // Get temporary room info
            numberOfPlayers = RoomManager.Instance.numberOfPlayers;
            maxNumberOfBosses = RoomManager.Instance.maxNumberOfBosses;
            maxNumberOfWorkers = RoomManager.Instance.maxNumberOfWorkers;
            currentMapIndex = RoomManager.Instance.currentMapIndex;
            currentModeIndex = RoomManager.Instance.currentModeIndex;
            roomName = RoomManager.Instance.roomName;
        }
        else
        {
            Debug.LogError("RoomManager.Instance is null! Please initialize it.");
        }

        // Ensure only the MasterClient recreates the room
        if (PhotonNetwork.IsMasterClient)
        {
            OnLeaveRoom();
            StartCoroutine(OnRecreateRoomSetup());
        }
        else
        {
            OnLeaveRoom();
            StartCoroutine(RejoinRoomAfterDelay());
        }
    }

    private IEnumerator OnRecreateRoomSetup()
    {
        yield return new WaitForSeconds(2f);

        if (isTransitioning) yield break;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)numberOfPlayers,
            IsOpen = true,
            IsVisible = true
        };

        Hashtable customProperties = new Hashtable { { "roomCode", roomName } };
        options.CustomRoomProperties = customProperties;

        PhotonNetwork.CreateRoom(roomName, options, null);
        Debug.Log($"Creating room: {roomName}");

        yield return new WaitForSeconds(2f);
        StartCoroutine(EndGameAndRecreateRoom());
    }

    private IEnumerator RejoinRoomAfterDelay()
    {
        yield return new WaitForSeconds(8f);

        if (isTransitioning) yield break;

        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        Debug.Log("Rejoined room successfully. Transitioning to waiting room.");
        StartCoroutine(EndGameAndRecreateRoom());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}. Retrying...");
        StartCoroutine(RejoinRoomAfterDelay());
    }

    private IEnumerator EndGameAndRecreateRoom()
    {
        RedTrafficLight.SetActive(false);
        GreenTrafficLight.SetActive(true);
        CarAnimator.SetBool("isTurningToNextScene", true);

        yield return new WaitForSeconds(2f);
        BackgroundMusic.Stop();

        OnReturnWaitingRoom();
    }

    private void OnReturnWaitingRoom()
    {
        PhotonNetwork.LoadLevel("WaitingRoomScene");
    }

    private void OnLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

}
