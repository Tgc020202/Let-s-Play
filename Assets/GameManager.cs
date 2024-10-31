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

    private Dictionary<ulong, bool> rolesAssigned = new Dictionary<ulong, bool>(); // Track roles per client

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

    private void OnRoleCountChanged(int oldValue, int newValue)
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
            // Randomize roles
            int randomRole = Random.Range(0, 2); // 0 = Worker, 1 = Boss

            if (randomRole == 1 && numberOfBosses.Value < VariableHolder.maxNumberOfBosses)
            {
                AssignBoss(clientId);
            }
            else if (numberOfWorkers.Value < VariableHolder.maxNumberOfWorkers)
            {
                AssignWorker(clientId);
            }
            else
            {
                Debug.Log("No available roles left for new client.");
            }
        }
        else
        {
            Debug.Log("Role already assigned for clientId: " + clientId); // For debugging
        }
    }

    private void AssignBoss(ulong clientId)
    {
        Debug.Log("Assigning Boss role to clientId: " + clientId);
        numberOfBosses.Value++;
        rolesAssigned[clientId] = true; // Mark this client as having an assigned role
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

    private void OnDestroy()
    {
        if (IsServer)
        {
            numberOfBosses.OnValueChanged -= OnRoleCountChanged;
            numberOfWorkers.OnValueChanged -= OnRoleCountChanged;
        }
    }
}
