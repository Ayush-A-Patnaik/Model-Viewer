using System;
using UnityEngine;

public class AxisClickHandler : MonoBehaviour
{
    [SerializeField] private AxisDirection _axisDirection;
    private AxisGizmo _axisGizmo;

    private void Awake()
    {
        _axisGizmo = FindObjectOfType<AxisGizmo>();
    }

    public void OnEnable()
    {
        MouseClickDispatcher.OnMouseClick += SendSnapDirection;
    }

    private void OnDisable()
    {
        MouseClickDispatcher.OnMouseClick -= SendSnapDirection;
    }

    private void SendSnapDirection(RaycastHit hit)
    {
        if (hit.collider.gameObject == gameObject)
        {
            Debug.Log($"{hit.collider.gameObject.name} is being clicked by the mouse");
            Debug.Log("Kuch toh ho raha hai");
            _axisGizmo.SnapToAxis(_axisDirection);
        }
    }
}
