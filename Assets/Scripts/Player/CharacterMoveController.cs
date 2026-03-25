using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class CharacterMoveController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runSpeed = 7f;
    [SerializeField] private float _strafeSpeed = 3f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _jumpHeight = 5f;

    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius;
    [SerializeField] private LayerMask _groundLayer;

    private const float Gravity = -9.81f;

    private float _verticalVelocity;
    private bool _isGrounded;
    private CharacterController _characterController;
    private Transform _cameraTransform;
    private Animator _animator;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _cameraTransform = Camera.main.transform;
        _animator = GetComponent<Animator>();   
    }
    private void OnEnable()
    {
        InputManager.SwitchControlMap(InputManager.ControlMap.Player);
        InputManager.Actions.Player.Jump.performed += Jump;
    }

    void Update()
    {
        CheckGround();
        Move();
    }

    private void CheckGround()
    {
        bool grounded = false;

        if (_groundCheck != null)
        {
            grounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundLayer, QueryTriggerInteraction.Ignore);
        }

        _isGrounded = _characterController.isGrounded || grounded;

        if (!_isGrounded)
        {
            _animator.SetBool("IsGrounded", false);
        }
    }

    private void Move()
    {
        if (_isGrounded)
        {
            // Slight downward velocity to keep grounded stable
            if (_verticalVelocity < -2f)
                _verticalVelocity = -2f;
        }

        _verticalVelocity += Gravity * Time.deltaTime;

        Vector2 inputPlayer = InputManager.Actions.Player.Move.ReadValue<Vector2>();
        bool isRunnig = InputManager.Actions.Player.Run.IsPressed();

        Vector3 move = Vector3.zero;
        if (inputPlayer.magnitude > .1f)
        {

            // Direccion del movimiento
            var cameraForward = _cameraTransform.forward;
            var cameraRight = _cameraTransform.right;// ver como añado el cameraRight
            var playerDirection = new Vector3(cameraForward.x, 0, cameraForward.z);

            playerDirection = Vector3.ClampMagnitude(playerDirection, 1f);
            move = playerDirection * inputPlayer.y + cameraRight * inputPlayer.x;

            // Rotacion del personaje
            Quaternion rotation = Quaternion.LookRotation(playerDirection, Vector3.up);
            this.transform.rotation = rotation;

        }

        float finalSpeed = isRunnig ?  _runSpeed : _moveSpeed;

        if(Mathf.Sign(move.z) > 0)
        {
            move = move * finalSpeed + Vector3.up * _verticalVelocity;
        }
        else if (move.z == 0 && move.x != 0)
        {
            move = move * _strafeSpeed + Vector3.up * _verticalVelocity;
        }
        else
        {
            move = move * finalSpeed/2 + Vector3.up * _verticalVelocity;
        }
        

        _characterController.Move(move * Time.deltaTime);
        UpdateAnimator(inputPlayer, isRunnig);
    }

    private void Jump(InputAction.CallbackContext ctx)
    {
        if (_isGrounded)
        {
            _animator.SetBool("IsGrounded", true);
            _isGrounded = false;
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * Gravity);
        }
    }

    private void UpdateAnimator(Vector2 moveInput, bool isRunning)
    {
        if (isRunning) 
        {
            _animator.SetFloat("Forward", moveInput.y);
        }
        else
        {
            _animator.SetFloat("Forward", moveInput.y * .5f);
            _animator.SetFloat("Strafe", moveInput.x);
        }     
    }
}
