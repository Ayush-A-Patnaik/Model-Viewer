using UnityEngine;
using System;
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
