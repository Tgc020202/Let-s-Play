using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public InputField roomNameInput;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button publicButton;
    public Button privateButton;
    public Button modeButton;
    public InputField roomCodeInput;
    public Transform roomListContent;
    public GameObject roomItemPrefab;
    public NumberOfPlayerSelection numberOfPlayerSelection;
    public MapSelection mapSelection;
    public ModeSelection modeSelection;

    // Panels
    public GameObject RoomSelectionUI;
    public GameObject PlayerSetupUI;
    public GameObject RoomSetupUI;
    public GameObject JoinRoomUI;
    // public GameObject WaitingRoomUI;

    // Audio
    private AudioSource BackgroundMusic;
    private Dictionary<string, GameObject> uiDictionary;
    private bool isPrivate = false;
    private bool isConnectedToMaster = false;

    private void Start()
    {
        RoomManager.Instance.ConnectToPhoton();

        // Audio
        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();

        // UI
        uiDictionary = new Dictionary<string, GameObject>
    {
        { "RoomSelectionUI", RoomSelectionUI },
        { "PlayerSetupUI", PlayerSetupUI },
        { "RoomSetupUI", RoomSetupUI },
        { "JoinRoomUI", JoinRoomUI },
    };

        // Button Assign
        createRoomButton.onClick.AddListener(() =>
        {
            OnUpdateNetworkRoleToHost(true);
            ToggleUIActive("PlayerSetupUI", true);
        });

        joinRoomButton.onClick.AddListener(() =>
        {
            OnUpdateNetworkRoleToHost(false);
            ToggleUIActive("JoinRoomUI", true);
        });

        publicButton.onClick.AddListener(() => OnSetPrivateClicked(false));
        privateButton.onClick.AddListener(() => OnSetPrivateClicked(true));
        modeButton.onClick.AddListener(OnCreateRoomButtonClicked);

        roomCodeInput.onEndEdit.AddListener(OnRoomCodeInputEndEdit);

        // UI based on entry point
        ToggleUIActive("RoomSelectionUI", true);
    }

    // UI Handler
    public void ToggleUIActive(string UIName, bool isActive)
    {
        foreach (var ui in uiDictionary)
        {
            ui.Value.SetActive(ui.Key == UIName ? isActive : !isActive);
        }
    }

    public void OnUpdateNetworkRoleToHost(bool isHost)
    {
        VariableHolder.networkRole = isHost ? NetworkRole.Host : NetworkRole.Client;
    }

    public void OnSetPrivateClicked(bool isPrivate)
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            this.isPrivate = isPrivate;
            ToggleUIActive("RoomSetupUI", true);
        }
        else
        {
            Debug.Log("Please enter Room Name");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        isConnectedToMaster = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Successfully joined the lobby.");
    }

    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInput.text;
        string roomCode = roomName;
        int numberOfPlayers = (byte)(numberOfPlayerSelection.bossCount + numberOfPlayerSelection.staffCount);

        // Store variables
        RoomManager.Instance.numberOfPlayers = numberOfPlayers;
        RoomManager.Instance.currentMapIndex = mapSelection.currentMapIndex;
        RoomManager.Instance.currentModeIndex = modeSelection.currentModeIndex;
        RoomManager.Instance.roomName = roomName;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = numberOfPlayers,
            IsVisible = !isPrivate,
            IsOpen = true
        };

        Hashtable customProperties = new Hashtable { { "roomCode", roomCode } };
        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnCreatedRoom()
    {
        SceneManager.LoadScene("WaitingRoomScene");

        string roomCode = roomNameInput.text;
        Hashtable customProperties = new Hashtable { { "roomCode", roomCode } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
    }

    private void OnRoomCodeInputEndEdit(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnJoinRoomByCodeClicked();
        }
    }

    public void OnJoinRoomByCodeClicked()
    {
        if (!isConnectedToMaster)
        {
            Debug.LogError("Cannot join room: Not connected to Master Server.");
            return;
        }

        string roomName = roomCodeInput.text;

        // Store variables
        RoomManager.Instance.roomName = roomName;

        PhotonNetwork.JoinRoom(roomName);
        SceneManager.LoadScene("WaitingRoomScene");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomInfo room in roomList)
        {
            if (!room.RemovedFromList)
            {
                GameObject roomItem = Instantiate(roomItemPrefab, roomListContent);
                roomItem.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
                roomItem.transform.Find("PlayerCountText").GetComponent<Text>().text = $"{room.PlayerCount}/{room.MaxPlayers}";

                Button joinButton = roomItem.GetComponent<Button>();

                // Check if the room is full
                if (room.PlayerCount >= room.MaxPlayers)
                {
                    joinButton.interactable = false; // Disable the join button
                    roomItem.transform.Find("PlayerCountText").GetComponent<Text>().color = Color.red; // Optionally change the color to indicate it's full
                }
                else
                {
                    joinButton.onClick.AddListener(() =>
                    {
                        string roomName = room.Name;

                        // Store variables
                        RoomManager.Instance.roomName = room.Name;
                        PhotonNetwork.JoinRoom(room.Name);
                        SceneManager.LoadScene("WaitingRoomScene");
                    });
                }
            }
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("roomCode"))
        {
            Debug.Log("RoomCode: " + PhotonNetwork.CurrentRoom.CustomProperties["roomCode"].ToString());
        }
    }
}
