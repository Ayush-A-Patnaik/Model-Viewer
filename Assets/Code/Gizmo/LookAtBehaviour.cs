using System;
using UnityEngine;
using UnityEngine.UI;

public class LookAtBehaviour : MonoBehaviour
{
    private GameObject _axisButton;
    public Transform SourceDirection;
    public Transform Target;

    [Range(-1f, 1f)]
    public float ViewThreshold = 0f;
    
    private void Awake()
    {
        _axisButton = GetComponentInChildren<Button>().gameObject;
        UpdateLookAt();
    }

    private void Start()
    {
        AxisGizmo.Instance.OnGizmoRotate += UpdateLookAt;
    }

    private void UpdateLookAt()
    {
        CalculateViewAngle();
        transform.LookAt(Target);
    }

    private void CalculateViewAngle()
    {
        // Vector3 axisWorld = transform.parent.TransformDirection(Direction).normalized;
        // Vector3 toCamera = (Target.position - transform.position).normalized;

        Vector3 A = SourceDirection.forward;
        Vector3 B = Target.forward;
        
        float dot = Vector3.Dot(A,B);
        
        bool visible = dot <= ViewThreshold;
        
        _axisButton.SetActive(visible);
    }
}
