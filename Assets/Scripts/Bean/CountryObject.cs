using UnityEngine;

public class CountryObject
{
    public string name;
    public Population population;
    public float immunity;      // Taux d'immunité collective (0-1)
    public float averageTemperature;  // Température moyenne en Celsius

    public CountryObject(string countryName, int initialPopulation, float temperature = 15f)
    {
        name = countryName;
        population = new Population(initialPopulation);
        immunity = 0f;
        averageTemperature = temperature;
    }

    public override string ToString()
    {
        return $"{name} - {population} | Immunité: {immunity:P0} | Temp: {averageTemperature:F1}°C";
    }
}
