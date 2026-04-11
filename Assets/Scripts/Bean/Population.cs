using UnityEngine;

public class Population
{
    public int total;           // Population totale
    public int infected;        // Nombre d'infectés
    public int dead;            // Nombre de morts

    public Population(int totalPopulation)
    {
        total = totalPopulation;
        infected = 0;
        dead = 0;
    }

    public int GetHealthy()
    {
        return total - infected - dead;
    }

    public override string ToString()
    {
        return $"Pop: {total} | Infectés: {infected} | Morts: {dead}";
    }
}
