using UnityEngine;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour, IScrollHandler, IDragHandler
{
    [SerializeField] private RectTransform mapContent;  // Contenu de la map à zoomer
    private float currentZoom = 1f;
    private float minZoom = 0.5f;
    private float maxZoom = 3f;
    private float zoomSpeed = 0.1f;
    
    // Pan avec un doigt
    private Vector2 lastDragPosition;

    void Update()
    {
        // Pinch zoom sur mobile (2 doigts)
        HandlePinchZoom();
    }

    public void OnScroll(PointerEventData eventData)
    {
        // Pour souris/scrollwheel
        float scrollDelta = eventData.scrollDelta.y;
        Zoom(scrollDelta > 0 ? 1 : -1);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Pan: déplacer la map avec un doigt
        if (mapContent == null)
            return;

        Vector2 delta = eventData.delta;
        mapContent.anchoredPosition += delta;
    }

    private void HandlePinchZoom()
    {
        if (Input.touchCount != 2)
            return;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        float prevDistance = Vector2.Distance(touch0.position - touch0.deltaPosition,
                                              touch1.position - touch1.deltaPosition);
        float currentDistance = Vector2.Distance(touch0.position, touch1.position);

        float distanceDelta = currentDistance - prevDistance;

        if (distanceDelta != 0)
        {
            Zoom(distanceDelta > 0 ? 1 : -1);
        }
    }

    private void Zoom(float direction)
    {
        currentZoom += direction * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        mapContent.localScale = Vector3.one * currentZoom;
    }
}
