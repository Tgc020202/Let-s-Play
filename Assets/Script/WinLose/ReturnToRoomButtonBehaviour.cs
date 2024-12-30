using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndGameSceneBehaviour : MonoBehaviourPunCallbacks
{
    // UI Components
    public Button winLoseButton;
    public Button returnButton;

    // Audio
    private AudioSource BackgroundMusic;

    // Animations
    public Animator CarAnimator;

    // GameObjects
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;

    // Defines
    private bool isTransitioning = false;
    private int numberOfPlayers;
    private int maxNumberOfBosses;
    private int maxNumberOfWorkers;
    private int currentMapIndex;
    private int currentModeIndex;
    private bool isPrivate;
    private string roomName;

    void Start()
    {
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
            numberOfPlayers = RoomManager.Instance.numberOfPlayers;
            maxNumberOfBosses = RoomManager.Instance.maxNumberOfBosses;
            maxNumberOfWorkers = RoomManager.Instance.maxNumberOfWorkers;
            currentMapIndex = RoomManager.Instance.currentMapIndex;
            currentModeIndex = RoomManager.Instance.currentModeIndex;
            isPrivate = RoomManager.Instance.isPrivate;
            roomName = RoomManager.Instance.roomName;
        }
        else
        {
            Debug.LogError("RoomManager.Instance is null! Please initialize it.");
        }

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

        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic")?.GetComponent<AudioSource>();

        if (BackgroundMusic != null)
        {
            BackgroundMusic.Play();
        }
    }

    private IEnumerator OnRecreateRoomSetup()
    {
        yield return new WaitForSeconds(2f);

        if (isTransitioning) yield break;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)numberOfPlayers,
            IsVisible = !isPrivate,
            IsOpen = true
        };

        Hashtable customProperties = new Hashtable
        {
            { "roomCode", roomName },
            { "currentMapIndex", currentMapIndex },
            { "currentModeIndex", currentModeIndex },
            { "numberOfPlayers", numberOfPlayers }
        };

        options.CustomRoomProperties = customProperties;
        options.CustomRoomPropertiesForLobby = new string[] { "roomCode", "currentMapIndex", "currentModeIndex", "numberOfPlayers" };

        PhotonNetwork.CreateRoom(roomName, options, null);

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
        Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (customProperties.ContainsKey("roomCode"))
        {
            string roomCode = customProperties["roomCode"].ToString();
            RoomManager.Instance.roomName = roomCode;
            Debug.Log("Room Code: " + roomCode);
        }

        if (customProperties.ContainsKey("currentMapIndex"))
        {
            int mapIndex = (int)customProperties["currentMapIndex"];
            RoomManager.Instance.currentMapIndex = mapIndex;
            Debug.Log("Map Index: " + mapIndex);
        }

        if (customProperties.ContainsKey("currentModeIndex"))
        {
            int modeIndex = (int)customProperties["currentModeIndex"];
            RoomManager.Instance.currentModeIndex = modeIndex;
            Debug.Log("Mode Index: " + modeIndex);
        }

        if (customProperties.ContainsKey("numberOfPlayers"))
        {
            int numberOfPlayers = (int)customProperties["numberOfPlayers"];
            RoomManager.Instance.numberOfPlayers = numberOfPlayers;
            Debug.Log("Number of Players: " + numberOfPlayers);
        }

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
