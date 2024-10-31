using UnityEngine;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            CameraBehaviour cameraBehaviour = Camera.main.GetComponent<CameraBehaviour>();
            if (cameraBehaviour != null)
            {
                cameraBehaviour.SetTarget(transform);
            }
            else
            {
                Debug.LogWarning("CameraBehaviour not found on main camera!");
            }
        }
    }

    // Movement of the player caught: Currently Unused
    [ServerRpc(RequireOwnership = false)]
    public void SetMovementEnabledServerRpc(bool enabled)
    {
        SetMovementEnabledClientRpc(enabled);
    }

    [ClientRpc]
    private void SetMovementEnabledClientRpc(bool enabled)
    {
        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = enabled;
        }
    }

    // Visible of the player caught
    [ServerRpc(RequireOwnership = false)]
    public void SetVisibilityServerRpc(bool enabled)
    {
        SetVisibilityClientRpc(enabled);
    }

    [ClientRpc]
    private void SetVisibilityClientRpc(bool enabled)
    {
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = enabled;
        }
    }

    // Collider of the player caught
    [ServerRpc(RequireOwnership = false)]
    public void SetColliderServerRpc(bool enabled)
    {
        SetColliderClientRpc(enabled);
    }

    [ClientRpc]
    private void SetColliderClientRpc(bool enabled)
    {
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = enabled;
        }
    }
}
