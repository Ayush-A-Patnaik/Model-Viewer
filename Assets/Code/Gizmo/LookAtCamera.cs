using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform Target;
    private void LateUpdate()
    {
        // transform.forward = Camera.main.transform.forward * -1f;
        // transform.LookAt(Camera.main.transform);
        transform.LookAt(Target);
    }
}
