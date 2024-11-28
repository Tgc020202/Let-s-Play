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
    public Text roomNameErrorText;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button publicButton;
    public Button privateButton;
    public Button modeButton;
    public Button backButton;
    public InputField roomCodeInput;
    public NumberOfPlayerSelection numberOfPlayerSelection;
    public MapSelection mapSelection;
    public ModeSelection modeSelection;
    public Transform roomListContent;
    public GameObject roomItemPrefab;
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    // Panels
    public GameObject RoomSelectionUI;
    public GameObject PlayerSetupUI;
    public GameObject RoomSetupUI;
    public GameObject JoinRoomUI;
    public GameObject MapUI;
    public GameObject TotalNumberUI;
    public GameObject ModeUI;

    // Audio
    private AudioSource BackgroundMusic;
    private Dictionary<string, GameObject> uiDictionary;
    private bool isPrivate = false;
    private bool isConnectedToMaster = false;

    private void Start()
    {
        RoomManager.Instance.ConnectToPhoton();

        // Audio
        GameObject bgmObject = GameObject.Find("AudioManager/BackgroundMusic");
        if (bgmObject != null)
        {
            BackgroundMusic = bgmObject.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogWarning("AudioManager/BackgroundMusic not found.");
        }

        // UI
        uiDictionary = new Dictionary<string, GameObject>
    {
        { "RoomSelectionUI", RoomSelectionUI },
        { "PlayerSetupUI", PlayerSetupUI },
        { "RoomSetupUI", RoomSetupUI },
        { "JoinRoomUI", JoinRoomUI },
        { "MapUI", MapUI },
        { "TotalNumberUI", TotalNumberUI },
        { "ModeUI", ModeUI },
    };

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
        backButton.onClick.AddListener(OnBackButtonClicked);

        roomCodeInput.onEndEdit.AddListener(OnRoomCodeInputEndEdit);

        ToggleUIActive("RoomSelectionUI", true);
    }

    // UI Handler
    public void ToggleUIActive(string UIName, bool isActive)
    {
        foreach (var ui in uiDictionary)
        {
            ui.Value.SetActive(ui.Key == UIName ? isActive : !isActive);
        }

        bool anySubUIActive = MapUI.activeSelf || TotalNumberUI.activeSelf || ModeUI.activeSelf;

        // If any of these UIs are active, set RoomSetupUI to active as well
        RoomSetupUI.SetActive(anySubUIActive);
    }

    public void OnUpdateNetworkRoleToHost(bool isHost)
    {
        VariableHolder.networkRole = isHost ? NetworkRole.Host : NetworkRole.Client;
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("Clicked");
        ToggleUIActive("RoomSelectionUI", true);
    }

    public void OnSetPrivateClicked(bool isPrivate)
    {
        string roomName = roomNameInput.text;

        if (!string.IsNullOrEmpty(roomName))
        {
            if (IsRoomNameAvailable(roomName))
            {
                roomNameErrorText.text = "";
                this.isPrivate = isPrivate;
                ToggleUIActive("MapUI", true);
            }
            else
            {
                roomNameErrorText.text = "Room name already exists. Please choose a different name.";
            }
        }
        else
        {
            roomNameInput.placeholder.GetComponent<Text>().text = "Please enter Room Name!";
        }
    }


    private bool IsRoomNameAvailable(string roomName)
    {
        foreach (var room in cachedRoomList)
        {
            if (room.Name == roomName)
            {
                return false;
            }
        }
        return true;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        isConnectedToMaster = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Successfully joined the lobby system.");
    }

    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInput.text;
        string roomCode = roomName;

        // Store variables
        RoomManager.Instance.numberOfPlayers = numberOfPlayerSelection.totalNumberOfPlayer;
        RoomManager.Instance.maxNumberOfBosses = numberOfPlayerSelection.bossCount;
        RoomManager.Instance.maxNumberOfWorkers = numberOfPlayerSelection.staffCount;
        RoomManager.Instance.currentMapIndex = mapSelection.currentMapIndex;
        RoomManager.Instance.currentModeIndex = modeSelection.currentModeIndex;
        RoomManager.Instance.roomName = roomName;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = RoomManager.Instance.numberOfPlayers,
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
        if (cachedRoomList.Count == 0)
        {
            cachedRoomList = new List<RoomInfo>(roomList);
        }
        else
        {
            foreach (var room in roomList)
            {
                bool roomExists = false;
                for (int i = 0; i < cachedRoomList.Count; i++)
                {
                    if (cachedRoomList[i].Name == room.Name)
                    {
                        roomExists = true;

                        if (room.RemovedFromList)
                        {
                            cachedRoomList.RemoveAt(i);
                        }
                        else
                        {
                            cachedRoomList[i] = room;
                        }
                        break;
                    }
                }

                if (!roomExists && !room.RemovedFromList)
                {
                    cachedRoomList.Add(room);
                }
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        foreach (Transform roomItem in roomListContent)
        {
            Destroy(roomItem.gameObject);
        }

        foreach (var room in cachedRoomList)
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
                    joinButton.interactable = false;
                    roomItem.transform.Find("PlayerCountText").GetComponent<Text>().color = Color.red;
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
