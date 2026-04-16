using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    private static PointManager instance;

    private int currentPoints = 0;
    private int totalPointsEarned = 0;
    private float pointMultiplier = 1.0f;  // Classique N3: Prime Coprogène

    // ── Passif : tous les N tours (pas chaque tour) ──────────────────────
    private const int   PASSIVE_INTERVAL         = 10;
    private const float PASSIVE_INFECTED_FACTOR  = 1.0f;   // Log10(inf+1) × 1.0
    private const float PASSIVE_DEAD_FACTOR      = 1.5f;   // Log10(dead+1) × 1.5
    private int passiveTurnCounter = 0;

    // ── Paliers (milestone) ───────────────────────────────────────────────
    private readonly HashSet<string> milestonesGranted = new HashSet<string>();

    // Infection : % de la population mondiale
    private static readonly (float pct, int pts, string label)[] INFECTION_MILESTONES =
    {
        (0.001f,  3,  "0.1%  monde infecté"),
        (0.01f,   6,  "1%    monde infecté"),
        (0.05f,  10,  "5%    monde infecté"),
        (0.10f,  15,  "10%   monde infecté"),
        (0.25f,  25,  "25%   monde infecté"),
        (0.50f,  40,  "50%   monde infecté"),
        (0.75f,  60,  "75%   monde infecté"),
        (1.00f, 100,  "100%  monde infecté"),
    };

    // Morts absolus
    private static readonly (int dead, int pts, string label)[] DEATH_MILESTONES =
    {
        (       100_000,  3,  "100 000 morts"),
        (     1_000_000,  8,  "1 million de morts"),
        (    10_000_000, 15,  "10 millions de morts"),
        (   100_000_000, 25,  "100 millions de morts"),
        (   500_000_000, 40,  "500 millions de morts"),
        ( 1_000_000_000, 60,  "1 milliard de morts"),
    };

    // Paliers de tours écoulés
    private static readonly (int turn, int pts, string label)[] TURN_MILESTONES =
    {
        ( 30,  5, "30 tours"),
        (100, 10, "100 tours"),
        (200, 15, "200 tours"),
        (365, 20, "365 tours (1 an)"),
    };

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static PointManager GetInstance() => instance;

    public void Initialize()
    {
        currentPoints = 0;
        totalPointsEarned = 0;
        pointMultiplier = 1.0f;
        passiveTurnCounter = 0;
        milestonesGranted.Clear();
    }

    public void ApplyPointMultiplier(float multiplier)
    {
        pointMultiplier = Mathf.Max(0.1f, multiplier);
        Debug.Log($"[PointManager] Multiplicateur de points: x{pointMultiplier:F2}");
    }

    // ── Passif ────────────────────────────────────────────────────────────
    /// <summary>
    /// Appelé chaque tour. Les points passifs ne sont accordés que tous les
    /// PASSIVE_INTERVAL tours, en quantité plus importante.
    /// </summary>
    public void GeneratePoints(long totalInfected, long totalDead)
    {
        passiveTurnCounter++;
        if (passiveTurnCounter < PASSIVE_INTERVAL)
            return;

        passiveTurnCounter = 0;

        float infPts  = Mathf.Log10((float)(totalInfected + 1)) * PASSIVE_INFECTED_FACTOR;
        float deadPts = Mathf.Log10((float)(totalDead + 1))     * PASSIVE_DEAD_FACTOR;
        int pts = Mathf.FloorToInt((infPts + deadPts) * pointMultiplier);

        if (pts > 0)
        {
            currentPoints      += pts;
            totalPointsEarned  += pts;
        }
    }

    // ── Paliers ───────────────────────────────────────────────────────────
    /// <summary>
    /// Vérifie les paliers franchis et accorde les points bonus correspondants.
    /// </summary>
    public void CheckMilestones(long totalInfected, long totalDead, long worldPopulation, int currentTurn)
    {
        // -- Infection % --
        if (worldPopulation > 0)
        {
            float infPct = (float)totalInfected / worldPopulation;
            foreach (var m in INFECTION_MILESTONES)
            {
                string key = $"inf_{m.pct}";
                if (infPct >= m.pct && milestonesGranted.Add(key))
                    GrantMilestoneBonus(m.pts, m.label);
            }
        }

        // -- Morts --
        foreach (var m in DEATH_MILESTONES)
        {
            string key = $"dead_{m.dead}";
            if (totalDead >= m.dead && milestonesGranted.Add(key))
                GrantMilestoneBonus(m.pts, m.label);
        }

        // -- Tours --
        foreach (var m in TURN_MILESTONES)
        {
            string key = $"turn_{m.turn}";
            if (currentTurn >= m.turn && milestonesGranted.Add(key))
                GrantMilestoneBonus(m.pts, m.label);
        }
    }

    private void GrantMilestoneBonus(int pts, string label)
    {
        int scaled = Mathf.RoundToInt(pts * pointMultiplier);
        currentPoints     += scaled;
        totalPointsEarned += scaled;
    }

    // ── Accesseurs ────────────────────────────────────────────────────────
    public int GetCurrentPoints()    => currentPoints;
    public int GetTotalPointsEarned() => totalPointsEarned;

    public bool SpendPoints(int amount)
    {
        if (currentPoints < amount) return false;
        currentPoints -= amount;
        return true;
    }

    public void AddPoints(int amount)
    {
        currentPoints     += amount;
        totalPointsEarned += amount;
    }
}
