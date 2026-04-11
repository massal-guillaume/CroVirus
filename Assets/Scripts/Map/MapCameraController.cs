using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contrôleur caméra pour la world map.
/// - Zoom : pinch (mobile) + scroll molette (PC)
/// - Pan  : drag uniquement si zoomé (zoom > zoomMin)
/// - Compatible PC et mobile (Input.touches + Mouse)
/// Attache ce script sur ta Camera principale (Orthographic).
/// </summary>
public class MapCameraController : MonoBehaviour
{
    [Header("Zoom")]
    public float zoomMin = 1.2f;      // Zoom max dézoomé (vue monde entier) - doit voir toute la map
    public float zoomMax = 0.1f;      // Zoom max zoomé (vue pays)
    public float zoomSpeed = 0.015f;  // Vitesse scroll molette PC (équilibre)
    public float pinchSpeed = 0.05f;  // Sensibilité pinch mobile
    public float zoomSmoothing = 8f;  // Lissage du zoom

    [Header("Pan")]
    public float panSpeed = 0.08f;    // Sensibilité du drag
    public float panSmoothing = 10f;  // Lissage du déplacement

    [Header("Limites de la map (en unités Unity)")]
    public float boundsX = 1.8f;      // Moitié largeur map (longitude max) — avec mapScale 0.01
    public float boundsY = 0.6f;      // Moitié hauteur map — permet movement en haut quand zoomé

    // ─── State interne ────────────────────────────────────────
    private Camera _cam;
    private float _targetZoom;
    private Vector3 _targetPosition;
    private Vector3 _dragOrigin;
    private bool _isDragging = false;
    private float _lastPinchDistance = 0f;

    // Seuil : en dessous de cette valeur d'orthographicSize, on autorise le pan
    private float _panThreshold => zoomMin * 0.98f;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (!_cam.orthographic)
        {
            Debug.LogWarning("[MapCamera] La caméra doit être en mode Orthographic.");
            _cam.orthographic = true;
        }

        _targetZoom = zoomMin;
        _targetPosition = new Vector3(0f, -0.3f, transform.position.z); 
        
        // Force apply the zoom immediately instead of lerping
        _cam.orthographicSize = zoomMin;
        _cam.nearClipPlane = 0.01f;
        _cam.farClipPlane = 1000f;
        Debug.Log($"[MapCameraController] Awake: orthographicSize set to {zoomMin}, nearPlane={_cam.nearClipPlane}, farPlane={_cam.farClipPlane}");
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
        ApplySmoothing();
    }

    // ─── Zoom ─────────────────────────────────────────────────
    void HandleZoom()
    {
        float delta = 0f;

        // PC : molette souris
        if (Mouse.current != null)
            delta -= Mouse.current.scroll.ReadValue().y * zoomSpeed;

        // Mobile : pinch
        if (Touchscreen.current != null && Touchscreen.current.touches.Count == 2)
        {
            var t0 = Touchscreen.current.touches[0];
            var t1 = Touchscreen.current.touches[1];
            float currentDist = Vector2.Distance(t0.position.ReadValue(), t1.position.ReadValue());

            if (t0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began || t1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _lastPinchDistance = currentDist;
                return;
            }

            float pinchDelta = _lastPinchDistance - currentDist;
            delta += pinchDelta * pinchSpeed;
            _lastPinchDistance = currentDist;
        }

        if (Mathf.Abs(delta) > 0.001f)
        {
            // CLAMP: max +/- 0.03 de changement par frame
            delta = Mathf.Clamp(delta, -0.03f, 0.03f);
            _targetZoom = Mathf.Clamp(_targetZoom + delta, zoomMax, zoomMin);

            // Si on revient au zoom max (dézoom complet) → reset position au centre
            if (_targetZoom >= zoomMin - 0.5f)
                _targetPosition = new Vector3(0f, 0f, transform.position.z);
        }
    }

    // ─── Pan ──────────────────────────────────────────────────
    void HandlePan()
    {
        // Pan désactivé si pas zoomé
        bool canPan = _cam.orthographicSize < _panThreshold;
        if (!canPan)
        {
            _isDragging = false;
            return;
        }

        // ── PC : clic gauche drag ──
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _dragOrigin = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                _isDragging = true;
            }

            if (Mouse.current.leftButton.isPressed && _isDragging)
            {
                Vector3 currentPos = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3 diff = _dragOrigin - currentPos;
                _targetPosition += diff;
                // On ne met pas à jour _dragOrigin ici pour un drag fluide
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                _isDragging = false;
        }

        // ── Mobile : 1 doigt drag ──
        if (Touchscreen.current != null && Touchscreen.current.touches.Count == 1)
        {
            var touch = Touchscreen.current.touches[0];

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _dragOrigin = _cam.ScreenToWorldPoint(touch.position.ReadValue());
                _isDragging = true;
            }
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && _isDragging)
            {
                Vector3 currentPos = _cam.ScreenToWorldPoint(touch.position.ReadValue());
                Vector3 diff = _dragOrigin - currentPos;
                _targetPosition += diff;
            }
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                _isDragging = false;
            }
        }

        ClampPosition();
    }

    // ─── Clamp dans les limites de la map ─────────────────────
    void ClampPosition()
    {
        // Plus on est zoomé, plus la zone de pan est restreinte
        float zoomRatio = _cam.orthographicSize / zoomMin;
        float maxX = boundsX * (1f - zoomRatio) * _cam.aspect;
        float maxY = boundsY * (1f - zoomRatio);

        _targetPosition.x = Mathf.Clamp(_targetPosition.x, -maxX, maxX);
        _targetPosition.y = Mathf.Clamp(_targetPosition.y, -maxY, maxY);
        _targetPosition.z = transform.position.z;
    }

    // ─── Lissage ──────────────────────────────────────────────
    void ApplySmoothing()
    {
        _cam.orthographicSize = Mathf.Lerp(
            _cam.orthographicSize, _targetZoom, Time.deltaTime * zoomSmoothing);

        transform.position = Vector3.Lerp(
            transform.position, _targetPosition, Time.deltaTime * panSmoothing);
    }

    // ─── API publique ─────────────────────────────────────────

    /// <summary>Zoom animé sur une position monde (ex: centrer sur un pays)</summary>
    public void FocusOn(Vector3 worldPosition, float targetSize = 15f)
    {
        _targetZoom = Mathf.Clamp(targetSize, zoomMax, zoomMin);
        _targetPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        ClampPosition();
    }

    /// <summary>Retourne à la vue monde complète</summary>
    public void ResetView()
    {
        _targetZoom = zoomMin;
        _targetPosition = new Vector3(0f, 0.1f, transform.position.z);
    }

    /// <summary>Est-ce que la map est zoomée (pan possible) ?</summary>
    public bool IsZoomed => _cam.orthographicSize < _panThreshold;
}
