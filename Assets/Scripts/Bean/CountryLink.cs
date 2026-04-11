using System;

public enum TransportType { Airplane, Car, Boat }

public class CountryLink
{
    public CountryObject sourceCountry;
    public CountryObject destinationCountry;
    public TransportType transportType;
    public float intensity;  // Intensité du lien (0-1), détermine le % d'infectés propagés

    public CountryLink(CountryObject source, CountryObject destination, TransportType type, float intensity = 0.5f)
    {
        sourceCountry = source;
        destinationCountry = destination;
        transportType = type;
        this.intensity = UnityEngine.Mathf.Clamp01(intensity);
    }

    public override string ToString()
    {
        return $"{sourceCountry.name} → {destinationCountry.name} [{transportType}] (intensité: {intensity:P0})";
    }
}
