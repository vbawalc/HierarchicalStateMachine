using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed = 1;

    [SerializeField]
    private float jumpHeight = 10;

    [SerializeField]
    private float gravity = 9.81f;

    private Vector2 movementVector;

    private CharacterController characterController;

    private float verticalVelocity;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    //Read Value from Input
    public void OnMove(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
    }

    public void Move()
    {
        verticalVelocity += -gravity * Time.deltaTime;

        if(characterController.isGrounded && verticalVelocity <0) //Stop when on the ground
        {
            verticalVelocity = 0;
        }

        Vector3 move = transform.right * movementVector.x + transform.forward*movementVector.y + transform.up*verticalVelocity;
        characterController.Move(move*movementSpeed*Time.deltaTime);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(characterController.isGrounded && context.performed)
        {
            Jump();
        }
    }

    private void Jump()
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * gravity);
    }
}
