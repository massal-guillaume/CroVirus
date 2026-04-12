using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    private static PointManager instance;
    
    private int currentPoints = 0;
    private int totalPointsEarned = 0;
    
    // Formule: Log10(Infectés + 1) × 0.1 + Log10(Morts + 1) × 0.15
    private const float INFECTED_MULTIPLIER = 0.1f;
    private const float DEAD_MULTIPLIER = 0.15f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static PointManager GetInstance()
    {
        return instance;
    }

    public void Initialize()
    {
        currentPoints = 0;
        totalPointsEarned = 0;
    }

    /// <summary>
    /// Génère les points passifs basés sur les infections et morts
    /// Formule logarithmique: Log10(Infectés + 1) × 0.1 + Log10(Morts + 1) × 0.15
    /// </summary>
    public void GeneratePoints(int totalInfected, int totalDead)
    {
        float infectedPoints = Mathf.Log10(totalInfected + 1) * INFECTED_MULTIPLIER;
        float deadPoints = Mathf.Log10(totalDead + 1) * DEAD_MULTIPLIER;
        
        float totalPointsThisTurn = infectedPoints + deadPoints;
        int pointsToAdd = Mathf.FloorToInt(totalPointsThisTurn);

        if (pointsToAdd > 0)
        {
            currentPoints += pointsToAdd;
            totalPointsEarned += pointsToAdd;
            
            Debug.Log($"  [POINTS] +{pointsToAdd} pts (Infected: {infectedPoints:F2}, Dead: {deadPoints:F2}) | Total: {currentPoints}");
        }
    }

    public int GetCurrentPoints()
    {
        return currentPoints;
    }

    public int GetTotalPointsEarned()
    {
        return totalPointsEarned;
    }

    /// <summary>
    /// Dépense des points (pour les skill trees plus tard)
    /// </summary>
    public bool SpendPoints(int amount)
    {
        if (currentPoints >= amount)
        {
            currentPoints -= amount;
            return true;
        }
        return false;
    }

    public void AddPoints(int amount)
    {
        currentPoints += amount;
        totalPointsEarned += amount;
    }
}
