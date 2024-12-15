using UnityEngine;
using Unity.Netcode;
using Photon.Pun;
using Photon.Voice.Unity;

public class ProximityVoiceChat : NetworkBehaviour
{
    // UI Components
    private Recorder voiceRecorder;

    // Defines
    public float chatRange = 10f;

    private void Start()
    {
        if (!IsOwner) return;

        voiceRecorder = GetComponent<Recorder>();
        if (!voiceRecorder)
        {
            Debug.LogError("Recorder component missing. Please add it to this GameObject.");
            return;
        }

        voiceRecorder.TransmitEnabled = false;
    }

    private void Update()
    {
        if (!IsOwner || voiceRecorder == null) return;

        bool isOtherPlayerInRange = false;

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
