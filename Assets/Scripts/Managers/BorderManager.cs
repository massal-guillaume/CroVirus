using System.Collections.Generic;
using UnityEngine;

public static class BorderManager
{
    private static List<CountryLink> borders = new List<CountryLink>();
    private static bool initialized = false;

    public static void Initialize()
    {
        if (initialized) return;

        borders.Clear();

        // Récupérer les pays depuis CountryManager
        CountryObject france = CountryManager.GetCountry("France");
        CountryObject germany = CountryManager.GetCountry("Germany");
        CountryObject italy = CountryManager.GetCountry("Italy");

        // Créer les liaisons entre les pays (BIDIRECTIONNELLES)
        // Les intensités représentent % de population qui voyagent via ce mode
        
        // France ↔ Germany (frontière terrestre, très accessible)
        AddBidirectionalBorder(france, germany, TransportType.Car, 0.01f);      // 1% en voiture
        AddBidirectionalBorder(france, germany, TransportType.Airplane, 0.001f); // 0.1% en avion
        
        // Germany ↔ Italy (frontière terrestre alpine)
        AddBidirectionalBorder(germany, italy, TransportType.Car, 0.005f);      // 0.5% en voiture
        AddBidirectionalBorder(germany, italy, TransportType.Airplane, 0.0005f); // 0.05% en avion
        
        // France ↔ Italy (frontière terrestre + voie maritime)
        AddBidirectionalBorder(france, italy, TransportType.Car, 0.003f);        // 0.3% en voiture
        AddBidirectionalBorder(france, italy, TransportType.Airplane, 0.0008f);  // 0.08% en avion
        AddBidirectionalBorder(france, italy, TransportType.Boat, 0.0002f);      // 0.02% en bateau

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

    private static void DebugPrintBorders()
    {
        Debug.Log("\n=== RÉSEAU DE FRONTIÈRES ===");
        foreach (CountryLink border in borders)
        {
            Debug.Log($"  {border}");
        }
        Debug.Log("============================\n");
    }
}
