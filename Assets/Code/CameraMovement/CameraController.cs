using AlligUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    private ModelViewerInput _input;
    private bool _orbiting, _panning;

    private float _yaw, _pitch, _verticalMove;

    private Vector2 _moveInput;
    
    [Header("References")]
    [SerializeField] private Transform _cameraRig;
    [SerializeField] private Transform _pivot;
    [SerializeField] private Camera _cam;

    [Header("Orbit")] 
    [SerializeField] private float _orbitSpeed = 0.2f;
    [SerializeField] private float _minPitch = -80f;
    [SerializeField] private float _maxPitch = 80f;

    [Header("Pan")] 
    [SerializeField] private float _panSpeed = 0.005f;

    [Header("Zoom")] 
    [SerializeField] private float _zoomSpeed = 0.5f;
    [SerializeField] private float _minZoom = -0.5f;
    [SerializeField] private float _maxZoom = -50f;

    [Header("Fly")] 
    [SerializeField] private float _flySpeed = 5f;
    [SerializeField] private float _fastMultiplier = 3f;
    
    [SerializeField] private float _virtualDistance = 10f; 
    // [Header("Focus Object")]
    // public GameObject FocusObject;
    // public float DistanceFromFocus;
    
    public Camera MainCamera => _cam;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnEnable()
    {
        _input = InputHandler.Instance.Input;
        _input.Camera.Enable();

        _input.Camera.OrbitBtn.started += _ => _orbiting = true;
        _input.Camera.OrbitBtn.canceled += _ => _orbiting = false;

        _input.Camera.PanBtn.started += _ => _panning = true;
        _input.Camera.PanBtn.canceled += _ => _panning = false;

        _input.Camera.Look.performed += OnLook;
        _input.Camera.Pan.performed += OnPan;

        _input.Camera.Zoom.performed += OnZoom;

        _input.Camera.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _input.Camera.Move.canceled += _ => _moveInput = Vector2.zero;
        
        _input.Camera.Vertical.performed += ctx => _verticalMove = ctx.ReadValue<float>();
        _input.Camera.Vertical.canceled += _ => _verticalMove = 0f;
        
        InputSystem.onAfterUpdate += ApplyMovement;
    }

    private void OnDisable()
    {
        _input.Camera.Disable();

        _input.Camera.OrbitBtn.started -= _ => _orbiting = true;
        _input.Camera.OrbitBtn.canceled -= _ => _orbiting = false;
        _input.Camera.PanBtn.started -= _ => _panning = true;
        _input.Camera.PanBtn.canceled -= _ => _panning = false;

        _input.Camera.Look.performed -= OnLook;
        _input.Camera.Pan.performed -= OnPan;
        _input.Camera.Zoom.performed -= OnZoom;
        
        InputSystem.onAfterUpdate -= ApplyMovement;
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        if (!_orbiting || _panning)
            return;

        Vector2 delta = ctx.ReadValue<Vector2>();

        _yaw += delta.x * _orbitSpeed;
        _pitch -= delta.y * _orbitSpeed;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        // _pivot.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        _pivot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void OnPan(InputAction.CallbackContext ctx)
    {
        if (!_panning)
            return;

        Vector2 delta = ctx.ReadValue<Vector2>();

        Vector3 right = MainCamera.transform.right;
        Vector3 up = MainCamera.transform.up;

        // _cameraRig.position -= (right * delta.x + up * delta.y) * _panSpeed;
        _pivot.position -= (right * delta.x + up * delta.y) * _panSpeed;
    }
    
    private void OnZoom(InputAction.CallbackContext ctx)
    {
        float scroll = ctx.ReadValue<float>();
        if (Mathf.Abs(scroll) < 0.01f)
            return;
        //_virtualDistance = Mathf.Clamp(_virtualDistance, _minZoom, _maxZoom);
        
        if (MainCamera.orthographic)
            ZoomOrthographic();
        else
            ZoomPerspective(scroll);
        
        _virtualDistance -= scroll * _zoomSpeed;
        
    }
    private void ZoomPerspective(float scroll)
    {
        Vector3 forward = MainCamera.transform.forward;
        
        // Vector3 newPos = _cameraRig.position + forward * scroll * _zoomSpeed;
        Vector3 newPos = _pivot.position + forward * scroll * _zoomSpeed;
        //newPos.z = Mathf.Clamp(newPos.z, _minZoom, _maxZoom);
        _pivot.position = newPos;
    }
    
    private void ZoomOrthographic()
    {
        float fovRad = MainCamera.fieldOfView * Mathf.Deg2Rad;
        MainCamera.orthographicSize = _virtualDistance * Mathf.Tan(fovRad / 2f);
    }
    
    public void SetProjection(bool isOrthographic)
    {
        float fovRad = MainCamera.fieldOfView * Mathf.Deg2Rad;

        if (isOrthographic)
        {
            MainCamera.orthographicSize = _virtualDistance * Mathf.Tan(fovRad / 2f);
            MainCamera.orthographic = true;
        }
        else
        {
            _virtualDistance = MainCamera.orthographicSize / Mathf.Tan(fovRad / 2f);
            MainCamera.orthographic = false;
        }
    }
    

    void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 move = ctx.ReadValue<Vector2>();
        if (move.sqrMagnitude < 0.001f)
            return;

        float speed = _flySpeed;
        if (_input.Camera.FastMove.IsPressed())
            speed *= _fastMultiplier;

        Vector3 dir =
            MainCamera.transform.forward * move.y +
            MainCamera.transform.right * move.x;

        _pivot.position += dir * speed * Time.deltaTime;
    }
    
    private void ApplyMovement()
    {
        if (!_orbiting)
            return;
        if (_moveInput.sqrMagnitude < 0.001f && Mathf.Abs(_verticalMove) < 0.001f)
            return;

        float speed = _flySpeed;
        if (_input.Camera.FastMove.IsPressed())
            speed *= _fastMultiplier;

        Vector3 dir =
            MainCamera.transform.forward * _moveInput.y +
            MainCamera.transform.right * _moveInput.x + 
            MainCamera.transform.up * _verticalMove;

        _pivot.position += dir * speed * Time.deltaTime;
    }

    public void SnapCameraView(Quaternion rotation)
    {
        //SetPosition();
        SetRotation(rotation);
    }

    // private void SetPosition()
    // {
    //     if (FocusObject == null) return;
    //     
    //     _cameraRig.position = FocusObject.transform.position + new Vector3(0, 1.75f, -DistanceFromFocus);
    //     
    // }
    private void SetRotation(Quaternion rotation)
    {
        // _pivot.localRotation = rotation;
        _cameraRig.rotation = rotation;
        _pivot.localRotation = Quaternion.identity;
        
        Vector3 euler = rotation.eulerAngles;
        _yaw = euler.y;
        _pitch = euler.x;
        
        if (_pitch > 180f)
            _pitch -= 360f;
    }
    
    public Quaternion GetRotation()
    {
        // return _pivot.localRotation;
        return _cameraRig.rotation;
    }
}