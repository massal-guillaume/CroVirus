using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class CountryInfoPopup : MonoBehaviour
{
    public static CountryInfoPopup Instance { get; private set; }

    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private TextMeshProUGUI countryNameText;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI infectedText;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private TextMeshProUGUI immunityText;

    private CountryObject currentCountry;
    private RectTransform panelRect;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        panelRect = GetComponent<RectTransform>();
        HidePopup();
    }

    void Update()
    {
        if (currentCountry != null && popupCanvasGroup.alpha > 0)
        {
            UpdateCountryStats();
            CheckClickOutside();
        }
    }

    private void CheckClickOutside()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, Mouse.current.position.ReadValue()))
            {
                HidePopup();
            }
        }
    }

    public void ShowCountryInfo(CountryObject country)
    {
        if (country == null)
            return;

        currentCountry = country;
        UpdateCountryStats();
        ShowPopup();
    }

    private void UpdateCountryStats()
    {
        if (currentCountry == null)
            return;

        countryNameText.text = currentCountry.name;
        populationText.text = $"Population: {currentCountry.population.total:N0}";
        infectedText.text = $"Infectes: {currentCountry.population.infected}";
        deathText.text = $"Morts: {currentCountry.population.dead}";
        // Formater l'immunité en pourcentage avec décimale
        float immunityPercent = currentCountry.immunity * 100f;
        immunityText.text = $"Immunite: {immunityPercent:F1}%";
    }

    public void ShowPopup()
    {
        popupCanvasGroup.alpha = 1f;
        popupCanvasGroup.interactable = true;
        popupCanvasGroup.blocksRaycasts = true;
    }

    public void HidePopup()
    {
        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.interactable = false;
        popupCanvasGroup.blocksRaycasts = false;
        currentCountry = null;
    }
}
