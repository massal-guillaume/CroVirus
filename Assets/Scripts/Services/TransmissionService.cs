using System.Collections.Generic;
using UnityEngine;

public class TransmissionService
{
    // Paramètres de transmission - Approche réaliste par personne
    private const int DAYS_TO_RECOVER = 10;  // Infectés restent infectés 10 jours
    private const float CONTACTS_PER_INFECTED_PER_DAY = 1.5f;  // Chaque infecté contacte 1.5 personnes
    private const float BASE_TRANSMISSION_PROBABILITY = 0.10f;  // 10% chance par contact
    private const float VARIABILITY = 0.3f; // ±30% variabilité
    
    // Accumulateurs de décimales (évite de perdre les petits chiffres)
    private static Dictionary<string, float> infectionDecimals = new Dictionary<string, float>();
    private static Dictionary<string, float> mortalityDecimals = new Dictionary<string, float>();
    
    public static void SimulateSpread(List<CountryObject> countries, Virus virus)
    {
        foreach (CountryObject country in countries)
        {
            Population pop = country.population;
            int infected = pop.infected;
            
            // Calculer l'effet température sur l'efficacité du virus
            float temperatureEffect = CalculateTemperatureEffect(country.averageTemperature, virus);
            
            // Accumulateur de décimales pour chaque pays
            if (!infectionDecimals.ContainsKey(country.name))
                infectionDecimals[country.name] = 0f;
            
            // Approche réaliste par personne:
            // Chaque infecté contacte CONTACTS_PER_INFECTED_PER_DAY personnes
            // Chaque contact a BASE_TRANSMISSION_PROBABILITY de transmettre
            // Affecté par température et variabilité
            float variability = Random.Range(1f - VARIABILITY, 1f + VARIABILITY);
            float newInfectionValue = infected * CONTACTS_PER_INFECTED_PER_DAY * BASE_TRANSMISSION_PROBABILITY * temperatureEffect * variability;
            infectionDecimals[country.name] += newInfectionValue;
            
            // Prendre les entiers
            int newInfected = (int)infectionDecimals[country.name];
            infectionDecimals[country.name] -= newInfected;  // Garder les décimales
            
            // Cap au nombre de personnes saines disponibles
            int healthy = pop.GetHealthy();
            newInfected = Mathf.Min(newInfected, healthy);
            
            pop.infected += newInfected;
            
            if (newInfected > 0)
                Debug.Log($"  [{country.name}] +{newInfected} infectés (contacts×prob×temp = {CONTACTS_PER_INFECTED_PER_DAY}×{BASE_TRANSMISSION_PROBABILITY:F2}×{temperatureEffect:F2}×{variability:F2}, accumulated: {infectionDecimals[country.name]:F2})");
        }
    }

    // Calculer l'efficacité du virus basé sur la température
    private static float CalculateTemperatureEffect(float temperature, Virus virus)
    {
        // Approche simple: bonus/malus de max 5% selon la température et les résistances du virus
        // Température référence: 15°C (neutre)
        // Froid < 15°C, Chaud > 15°C
        
        float coldResistance = virus.coldResistance;      // 0-1
        float heatResistance = virus.heatResistance;      // 0-1
        
        float tempDifference = temperature - 15f;  // Écart à la température neutre
        
        if (coldResistance > heatResistance)
        {
            // Virus préfère le froid
            // Chaque degré en-dessous de 15°C → bonus progressif
            // Chaque degré au-dessus de 15°C → malus progressif
            float effect = 1f + (Mathf.Clamp(tempDifference, -10f, 10f) / 200f) * coldResistance;  // Max ±5%
            return Mathf.Clamp01(effect);
        }
        else
        {
            // Virus préfère le chaud
            // Chaque degré au-dessus de 15°C → bonus progressif
            // Chaque degré en-dessous de 15°C → malus progressif
            float effect = 1f - (Mathf.Clamp(tempDifference, -10f, 10f) / 200f) * heatResistance;  // Max ±5%
            return Mathf.Clamp01(effect);
        }
    }

