using UnityEngine;
using System;
using System.Collections.Generic;
using AlligUtils;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private const float DragThreshold = 10f; 
    
    public static InputHandler Instance;
    
    private ModelViewerInput _input;
    private Vector2 _mouseDownPosition;
    private bool _isDragging = false, _isMouseHeld = false;

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

        _input.BasicInput.MouseLeft.started  += OnLeftMouseStarted;
        _input.BasicInput.MouseLeft.performed += OnLeftMousePerformed;
        _input.BasicInput.MouseLeft.canceled  += OnMouseCanceled;
        
        _input.BasicInput.MultiSelectBtn.performed += ctx =>
        {
            SelectionHandler.Instance.IsMultiSelect = _input.BasicInput.MultiSelectBtn.IsPressed();
        };
        _input.BasicInput.MultiSelectBtn.canceled += ctx =>
        {
            SelectionHandler.Instance.IsMultiSelect = _input.BasicInput.MultiSelectBtn.IsPressed();
        };

        InputSystem.onAfterUpdate += CheckDrag;
    }

    private void OnDisable()
    {
        _input.BasicInput.MouseLeft.started  -= OnLeftMouseStarted;
        _input.BasicInput.MouseLeft.performed -=  OnLeftMousePerformed;
        _input.BasicInput.MouseLeft.canceled  -= OnMouseCanceled;
        _input.BasicInput.Disable();
        InputSystem.onAfterUpdate -= CheckDrag;
    }

    private void OnLeftMouseStarted(InputAction.CallbackContext ctx)
    {
        _mouseDownPosition = Mouse.current.position.ReadValue();
        _isMouseHeld = true;
        _isDragging = false;
    }
    private void OnLeftMousePerformed(InputAction.CallbackContext ctx)
    {
        MouseClickDispatcher.ProcessMouseClick();
    }

    private void OnMouseCanceled(InputAction.CallbackContext ctx)
    {
        _isMouseHeld  = false;
        if (_isDragging)
            MouseClickDispatcher.DragCanceled?.Invoke();
    }
    private void CheckDrag()
    {
        if (!_isMouseHeld) return;
        
        Vector2 current =  Mouse.current.position.ReadValue();

        if (!_isDragging)
        {
            if (Vector2.Distance(current, _mouseDownPosition) > DragThreshold)
            {
                _isDragging = true;
                MouseClickDispatcher.DragStarted?.Invoke();
            }
        }
        
        if (_isDragging)
        {
            MouseClickDispatcher.DragUpdated?.Invoke(_mouseDownPosition, current);
        }
    }
}

public static class MouseClickDispatcher
{
    public static Action<GameObject> OnObjectClick;
    public static Action OnEmptyClicked;
    
    public static Action DragStarted;                  
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

}

