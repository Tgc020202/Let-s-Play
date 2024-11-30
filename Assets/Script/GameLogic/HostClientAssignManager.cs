// using System.Collections;
// using UnityEngine;
// using Unity.Netcode;

// public class HostClientAssignManager : MonoBehaviour
// {
//     private GameManager gameManager;
//     private void Awake()
//     {
//         gameManager = FindObjectOfType<GameManager>();
//         StartCoroutine(DelayedAssignRole());
//     }

//     private IEnumerator DelayedAssignRole()
//     {
//         yield return new WaitForSeconds(1f);
//         AssignRoleAutomatically();
//     }

//     private void AssignRoleAutomatically()
//     {
//         if (NetworkManager.Singleton == null)
//         {
//             Debug.LogError("NetworkManager is not initialized.");
//             return;
//         }

//         if (VariableHolder.networkRole != null)
//         {
//             if (VariableHolder.networkRole == NetworkRole.Host)
//             {
//                 Debug.Log("No host found. Starting as Host...");
//                 NetworkManager.Singleton.StartHost();

//                 if (NetworkManager.Singleton.IsHost)
//                 {
//                     gameManager.RequestRoleAssignmentServerRpc();
//                 }
//             }
//             else if (VariableHolder.networkRole == NetworkRole.Client)
//             {
//                 Debug.Log("Host exists. Starting as Client...");
//                 NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
//                 NetworkManager.Singleton.StartClient();
//             }
//             else
//             {
//                 Debug.LogError("Error in assigning network role.");
//             }
//         }
//         else
//         {
//             Debug.Log("Error in assigning network role at Lobby.");
//         }
//     }

//     private void OnClientConnected(ulong clientId)
//     {
//         if (clientId == NetworkManager.Singleton.LocalClientId)
//         {
//             Debug.Log("Client connected: " + clientId);
//             gameManager.RequestRoleAssignmentServerRpc();
//         }
//     }
// }
