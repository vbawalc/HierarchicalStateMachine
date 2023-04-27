using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    //Reference Variables
    CharacterController _characterController;
    Animator _animator;
    PlayerInput _playerInput;

    //Variables to store setter/getter parameter IDs
    int _isWalkingHash;
    int _isRunningHash;

    //Variables to store player input Values
    Vector2 _currentMovementinput;
    Vector3 _currentMovement;
    Vector3 _appliedMovement;
    bool _isMovementPressed;
    bool _isRunPressed;

    //Gravity variables
    float _gravity = -9.8f;
    float _groundedGravity = -.05f;

    //Jumping variables
    bool _isJumpPressed = false;
    float _initialJumpVelocity;
    float _maxJumpHeight = 4.0f;
    float _maxJumpTime = .75f;
    bool _isJumping = false;
    int _isJumpingHash;
    int _jumpCountHash;
    bool _requireNewJumpPress = false;
    int _jumpCount = 0;
    Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();
    Coroutine _currentJumpResetRoutine = null;

    //State variables
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    //Getters and setters
    public CharacterController CharacterController { get { return _characterController; } set { _characterController = value; } }
    public PlayerBaseState CurrentState {get { return _currentState; } set { _currentState = value; }}
    public Animator Animator { get { return _animator; }}
    public Coroutine CurrentJumpResetRoutine { get { return _currentJumpResetRoutine; } set { _currentJumpResetRoutine = value; }}
    public Dictionary<int,float> InitialJumpVelocities { get { return _initialJumpVelocities; }}
    public int JumpCount { get { return _jumpCount; } set { _jumpCount = value; }}
    public int IsWalkingHash { get { return _isWalkingHash; }}
    public int IsRunningHash { get { return _isRunningHash; }}
    public int IsJumpingHash { get { return _isJumpingHash; }}
    public int JumpCountHash { get { return _jumpCountHash; }}
    public bool RequireNewJumpPress { get { return _requireNewJumpPress; } set { _requireNewJumpPress = value; }}
    public bool IsJumping { set { _isJumping = value; }}
    public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; }}
    public float AppliedMovementY { get {  return _appliedMovement.y ; } set { _appliedMovement.y = value; }}
    public float GroundedGravity { get { return _groundedGravity; } set { _groundedGravity = value; }}
    public bool IsJumpPressed { get { return _isJumpPressed; } set { _isJumpPressed = value; }}
    public bool IsMovementPressed { get { return _isMovementPressed; } set { _isMovementPressed = value; }}
    public bool IsRunPressed { get { return _isRunPressed; } set { _isRunPressed = value; }}
    public Dictionary<int,float> JumpGravities { get { return _jumpGravities; }}
    public float AppliedMovementX { get { return _appliedMovement.x; } set { _appliedMovement.x = value; }}
    public float AppliedMovementZ { get { return _appliedMovement.z; } set { _appliedMovement.z = value; }}
    public float RunMultiplier { get { return _runMultiplier; } }
    public Vector2 CurrentMovementInput { get { return _currentMovementinput; } }


    //Constants
    float _rotationFactorPerFrame = 15.0f;
    float _runMultiplier = 4.0f;
    int _zero = 0;

   

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        _currentState.UpdateStates();
        _characterController.Move(_appliedMovement * Time.deltaTime);
    }

    //Callback Handler to set player input values
    void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementinput = context.ReadValue<Vector2>();
        _isMovementPressed = _currentMovementinput.x != _zero || _currentMovementinput .y != _zero;
    }

    //Callback Handler for jump button
    void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
        _requireNewJumpPress = false;
    }
    
    //Callback Handler for run button
   void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    //Enable character controls action map
    
    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }

    //Disable character controls action map
    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    } 
    //Awake is called earlier than Start in Unity's Event life cycle 
    private void Awake()
    {
        //set reference variables
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        //setup state
        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        //set parameter hash references
        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _jumpCountHash = Animator.StringToHash("jumpCount");

        //set player input callbacks
        _playerInput.CharacterControls.Move.started += OnMovementInput;
        _playerInput.CharacterControls.Move.canceled += OnMovementInput;
        _playerInput.CharacterControls.Move.performed += OnMovementInput;
        _playerInput.CharacterControls.Run.started += OnRun;
        _playerInput.CharacterControls.Run.canceled += OnRun;
        _playerInput.CharacterControls.Jump.started += OnJump;
        _playerInput.CharacterControls.Jump.canceled += OnJump;

        SetupJumpVariables();
    }

    void SetupJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * (_maxJumpHeight +2)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (_maxJumpHeight+2)) / (timeToApex*1.25f);
        float thirdJumpGravity = (-2 * (_maxJumpHeight + 4)) / Mathf.Pow((timeToApex * 1.5f), 2);
        float thirdJumpInitialVelocity = (2 * (_maxJumpHeight + 4)) / (timeToApex * 1.5f);

        //Add Values to Dictionaries
        _initialJumpVelocities.Add(1, _initialJumpVelocity);
        _initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        _initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        _jumpGravities.Add(0,_gravity);
        _jumpGravities.Add(1, _gravity);
        _jumpGravities.Add(2, secondJumpGravity);
        _jumpGravities.Add(3, thirdJumpGravity);
    }

    void HandleRotation()
    {
        Vector3 possitionToLookAt;

        //change in possition our character should point to
        possitionToLookAt.x = _currentMovementinput.x;
        possitionToLookAt.y = _zero;
        possitionToLookAt.z = _currentMovementinput.y;

        // current rotation of our character
        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            //creates new rotation based on player movement input
            Quaternion targetRotation = Quaternion.LookRotation(possitionToLookAt);
            //rotates character to face desired possition
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }
    }
}
