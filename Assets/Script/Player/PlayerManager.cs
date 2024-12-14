using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerManager : NetworkBehaviour
{
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.white);
    private SpriteRenderer spriteRenderer;
    public bool isImmuneToCatch;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        PlayerColor.OnValueChanged += OnColorChanged;
    }

    private void OnDestroy()
    {
        PlayerColor.OnValueChanged -= OnColorChanged;
    }

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

    // Set player immunity status for the worker role
    [ServerRpc(RequireOwnership = false)]
    public void SetImmunityServerRpc(bool enabled)
    {
        SetImmunityClientRpc(enabled);
    }

    [ClientRpc]
    private void SetImmunityClientRpc(bool enabled)
    {
        isImmuneToCatch = enabled;
    }

    // Set color changes for the worker role
    private void OnColorChanged(Color oldColor, Color newColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = newColor;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerColorServerRpc(Color color)
    {
        if (IsServer)
        {
            PlayerColor.Value = color;
        }
    }

    public void ResetColorAfterDelay(float delay)
    {
        StartCoroutine(ResetColorCoroutine(delay));
    }

    private IEnumerator ResetColorCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        RequestColorResetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestColorResetServerRpc()
    {
        PlayerColor.Value = Color.white;
    }
}
