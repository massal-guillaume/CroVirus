using System.Collections.Generic;
using UnityEngine;

public class TransmissionService
{
    // Paramètres de transmission - Approche réaliste par personne
    private const int DAYS_TO_RECOVER = 7;  // Infectés restent infectés 7 jours
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
            
            // Calculer les modificateurs du virus selon les skills
            float transmissionModifier = CalculateTransmissionModifier(virus, country);
            
            // Accumulateur de décimales pour chaque pays
            if (!infectionDecimals.ContainsKey(country.name))
                infectionDecimals[country.name] = 0f;
            
            // Approche réaliste par personne:
            // Chaque infecté contacte CONTACTS_PER_INFECTED_PER_DAY personnes
            // Chaque contact a BASE_TRANSMISSION_PROBABILITY de transmettre
            // Affecté par température, skills, et variabilité
            float variability = Random.Range(1f - VARIABILITY, 1f + VARIABILITY);
            float newInfectionValue = infected * CONTACTS_PER_INFECTED_PER_DAY * BASE_TRANSMISSION_PROBABILITY * temperatureEffect * transmissionModifier * variability;
            infectionDecimals[country.name] += newInfectionValue;
            
            // Prendre les entiers
            int newInfected = (int)infectionDecimals[country.name];
            infectionDecimals[country.name] -= newInfected;  // Garder les décimales
            
            // Cap au nombre de personnes saines disponibles
            int healthy = pop.GetHealthy();
            newInfected = Mathf.Min(newInfected, healthy);

            pop.infected += newInfected;
            