    public static void SimulateMortality(List<CountryObject> countries, Virus virus)
    {
        foreach (CountryObject country in countries)
        {
            Population pop = country.population;
            
            // Accumulateur de décimales pour chaque pays
            if (!mortalityDecimals.ContainsKey(country.name))
                mortalityDecimals[country.name] = 0f;
            
            // Mortalité étalée sur DAYS_TO_RECOVER pour plus de réalisme
            float mortalityValue = (pop.infected * virus.lethality) / DAYS_TO_RECOVER;
            
            // BOOST à la toute fin: si 98%+ de la population est affectée (infectée + morte)
            int affectedPopulation = pop.infected + pop.dead;
            float affectionRatePerCountry = pop.total > 0 ? (float)affectedPopulation / pop.total : 0f;
            
            if (affectionRatePerCountry >= 0.98f)
            {
                mortalityValue *= 50f;  // 50x plus rapide à la toute fin
            }
            
            mortalityDecimals[country.name] += mortalityValue;
            
            // Prendre les entiers
            int newDeaths = (int)mortalityDecimals[country.name];
            mortalityDecimals[country.name] -= newDeaths;  // Garder les décimales
            
            pop.dead += newDeaths;
            pop.infected = Mathf.Max(0, pop.infected - newDeaths);
            
            if (newDeaths > 0)
                Debug.Log($"  [{country.name}] -{newDeaths} morts (accumulated: {mortalityDecimals[country.name]:F2})");
        }
    }

    public static int GetTotalInfected(List<CountryObject> countries)
    {
        int total = 0;
        foreach (CountryObject country in countries)
        {
            total += country.population.infected;
        }
        return total;
    }

    public static int GetTotalDead(List<CountryObject> countries)
    {
        int total = 0;
        foreach (CountryObject country in countries)
        {
            total += country.population.dead;
        }
        return total;
    }

    // ─── Transmission inter-pays ────────────────────────────────

    public static void SimulateInterCountrySpread(List<CountryObject> countries)
    {
        // Les voyageurs voyagent en permanence (sains ET infectés)
        // Chaque personne a un % de chance de voyager (rare au début)
        foreach (CountryObject sourceCountry in countries)
        {
            Population sourcePop = sourceCountry.population;
            
            // Récupérer toutes les frontières sortantes
            List<CountryLink> outgoingBorders = BorderManager.GetBordersFrom(sourceCountry);
            
            foreach (CountryLink border in outgoingBorders)
            {
                CountryObject destCountry = border.destinationCountry;
                Population destPop = destCountry.population;
                
                // Taux de voyage = % de la population QUI VOYAGE (ex: 0.05 = 5%)
                float travelRate = border.intensity;
                
                // 5% de la population totale voyage
                int totalTravelers = (int)(sourcePop.total * travelRate);
                
                // Parmi les voyageurs, le ratio infecté/sain est RÉDUIT car les malades voyagent beaucoup moins
                float infectionRatio = sourcePop.total > 0 ? (float)sourcePop.infected / sourcePop.total : 0f;
                float sicknessTravelReduction = 0.1f;  // Les malades voyagent 10x moins que la proportion (90% moins)
                
                // Aléatoire sur le nombre d'infectés voyageurs
                float variability = Random.Range(1f - VARIABILITY, 1f + VARIABILITY);
                int infectedTravelers = (int)(totalTravelers * infectionRatio * sicknessTravelReduction * variability);
                int healthyTravelers = totalTravelers - infectedTravelers;
                
                // Cap aux personnes disponibles
                infectedTravelers = Mathf.Min(Mathf.Max(0, infectedTravelers), sourcePop.infected);
                int healthy = sourcePop.GetHealthy();
                healthyTravelers = Mathf.Min(Mathf.Max(0, healthyTravelers), healthy);
                
                totalTravelers = infectedTravelers + healthyTravelers;
                
                if (totalTravelers > 0)
                {
                    // Retirer du pays source
                    sourcePop.infected -= infectedTravelers;
                    sourcePop.total -= totalTravelers;
                    
                    // Ajouter au pays destination
                    destPop.infected += infectedTravelers;
                    destPop.total += totalTravelers;
                    
                    Debug.Log($"  [{sourceCountry.name}] → [{destCountry.name}] via {border.transportType}: {infectedTravelers} infectés + {healthyTravelers} sains = {totalTravelers} voyageurs");
                }
            }
        }
    }
}
