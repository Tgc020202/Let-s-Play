using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        startHostButton.onClick.AddListener(() =>
        {
            Debug.Log("Starting Host...");
            NetworkManager.Singleton.StartHost();
            // if (NetworkManager.Singleton.IsHost)
            // {
            //     gameManager.RequestRoleAssignmentServerRpc();
            // }
            Hide();
        });

        startClientButton.onClick.AddListener(() =>
        {
            Debug.Log("Starting Client...");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Client connected: " + clientId);
            // gameManager.RequestRoleAssignmentServerRpc();
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
