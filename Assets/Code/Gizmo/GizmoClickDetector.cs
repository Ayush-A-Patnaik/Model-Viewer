using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GizmoClickDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RawImage _gizmoImage;
    [SerializeField] private Camera _gizmoCamera;
    [SerializeField] private AxisGizmo _axisGizmo;

    [Header("Settings")]
    [SerializeField] private LayerMask _gizmoLayerMask;

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        ProcessClick(Mouse.current.position.ReadValue());
    }

    private void ProcessClick(Vector2 mousePosition)
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(
            _gizmoImage.rectTransform,
            mousePosition,
            null))
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _gizmoImage.rectTransform,
            mousePosition,
            null,
            out Vector2 localPoint);

        Rect rect = _gizmoImage.rectTransform.rect;

        float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        Vector2 gizmoScreenPoint = new Vector2(
            normalizedX * _gizmoCamera.pixelWidth,
            normalizedY * _gizmoCamera.pixelHeight
        );

        Ray ray = _gizmoCamera.ScreenPointToRay(gizmoScreenPoint);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _gizmoLayerMask))
        {
            AxisClickHandler axis = hit.collider.GetComponent<AxisClickHandler>();

            if (axis != null)
            {
                _axisGizmo.SnapToAxis(axis.AxisDirection);
            }
        }
    }

}
