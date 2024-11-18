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
    private int currentMapIndex;
    private int currentModeIndex;
    private string roomName;

    void Start()
    {
        BackgroundMusic = GameObject.Find("AudioManager/BackgroundWinMusic").GetComponent<AudioSource>();

        // Get temporary room info
        numberOfPlayers = RoomManager.Instance.numberOfPlayers;
        currentMapIndex = RoomManager.Instance.currentMapIndex;
        currentModeIndex = RoomManager.Instance.currentModeIndex;
        roomName = RoomManager.Instance.roomName;

        returnButton.onClick.AddListener(OnReturnButtonClick);
        returnButton.interactable = false;

        // Ensure only the MasterClient recreates the room
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
            StartCoroutine(OnRecreateRoomSetup());
        }
        else
        {
            PhotonNetwork.LeaveRoom();
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

        // All players return to the waiting room
        OnReturnWaitingRoom();
    }

    private IEnumerator OnRecreateRoomSetup()
    {
        // Wait for Photon to process room leave
        yield return new WaitForSeconds(5f);

        // Store variables
        RoomManager.Instance.numberOfPlayers = numberOfPlayers;
        RoomManager.Instance.currentMapIndex = currentMapIndex;
        RoomManager.Instance.currentModeIndex = currentModeIndex;
        RoomManager.Instance.roomName = roomName;

        // Create new room with the same name and properties
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)numberOfPlayers,
            IsOpen = true,
            IsVisible = true
        };

        // Custom room properties if needed
        Hashtable customProperties = new Hashtable { { "roomCode", roomName } };
        options.CustomRoomProperties = customProperties;

        PhotonNetwork.CreateRoom(roomName, options, null);

        // Wait for room creation to finish
        yield return new WaitForSeconds(2f);
        returnButton.interactable = true;
    }

    private IEnumerator RejoinRoomAfterDelay()
    {
        // Join the room again after a short delay
        yield return new WaitForSeconds(20f);

        // After the MasterClient recreates the room, join it
        PhotonNetwork.JoinRoom(roomName);
        returnButton.interactable = true;
    }

    private void OnReturnWaitingRoom()
    {
        // Load the waiting room scene for all players
        PhotonNetwork.LoadLevel("WaitingRoomScene");
    }

    // public override void OnDisconnected(DisconnectCause cause)
    // {
    //     SceneManager.LoadScene("LobbyScene");
    // }
}
