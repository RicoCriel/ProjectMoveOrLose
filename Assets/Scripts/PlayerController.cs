using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _acceleration = 10f; 
    [SerializeField] private float _deceleration = 10f; 
    [SerializeField] private float _maxSpeed = 10f; 
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private float _verticalLookSpeed = 80f;
    [SerializeField] private float _jumpHeight = 2f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _cameraTransform;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _currentSpeed = 0f;
    private float _xRotation = 0f;
    private float _gravity = -9.81f;
    private bool _isJumping = false;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
            _isJumping = false;
        }

        // Horizontal movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate acceleration
        float targetSpeed = Mathf.Clamp01(Mathf.Sqrt(moveX * moveX + moveZ * moveZ));
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed * _maxSpeed, Time.deltaTime * (_acceleration * (targetSpeed > 0 ? 1 : _deceleration)));

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        _controller.Move(moveDirection * _currentSpeed * Time.deltaTime);

        // Rotation based on mouse input
        float mouseX = Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // Vertical look
        float mouseY = Input.GetAxis("Mouse Y") * _verticalLookSpeed * Time.deltaTime;
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // Jumping
        if (Input.GetButtonDown("Jump") && _isGrounded && !_isJumping)
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _isJumping = true;
        }

        // Apply gravity only if not grounded
        if (!_isGrounded)
        {
            _velocity.y += _gravity * Time.deltaTime;
        }

        // Bunnyhopping
        if (_isGrounded && _isJumping && Input.GetButtonDown("Jump"))
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _isJumping = true;
        }

        _controller.Move(_velocity * Time.deltaTime);
    }
}
