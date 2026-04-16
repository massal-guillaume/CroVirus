using System;
using System.Collections.Generic;
using UnityEngine;

public static class BorderManager
{
    [Serializable]
    private class BorderEntry
    {
        public string from;
        public string to;
        public string type;
        public float strength;
    }

    [Serializable]
    private class BorderList
    {
        public List<BorderEntry> borders;
    }

    private static List<CountryLink> borders = new List<CountryLink>();
    private static bool initialized = false;

    public static void Initialize()
    {
        // Si déjà initialisé ET des frontières sont chargées, ne rien faire
        if (initialized && borders.Count > 0) return;

        borders.Clear();
        initialized = false;

        TextAsset jsonFile = Resources.Load<TextAsset>("border");
        if (jsonFile != null)
        {
            BorderList data = JsonUtility.FromJson<BorderList>(jsonFile.text);
            if (data != null && data.borders != null)
            {
                foreach (BorderEntry entry in data.borders)
                {
                    CountryObject source = CountryManager.GetCountry(entry.from);
                    CountryObject destination = CountryManager.GetCountry(entry.to);

                    if (source == null)
                    {
                        Debug.LogWarning($"BorderManager: pays introuvable '{entry.from}'");
                        continue;
                    }
                    if (destination == null)
                    {
                        Debug.LogWarning($"BorderManager: pays introuvable '{entry.to}'");
                        continue;
                    }

                    if (!Enum.TryParse(entry.type, true, out TransportType transportType))
                    {
                        Debug.LogWarning($"BorderManager: type de transport inconnu '{entry.type}', utilisation de Car par défaut");
                        transportType = TransportType.Car;
                    }

                    AddBidirectionalBorder(source, destination, transportType, entry.strength);
                }
            }
        }
        else
        {
            Debug.LogError("BorderManager: fichier 'border.json' introuvable dans Resources/");
        }

        initialized = true;
        Debug.Log($"BorderManager initialisé avec {borders.Count} liaisons bidirectionnelles");
        DebugPrintBorders();
    }

    public static void AddBidirectionalBorder(CountryObject country1, CountryObject country2, TransportType type, float intensity)
    {
        // Ajouter le lien dans les deux sens
        AddBorder(country1, country2, type, intensity);
        AddBorder(country2, country1, type, intensity);
    }

    public static void AddBorder(CountryObject source, CountryObject destination, TransportType type, float intensity)
    {
        if (source == null || destination == null)
        {
            Debug.LogWarning("Impossible d'ajouter une frontière: pays null");
            return;
        }

        CountryLink link = new CountryLink(source, destination, type, intensity);
        borders.Add(link);
    }

    public static List<CountryLink> GetAllBorders()
    {
        return new List<CountryLink>(borders);
    }

    public static List<CountryLink> GetBordersFrom(CountryObject source)
    {
        List<CountryLink> result = new List<CountryLink>();
        foreach (CountryLink border in borders)
        {
            if (border.sourceCountry == source)
                result.Add(border);
        }
        return result;
    }

    public static List<CountryLink> GetBordersTo(CountryObject destination)
    {
        List<CountryLink> result = new List<CountryLink>();
        foreach (CountryLink border in borders)
        {
            if (border.destinationCountry == destination)
                result.Add(border);
        }
        return result;
    }

    public static List<CountryLink> GetBordersBetween(CountryObject country1, CountryObject country2)
    {
        List<CountryLink> result = new List<CountryLink>();
        foreach (CountryLink border in borders)
        {
            bool match = (border.sourceCountry == country1 && border.destinationCountry == country2) ||
                         (border.sourceCountry == country2 && border.destinationCountry == country1);
            if (match)
                result.Add(border);
        }
        return result;
    }

    public static void DebugPrintBorders()
    {
        Debug.Log("\n=== RÉSEAU DE FRONTIÈRES ===");
        foreach (CountryLink border in borders)
        {
            Debug.Log($"  {border}");
        }
        Debug.Log($"Total: {borders.Count} liaisons\n============================\n");
    }
}
