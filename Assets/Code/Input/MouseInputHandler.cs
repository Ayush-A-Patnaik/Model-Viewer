using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class MouseInputHandler : MonoBehaviour
{
    private ModelViewerInput _input;

    private void Awake()
    {
        _input = new ModelViewerInput();
    }

    private void OnEnable()
    {
        _input.BasicInput.Enable();

        _input.BasicInput.MouseLeft.performed += OnLeftMousePerformed;
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
    
    public static void ProcessMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            OnMouseClick?.Invoke(hit);
        }
    }
}