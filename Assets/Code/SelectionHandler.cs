using System;
using System.Collections.Generic;
using AlligUtils;
using UnityEngine;

public class SelectionHandler : MonoBehaviour
{
    public static SelectionHandler Instance;
    [SerializeField] private GameObject _selectedObject = null;

    private LayerMask _originalLayer;
    private LayerMask _selectionLayer;
    public bool ShouldMultiSelect = false;

    private HashSet<GameObject> _selectedObjects = new();

    public GameObject SelectedObject
    {
        get => _selectedObject;
        set
        {
            if (value == null)
            {
                _selectedObject.layer = _originalLayer;
                _selectedObject = null;
            }
            else
            {
                _selectedObject = value;
                _selectedObject.layer = _selectionLayer;
            }
            //need to set some outline callback + desection logic
        }
    }

    public HashSet<GameObject> SelectedObjects
    {
        get => _selectedObjects;
        set { _selectedObjects = value; }
    }

    private void Awake()
    {
        _originalLayer = LayerMask.NameToLayer("Pickable");
        _selectionLayer = LayerMask.NameToLayer("SelectionMask");

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        MouseClickDispatcher.OnMouseClick += OnObjectSelect;
    }

    private void OnObjectSelect(RaycastHit hit)
    {
        GameObject go = hit.collider.gameObject;

        if (go == SelectedObject)
        {
            SelectedObject = null;
            return;
        }

        SelectedObject = go;
        SelectedObject.name.Print("Selected Object: ");
    }
}