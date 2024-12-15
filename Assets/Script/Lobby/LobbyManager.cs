using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    // UI Components
    public InputField roomNameInput;
    public InputField roomCodeInput;
    public Text roomNameErrorText;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button publicButton;
    public Button privateButton;
    public Button modeButton;
    public Button backButton1;
    public Button backButton2;
    public Transform roomListContent;

    // Scripts
    public NumberOfPlayerSelection numberOfPlayerSelection;
    public MapSelection mapSelection;
    public ModeSelection modeSelection;

    // Audio
    private AudioSource BackgroundMusic;

    // Animations
    public Animator CarAnimator;

    // GameObjects
    public GameObject RoomSelectionUI;
    public GameObject PlayerSetupUI;
    public GameObject RoomSetupUI;
    public GameObject JoinRoomUI;
    public GameObject MapUI;
    public GameObject TotalNumberUI;
    public GameObject ModeUI;
    public GameObject roomItemPrefab;
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;

    // Defines
    private Dictionary<string, GameObject> uiDictionary;
    private bool isTransitioning = false;
    private bool isPrivate = false;
    private bool isConnectedToMaster = false;
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    // Messages
    private const string RoomNameInvalidMessage = "Room name already exists. Please choose a different name.";
    private const string UsernameEmptyMessage = "Please enter Room Name.";
    private const string EmptyMessage = "";

    private void Start()
    {
        GreenTrafficLight.SetActive(false);
        RoomManager.Instance.ConnectToPhoton();

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
        backButton1.onClick.AddListener(OnBackButtonClicked);
        backButton2.onClick.AddListener(OnBackButtonClicked);
        roomCodeInput.onEndEdit.AddListener(OnRoomCodeInputEndEdit);

        ToggleUIActive("RoomSelectionUI", true);

        GameObject bgmObject = GameObject.Find("AudioManager/BackgroundMusic");
        if (bgmObject != null)
        {
            BackgroundMusic = bgmObject.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogWarning("AudioManager/BackgroundMusic not found.");
        }
    }

    public void ToggleUIActive(string UIName, bool isActive)
    {
        foreach (var ui in uiDictionary)
        {
            ui.Value.SetActive(ui.Key == UIName ? isActive : !isActive);
        }

        bool anySubUIActive = MapUI.activeSelf || TotalNumberUI.activeSelf || ModeUI.activeSelf;
        RoomSetupUI.SetActive(anySubUIActive);
    }

    public void OnUpdateNetworkRoleToHost(bool isHost)
    {
        VariableHolder.networkRole = isHost ? NetworkRole.Host : NetworkRole.Client;
    }

    public void OnBackButtonClicked()
    {
        ToggleUIActive("RoomSelectionUI", true);
    }

    public void OnSetPrivateClicked(bool isPrivate)
    {
        string roomName = roomNameInput.text;

        if (!string.IsNullOrEmpty(roomName))
        {
            if (IsRoomNameAvailable(roomName))
            {
                roomNameErrorText.text = EmptyMessage;
                this.isPrivate = isPrivate;
                ToggleUIActive("MapUI", true);
            }
            else
            {
                roomNameErrorText.text = RoomNameInvalidMessage;
            }
        }
        else
        {
            roomNameInput.placeholder.GetComponent<Text>().text = UsernameEmptyMessage;
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
        isConnectedToMaster = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Successfully joined the lobby system.");
    }

    public void OnCreateRoomButtonClicked()
    {
        // Store variables
        RoomManager.Instance.numberOfPlayers = numberOfPlayerSelection.totalNumberOfPlayer;
        RoomManager.Instance.maxNumberOfBosses = numberOfPlayerSelection.bossCount;
        RoomManager.Instance.maxNumberOfWorkers = numberOfPlayerSelection.staffCount;
        RoomManager.Instance.currentMapIndex = mapSelection.currentMapIndex;
        RoomManager.Instance.currentModeIndex = modeSelection.currentModeIndex;
        RoomManager.Instance.roomName = roomNameInput.text;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = RoomManager.Instance.numberOfPlayers,
            IsVisible = !isPrivate,
            IsOpen = true
        };

        Hashtable customProperties = new Hashtable { { "roomCode", roomNameInput.text } };
        PhotonNetwork.CreateRoom(roomNameInput.text, options, null);
    }

    public override void OnCreatedRoom()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        LoadAnimation("WaitingRoomScene");

        Hashtable customProperties = new Hashtable { { "roomCode", roomNameInput.text } };
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

        if (isTransitioning) return;
        isTransitioning = true;

        PhotonNetwork.JoinRoom(roomName);
        LoadAnimation("WaitingRoomScene");
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

                if (room.PlayerCount >= room.MaxPlayers)
                {
                    joinButton.interactable = false;
                    roomItem.transform.Find("PlayerCountText").GetComponent<Text>().color = Color.red;
                }
                else
                {
                    joinButton.onClick.AddListener(() =>
                    {
                        // Store variables
                        RoomManager.Instance.roomName = room.Name;

                        if (isTransitioning) return;
                        isTransitioning = true;

                        PhotonNetwork.JoinRoom(room.Name);
                        LoadAnimation("WaitingRoomScene");
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

    void LoadAnimation(string sceneName)
    {
        RedTrafficLight.SetActive(false);
        GreenTrafficLight.SetActive(true);
        CarAnimator.SetBool("isTurningToNextScene", true);

        StartCoroutine(DelayedSceneTransition(sceneName));
    }

    IEnumerator DelayedSceneTransition(string sceneName)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneName);
        BackgroundMusic.Stop();
    }
}
