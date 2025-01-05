using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using EventData = ExitGames.Client.Photon.EventData;

public class HostClientAssignManager : MonoBehaviour
{
    // Scripts
    private GameManager gameManager;
    private RelayManager relayManager;

    // Defines
    private const string JoinCodeKey = "JoinCode";

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        relayManager = FindObjectOfType<RelayManager>();
        StartCoroutine(DelayedAssignRole());
    }

    private IEnumerator DelayedAssignRole()
    {
        yield return new WaitForSeconds(1f);
        AssignRoleAutomatically();
    }

    private void AssignRoleAutomatically()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartHostRelay();
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            PhotonNetwork.NetworkingClient.EventReceived += OnRoomPropertiesUpdated;
        }
    }

    private async void StartHostRelay()
    {
        string joinCode = await relayManager.StartRelay(4);
        Debug.Log($"Join Code Created: {joinCode}");

        // Store the join code in Photon Custom Room Properties
        Hashtable roomProperties = new Hashtable
        {
            { JoinCodeKey, joinCode }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        if (NetworkManager.Singleton.IsHost)
        {
            gameManager.RequestRoleAssignmentServerRpc(SessionManager.Instance.username);
        }
    }

    private void OnRoomPropertiesUpdated(EventData photonEvent)
    {
        const byte CustomPropertiesChangedEventCode = 253; // Photon-defined code for property updates
        if (photonEvent.Code == CustomPropertiesChangedEventCode)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(JoinCodeKey))
            {
                string joinCode = PhotonNetwork.CurrentRoom.CustomProperties[JoinCodeKey] as string;
                Debug.Log($"Retrieved Join Code: {joinCode}");

                // Join the Relay using the retrieved join code
                relayManager.JoinRelay(joinCode);

                // Unsubscribe from the event listener to avoid duplicate calls
                PhotonNetwork.NetworkingClient.EventReceived -= OnRoomPropertiesUpdated;
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            gameManager.RequestRoleAssignmentServerRpc(SessionManager.Instance.username);
        }
    }
}