            if (newInfected > 0)
                Debug.Log($"  [{country.name}] +{newInfected} infectés (temp×skills = {temperatureEffect:F2}×{transmissionModifier:F2}, accumulated: {infectionDecimals[country.name]:F2})");
        }
    }
    
    /// <summary>
    /// Calculate transmission modifier based on virus skills and country parameters
    /// </summary>
    private static float CalculateTransmissionModifier(Virus virus, CountryObject country)
    {
        float modifier = 1f;
        
        // Transport modifiers (avg of available transports)
        float transportBonus = (virus.airborneModifier + virus.seaModifier + virus.landModifier) / 3f;
        modifier += transportBonus;
        
        // Vector modifiers (disease vectors boost transmission)
        float vectorBonus = (virus.dogModifier + virus.petModifier + virus.flyModifier + virus.urineModifier) / 4f;
        modifier += vectorBonus;
        
        // Hygiene exploitation: countries with poor hygiene are more vulnerable
        // (1 - hygiene) means low-hygiene countries get full bonus
        float hygieneVulnerability = (1f - country.hygiene) * virus.hygieneExploitation;
        modifier += hygieneVulnerability;
        
        // Wealth exploitation: paradoxically, wealthy countries can be vulnerable too
        // (applies if wealth is high and virus has wealth exploitation)
        float wealthVulnerability = country.wealth * virus.wealthExploitation;
        modifier += wealthVulnerability;
        
        return Mathf.Max(modifier, 0.1f);  // Never go below 10% (floor to prevent division by zero issues)
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
        int worldTotal = 0;
        int worldInfected = 0;
        int worldDead = 0;

        for (int i = 0; i < countries.Count; i++)
        {
            Population p = countries[i].population;
            worldTotal += p.total;
            worldInfected += p.infected;
            worldDead += p.dead;
        }

        int worldHealthy = Mathf.Max(0, worldTotal - worldInfected - worldDead);
        int worldLiving = Mathf.Max(0, worldTotal - worldDead);
        bool allWorldLivingInfected = worldLiving > 0 && worldHealthy == 0;

        foreach (CountryObject country in countries)
        {
            Population pop = country.population;
            
            // Accumulateur de décimales pour chaque pays
            if (!mortalityDecimals.ContainsKey(country.name))
                mortalityDecimals[country.name] = 0f;
            
            // Virus drug resistance: virus fights antiviral treatments (increases lethality)
            // Country drug resistance: country healthcare system fights virus (decreases lethality)
            float drugFactor = (1f + virus.drugResistance) / (1f + country.drugResistance);
            
            // Virus immunity bypass: chance to ignore natural human genetic immunity
            // If bypass succeeds, immunity provides no protection
            float immunityFactor = 1f;
            if (Random.value >= virus.immunityBypass)
            {
                // Immunity bypass failed - country immunity protects population
                immunityFactor = 1f - country.immunity;  // Low immunity = higher lethality
            }
            // else: bypass succeeded - immunity has no effect (immunityFactor stays 1.0)

            // Mortalite skill conditions (low-intensity profile):
            // - Anus (mortalityRate): stronger in cold countries
            // - Poumon (respiratoryDamage): stronger in hot or arid countries
            // - Bouffe (foodContamination): stronger with low hygiene, boosted when hot
            // - Cerveau (mentalDamage): only active in rich countries
            bool isColdCountry = country.averageTemperature <= 8f;
            bool isHotCountry = country.averageTemperature >= 28f;
            bool isLowHygiene = country.hygiene <= 0.45f;
            bool isRichCountry = country.wealth >= 0.70f;

            string climate = string.IsNullOrEmpty(country.climateType) ? "" : country.climateType.ToLowerInvariant();
            bool isAridClimate = climate.Contains("arid") || climate.Contains("desert") || climate.Contains("dry");

            float anusBonus = virus.mortalityRate * (isColdCountry ? 0.08f : 0.03f);
            anusBonus = Mathf.Clamp(anusBonus, 0f, 0.10f);

            float poumonBonus = 0f;
            if (isHotCountry || isAridClimate)
                poumonBonus = virus.respiratoryDamage * 0.05f;
            else
                poumonBonus = virus.respiratoryDamage * 0.01f;
            poumonBonus = Mathf.Clamp(poumonBonus, 0f, 0.06f);

            float bouffeBonus = 0f;
            if (isLowHygiene)
            {
                float warmBoost = isHotCountry ? 1.30f : 1f;
                bouffeBonus = virus.foodContamination * 0.05f * warmBoost;
            }
            else
            {
                bouffeBonus = virus.foodContamination * 0.01f;
            }
            bouffeBonus = Mathf.Clamp(bouffeBonus, 0f, 0.06f);

            float cerveauBonus = isRichCountry ? virus.mentalDamage * 0.06f : 0f;
            cerveauBonus = Mathf.Clamp(cerveauBonus, 0f, 0.08f);

            float skillBonus = anusBonus + poumonBonus + bouffeBonus + cerveauBonus;
            skillBonus = Mathf.Clamp(skillBonus, 0f, 0.18f);
            
            // Final lethality = base × drug combat × immunity combat
            float baseLethality = virus.lethality * drugFactor * immunityFactor;
            float effectiveLethality = Mathf.Clamp01(baseLethality + skillBonus);
            
            // Mortalité étalée sur DAYS_TO_RECOVER pour plus de réalisme
            float mortalityValue = (pop.infected * effectiveLethality) / DAYS_TO_RECOVER;

            // Effondrement global: quand toute la population vivante mondiale est infectee,
            // le systeme sature et les morts/tour accelerent meme sans augmenter la letalite.
            if (allWorldLivingInfected)
            {
                float deadRateWorld = worldTotal > 0 ? (float)worldDead / worldTotal : 0f;
                float globalCollapseMultiplier = Mathf.Lerp(1.6f, 3.5f, deadRateWorld);
                mortalityValue *= globalCollapseMultiplier;
            }
            
            // BOOST de fin: actif seulement quand 98%+ de la population est morte
            // ET qu'il ne reste aucun sain (donc 100% de la population est affectee).
            float deadRatePerCountry = pop.total > 0 ? (float)pop.dead / pop.total : 0f;
            bool allPopulationAffected = pop.GetHealthy() == 0 && pop.infected > 0;

            if (deadRatePerCountry >= 0.98f && allPopulationAffected)
            {
                // Acceleration progressive entre 98% et 100% de morts.
                float t = Mathf.InverseLerp(0.98f, 1.0f, deadRatePerCountry);
                float endgameMultiplier = Mathf.Lerp(120f, 500f, t);
                mortalityValue *= endgameMultiplier;

                // Garantit au moins 1 mort/tour si des infectes restent, afin d'eviter une stagnation.
                if (pop.infected > 0)
                {
                    mortalityValue = Mathf.Max(mortalityValue, 1f);
                }
            }
            
            mortalityDecimals[country.name] += mortalityValue;
            
            // Prendre les entiers sans perdre de "stock" de mortalite quand on cap au nb d'infectes.
            int plannedDeaths = (int)mortalityDecimals[country.name];

            // Ne jamais tuer plus d'infectes qu'il n'y en a.
            int newDeaths = Mathf.Clamp(plannedDeaths, 0, pop.infected);

            // Soustraire uniquement les morts effectivement appliquees.
            mortalityDecimals[country.name] -= newDeaths;

            pop.dead += newDeaths;
            pop.infected = Mathf.Max(0, pop.infected - newDeaths);
            
            if (newDeaths > 0)
                Debug.Log($"  [{country.name}] -{newDeaths} morts (base:{baseLethality:F3} + bonus:{skillBonus:F3} [anus:{anusBonus:F3} poumon:{poumonBonus:F3} bouffe:{bouffeBonus:F3} cerveau:{cerveauBonus:F3}] => {effectiveLethality:F3}, cond:cold={isColdCountry},hot={isHotCountry},arid={isAridClimate},lowHyg={isLowHygiene},rich={isRichCountry}, bypass:{virus.immunityBypass:P0}, accumulated:{mortalityDecimals[country.name]:F2})");
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
