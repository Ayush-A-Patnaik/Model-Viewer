using System;
using System.Collections.Generic;
using AlligUtils;
using UnityEngine;

public class SelectionHandler : MonoBehaviour
{
    public static SelectionHandler Instance;
    [SerializeField] private GameObject _currentSelectedObject = null;

    // private LayerMask _originalLayer;
    // private LayerMask _selectionLayer;
    public bool IsMultiSelect = false;
    
    // private readonly HashSet<Renderer> _selectedRenderers = new HashSet<Renderer>();
    // public HashSet<Renderer> SelectedRenderers => _selectedRenderers;

    private HashSet<GameObject> _selectedObjects = new();

    public GameObject CurrentSelectedObject
    {
        get => _currentSelectedObject;
        set
        {
            _currentSelectedObject = value;
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
        // _originalLayer = LayerMask.NameToLayer("Pickable");
        // _selectionLayer = LayerMask.NameToLayer("SelectionMask");

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        MouseClickDispatcher.OnObjectClick += OnObjectSelect;
        MouseClickDispatcher.OnEmptyClicked += DeselectAll;
    }

    private void OnObjectSelect(GameObject go)
    {
        MeshSelection.AddObject(go, IsMultiSelect);
    }
    
    private void DeselectAll()
    {
        MeshSelection.ClearAllObjects();
    }

    [ContextMenu("Show Current selections")]
    public void ShowSelections()
    {
        _selectedObjects.Print("Selected Object in list: ", Palette.YellowSunflower);
    }
}

public static class MeshSelection
{
    private static HashSet<GameObject> _objects = new();

    public static void AddObject(GameObject go, bool isMutliSelect)
    {
        if (!isMutliSelect)
        {
            if (_objects.Contains(go) && _objects.Count == 1)
            {
                ClearAllObjects();
            }
            else
            {
                ClearAllObjects();
                AddWithOutline(go);   
            }
        }
        else
        {
            if (_objects.Contains(go))
                Remove(go);
            else
                AddWithOutline(go);
        }
    }
    
    public static void AddObjects(List<GameObject> objects, bool isMultiSelect)
    {
        if (!isMultiSelect)
            ClearAllObjects();

        foreach (var go in objects)
        {
            if (!_objects.Contains(go))
                AddWithOutline(go);
        }
    }

    public static void ClearAllObjects()
    {
        if (_objects.Count == 0) return;
        foreach (var go in _objects)
        {
            ManipulateOutline(go, false);
        }
        _objects.Clear();
    }
    
    private static void AddWithOutline(GameObject go)
    {
        _objects.Add(go);
        ManipulateOutline(go, true);
    }

    private static void Remove(GameObject go)
    {
        ManipulateOutline(go, false);
        _objects.Remove(go);
        
    }

    private static void ManipulateOutline(GameObject go, bool enable)
    {
        if (go.TryGetComponent<Outline>(out var outline))
        {
            outline.enabled = enable;
        }
    }
}