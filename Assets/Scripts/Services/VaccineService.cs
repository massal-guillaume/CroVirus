using System.Collections.Generic;
using UnityEngine;

public static class VaccineService
{
    private static readonly Dictionary<string, float> vaccinationDecimals = new Dictionary<string, float>();

    public static void SimulatePreparation(List<CountryObject> countries, Virus virus)
    {
        if (virus == null || !virus.isVaccineResearchActive)
            return;

        if (virus.vaccinePreparationProgress >= 100f)
        {
            virus.vaccinePreparationProgress = 100f;
            return;
        }

        int totalPopulation = 0;
        int totalAlive = 0;

        foreach (CountryObject country in countries)
        {
            totalPopulation += country.population.total;
            totalAlive += country.population.GetAlive();
        }

        float aliveRatio = totalPopulation > 0 ? (float)totalAlive / totalPopulation : 0f;
        float capacity = virus.vaccineCapacityMin + (1f - virus.vaccineCapacityMin) * Mathf.Pow(Mathf.Clamp01(aliveRatio), virus.vaccineCapacityGamma);
        float progressDelta = virus.vaccineBasePrepRate * virus.vaccineGlobalMultiplier * virus.vaccineEventMultiplier * virus.playerSabotageSlowdown * capacity;

        virus.vaccinePreparationProgress = Mathf.Clamp(virus.vaccinePreparationProgress + progressDelta, 0f, 100f);
    }

    public static int SimulateSpread(List<CountryObject> countries, Virus virus)
    {
        if (virus == null || virus.vaccinePreparationProgress < 100f)
            return 0;

        int worldNewVaccinated = 0;
        int worldAlive = 0;
        int worldInfected = 0;

        foreach (CountryObject country in countries)
        {
            worldAlive += country.population.GetAlive();
            worldInfected += country.population.infected;
        }

        float infectedRatio = worldAlive > 0 ? (float)worldInfected / worldAlive : 0f;
        float threshold = Mathf.Max(0.0001f, virus.vaccineEndgameInfectedThreshold);
        float endgameT = Mathf.Clamp01(infectedRatio / threshold);
        float endgameBoost = Mathf.Lerp(virus.vaccineEndgameMaxBoost, 1f, endgameT);

        foreach (CountryObject country in countries)
        {
            Population pop = country.population;
            int alive = pop.GetAlive();
            int availableToVaccinate = Mathf.Max(0, alive - pop.vaccinated);

            if (availableToVaccinate == 0)
                continue;

            if (!vaccinationDecimals.ContainsKey(country.name))
                vaccinationDecimals[country.name] = 0f;

            float plannedVaccinations = availableToVaccinate * virus.vaccineSpreadRate * virus.vaccineSpreadMultiplier * endgameBoost;
            vaccinationDecimals[country.name] += plannedVaccinations;

            int newVaccinated = Mathf.Min((int)vaccinationDecimals[country.name], availableToVaccinate);
            vaccinationDecimals[country.name] -= newVaccinated;

            if (newVaccinated <= 0)
                continue;

            int curedInfected = Mathf.Min(pop.infected, newVaccinated);
            pop.infected -= curedInfected;
            pop.vaccinated += newVaccinated;
            worldNewVaccinated += newVaccinated;
        }

        return worldNewVaccinated;
    }

    public static int GetTotalVaccinated(List<CountryObject> countries)
    {
        int total = 0;
        foreach (CountryObject country in countries)
        {
            total += country.population.vaccinated;
        }

        return total;
    }
}
