using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public Text roleText;
    public GameObject BossControllerUI;
    public GameObject WorkerControllerUI;
    public GameObject BasicControllerUI;

    private NetworkVariable<int> numberOfBosses = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> numberOfWorkers = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Dictionary<ulong, bool> rolesAssigned = new Dictionary<ulong, bool>();

    private int maxNumberOfBosses;
    private int maxNumberOfWorkers;
    private bool isGameStart = false;
    private bool oneshot = true;

    void Start()
    {
        maxNumberOfBosses = RoomManager.Instance.maxNumberOfBosses;
        maxNumberOfWorkers = RoomManager.Instance.maxNumberOfWorkers;
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
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer || IsHost)
        {
            Debug.Log("Server is starting...");
            numberOfBosses.OnValueChanged += OnRoleCountChanged;
            numberOfWorkers.OnValueChanged += OnRoleCountChanged;

            AssignRoleLogic(NetworkManager.Singleton.LocalClientId);
        }

        if (IsClient && !IsHost)
        {
            RequestRoleAssignmentServerRpc();
        }
    }

    public void OnRoleCountChanged(int oldValue, int newValue)
    {
        Debug.Log("Number of Bosses: " + numberOfBosses.Value);
        Debug.Log("Number of Workers: " + numberOfWorkers.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestRoleAssignmentServerRpc(ServerRpcParams rpcParams = default)
    {
        AssignRoleLogic(rpcParams.Receive.SenderClientId);
    }

    private void AssignRoleLogic(ulong clientId)
    {
        if (!rolesAssigned.ContainsKey(clientId) || !rolesAssigned[clientId])
        {
            int randomRole = Random.Range(0, 2); // 0 = Worker, 1 = Boss
            // int randomRole = IsServer || IsHost ? 1 : 0; // Debug uses purpose

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
        }
        else
        {
            Debug.Log("Role already assigned for clientId: " + clientId);
        }
    }

    private void AssignBoss(ulong clientId)
    {
        Debug.Log("Assigning Boss role to clientId: " + clientId);
        numberOfBosses.Value++;
        rolesAssigned[clientId] = true;
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
        // This code runs only on the server, updating the NetworkVariable
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
            numberOfBosses.Value = 0;
            numberOfWorkers.Value = 0;

            NetworkManager.Singleton.Shutdown();
        }
    }
}
