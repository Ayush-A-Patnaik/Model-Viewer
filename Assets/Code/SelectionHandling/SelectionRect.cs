using UnityEngine;
using UnityEngine.UI;

public class SelectionRect : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _selectionRoot;
    [SerializeField] private RawImage _rectImage; 

    //private Vector2 _startPos;
    private bool _active;

    private void OnEnable()
    {
        MouseClickDispatcher.DragStarted  += OnDragStarted;
        MouseClickDispatcher.DragUpdated  += OnDragUpdated;
        MouseClickDispatcher.DragCanceled += OnDragCanceled;
        
        _rectImage.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        MouseClickDispatcher.DragStarted  -= OnDragStarted;
        MouseClickDispatcher.DragUpdated  -= OnDragUpdated;
        MouseClickDispatcher.DragCanceled -= OnDragCanceled;
    }

    private void OnDragStarted()
    {
        _active = true;
        _rectImage.gameObject.SetActive(true);
    }

    private void OnDragUpdated(Vector2 start, Vector2 end)
    {
        // float width  = current.x - start.x;
        // float height = current.y - start.y;
        //
        // var rt = _rectImage.rectTransform;
        // rt.anchoredPosition = new Vector2(
        //     Mathf.Min(start.x, current.x),
        //     Mathf.Min(start.y, current.y)
        // );
        // rt.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        
        Camera eventCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay? null : _canvas.worldCamera;
   

        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            _selectionRoot,
            start,
            null,
            out var localStart
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            _selectionRoot,
            end,
            null,
            out var localEnd
        );
        
        Vector2 min = Vector2.Min(localStart, localEnd);
        Vector2 max = Vector2.Max(localStart, localEnd);

        var rt = _rectImage.rectTransform;
        rt.anchoredPosition = min;
        rt.sizeDelta = max - min;
    }

    private void OnDragCanceled()
    {
        _active = false;
        _rectImage.gameObject.SetActive(false);
    }
}