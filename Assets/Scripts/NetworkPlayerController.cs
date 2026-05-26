using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float groundedGravity = -.5f;
    [SerializeField] float jumpHeight = 2f;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector2 inputDir = new(horizontalInput, verticalInput);

        bool isJumpPressed = Input.GetButtonDown("Jump");

        if (IsServer)
            MovePlayer(inputDir, isJumpPressed);
        else
            MovePlayerRPC(inputDir, isJumpPressed);
    }

    [Rpc(SendTo.Server)]
    private void MovePlayerRPC(Vector2 moveInput, bool isJumpPressed)
    {
        MovePlayer(moveInput, isJumpPressed);
    }

    private void MovePlayer(Vector2 moveInput, bool isJumpPressed)
    {
        if (characterController == null || !characterController.enabled) return;

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = groundedGravity;
            }

            if (isJumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 horiMove = moveDir * moveSpeed;
        Vector3 vertiMove = Vector3.up * verticalVelocity;
        Vector3 finalMove = horiMove + vertiMove;

        characterController.Move(finalMove * Time.deltaTime);
    }
}
