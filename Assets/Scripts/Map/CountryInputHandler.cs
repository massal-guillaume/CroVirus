using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestion unifiée des inputs sur les pays — PC et Mobile.
/// 
/// PROBLÈME à résoudre : sur mobile, un doigt qui drag la map
/// ne doit PAS déclencher un clic sur un pays.
/// Ce script distingue un "tap" (doigt qui ne bouge pas) d'un "drag".
/// 
/// Attache ce script sur le même GameObject que MapCameraController.
/// </summary>
public class CountryInputHandler : MonoBehaviour
{
    [Header("Seuil tap vs drag (pixels)")]
    [Tooltip("Si le doigt/souris bouge plus que ça entre Down et Up → c'est un drag, pas un tap")]
    public float dragThreshold = 10f;

    private Camera _cam;
    private MapCameraController _mapCamera;

    // State pour distinguer tap / drag
    private Vector2 _inputDownPosition;
    private bool _inputDown = false;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _mapCamera = GetComponent<MapCameraController>();
    }

    void Update()
    {
        HandlePC();
        HandleMobile();
    }

    // ─── PC ───────────────────────────────────────────────────
    void HandlePC()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _inputDownPosition = Mouse.current.position.ReadValue();
            _inputDown = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && _inputDown)
        {
            _inputDown = false;
            float moved = Vector2.Distance(_inputDownPosition, Mouse.current.position.ReadValue());

            // C'est un vrai clic, pas un drag
            if (moved < dragThreshold)
                TrySelectCountry(Mouse.current.position.ReadValue());
        }
    }

    // ─── Mobile ───────────────────────────────────────────────
    void HandleMobile()
    {
        // On ignore si aucun écran tactile ou plusieurs doigts (pinch zoom en cours)
        if (Touchscreen.current == null || Touchscreen.current.touches.Count != 1) return;

        var touch = Touchscreen.current.touches[0];

        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            _inputDownPosition = touch.position.ReadValue();
            _inputDown = true;
        }
        else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended && _inputDown)
        {
            _inputDown = false;
            float moved = Vector2.Distance(_inputDownPosition, touch.position.ReadValue());

            if (moved < dragThreshold)
                TrySelectCountry(touch.position.ReadValue());
        }
        else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            _inputDown = false;
        }
    }

    // ─── Raycasting vers un pays ──────────────────────────────
    void TrySelectCountry(Vector2 screenPos)
    {
        // Convertit la position écran en rayon 2D
        Vector3 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);

        if (hit.collider != null)
        {
            Country country = hit.collider.GetComponent<Country>();
            if (country != null)
                WorldMap.Instance?.HandleCountryClick(country);
        }
        else
        {
            // Clic dans le vide → désélectionne
            WorldMap.Instance?.DeselectCurrent();
        }
    }
}
