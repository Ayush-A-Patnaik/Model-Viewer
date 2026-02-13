using UnityEngine;

public class AxisGizmo : MonoBehaviour
{
    public static AxisGizmo Instance;
    
    [Header("Camera References")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _gizmoPivot;
    [SerializeField] private CameraController _cameraController;
    
    
    [Header("Snap Settings")]
    [SerializeField] private float _snapDuration = 0.3f;
    [SerializeField] private AnimationCurve _snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Interaction")]
    [SerializeField] private float _clickRadius = 30f;
    
    private bool _isSnapping;
    private float _snapProgress;
    private Quaternion _snapStartRotation;
    private Quaternion _snapTargetRotation;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void LateUpdate()
    {
        if (_mainCamera == null || _gizmoPivot == null)
            return;
        
        if (!_isSnapping)
        {
            _gizmoPivot.rotation = Quaternion.Inverse(_mainCamera.transform.rotation);
        }
        else
        {
            UpdateSnapping();
        }
    }
    
    private void UpdateSnapping()
    {
        _snapProgress += Time.deltaTime / _snapDuration;
        
        if (_snapProgress >= 1f)
        {
            _snapProgress = 1f;
            _isSnapping = false;
        }
        
        float t = _snapCurve.Evaluate(_snapProgress);
        Quaternion currentRotation = Quaternion.Slerp(_snapStartRotation, _snapTargetRotation, t);
        
        _cameraController.SetRotation(currentRotation);
    }
    
    
    public void SnapToAxis(AxisDirection axis)
    {
        Vector3 direction = Vector3.zero;
        
        switch (axis)
        {
            case AxisDirection.PositiveX: direction = Vector3.right; break;
            case AxisDirection.NegativeX: direction = Vector3.left; break;
            case AxisDirection.PositiveY: direction = Vector3.up; break;
            case AxisDirection.NegativeY: direction = Vector3.down; break;
            case AxisDirection.PositiveZ: direction = Vector3.forward; break;
            case AxisDirection.NegativeZ: direction = Vector3.back; break;
        }
        
        _snapStartRotation = _mainCamera.transform.rotation;
        _snapTargetRotation = Quaternion.LookRotation(-direction, Vector3.up);
        
        _snapProgress = 0f;
        _isSnapping = true;
    }
}

public enum AxisDirection
{
    None,
    PositiveX,
    NegativeX,
    PositiveY,
    NegativeY,
    PositiveZ,
    NegativeZ
}
