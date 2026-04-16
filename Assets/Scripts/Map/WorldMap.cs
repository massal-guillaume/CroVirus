using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Manager central de la world map — version mobile+PC.
/// Singleton accessible via WorldMap.Instance.
/// </summary>
public class WorldMap : MonoBehaviour
{
    public static WorldMap Instance { get; private set; }

    [Header("Events globaux")]
    public UnityEvent<Country> onCountrySelected;
    public UnityEvent<Country> onCountryDeselected;

    private Dictionary<string, Country> _countries = new Dictionary<string, Country>(System.StringComparer.OrdinalIgnoreCase);
    private Country _selectedCountry;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─── Appelé par CountryInputHandler ──────────────────────
    public void HandleCountryClick(Country country)
    {
        if (_selectedCountry != null && _selectedCountry != country)
        {
            _selectedCountry.SetSelected(false);
            onCountryDeselected?.Invoke(_selectedCountry);
        }

        bool wasSelected = _selectedCountry == country;
        _selectedCountry = wasSelected ? null : country;
        country.SetSelected(!wasSelected);

        if (!wasSelected)
        {
            onCountrySelected?.Invoke(country);
            // Ouvrir le popup avec les infos du pays
            ShowCountryPopup(country);
        }
        else
        {
            onCountryDeselected?.Invoke(country);
        }
    }

    private void ShowCountryPopup(Country country)
    {
    }

    public void DeselectCurrent()
    {
        if (_selectedCountry == null) return;
        _selectedCountry.SetSelected(false);
        onCountryDeselected?.Invoke(_selectedCountry);
        _selectedCountry = null;
    }

    // ─── Registre ─────────────────────────────────────────────
    public void RegisterCountry(Country country)
    {
        if (!string.IsNullOrEmpty(country.countryName))
            _countries[country.countryName] = country;
        if (!string.IsNullOrEmpty(country.countryCode))
            _countries[country.countryCode] = country;
    }

    // ─── API publique ─────────────────────────────────────────
    public Country GetCountry(string nameOrCode)
    {
        _countries.TryGetValue(nameOrCode, out Country c);
        return c;
    }

    public void SetCountryColor(string nameOrCode, Color color) =>
        GetCountry(nameOrCode)?.SetColor(color);

    public void ResetAllColors()
    {
        var seen = new HashSet<Country>();
        foreach (var c in _countries.Values)
            if (seen.Add(c)) c.ResetColor();
    }

    public void ColorizeGroup(IEnumerable<string> countryNames, Color color)
    {
        foreach (var name in countryNames)
            SetCountryColor(name, color);
    }

    public IEnumerable<Country> GetAllCountries()
    {
        var seen = new HashSet<Country>();
        foreach (var c in _countries.Values)
            if (seen.Add(c)) yield return c;
    }

    public Country SelectedCountry => _selectedCountry;
}
