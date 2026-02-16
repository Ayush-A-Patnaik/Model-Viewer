using AlligUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    private ModelViewerInput _input;
    private bool _orbiting, _panning;

    private float _yaw, _pitch, _verticalMove;

    private Vector2 _moveInput;
    
    [Header("References")]
    [SerializeField] private Transform _cameraRig;
    [SerializeField] Transform _pivot;
    [SerializeField] Camera _cam;

    [Header("Orbit")] 
    [SerializeField] float _orbitSpeed = 0.2f;
    [SerializeField] float _minPitch = -80f;
    [SerializeField] float _maxPitch = 80f;

    [Header("Pan")] 
    [SerializeField] float _panSpeed = 0.005f;

    [Header("Zoom")] 
    [SerializeField] float _zoomSpeed = 0.5f;
    [SerializeField] float _minZoom = -0.5f;
    [SerializeField] float _maxZoom = -50f;

    [Header("Fly")] 
    [SerializeField] float _flySpeed = 5f;
    [SerializeField] float _fastMultiplier = 3f;
    
    [Header("Focus Object")]
    public GameObject FocusObject;
    public float DistanceFromFocus;
    

    private void Awake()
    {
        _input = new ModelViewerInput();
    }

    private void OnEnable()
    {
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

        _pivot.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void OnPan(InputAction.CallbackContext ctx)
    {
        if (!_panning)
            return;

        Vector2 delta = ctx.ReadValue<Vector2>();

        Vector3 right = _cam.transform.right;
        Vector3 up = _cam.transform.up;

        _cameraRig.position -= (right * delta.x + up * delta.y) * _panSpeed;
    }

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        float scroll = ctx.ReadValue<float>();
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        "Zooming".Print();
        
        Vector3 pos = _pivot.transform.localPosition;
        pos.z += scroll * _zoomSpeed;
        pos.z = Mathf.Clamp(pos.z, _maxZoom, _minZoom);
        _pivot.transform.localPosition = pos;
        
        #region FOV method
        // float fov = _cam.fieldOfView;
        //
        // fov -= scroll * _zoomSpeed * Time.deltaTime;
        //
        // _cam.fieldOfView = Mathf.Clamp(fov, 15f, 100f);
        #endregion
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
            _cam.transform.forward * move.y +
            _cam.transform.right * move.x;

        _cameraRig.position += dir * speed * Time.deltaTime;
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
            _cam.transform.forward * _moveInput.y +
            _cam.transform.right * _moveInput.x + 
            _cam.transform.up * _verticalMove;

        _cameraRig.position += dir * speed * Time.deltaTime;
    }

    public void SnapCameraView(Quaternion rotation)
    {
        //SetPosition();
        SetRotation(rotation);
    }

    private void SetPosition()
    {
        if (FocusObject == null) return;
        
        _cameraRig.position = FocusObject.transform.position + new Vector3(0, 1.75f, -DistanceFromFocus);
        
    }
    private void SetRotation(Quaternion rotation)
    {
        _pivot.localRotation = rotation;
        
        Vector3 euler = rotation.eulerAngles;
        _yaw = euler.y;
        _pitch = euler.x;
        
        if (_pitch > 180f)
            _pitch -= 360f;
    }
    
    public Quaternion GetRotation()
    {
        return _pivot.localRotation;
    }
}