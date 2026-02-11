using UnityEngine;

public class GizmoUIHandler : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    
    [Header("Gizmo Settings")]
    [SerializeField] private float _gizmoWidth = 400f;
    [SerializeField] private float _gizmoHeight = 300f;
    [SerializeField] private Vector2 _gizmoOffset = new Vector2(-30f, -30f);
    
    private void Awake()
    {
        SetupGizmoRect();
    }
    
    private void SetupGizmoRect()
    {
        _rectTransform.anchorMin = new Vector2(1, 1);
        _rectTransform.anchorMax = new Vector2(1, 1);
        _rectTransform.pivot = new Vector2(1, 1);
        _rectTransform.anchoredPosition = _gizmoOffset;
        _rectTransform.sizeDelta = new Vector2(_gizmoWidth, _gizmoHeight);
    }
}
