using UnityEngine;
using Unity.Netcode;

public class PlayerRoleManager : NetworkBehaviour
{
    public NetworkVariable<RoleType> playerRole = new NetworkVariable<RoleType>(RoleType.None, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

    private void Start()
    {
        if (IsOwner)
        {
            // Ensure UI updates when the role changes for the local player
            playerRole.OnValueChanged += OnRoleChanged;
        }
    }

    private void OnRoleChanged(RoleType oldRole, RoleType newRole)
    {
        // Update UI here based on the new role (Boss or Worker)
        Debug.Log("Role changed to: " + newRole.ToString());
    }
}
