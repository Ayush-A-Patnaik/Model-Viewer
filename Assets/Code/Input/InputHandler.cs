using UnityEngine;
using System;
using System.Collections.Generic;
using AlligUtils;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private const float DragThreshold = 100f; 
    
    public static InputHandler Instance;
    
    private ModelViewerInput _input;
    private Vector2 _mouseDownPosition;
    private bool _isDragging = false;

    public ModelViewerInput Input
    {
        get => _input;
        set => _input = value;
    }
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
        
        Input = new ModelViewerInput();
    }
    
    private void OnEnable()
    {
        _input.BasicInput.Enable();

        //_input.BasicInput.MouseLeft.started  += OnLeftMouseStarted;
        _input.BasicInput.MouseLeft.performed += OnLeftMousePerformed;
        //_input.BasicInput.MouseLeft.canceled  += OnLeftMouseCanceled;
        
        _input.BasicInput.MultiSelectBtn.performed += ctx =>
        {
            SelectionHandler.Instance.IsMultiSelect = _input.BasicInput.MultiSelectBtn.IsPressed();
        };
        _input.BasicInput.MultiSelectBtn.canceled += ctx =>
        {
            SelectionHandler.Instance.IsMultiSelect = _input.BasicInput.MultiSelectBtn.IsPressed();
        };
    }

    private void OnDisable()
    {
        _input.BasicInput.MouseLeft.performed -=  OnLeftMousePerformed;
        _input.BasicInput.Disable();

    }

    private void OnLeftMouseStarted(InputAction.CallbackContext ctx)
    {
        _mouseDownPosition = Mouse.current.position.ReadValue();
        _isDragging = false;
    }
    private void OnLeftMousePerformed(InputAction.CallbackContext ctx)
    {
        MouseClickDispatcher.ProcessMouseClick();
    }
    
    private void OnLeftMouseCanceled(InputAction.CallbackContext ctx)
    {
        if (_isDragging)
        {
            MouseClickDispatcher.ProcessDragSelect(
                _mouseDownPosition,
                Mouse.current.position.ReadValue(),
                SelectionHandler.Instance.IsMultiSelect
            );
        }
        else
        {
            MouseClickDispatcher.ProcessMouseClick();
        }

        _isDragging = false;
        MouseClickDispatcher.DragCanceled?.Invoke();
    }
}

public static class MouseClickDispatcher
{
    public static Action<GameObject> OnObjectClick;
    public static Action OnEmptyClicked;
    
    public static Action<Vector2> DragStarted;                  
    public static Action<Vector2, Vector2> DragUpdated;         
    public static Action DragCanceled;
    
    private static LayerMask _pickableMask =  1 << LayerMask.NameToLayer("Pickable");
    private static LayerMask _selectionMask = 1 << LayerMask.NameToLayer("SelectionMask");
    public static void ProcessMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _pickableMask);
        
        if (didHit)  
        {
            OnObjectClick?.Invoke(hit.collider.gameObject);
        }
        else
        {
            OnEmptyClicked?.Invoke();
        }
    }
    
    public static void ProcessDragSelect(Vector2 startScreen, Vector2 endScreen, bool isMultiSelect)
    {
        Rect screenRect = new Rect(
            Mathf.Min(startScreen.x, endScreen.x),
            Mathf.Min(startScreen.y, endScreen.y),
            Mathf.Abs(startScreen.x - endScreen.x),
            Mathf.Abs(startScreen.y - endScreen.y)
        );
        
        var pickables = GameObject.FindObjectsByType<Collider>(FindObjectsSortMode.None);
        var hits = new List<GameObject>();

        foreach (var col in pickables)
        {
            if (((_pickableMask.value) & (1 << col.gameObject.layer)) == 0) continue;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(col.bounds.center);
            
            if (screenPos.z < 0) continue;

            if (screenRect.Contains(new Vector2(screenPos.x, screenPos.y)))
                hits.Add(col.gameObject);
        }

        MeshSelection.AddObjects(hits, isMultiSelect);
    }
}

