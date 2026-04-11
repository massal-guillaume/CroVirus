using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Composant attaché à chaque GameObject pays.
/// C'est ici que tu branches toute ta logique custom.
/// </summary>
public class Country : MonoBehaviour
{
    [Header("Identité")]
    public string countryName;
    public string countryCode; // ISO 3166-1 alpha-3 (ex: "FRA", "USA")

    [Header("Visuel")]
    public Color defaultColor = new Color(0.486f, 0.988f, 0f, 1f);     // RGB(124, 252, 0, 255)
    public Color highlightColor = new Color(0.6f, 1f, 0.2f);   // Brighter version
    public Color selectedColor = new Color(0.1f, 0.5f, 0.9f);    // Blue

    [Header("Material")]
    public Material countryMaterial;  // Référence au CountryMaterial

    [Header("Events — branche ton code ici")]
    public UnityEvent<Country> onCountryClicked;
    public UnityEvent<Country> onCountryHovered;
    public UnityEvent<Country> onCountryUnhovered;

    // ─── State ───────────────────────────────────────────────
    private MeshRenderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private Color _currentColor;
    private bool _isSelected = false;

    // Données custom : stocke ce que tu veux (score, statut, ressources...)
    private Dictionary<string, object> _customData = new Dictionary<string, object>();

    // ─── Init ────────────────────────────────────────────────
    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        LoadDefaultColorFromMaterial();
    }

    private void LoadDefaultColorFromMaterial()
    {
        // Charger le CountryMaterial s'il n'est pas assigné
        if (countryMaterial == null)
        {
            countryMaterial = Resources.Load<Material>("CountryMaterial");
        }

        // Récupérer la couleur _Color du material
        if (countryMaterial != null && countryMaterial.HasProperty("_Color"))
        {
            defaultColor = countryMaterial.GetColor("_Color");
        }
    }

    void Update()
    {
        UpdateInfectionColor();
    }

    // ─── Interaction souris ──────────────────────────────────
    void OnMouseEnter()
    {
        if (!_isSelected)
            SetColor(highlightColor);
        onCountryHovered?.Invoke(this);
    }

    void OnMouseExit()
    {
        if (!_isSelected)
        {
            // Recalculate infection color to override the highlight
            UpdateInfectionColor();
        }
        onCountryUnhovered?.Invoke(this);
    }

    void OnMouseDown()
    {
        WorldMap.Instance?.HandleCountryClick(this);
        onCountryClicked?.Invoke(this);
    }

    // ─── API publique ────────────────────────────────────────

    /// <summary>Change la couleur du pays (ex: selon un événement)</summary>
    public void SetColor(Color color)
    {
        _currentColor = color;
        
        if (_renderer == null) return;

        Material mat = _renderer.material;
        if (mat != null)
        {
            mat.SetColor("_Color", color);
        }
    }

    /// <summary>Reset à la couleur par défaut</summary>
    public void ResetColor() => SetColor(defaultColor);

    /// <summary>Marque le pays comme sélectionné</summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        SetColor(selected ? selectedColor : defaultColor);
    }

    /// <summary>Stocke une donnée custom sur ce pays</summary>
    public void SetData(string key, object value) => _customData[key] = value;

    /// <summary>Récupère une donnée custom</summary>
    public T GetData<T>(string key)
    {
        if (_customData.TryGetValue(key, out object val) && val is T typed)
            return typed;
        return default;
    }

    public bool HasData(string key) => _customData.ContainsKey(key);

    /// <summary>Met à jour la couleur du pays selon le taux d'infection et de mortalité</summary>
    void UpdateInfectionColor()
    {
        // Lazy-load le renderer si nécessaire
        if (_renderer == null)
        {
            _renderer = GetComponent<MeshRenderer>();
            if (_renderer == null) return;
        }

        // Récupère les données du pays depuis le CountryManager
        CountryObject countryData = CountryManager.GetCountry(countryName);
        if (countryData == null) return;

        // Calcule le taux total d'affection (infectés + morts)
        int totalAffected = countryData.population.infected + countryData.population.dead;
        float affectionRate = countryData.population.total > 0 
            ? (float)totalAffected / countryData.population.total 
            : 0f;
        affectionRate = Mathf.Clamp01(affectionRate);
        
        // Toujours rouge pour l'affection (infectés ou morts)
        Color targetColor = Color.red;
        
        // Interpolation: vert (0%) → rouge (100%)
        Color infectionColor = Color.Lerp(defaultColor, targetColor, affectionRate);
        
        // Applique la couleur d'infection
        SetColor(infectionColor);
    }
}
