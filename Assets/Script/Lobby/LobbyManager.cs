using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    // UI Elements
    public InputField roomNameInput;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button publicButton;
    public Button privateButton;
    public Button modeButton;
    public InputField roomCodeInput;
    public Text roomCodeText;
    public Button playButton; // Only for room owner
    public Button readyButton; // Only for room joiner
    public Transform roomListContent;
    public GameObject roomItemPrefab; // Prefab for each room item in the list
    public NumberOfPlayerSelection numberOfPlayerSelection;
    public MapSelection mapSelection;
    public ModeSelection modeSelection;

    // Panels
    public GameObject RoomSelectionUI;
    public GameObject PlayerSetupUI;
    public GameObject RoomSetupUI;
    public GameObject JoinRoomUI;
    public GameObject WaitingRoomUI;

    // Audio
    private AudioSource BackgroundMusic;
    private Dictionary<string, GameObject> uiDictionary;
    private bool isPrivate = false;
    private bool isConnectedToMaster = false;
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
        playButton.interactable = PhotonNetwork.IsMasterClient; // Only room owner can start

        // Audio
        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();

        // UI
        uiDictionary = new Dictionary<string, GameObject>
        {
            { "RoomSelectionUI", RoomSelectionUI },
            { "PlayerSetupUI", PlayerSetupUI },
            { "RoomSetupUI", RoomSetupUI },
            { "JoinRoomUI", JoinRoomUI },
            { "WaitingRoomUI", WaitingRoomUI }
        };

        // Button Assign
        createRoomButton.onClick.AddListener(() =>
        {
            OnUpdateNetworkRoleToHost(true);
            ToggleUIActive("PlayerSetupUI", true);
        }
        );
        joinRoomButton.onClick.AddListener(() =>
        {
            OnUpdateNetworkRoleToHost(false);
            ToggleUIActive("JoinRoomUI", true);
        }
        );
        publicButton.onClick.AddListener(() => OnSetPrivateClicked(false));
        privateButton.onClick.AddListener(() => OnSetPrivateClicked(true));
        modeButton.onClick.AddListener(OnCreateRoomButtonClicked);
        playButton.onClick.AddListener(OnPlayButtonClicked);
        readyButton.onClick.AddListener(OnReadyButtonClicked);

        roomCodeInput.onEndEdit.AddListener(OnRoomCodeInputEndEdit);

        // Initialize the first UI
        if (VariableHolder.isFromEndGameToRoom)
        {
            ToggleUIActive("WaitingRoomUI", true);
            VariableHolder.isFromEndGameToRoom = false;
        }
        else
        {
            ToggleUIActive("RoomSelectionUI", true);
        }
    }

    // Create Room
    public void OnSetPrivateClicked(bool isPrivate)
    {
        this.isPrivate = isPrivate; // Correctly set the private flag
        ToggleUIActive("RoomSetupUI", true);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        isConnectedToMaster = true;
        PhotonNetwork.JoinLobby(); // Join the lobby after connecting
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Successfully joined the lobby.");
    }

    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInput.text;
        string roomCode = roomName; // or generate a unique code

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)(numberOfPlayerSelection.bossCount + numberOfPlayerSelection.staffCount),
            IsVisible = !isPrivate,
            IsOpen = true
        };

        Hashtable customProperties = new Hashtable { { "roomCode", roomCode } };
        PhotonNetwork.CreateRoom(roomName, options, null);
    }


    public override void OnCreatedRoom()
    {
        ToggleUIActive("WaitingRoomUI", true);

        // Set the custom properties for the room after it's created
        string roomCode = roomNameInput.text; // or use a generated code
        Hashtable customProperties = new Hashtable { { "roomCode", roomCode } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

        // Display room code
        roomCodeText.text = roomCode.ToString(); // Display the room code
    }

    // Join Room by code
    private void OnRoomCodeInputEndEdit(string input)
    {
        // Check if Enter key was pressed
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
            return; // Exit if not connected
        }

        string roomCode = roomCodeInput.text;
        PhotonNetwork.JoinRoom(roomCode);
        ToggleUIActive("WaitingRoomUI", true);
    }

    // Populate Room List for Public Rooms
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject); // Clear existing room list
        }

        foreach (RoomInfo room in roomList)
        {
            if (!room.RemovedFromList)
            {
                GameObject roomItem = Instantiate(roomItemPrefab, roomListContent);
                roomItem.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
                roomItem.transform.Find("PlayerCountText").GetComponent<Text>().text = $"{room.PlayerCount}/{room.MaxPlayers}";

                Button joinButton = roomItem.GetComponent<Button>();
                joinButton.onClick.AddListener(() =>
                {
                    PhotonNetwork.JoinRoom(room.Name);  // no happen anything
                    ToggleUIActive("WaitingRoomUI", true);
                });
            }
        }
    }

    // Ready Button Clicked - Sets "isReady" property for each player
    public void OnReadyButtonClicked()
    {
        Hashtable playerProperties = new Hashtable { { "isReady", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    // Play Button Clicked - Checks if all players are ready before starting the game
    public void OnPlayButtonClicked()
    {
        bool allReady = true;
        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!player.CustomProperties.ContainsKey("isReady") || !(bool)player.CustomProperties["isReady"])
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            // Load the selected map and mode
            string selectedMap = mapSelection.currentMapIndex.ToString();
            string selectedMod = modeSelection.currentModeIndex.ToString();
            Debug.Log("Map: " + selectedMap);
            Debug.Log("Mod: " + selectedMod);

            // Stop the background music
            BackgroundMusic.Stop();
            PhotonNetwork.LoadLevel("Game-Map" + selectedMap + "-Mode" + selectedMod);
        }
        else
        {
            Debug.Log("Not all players are ready.");
        }
    }

    // Ensure play button is only enabled for the room owner
    public override void OnJoinedRoom()
    {
        playButton.interactable = PhotonNetwork.IsMasterClient;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("roomCode"))
        {
            roomCodeText.text = PhotonNetwork.CurrentRoom.CustomProperties["roomCode"].ToString();
        }
    }

    // Update play button availability when room owner changes
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        playButton.interactable = PhotonNetwork.IsMasterClient;
    }

    public void ToggleUIActive(string UIName, bool isActive)
    {
        foreach (var ui in uiDictionary)
        {
            ui.Value.SetActive(ui.Key == UIName ? isActive : !isActive);
        }
    }

    // Update network role
    public void OnUpdateNetworkRoleToHost(bool isHost)
    {
        if (isHost)
        {
            VariableHolder.networkRole = NetworkRole.Host;
        }
        else
        {
            VariableHolder.networkRole = NetworkRole.Client;
        }
    }
}
