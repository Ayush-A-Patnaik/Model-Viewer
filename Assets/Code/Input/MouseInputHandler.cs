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

    
    
}