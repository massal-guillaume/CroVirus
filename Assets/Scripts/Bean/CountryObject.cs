using UnityEngine;

public class CountryObject
{
    public string name;
    public Population population;
    public float immunity;                 // Immunité génétique naturelle des individus (0-1)
    public float averageTemperature;       // Température moyenne en Celsius
    public float wealth;                   // Richesse du pays (0-1)
    public float hygiene;                  // Hygiène du pays (0-1)
    public float drugResistance;           // Qualité du système de santé / drogues disponibles (0-1)
    public string climateType;             // Type de climat
    public int borderClosedTurns = 0;      // Tours restants de fermeture des frontières
    public bool borderPermanentlyClosed = false; // Fermeture permanente des frontières

    public CountryObject(string countryName, int initialPopulation, float temperature = 15f, float wealth = 0.8f, float hygiene = 0.8f, float drugResistance = 0.7f, string climateType = "Temperate")
    {
        name = countryName;
        population = new Population(initialPopulation);
        immunity = 0f;
        averageTemperature = temperature;
        this.wealth = Mathf.Clamp01(wealth);
        this.hygiene = Mathf.Clamp01(hygiene);
        this.drugResistance = Mathf.Clamp01(drugResistance);
        this.climateType = climateType;
    }

    public override string ToString()
    {
        return $"{name} - {population} | Immunité: {immunity:P0} | Temp: {averageTemperature:F1}°C | Wealth: {wealth:P0} | Hygiene: {hygiene:P0}";
    }
}
