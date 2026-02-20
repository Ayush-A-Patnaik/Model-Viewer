using System;
using System.Collections.Generic;
using AlligUtils;
using UnityEngine;

public class SelectionHandler : MonoBehaviour
{
    public static SelectionHandler Instance;
    [SerializeField] private GameObject _currentSelectedObject = null;
    
    public bool IsMultiSelect = false;

    private Vector2 _dragStart, _dragEnd;
    
    // private readonly HashSet<Renderer> _selectedRenderers = new HashSet<Renderer>();
    // public HashSet<Renderer> SelectedRenderers => _selectedRenderers;

    //private HashSet<GameObject> _selectedObjects = new();

    public GameObject CurrentSelectedObject
    {
        get => _currentSelectedObject;
        set
        {
            _currentSelectedObject = value;
            //need to set some outline callback + desection logic
        }
    }
    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        
    }

    private void OnEnable()
    {
        MouseClickDispatcher.OnObjectClick += OnObjectSelect;
        MouseClickDispatcher.OnEmptyClicked += DeselectAll;
        MouseClickDispatcher.DragUpdated  += OnDragUpdated;
    }

    private void OnDisable()
    {
        MouseClickDispatcher.OnObjectClick -= OnObjectSelect;
        MouseClickDispatcher.OnEmptyClicked -= DeselectAll;
        MouseClickDispatcher.DragUpdated  -= OnDragUpdated;
    }

    private void OnObjectSelect(GameObject go)
    {
        MeshSelection.AddObject(go, IsMultiSelect);
    }
    
    private void DeselectAll()
    {
        if(IsMultiSelect) return;
        MeshSelection.ClearAllObjects();
    }

    private void OnDragUpdated(Vector2 start, Vector2 end)
    {
        _dragStart = start;
        _dragEnd = end;
        SelectMeshesOnDrag(_dragStart, _dragEnd);
    }
    
    private void SelectMeshesOnDrag(Vector2 startScreen, Vector2 endScreen)
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
            if ((1 << LayerMask.NameToLayer("Pickable") & (1 << col.gameObject.layer)) == 0) continue;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(col.bounds.center);
            
            if (screenPos.z < 0) continue;

            if (screenRect.Contains(new Vector2(screenPos.x, screenPos.y)))
                hits.Add(col.gameObject);
        }

        MeshSelection.AddObjects(hits, IsMultiSelect);
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