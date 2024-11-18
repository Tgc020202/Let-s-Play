using UnityEngine;
using UnityEngine.UI;

public class RoomCodeSetup : MonoBehaviour
{
    public Text roomNameText;
    void Start()
    {
        // Assign room code or "XXXX" if null
        if (roomNameText != null)
        {
            string roomName = RoomManager.Instance.roomName;
            roomNameText.text = !string.IsNullOrEmpty(roomName) ? roomName : "XXXX";
        }
    }
}
