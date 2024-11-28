using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class WaitingRoomManager : MonoBehaviourPunCallbacks
{
    // UI Elements
    public Button playButton;
    public Button readyButton;
    public Button leaveButton;
    public Text roomNameText;
    private Outline playButtonOutline;
    private Outline readyButtonOutline;
    private Outline leaveButtonOutline;
    private bool readyClicked = false;
    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        string roomName = RoomManager.Instance.roomName;
        roomNameText.text = roomName;

        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable playerProperties = new Hashtable { { "isReady", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }

        // Toggle button visibility based on ownership
        UpdateUIForOwnership();

        playButtonOutline = playButton.GetComponent<Outline>();
        readyButtonOutline = readyButton.GetComponent<Outline>();
        leaveButtonOutline = leaveButton.GetComponent<Outline>();

        playButtonOutline.effectColor = Color.red;
        readyButtonOutline.effectColor = Color.red;
        leaveButtonOutline.effectColor = Color.red;

        playButton.onClick.AddListener(OnPlayButtonClicked);
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        leaveButton.onClick.AddListener(OnLeaveButtonClicked);

        UpdatePlayButtonInteractable();
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
    }

    public void OnPlayButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == RoomManager.Instance.numberOfPlayers && AreAllPlayersReady())
        {
            readyButtonOutline.effectColor = Color.green;

            int mapIndex = RoomManager.Instance.currentMapIndex;
            int modeIndex = RoomManager.Instance.currentModeIndex;
            RoomManager.Instance.StartGame("Game-Map" + mapIndex + "-Mode" + modeIndex);
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
}
