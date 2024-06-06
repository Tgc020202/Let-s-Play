using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform player;
    void LateUpdate()
    {
        Vector3 cameraPosition = transform.position;
        cameraPosition.x = player.position.x;
        cameraPosition.y = player.position.y;
        transform.position = cameraPosition;
    }
}

