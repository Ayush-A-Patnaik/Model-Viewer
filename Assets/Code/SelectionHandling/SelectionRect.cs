using UnityEngine;
using UnityEngine.UI;

public class SelectionRect : MonoBehaviour
{
    [SerializeField] private RawImage _rectImage; 

    private Vector2 _startPos;
    private bool _active;

    private void OnEnable()
    {
        MouseClickDispatcher.DragStarted  += OnDragStarted;
        MouseClickDispatcher.DragUpdated  += OnDragUpdated;
        MouseClickDispatcher.DragCanceled += OnDragCanceled;
        _rectImage.enabled = false;
    }

    private void OnDisable()
    {
        MouseClickDispatcher.DragStarted  -= OnDragStarted;
        MouseClickDispatcher.DragUpdated  -= OnDragUpdated;
        MouseClickDispatcher.DragCanceled -= OnDragCanceled;
    }

    private void OnDragStarted(Vector2 startPos)
    {
        _startPos = startPos;
        _active = true;
        _rectImage.enabled = true;
    }

    private void OnDragUpdated(Vector2 start, Vector2 current)
    {
        float width  = current.x - start.x;
        float height = current.y - start.y;

        var rt = _rectImage.rectTransform;
        rt.anchoredPosition = new Vector2(
            Mathf.Min(start.x, current.x),
            Mathf.Min(start.y, current.y)
        );
        rt.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
    }

    private void OnDragCanceled()
    {
        _active = false;
        _rectImage.enabled = false;
    }
}