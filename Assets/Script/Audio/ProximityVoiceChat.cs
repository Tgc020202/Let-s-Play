using UnityEngine;
using Photon.Voice.Unity;
using Photon.Pun;
using Unity.Netcode;

public class ProximityVoiceChat : NetworkBehaviour
{
    public float chatRange = 10f;
    private Recorder voiceRecorder;

    private void Start()
    {
        if (!IsOwner) return; // Only set up for the local player

        voiceRecorder = GetComponent<Recorder>();
        if (!voiceRecorder)
        {
            Debug.LogError("Recorder component missing. Please add it to this GameObject.");
            return;
        }

        // Ensure recorder starts disabled
        voiceRecorder.TransmitEnabled = false;
    }

    private void Update()
    {
        if (!IsOwner || voiceRecorder == null) return;

        bool isOtherPlayerInRange = false;

        // Proximity detection for players spawned by Netcode
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, chatRange);
        foreach (var hit in hits)
        {
            if (hit.gameObject != this.gameObject && hit.GetComponent<NetworkObject>() != null)
            {
                isOtherPlayerInRange = true;
                Debug.Log("Can talk");
                break;
            }
        }

        voiceRecorder.TransmitEnabled = isOtherPlayerInRange;
    }
}
