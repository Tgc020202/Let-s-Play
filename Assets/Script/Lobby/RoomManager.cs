using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviour
{
    // Defines
    public int numberOfPlayers;
    public int maxNumberOfBosses;
    public int maxNumberOfWorkers;
    public int currentMapIndex;
    public int currentModeIndex;
    public string roomName;
    public bool isBossWin;
    public bool isPrivate;
    public bool gameStarted = false;
    public List<string> mostVotePlayer;

    // Instance
    public static RoomManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void StartGame(string scene)
    {
        gameStarted = true;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(scene);
        }
    }

    public void EndGame()
    {
        gameStarted = false;
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.CurrentRoom.IsOpen = true;
    }
}
