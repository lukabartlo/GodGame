using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    private InputSystem_Actions _cameraActions;
    private InputAction _movement;
    private Transform _cameraTransform;
    
    [Header("Mouvement")]
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _damping = 15f;
    private float _speed;
    
    [Header("Zoom")]
    [SerializeField] private Transform _startZoomTransform;
    [SerializeField] private Transform _endZoomTransform;
    [SerializeField] private float _zoomCompletion = 0.5f;
    
    [SerializeField] private float _stepSize = 2f;
    [SerializeField] private float _zoomDampening = 7.5f;
    [SerializeField] private float _zoomSpeed = 2f;
    
    [Header("Rotation")]
    [SerializeField] private float _maxRotationSpeed = 1f;

    [Header("ScreenEdgeMotion")] 
    [SerializeField, Range(0.0f, 0.1f)] private float _edgeTolerance = 0.05f;
    [SerializeField] private bool _useScreenEdge = true;

    private Vector3 _targetPosition;
    
    private Vector3 _horizontalVelocity;
    private Vector3 _lastPosition;

    private Vector3 _startDrag;

    private void Awake()
    {
        _cameraActions = new InputSystem_Actions();
        _cameraTransform = this.GetComponentInChildren<Camera>().transform;
    }

    private void OnEnable()
    {
        /*_zoomHeight = _cameraTransform.localPosition.y;*/
        _cameraTransform.LookAt(this.transform);
        
        _lastPosition = this.transform.position;
        _movement = _cameraActions.Player.Move;
        _cameraActions.Player.RotateCamera.performed += RotateCamera;
        _cameraActions.Player.ZoomCamera.performed += ZoomCamera;
        _cameraActions.Player.Enable();
    }

    private void OnDisable()
    {
        _cameraActions.Player.RotateCamera.performed -= RotateCamera;
        _cameraActions.Player.ZoomCamera.performed -= ZoomCamera;
        _cameraActions.Disable();
    }

    private void Update()
    {
        GetKeyboardMovement();
        UpdateVelocity();
        UpdateCameraPosition();
        UpdateBasePosition();
    }

    private void UpdateVelocity()
    {
        _horizontalVelocity = (this.transform.position - _lastPosition) / Time.deltaTime;
        _horizontalVelocity.y = 0;
        _lastPosition = this.transform.position;
    }

    private void GetKeyboardMovement()
    {
        Vector3 inputValue = _movement.ReadValue<Vector2>().x * GetCameraRight() + _movement.ReadValue<Vector2>().y * GetCameraForward();

        inputValue = inputValue.normalized;

        if (inputValue.sqrMagnitude > 0.1f)
            _targetPosition += inputValue;
    }

    private Vector3 GetCameraRight()
    {
        Vector3 right = _cameraTransform.right;
        right.y = 0;
        return right;
    }
    
    private Vector3 GetCameraForward()
    {
        Vector3 forward = _cameraTransform.forward;
        forward.y = 0;
        return forward;
    }

    private void UpdateBasePosition()
    {
        if (_targetPosition.sqrMagnitude > 0.1f)
        {
            _speed = Mathf.Lerp(_speed, _maxSpeed, Time.deltaTime * _acceleration);
            transform.position += _targetPosition * (_speed * Time.deltaTime);
        }

        else
        {
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero, Time.deltaTime * _damping);
            transform.position += _horizontalVelocity * Time.deltaTime;
        }

        _targetPosition = Vector3.zero;
    }

    private void RotateCamera(InputAction.CallbackContext inputValue)
    {
        if (!Mouse.current.middleButton.isPressed)
            return;

        float value = inputValue.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f, value * _maxRotationSpeed + transform.rotation.eulerAngles.y, 0f);
    }

    private void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        float value = inputValue.ReadValue<Vector2>().y / 100f;
        
        _zoomCompletion += value;
        _zoomCompletion =  Mathf.Clamp(_zoomCompletion, 0, 1);
    }

    private void UpdateCameraPosition()
    {
        Vector3 zoomTarget = Vector3.Lerp(_startZoomTransform.position, _endZoomTransform.position, _zoomCompletion);
        
        _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, zoomTarget, Time.deltaTime * _zoomDampening);
        _cameraTransform.LookAt(this.transform);
    }
}
