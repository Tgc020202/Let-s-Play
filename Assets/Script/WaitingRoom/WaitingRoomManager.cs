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
    public Text roomNameText;

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

        playButton.interactable = PhotonNetwork.IsMasterClient;
        readyButton.interactable = !PhotonNetwork.IsMasterClient;

        playButton.onClick.AddListener(OnPlayButtonClicked);
        readyButton.onClick.AddListener(OnReadyButtonClicked);

        UpdatePlayButtonInteractable();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Number of Players: " + PhotonNetwork.CurrentRoom.PlayerCount);
        }
    }

    public void OnReadyButtonClicked()
    {
        bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("isReady") && (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];

        Hashtable playerProperties = new Hashtable { { "isReady", !isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public void OnPlayButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == RoomManager.Instance.numberOfPlayers && AreAllPlayersReady())
        {
            int mapIndex = RoomManager.Instance.currentMapIndex;
            int modeIndex = RoomManager.Instance.currentModeIndex;
            RoomManager.Instance.StartGame("Game-Map" + mapIndex + "-Mode" + modeIndex);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // Update play button interactability whenever any player's properties change
        UpdatePlayButtonInteractable();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Recheck readiness when a new player joins the room
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
}
