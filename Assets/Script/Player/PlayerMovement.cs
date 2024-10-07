using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveVelocity;
    private Joystick joystick;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        joystick = FindObjectOfType<Joystick>();
    }

    void Update()
    {
        Vector2 moveInput = joystick.GetInputDirection();

        // Keyboard Input
        if (moveInput == Vector2.zero)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveInput.y = 1;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                moveInput.y = -1;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveInput.x = -1;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveInput.x = 1;
        }

        moveVelocity = moveInput.normalized * speed;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
    }
}
