using Photon.Pun;
using UnityEngine;

public class PhotonConnect : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Connect to Photon Cloud
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon: " + cause.ToString());
    }
}
