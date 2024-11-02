using UnityEngine;

public class CollisionTriggerDisplay : MonoBehaviour
{
    public bool canCatchPlayer = false;
    public bool canHelpPlayer = false;
    public GameObject targetPlayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        if (other.CompareTag("Player") && other.gameObject != gameObject)
        {
            Debug.Log("Catch/Help: Player within range: " + other.gameObject.name);
            targetPlayer = other.gameObject;
            canCatchPlayer = canHelpPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject == targetPlayer)
        {
            Debug.Log("Player moved out of range: " + other.gameObject.name);
            canCatchPlayer = canHelpPlayer = false;
            targetPlayer = null;
        }
    }
}
