using UnityEngine;
using UnityEngine.UI;

public class RoomCodeSetup : MonoBehaviour
{
    public Text roomCode;
    void Start()
    {
        // Assign room code or "XXXX" if null
        if (roomCode != null)
        {
            roomCode.text = !string.IsNullOrEmpty(VariableHolder.roomCode) ? VariableHolder.roomCode : "XXXX";
        }
    }
}
