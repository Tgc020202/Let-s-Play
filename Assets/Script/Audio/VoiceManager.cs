using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;

public class VoiceManager : MonoBehaviourPun
{
    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}
