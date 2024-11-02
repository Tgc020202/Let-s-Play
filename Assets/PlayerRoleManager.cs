using UnityEngine;
using Unity.Netcode;

public class PlayerRoleManager : NetworkBehaviour
{
    public NetworkVariable<RoleType> playerRole = new NetworkVariable<RoleType>(RoleType.None, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

    private void Start()
    {
        if (IsOwner)
        {
            playerRole.OnValueChanged += OnRoleChanged;
        }
    }

    private void OnRoleChanged(RoleType oldRole, RoleType newRole)
    {
        Debug.Log("Role changed to: " + newRole.ToString());
    }
}
