using AlligUtils;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GizmoUIHandler : MonoBehaviour
{
    [SerializeField]
    private SerializedDictionary<Button, AxisDirection>  _axisButtons;

    [SerializeField] private Button _projectionBtn;
    private TextMeshProUGUI _projectionBtnText;
    private bool _isIso = false;
    
    private void Awake()
    {
        _projectionBtnText =  _projectionBtn.GetComponentInChildren<TextMeshProUGUI>();
        AddListenerToButtons();
    }

    private void AddListenerToButtons()
    {
        foreach (var (axisButton, direction) in _axisButtons)
        {
            axisButton.onClick.AddListener(delegate
            {
                direction.Print("Snapping to axis: ");
                AxisGizmo.Instance.SnapToAxis(direction);
            });
        }

        _projectionBtn.onClick.AddListener(delegate
        {
            _isIso = !_isIso;
            // CameraController.Instance.MainCamera.orthographic = _isIso;
            CameraController.Instance.SetProjection(_isIso);
            _projectionBtnText.text = _isIso? "Iso" : "Persp";
        });
        
    }
    
}
