using UnityEngine;
using UnityEngine.UI;

public class RoomCodeSetup : MonoBehaviour
{
    // UI Components
    public Text roomNameText;

    // Defines
    private string roomName;

    void Start()
    {
        if (roomNameText != null)
        {
            roomName = RoomManager.Instance != null && !string.IsNullOrEmpty(RoomManager.Instance.roomName) ? RoomManager.Instance.roomName : "XXXX";
            roomNameText.text = roomName;
        }
    }
}
