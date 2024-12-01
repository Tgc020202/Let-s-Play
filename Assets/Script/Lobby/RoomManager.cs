using Photon.Pun;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    public bool gameStarted = false;

    // Add variables to store
    public int numberOfPlayers;
    public int maxNumberOfBosses;
    public int maxNumberOfWorkers;
    public int currentMapIndex;
    public int currentModeIndex;
    public string roomName;
    public bool isBossWin;

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
