using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class WaitingRoomManager : MonoBehaviourPunCallbacks
{
    // UI Components
    public Button playButton;
    public Button readyButton;
    public Button leaveButton;
    public Button votingButton;
    public Text roomNameText;
    public Text gameModeText;
    public Text mapText;
    private Outline playButtonOutline;
    private Outline readyButtonOutline;
    private Outline leaveButtonOutline;
    public Transform playerListContent;
    public Transform votePlayerListContent;

    // Audio
    private AudioSource BackgroundMusic;

    // Animations
    public Animator CarAnimator;

    // GameObjects
    public GameObject RedTrafficLight;
    public GameObject GreenTrafficLight;
    public GameObject playerButtonPrefab;
    public GameObject VotingBossUI;

    // Defines
    private bool isTransitioning = false;
    private bool isVoted = false;
    private bool readyClicked = false;
    private string roomName;
    private string map;
    private string voteWho;
    private int currentNumberOfPlayers = 0;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        roomName = RoomManager.Instance.roomName;
        roomNameText.text = roomName;
        votingButton.gameObject.SetActive(false);

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


        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnectedAndReady)
        {
            Hashtable playerProperties = new Hashtable { { "isReady", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }

        RetryResetVoteCounts();
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
        votingButton.onClick.AddListener(OnVotingButtonClicked);

        GreenTrafficLight.SetActive(false);

        UpdatePlayButtonInteractable();

        BackgroundMusic = GameObject.Find("AudioManager/BackgroundMusic").GetComponent<AudioSource>();
    }

    private void RetryResetVoteCounts()
    {
        StartCoroutine(ResetVoteCountsWithRetry());
    }

    private IEnumerator ResetVoteCountsWithRetry()
    {
        while (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return new WaitForSeconds(1f);
        }
        ResetVoteCounts();
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.CurrentRoom != null && RoomManager.Instance != null && RoomManager.Instance.currentModeIndex == 2)
        {
            currentNumberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

            if (currentNumberOfPlayers == RoomManager.Instance.numberOfPlayers)
            {
                votingButton.gameObject.SetActive(true);
            }
            else
            {
                votingButton.gameObject.SetActive(false);
                if (PhotonNetwork.IsConnected)
                {
                    ResetVoteCounts();

                }
                isVoted = false;
            }
        }

        LogMostVotedPlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerCountProperty()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnectedAndReady)
        {
            Hashtable gameProperties = new Hashtable { { "currentNumberOfPlayers", PhotonNetwork.CurrentRoom.PlayerCount } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(gameProperties);
        }
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
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Hashtable playerProperties = new Hashtable { { "isReady", false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }
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

    public void OnVotingButtonClicked()
    {
        foreach (Transform child in votePlayerListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerButton = Instantiate(playerButtonPrefab, votePlayerListContent);
            playerButton.GetComponentInChildren<Text>().text = player.NickName;

            Button voteButton = playerButton.GetComponent<Button>();
            Player currentPlayer = player;
            voteButton.onClick.AddListener(() => CastVote(currentPlayer, playerButton));
        }

        VotingBossUI.SetActive(true);
    }

    public void CastVote(Player targetPlayer, GameObject playerButton)
    {
        if (isVoted)
        {
            Player previousVotePlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.NickName == voteWho);

            if (previousVotePlayer != null)
            {
                Hashtable playerProperties = previousVotePlayer.CustomProperties;
                if (!playerProperties.ContainsKey("VoteCount"))
                {
                    playerProperties["VoteCount"] = 0;
                }
                playerProperties["VoteCount"] = (int)playerProperties["VoteCount"] - 1;
                previousVotePlayer.SetCustomProperties(playerProperties);

                Debug.Log(previousVotePlayer.NickName + " now has " + playerProperties["VoteCount"] + " vote(s).");
            }
        }
        else
        {
            isVoted = true;
        }

        voteWho = targetPlayer.NickName;

        Hashtable targetProperties = targetPlayer.CustomProperties;
        if (!targetProperties.ContainsKey("VoteCount"))
        {
            targetProperties["VoteCount"] = 0;
        }
        targetProperties["VoteCount"] = (int)targetProperties["VoteCount"] + 1;
        targetPlayer.SetCustomProperties(targetProperties);
        Debug.Log(targetPlayer.NickName + " has " + targetProperties["VoteCount"] + " vote(s).");

        VotingBossUI.SetActive(false);
    }

    private void LogMostVotedPlayers()
    {
        List<Player> sortedPlayers = PhotonNetwork.PlayerList.ToList();

        sortedPlayers.Sort((x, y) =>
        {
            int xVotes = x.CustomProperties.ContainsKey("VoteCount") ? (int)x.CustomProperties["VoteCount"] : 0;
            int yVotes = y.CustomProperties.ContainsKey("VoteCount") ? (int)y.CustomProperties["VoteCount"] : 0;
            return yVotes.CompareTo(xVotes);
        });

        RoomManager.Instance.mostVotePlayer.Clear();

        if (sortedPlayers.Count > 0)
        {
            int numberOfBosses = RoomManager.Instance.maxNumberOfBosses;
            for (int i = 0; i < Mathf.Min(numberOfBosses, sortedPlayers.Count); i++)
            {
                RoomManager.Instance.mostVotePlayer.Add(sortedPlayers[i].NickName);
                Debug.Log($"Boss {i + 1}: {sortedPlayers[i].NickName}");
            }
        }
        else
        {
            Debug.Log("No players received votes.");
        }

        foreach (Player player in sortedPlayers)
        {
            int voteCount = player.CustomProperties.ContainsKey("VoteCount") ? (int)player.CustomProperties["VoteCount"] : 0;
            Debug.Log($"Player: {player.NickName}, Votes: {voteCount}");
        }
    }

    private void ResetVoteCounts()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Hashtable voteProperties = new Hashtable { { "VoteCount", 0 } };
            player.SetCustomProperties(voteProperties);
        }
        Debug.Log("Vote counts reset for all players.");
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
        base.OnJoinedRoom();
        if (PhotonNetwork.CurrentRoom != null)
        {
            currentNumberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        }

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
