using AlligUtils;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

public class GizmoUIHandler : MonoBehaviour
{
    [SerializeField]
    private SerializedDictionary<Button, AxisDirection>  _axisButtons;

    [SerializeField] private Button _ortho, _perpective;
    
    private void Awake()
    {
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
    }
    
}
