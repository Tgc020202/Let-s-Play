using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections;

public class WaitingRoomManager : MonoBehaviourPunCallbacks
{
    // UI Components
    public Button playButton;
    public Button readyButton;
    public Button leaveButton;
    public Text roomNameText;
    public Text gameModeText;
    public Text mapText;
    private Outline playButtonOutline;
    private Outline readyButtonOutline;
    private Outline leaveButtonOutline;
    public Transform playerListContent;

    // Audio
    private AudioSource BackgroundMusic;

    // Animations
    public Animator CarAnimator;

    // GameObjects
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;
    public GameObject playerButtonPrefab;

    // Defines
    private bool isTransitioning = false;
    private bool readyClicked = false;
    private string roomName;
    private string map;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        roomName = RoomManager.Instance.roomName;
        roomNameText.text = roomName;

        switch (RoomManager.Instance.currentModeIndex)
        {
            case 1:
                gameModeText.text = "Secret Murder";
                break;
            case 2:
                gameModeText.text = "Vote Murder";
                break;
            default:
                gameModeText.text = "Random Mode";
                break;
        }

        switch (RoomManager.Instance.currentMapIndex)
        {
            case 1:
                mapText.text = "Small Map";
                break;
            case 2:
                mapText.text = "Large Map";
                break;
            default:
                mapText.text = "Random Map";
                break;
        }


        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable playerProperties = new Hashtable { { "isReady", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }

        UpdateUIForOwnership();
        UpdatePlayerList();

        playButtonOutline = playButton.GetComponent<Outline>();
        readyButtonOutline = readyButton.GetComponent<Outline>();
        leaveButtonOutline = leaveButton.GetComponent<Outline>();

        playButtonOutline.effectColor = Color.red;
        readyButtonOutline.effectColor = Color.red;
        leaveButtonOutline.effectColor = Color.red;

        playButton.onClick.AddListener(OnPlayButtonClicked);
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        leaveButton.onClick.AddListener(OnLeaveButtonClicked);

        GreenTrafficLight.SetActive(false);

        UpdatePlayButtonInteractable();

        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        foreach (Transform player in playerListContent)
        {
            Destroy(player.gameObject);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerButton = Instantiate(playerButtonPrefab, playerListContent);
            if (playerButton != null)
            {
                playerButton.GetComponentInChildren<Text>().text = player.NickName;
            }
        }
    }

    public void OnLeaveButtonClicked()
    {
        Hashtable playerProperties = new Hashtable { { "isReady", false } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        PhotonNetwork.LeaveRoom();
    }

    public void OnReadyButtonClicked()
    {
        bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("isReady") && (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];
        readyClicked = !readyClicked;
        readyButtonOutline.effectColor = readyClicked ? Color.green : Color.red;

        Hashtable playerProperties = new Hashtable { { "isReady", !isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        if (readyClicked)
        {
            if (isTransitioning) return;
            isTransitioning = true;
            RedTrafficLight.SetActive(false);
            GreenTrafficLight.SetActive(true);
            CarAnimator.SetBool("isTurningToNextScene", true);
        }
        else
        {
            isTransitioning = false;
            RedTrafficLight.SetActive(true);
            GreenTrafficLight.SetActive(false);
        }

    }

    public void OnPlayButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == RoomManager.Instance.numberOfPlayers && AreAllPlayersReady())
        {
            playButtonOutline.effectColor = Color.green;

            int mapIndex = RoomManager.Instance.currentMapIndex;
            int modeIndex = RoomManager.Instance.currentModeIndex;

            if (isTransitioning) return;
            isTransitioning = true;

            LoadAnimation("Game-Map" + mapIndex + "-Mode" + modeIndex);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Master client switched to: " + newMasterClient.NickName);
        UpdateUIForOwnership();

        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable playerProperties = new Hashtable { { "isReady", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public override void OnJoinedRoom()
    {
        Hashtable playerProperties = new Hashtable { { "isReady", PhotonNetwork.IsMasterClient } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdatePlayButtonInteractable();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
        UpdatePlayButtonInteractable();
    }

    private void UpdatePlayButtonInteractable()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            playButton.interactable = AreAllPlayersReady();
        }
    }

    private bool AreAllPlayersReady()
    {
        if (PhotonNetwork.CurrentRoom.Players.Count < RoomManager.Instance.numberOfPlayers)
        {
            return false;
        }

        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!player.CustomProperties.ContainsKey("isReady") || !(bool)player.CustomProperties["isReady"])
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateUIForOwnership()
    {
        playButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        readyButton.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
        leaveButton.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
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
