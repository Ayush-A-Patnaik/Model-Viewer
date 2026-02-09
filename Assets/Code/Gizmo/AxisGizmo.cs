using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AxisGizmo : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _gizmoCamera;
    [SerializeField] private Transform _gizmoPivot;
    [SerializeField] private CameraController _cameraController;
    
    [Header("Gizmo Settings")]
    [SerializeField] private float _gizmoWidth = 100f;
    [SerializeField] private float _gizmoHeight = 100f;
    [SerializeField] private Vector2 _gizmoOffset = new Vector2(-60f, -60f);
    
    [Header("Snap Settings")]
    [SerializeField] private float _snapDuration = 0.3f;
    [SerializeField] private AnimationCurve _snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Interaction")]
    [SerializeField] private float _clickRadius = 30f;
    
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private RawImage _rawImage;
    
    private bool _isSnapping;
    private float _snapProgress;
    private Quaternion _snapStartRotation;
    private Quaternion _snapTargetRotation;
    
    private void Awake()
    {
        SetupGizmoUI();
        SetupGizmoCamera();
    }
    
    private void SetupGizmoUI()
    {
        // Find or create canvas
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null)
        {
            Debug.LogError("AxisGizmo must be child of a Canvas!");
            return;
        }
        
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform == null)
        {
            _rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        // Position in top-right corner
        _rectTransform.anchorMin = new Vector2(1, 1);
        _rectTransform.anchorMax = new Vector2(1, 1);
        _rectTransform.pivot = new Vector2(1, 1);
        _rectTransform.anchoredPosition = _gizmoOffset;
        _rectTransform.sizeDelta = new Vector2(_gizmoWidth, _gizmoHeight);
        
        // Setup RawImage to display render texture
        _rawImage = GetComponent<RawImage>();
        if (_rawImage == null)
        {
            _rawImage = gameObject.AddComponent<RawImage>();
        }
    }
    
    private void SetupGizmoCamera()
    {
        if (_gizmoCamera == null)
        {
            Debug.LogError("Gizmo Camera not assigned!");
            return;
        }
        
        // // Create render texture
        // _renderTexture = new RenderTexture((int)_gizmoSize, (int)_gizmoSize, 16);
        // _renderTexture.antiAliasing = 4;
        // _gizmoCamera.targetTexture = _renderTexture;
        // _rawImage.texture = _renderTexture;
        
        // Configure gizmo camera
        // _gizmoCamera.clearFlags = CameraClearFlags.SolidColor;
        // _gizmoCamera.backgroundColor = new Color(0, 0, 0, 0);
        // _gizmoCamera.orthographic = true;
        // _gizmoCamera.orthographicSize = 2f;
        // _gizmoCamera.nearClipPlane = 0.1f;
        // _gizmoCamera.farClipPlane = 10f;
        _gizmoCamera.depth = _mainCamera.depth + 1;
        
        // Position camera to look at gizmo
        _gizmoCamera.transform.position = _gizmoPivot.position + Vector3.back * 5f;
        _gizmoCamera.transform.LookAt(_gizmoPivot);
    }
    
    private void LateUpdate()
    {
        if (_mainCamera == null || _gizmoPivot == null)
            return;
        
        // Update gizmo rotation to match main camera
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
        
        // Apply to camera controller's pivot
        _cameraController.SetRotation(currentRotation);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Convert screen position to local position on gizmo
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out Vector2 localPoint);
        
        // Convert to normalized coordinates (-1 to 1)
        Vector2 normalizedPoint = localPoint / (_gizmoHeight * 0.5f);
        
        // Determine which axis was clicked
        AxisDirection clickedAxis = GetClickedAxis(normalizedPoint);
        
        if (clickedAxis != AxisDirection.None)
        {
            SnapToAxis(clickedAxis);
        }
    }
    
    private AxisDirection GetClickedAxis(Vector2 normalizedPoint)
    {
        float threshold = _clickRadius / (_gizmoHeight * 0.5f);
        
        // Project current camera rotation to determine axis positions on screen
        Vector3 xAxis = _gizmoPivot.rotation * Vector3.right;
        Vector3 yAxis = _gizmoPivot.rotation * Vector3.up;
        Vector3 zAxis = _gizmoPivot.rotation * Vector3.forward;
        
        // Convert 3D positions to 2D gizmo space
        Vector2 xPos = new Vector2(xAxis.x, xAxis.y);
        Vector2 yPos = new Vector2(yAxis.x, yAxis.y);
        Vector2 zPos = new Vector2(zAxis.x, zAxis.y);
        
        // Check distances
        if (Vector2.Distance(normalizedPoint, xPos) < threshold)
            return xAxis.z > 0 ? AxisDirection.PositiveX : AxisDirection.NegativeX;
        if (Vector2.Distance(normalizedPoint, -xPos) < threshold)
            return xAxis.z > 0 ? AxisDirection.NegativeX : AxisDirection.PositiveX;
            
        if (Vector2.Distance(normalizedPoint, yPos) < threshold)
            return yAxis.z > 0 ? AxisDirection.PositiveY : AxisDirection.NegativeY;
        if (Vector2.Distance(normalizedPoint, -yPos) < threshold)
            return yAxis.z > 0 ? AxisDirection.NegativeY : AxisDirection.PositiveY;
            
        if (Vector2.Distance(normalizedPoint, zPos) < threshold)
            return zAxis.z > 0 ? AxisDirection.PositiveZ : AxisDirection.NegativeZ;
        if (Vector2.Distance(normalizedPoint, -zPos) < threshold)
            return zAxis.z > 0 ? AxisDirection.NegativeZ : AxisDirection.PositiveZ;
        
        return AxisDirection.None;
    }
    
    private void SnapToAxis(AxisDirection axis)
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
    
    // private void OnDestroy()
    // {
    //     if (_renderTexture != null)
    //     {
    //         _renderTexture.Release();
    //         Destroy(_renderTexture);
    //     }
    // }
    
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
}
