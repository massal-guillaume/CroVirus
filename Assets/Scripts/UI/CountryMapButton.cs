using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountryMapButton : MonoBehaviour
{
    [SerializeField] private string countryName = "France";
    [SerializeField] private Image countryImage;  // Cercle/UIElement à colorer
    [SerializeField] private TextMeshProUGUI countryLabel;
    private Button button;
    private CountryObject country;
    
    private Color infectionColor = Color.red;
    private Color healthyColor = Color.green;
    
    void Start()
    {
        // Charger le pays depuis CountryManager
        country = CountryManager.GetCountry(countryName);
        
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnCountryClicked);
        
        if (countryLabel != null && country != null)
            countryLabel.text = country.name;
    }

    void Update()
    {
        if (country == null) return;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (countryImage == null || country == null)
            return;

        // Calculer % d'infectés
        float infectionRate = country.population.total > 0 
            ? (float)country.population.infected / country.population.total 
            : 0f;

        // Lerp entre vert (sain) et rouge (infecté)
        Color displayColor = Color.Lerp(healthyColor, infectionColor, infectionRate);
        countryImage.color = displayColor;
    }

    private void OnCountryClicked()
    {
        if (country == null)
        {
            Debug.LogWarning($"Country '{countryName}' not found!");
            return;
        }
        if (CountryInfoPopup.Instance == null)
        {
            Debug.LogWarning("CountryInfoPopup not found in scene!");
            return;
        }
        Debug.Log($"Clicked: {country.name}");
        CountryInfoPopup.Instance.ShowCountryInfo(country);
    }
}
