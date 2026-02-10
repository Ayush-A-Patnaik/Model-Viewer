using UnityEngine;

[ExecuteAlways]
public class AxisGizmoVisual : MonoBehaviour
{
    [Header("Axis Settings")]
    [SerializeField] private float _axisLength = 1.5f;
    [SerializeField] private float _axisThickness = 0.08f;
    [SerializeField] private float _arrowHeadSize = 0.2f;
    
    [Header("Colors")]
    [SerializeField] private Color _xAxisColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color _yAxisColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _zAxisColor = new Color(0.2f, 0.4f, 1f);
    [SerializeField] private Color _negativeAxisColor = new Color(0.5f, 0.5f, 0.5f);
    
    [Header("Materials")]
    [SerializeField] private Material _axisMaterial;
    
    [Header("Label Fade")]
    [SerializeField] private float _fadeStartDot = 0.85f;
    [SerializeField] private float _fadeEndDot = 0.98f;
    
    private GameObject _xAxisPositive;
    private GameObject _xAxisNegative;
    private GameObject _yAxisPositive;
    private GameObject _yAxisNegative;
    private GameObject _zAxisPositive;
    private GameObject _zAxisNegative;
    
    private void Start()
    {
        
    }

    [ContextMenu("Create Gizmo")]
    public void CreateGizmo()
    {
        CreateAxisMaterial();
        CreateAxes();
    }
    
    private void CreateAxisMaterial()
    {
        if (_axisMaterial == null)
        {
            _axisMaterial = new Material(Shader.Find("Unlit/Color"));
        }
    }
    
    private void CreateAxes()
    {
        _xAxisPositive = CreateAxisArrow("X+", Vector3.right, _xAxisColor);
        _xAxisNegative = CreateAxisArrow("X-", Vector3.left, _negativeAxisColor);
        
        _yAxisPositive = CreateAxisArrow("Y+", Vector3.up, _yAxisColor);
        _yAxisNegative = CreateAxisArrow("Y-", Vector3.down, _negativeAxisColor);

        _zAxisPositive = CreateAxisArrow("Z+", Vector3.forward, _zAxisColor);
        _zAxisNegative = CreateAxisArrow("Z-", Vector3.back, _negativeAxisColor);
    }
    
    private GameObject CreateAxisArrow(string name, Vector3 direction, Color color)
    {
        GameObject axisRoot = new GameObject(name);
        axisRoot.transform.SetParent(transform);
        axisRoot.transform.localPosition = Vector3.zero;
        axisRoot.transform.localRotation = Quaternion.identity;
        
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(axisRoot.transform);
        
        float shaftLength = _axisLength - _arrowHeadSize;
        shaft.transform.localPosition = direction * (shaftLength * 0.5f);
        shaft.transform.localScale = new Vector3(_axisThickness, shaftLength * 0.5f, _axisThickness);
        shaft.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
        
        GameObject arrowHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        arrowHead.name = "ArrowHead";
        arrowHead.transform.SetParent(axisRoot.transform);
        arrowHead.transform.localPosition = direction * _axisLength;
        arrowHead.transform.localScale = Vector3.one * _arrowHeadSize;
        
        Material mat = new Material(_axisMaterial);
        mat.color = color;
        
        shaft.GetComponent<Renderer>().material = mat;
        arrowHead.GetComponent<Renderer>().material = mat;
        
        DestroyImmediate(shaft.GetComponent<Collider>());
        DestroyImmediate(arrowHead.GetComponent<Collider>());
        
        CreateAxisLabel(axisRoot, direction, name, color);
        
        
        return axisRoot;
    }
    
    private void CreateAxisLabel(GameObject parent, Vector3 direction, string text, Color color)
    {
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent.transform);
        labelObj.transform.localPosition = direction * (_axisLength + 0.3f);
        
        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = text.Replace("+", "").Replace("-", "");
        textMesh.fontSize = 40;
        textMesh.characterSize = 0.15f;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        
        labelObj.transform.localRotation = Quaternion.identity;
        labelObj.transform.localScale = Vector3.one * 0.5f;
    }
    
    // private void Update()
    // {
    //     FaceCameraForLabels(_xAxisPositive);
    //     FaceCameraForLabels(_xAxisNegative);
    //     FaceCameraForLabels(_yAxisPositive);
    //     FaceCameraForLabels(_yAxisNegative);
    //     FaceCameraForLabels(_zAxisPositive);
    //     FaceCameraForLabels(_zAxisNegative);
    // }
    //
    // private void FaceCameraForLabels(GameObject axisObject)
    // {
    //     if (axisObject == null) return;
    //     if (Camera.main == null) return;
    //
    //     Transform label = axisObject.transform.Find("Label");
    //     if (label == null) return;
    //
    //     Camera cam = Camera.main;
    //
    //     // --- Billboard (ignore parent rotation) ---
    //     Vector3 dir = label.position - cam.transform.position;
    //     label.rotation = Quaternion.LookRotation(dir, cam.transform.up);
    //
    //     // --- Fade when aligned with view ---
    //     Vector3 toLabel = dir.normalized;
    //     float dot = Vector3.Dot(cam.transform.forward, toLabel);
    //
    //     float alpha = 1f;
    //     if (dot > _fadeStartDot)
    //     {
    //         alpha = Mathf.InverseLerp(_fadeEndDot, _fadeStartDot, dot);
    //     }
    //
    //     TextMesh textMesh = label.GetComponent<TextMesh>();
    //     if (textMesh != null)
    //     {
    //         Color c = textMesh.color;
    //         c.a = alpha;
    //         textMesh.color = c;
    //     }
    // }

}
