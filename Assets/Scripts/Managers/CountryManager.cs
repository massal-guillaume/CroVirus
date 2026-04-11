using System.Collections.Generic;
using UnityEngine;

public static class CountryManager
{
    private static List<CountryObject> countries = new List<CountryObject>();
    private static bool initialized = false;

    public static void Initialize()
    {
        if (initialized) return;

        // Créer les 3 pays de départ avec températures moyennes (Celsius)
        countries.Clear();
        countries.Add(new CountryObject("France", 67000000, 13f));       // ~13°C en moyenne
        countries.Add(new CountryObject("Germany", 83000000, 9f));       // ~9°C en moyenne
        countries.Add(new CountryObject("Italy", 58000000, 15f));        // ~15°C en moyenne

        initialized = true;
        Debug.Log($"CountryManager initialisé avec {countries.Count} pays");
    }

    public static CountryObject GetCountry(string name)
    {
        // Auto-init si pas encore fait
        if (!initialized)
            Initialize();
        
        foreach (CountryObject country in countries)
        {
            if (country.name == name)
                return country;
        }
        Debug.LogWarning($"Pays '{name}' non trouvé dans CountryManager");
        return null;
    }

    public static List<CountryObject> GetAllCountries()
    {
        return new List<CountryObject>(countries);  // Retourne une copie pour éviter modifications
    }

    public static void AddCountry(string name, int population, float temperature = 15f)
    {
        if (GetCountry(name) != null)
        {
            Debug.LogWarning($"Le pays '{name}' existe déjà");
            return;
        }
        countries.Add(new CountryObject(name, population, temperature));
        Debug.Log($"Pays '{name}' ajouté au CountryManager");
    }
}
