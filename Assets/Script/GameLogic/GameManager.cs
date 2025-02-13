using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    // UI Components
    public Transform[] spawnPoints;
    public Text roleText;
    public Text numberOfTargetsText;

    // GameObjects
    public GameObject BossControllerUI;
    public GameObject WorkerControllerUI;
    public GameObject BasicControllerUI;

    // Defines
    private Dictionary<ulong, bool> rolesAssigned = new Dictionary<ulong, bool>();
    public List<ulong> listBossAssigned = new List<ulong>();
    private int maxNumberOfBosses;
    private int maxNumberOfWorkers;
    private bool isGameStart = false;
    private bool oneshot = true;

    // Network Variables
    private NetworkVariable<int> numberOfBosses = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> numberOfWorkers = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        maxNumberOfBosses = RoomManager.Instance != null ? RoomManager.Instance.maxNumberOfBosses : VariableHolder.defaultMaxNumberOfBosses;
        maxNumberOfWorkers = RoomManager.Instance != null ? RoomManager.Instance.maxNumberOfWorkers : VariableHolder.defaultMaxNumberOfWorkers;
    }

    void Update()
    {
        if (IsServer)
        {
            if (numberOfWorkers.Value > 0 && oneshot)
            {
                isGameStart = true;
                oneshot = false;
            }

            if (numberOfWorkers.Value <= 0 && isGameStart)
            {
                GameObject.FindObjectOfType<GameViewTextBehaviour>()?.EndGameServerRpc(true);
                EndGame();
            }
        }

        numberOfTargetsText.text = "Number of Targets: " + numberOfWorkers.Value;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer || IsHost)
        {
            Debug.Log("Server is starting...");
            numberOfBosses.OnValueChanged += OnRoleCountChanged;
            numberOfWorkers.OnValueChanged += OnRoleCountChanged;

            if (!rolesAssigned.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                AssignRoleLogic(NetworkManager.Singleton.LocalClientId, SessionManager.Instance.username);
            }
        }

        if (IsClient && !IsHost)
        {
            if (!rolesAssigned.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                RequestRoleAssignmentServerRpc(SessionManager.Instance.username);
            }
        }
    }

    public void OnRoleCountChanged(int oldValue, int newValue)
    {
        Debug.Log("Number of Bosses: " + numberOfBosses.Value);
        Debug.Log("Number of Workers: " + numberOfWorkers.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestRoleAssignmentServerRpc(string username, ServerRpcParams rpcParams = default)
    {
        AssignRoleLogic(rpcParams.Receive.SenderClientId, username);
    }

    private void AssignRoleLogic(ulong clientId, string username)
    {
        if (rolesAssigned.ContainsKey(clientId) && rolesAssigned[clientId]) return;

        int randomRole = Random.Range(0, 2); // 0 = Worker, 1 = Boss

        switch (RoomManager.Instance.currentModeIndex)
        {
            case 1:
                if (randomRole == 1 && numberOfBosses.Value < maxNumberOfBosses)
                {
                    AssignBoss(clientId);
                }
                else if (numberOfWorkers.Value < maxNumberOfWorkers)
                {
                    AssignWorker(clientId);
                }
                else
                {
                    AssignBoss(clientId);
                }
                break;
            case 2:
                bool isBoss = false;
                for (int i = 0; i < maxNumberOfBosses; i++)
                {
                    if (RoomManager.Instance.mostVotePlayer[i] == username)
                    {
                        AssignBoss(clientId);
                        isBoss = true;
                        break;
                    }
                }
                if (!isBoss) AssignWorker(clientId);
                break;
            default:
                Debug.Log("Nothing");
                break;
        }
        SpawnPlayer(clientId);
    }


    private void AssignBoss(ulong clientId)
    {
        Debug.Log("Assigning Boss role to clientId: " + clientId);
        numberOfBosses.Value++;
        rolesAssigned[clientId] = true;
        listBossAssigned.Add(clientId);
        UpdateRoleUIClientRpc(clientId, "Boss");
    }

    private void AssignWorker(ulong clientId)
    {
        Debug.Log("Assigning Worker role to clientId: " + clientId);
        numberOfWorkers.Value++;
        rolesAssigned[clientId] = true;
        UpdateRoleUIClientRpc(clientId, "Worker");
    }

    [ClientRpc]
    private void UpdateRoleUIClientRpc(ulong clientId, string role)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        roleText.text = role;
        BossControllerUI.SetActive(role == "Boss");
        WorkerControllerUI.SetActive(role == "Worker");
        BasicControllerUI.SetActive(true);

        Debug.Log("Updated UI for clientId: " + clientId + " with role: " + role);
    }

    // private void SpawnPlayer(ulong clientId)
    // {
    //     if (spawnPoints.Length == 0)
    //     {
    //         Debug.LogError("No spawn points assigned in the Inspector!");
    //         return;
    //     }

    //     int spawnIndex = Random.Range(0, spawnPoints.Length);
    //     Transform spawnPoint = spawnPoints[spawnIndex];

    //     var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

    //     if (playerObject == null)
    //     {
    //         Debug.LogError($"PlayerObject for client {clientId} not found!");
    //         return;
    //     }

    //     NetworkObject playerNetworkObject = playerObject.GetComponent<NetworkObject>();
    //     if (playerNetworkObject != null && !playerNetworkObject.IsSpawned)
    //     {
    //         playerNetworkObject.SpawnAsPlayerObject(clientId);
    //     }
    //     else
    //     {
    //         Debug.Log("Player object already spawned.");
    //     }

    //     playerObject.transform.position = spawnPoint.position;
    //     playerObject.transform.rotation = spawnPoint.rotation;

    //     Debug.Log($"Player {clientId} moved to {spawnPoint.position}");
    // }

    private void SpawnPlayer(ulong clientId)
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned in the Inspector!");
            return;
        }

        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnIndex];

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            Debug.LogError($"Client {clientId} not found in ConnectedClients!");
            return;
        }

        var playerObject = client.PlayerObject;

        if (playerObject == null)
        {
            Debug.LogError($"PlayerObject for client {clientId} is null!");
            return;
        }

        var networkObject = playerObject.GetComponent<NetworkObject>();
        if (networkObject == null || !networkObject.IsSpawned)
        {
            Debug.LogError($"NetworkObject for client {clientId} is either null or not spawned!");
            return;
        }

        playerObject.transform.position = spawnPoint.position;
        playerObject.transform.rotation = spawnPoint.rotation;
        Debug.Log($"Player {clientId} spawned at {spawnPoint.position}");
    }

    public void UpdateWorkerCountRequest(int delta)
    {
        if (IsClient)
        {
            RequestWorkerCountUpdateServerRpc(delta);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestWorkerCountUpdateServerRpc(int delta)
    {
        numberOfWorkers.Value += delta;
    }

    private void OnDestroy()
    {
        if (IsServer)
        {
            numberOfBosses.OnValueChanged -= OnRoleCountChanged;
            numberOfWorkers.OnValueChanged -= OnRoleCountChanged;
        }
    }

    [ServerRpc]
    private void DespawnPlayerServerRpc(ulong clientId)
    {
        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerObject != null)
        {
            playerObject.Despawn();
            Debug.Log($"Despawned player object for clientId: {clientId}");
        }
    }
    public void EndGame()
    {
        if (IsServer)
        {
            foreach (var playerObject in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (playerObject.PlayerObject != null)
                {
                    DespawnPlayerServerRpc(playerObject.ClientId);
                }
            }

            rolesAssigned.Clear();
            listBossAssigned.Clear();
            numberOfBosses.Value = 0;
            numberOfWorkers.Value = 0;

            NetworkManager.Singleton.Shutdown();
        }
    }
}
