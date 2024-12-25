using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    // UI Components
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 fixedPosition;

    // Defines
    private bool isStopped = false;
    public float speed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (IsOwner)
        {
            CameraBehaviour cameraBehaviour = Camera.main.GetComponent<CameraBehaviour>();
            if (cameraBehaviour != null)
            {
                cameraBehaviour.SetTarget(transform);
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (isStopped)
        {
            rb.position = fixedPosition;
        }

        moveInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveInput.y = 1;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveInput.y = -1;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveInput.x = -1;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveInput.x = 1;

        // Normalize the input to prevent faster diagonal movement
        moveInput.Normalize();

        MovePlayerServerRpc(moveInput);
    }

    // Movement of the player
    [ServerRpc]
    private void MovePlayerServerRpc(Vector2 direction)
    {
        // Move the player based on direction
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }

    // Speed of the player movement
    [ServerRpc]
    public void IncreaseSpeedServerRpc(bool isIncrease, float boost)
    {
        speed = isIncrease ? speed + boost : speed - boost;
        Debug.Log("Speed become: " + speed);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopServerRpc(bool isStop)
    {
        isStopped = isStop;

        if (isStop)
        {
            fixedPosition = rb.position;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            speed = 0f;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.None;
            speed = 5f;
        }

        Debug.Log($"Speed set to: {speed}. Rigidbody constraints updated.");
    }
}
