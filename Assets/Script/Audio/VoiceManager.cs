using Photon.Voice.Unity;
using Photon.Pun;
using UnityEngine;

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
