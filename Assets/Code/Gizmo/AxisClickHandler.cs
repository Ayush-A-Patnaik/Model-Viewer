using System;
using AlligUtils;
using UnityEngine;
using UnityEngine.UI;
public class AxisClickHandler : MonoBehaviour
{
    [SerializeField] private AxisDirection _axisDirection;
    
    private Button _axisButton;

    private void Awake()
    {
        _axisButton = GetComponent<Button>();
        if(_axisButton == null)
            _axisButton = gameObject.AddComponent<Button>();
        
    }

    public void Start()
    {
        _axisButton.onClick.AddListener(delegate
        {
            _axisDirection.Print("Snapping to axis: ");
            AxisGizmo.Instance.SnapToAxis(_axisDirection);
        });
    }
}
