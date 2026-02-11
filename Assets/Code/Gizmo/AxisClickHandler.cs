using System;
using UnityEngine;

public class AxisClickHandler : MonoBehaviour
{
    [SerializeField] private AxisDirection _axisDirection;
    public AxisDirection AxisDirection => _axisDirection;
}
