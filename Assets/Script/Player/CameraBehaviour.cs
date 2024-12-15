using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    // UI Components
    public Vector3 offset;
    public Transform target;

    // Defines
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            transform.LookAt(target);
        }
        else
        {
            Debug.LogWarning("Camera target is null!");
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log($"Camera target set to: {target.name}");
    }

    public void ChangeTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
