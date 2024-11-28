using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndGameSceneBehaviour : MonoBehaviourPunCallbacks
{
    public Button returnButton;
    private AudioSource BackgroundMusic;

    private bool isTransitioning = false;

    private int numberOfPlayers;
    private int maxNumberOfBosses;
    private int maxNumberOfWorkers;
    private int currentMapIndex;
    private int currentModeIndex;
    private string roomName;

    void Start()
    {
        BackgroundMusic = GameObject.Find("AudioManager/BackgroundWinMusic").GetComponent<AudioSource>();

        // Get temporary room info
        numberOfPlayers = RoomManager.Instance.numberOfPlayers;
        maxNumberOfBosses = RoomManager.Instance.maxNumberOfBosses;
        maxNumberOfWorkers = RoomManager.Instance.maxNumberOfWorkers;
        currentMapIndex = RoomManager.Instance.currentMapIndex;
        currentModeIndex = RoomManager.Instance.currentModeIndex;
        roomName = RoomManager.Instance.roomName;

        returnButton.onClick.AddListener(OnReturnButtonClick);
        returnButton.interactable = false;

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

    void OnReturnButtonClick()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(EndGameAndRecreateRoom());
    }

    private IEnumerator EndGameAndRecreateRoom()
    {
        yield return new WaitForSeconds(2f);
        BackgroundMusic.Stop();

        OnReturnWaitingRoom();
    }

    private IEnumerator OnRecreateRoomSetup()
    {
        yield return new WaitForSeconds(2f);

        // Store variables
        RoomManager.Instance.numberOfPlayers = numberOfPlayers;
        RoomManager.Instance.maxNumberOfBosses = maxNumberOfBosses;
        RoomManager.Instance.maxNumberOfWorkers = maxNumberOfWorkers;
        RoomManager.Instance.currentMapIndex = currentMapIndex;
        RoomManager.Instance.currentModeIndex = currentModeIndex;
        RoomManager.Instance.roomName = roomName;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)numberOfPlayers,
            IsOpen = true,
            IsVisible = true
        };

        Hashtable customProperties = new Hashtable { { "roomCode", roomName } };
        options.CustomRoomProperties = customProperties;

        PhotonNetwork.CreateRoom(roomName, options, null);

        yield return new WaitForSeconds(2f);
        returnButton.interactable = true;
    }

    private IEnumerator RejoinRoomAfterDelay()
    {
        yield return new WaitForSeconds(10f);

        PhotonNetwork.JoinRoom(roomName);
        returnButton.interactable = true;
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
