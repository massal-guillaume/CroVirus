using UnityEngine;

public class Population
{
    public int total;           // Population totale
    public int infected;        // Nombre d'infectés
    public int dead;            // Nombre de morts
    public int vaccinated;      // Nombre de vaccinés (immunisés)

    public Population(int totalPopulation)
    {
        total = totalPopulation;
        infected = 0;
        dead = 0;
        vaccinated = 0;
    }

    public int GetHealthy()
    {
        return Mathf.Max(0, total - infected - dead - vaccinated);
    }

    public int GetAlive()
    {
        return Mathf.Max(0, total - dead);
    }

    public override string ToString()
    {
        return $"Pop: {total} | Infectés: {infected} | Morts: {dead} | Vaccinés: {vaccinated}";
    }
}
