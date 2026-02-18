using UnityEngine;
using System;
using AlligUtils;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance;
    
    private ModelViewerInput _input;

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

        _input.BasicInput.MouseLeft.performed += OnLeftMousePerformed;
        
        _input.BasicInput.MultiSelectBtn.performed += ctx =>
        {
            SelectionHandler.Instance.ShouldMultiSelect = _input.BasicInput.MultiSelectBtn.IsPressed();
        };
        _input.BasicInput.MultiSelectBtn.canceled += ctx =>
        {
            SelectionHandler.Instance.ShouldMultiSelect = _input.BasicInput.MultiSelectBtn.IsPressed();
        };
    }

    private void OnDisable()
    {
        _input.BasicInput.MouseLeft.performed -=  OnLeftMousePerformed;
        _input.BasicInput.Disable();
        
    }

    private void OnLeftMousePerformed(InputAction.CallbackContext ctx)
    {
        MouseClickDispatcher.ProcessMouseClick();
    }
}

public static class MouseClickDispatcher
{
    public static Action<RaycastHit> OnMouseClick;
    private static LayerMask _hitDetectLayer = (1 << LayerMask.NameToLayer("Pickable")) |  (1 << LayerMask.NameToLayer("SelectionMask"));
    public static void ProcessMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, _hitDetectLayer))
        {
            OnMouseClick?.Invoke(hit);
        }
    }
}
