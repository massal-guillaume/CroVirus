using UnityEngine;

public class Virus
{
    public string name;
    public float infectivity;              // Contagiosité (0-1)
    public float lethality;                // Létalité (0-1)
    public float coldResistance;           // Résistance au froid (0-1)
    public float heatResistance;           // Résistance à la chaleur (0-1)
    
    // ─── TRANSPORT MODIFIERS ───────────────────────
    public float airborneModifier = 0.0f;  // Aviation transport
    public float seaModifier = 0.0f;       // Maritime transport
    public float landModifier = 0.0f;      // Land/Terrestre transport
    
    // ─── VECTOR MODIFIERS ──────────────────────────
    public float dogModifier = 0.0f;       // Dog feces vector
    public float petModifier = 0.0f;       // Exotic pets vector
    public float flyModifier = 0.0f;       // Fly vector
    public float urineModifier = 0.0f;     // Urine transmission
    
    // ─── EXPLOITATION MODIFIERS ────────────────────
    public float hygieneExploitation = 0.0f;  // Targets low-hygiene countries
    public float wealthExploitation = 0.0f;   // Targets rich countries
    public float drugResistance = 0.0f;       // Resists antiviral treatments (0-1: virus fights drugs)
    public float immunityBypass = 0.0f;       // Chance to bypass genetic immunity (0-1: probability)

    // ─── MORTALITY MODIFIERS ─────────────────────
    public float mortalityRate = 0.0f;        // Core lethality bonus (Anus branch)
    public float respiratoryDamage = 0.0f;    // Conditional lethality (Poumon branch)
    public float foodContamination = 0.0f;    // Conditional lethality (Bouffe branch)
    public float mentalDamage = 0.0f;         // Conditional lethality (Cerveau branch)

    // ─── SPECIAL SKILL EFFECTS ────────────────────
    public float autoTransmissionSkillChance = 0f;      // Classique N1: % chance/tour
    public float autoTransmissionSkillChanceTimed = 0f; // Cacastellaire N3: % chance temporary
    public int autoTransmissionSkillTurnsLeft = 0;      // Cacastellaire N3: turns remaining

    public float autoLethalSymptomChance = 0f;          // ongoing chance (permanent)
    public float autoLethalSymptomChanceTimed = 0f;     // Myco N1 / Nano N3: % chance temporary
    public int autoLethalSymptomTurnsLeft = 0;          // turns remaining

    public float pointGainMultiplier = 1.0f;            // Classique N3: multiplicateur de points
    public float vaccineMaxProgressMultiplier = 0f;     // Myco N3: plafond vaccin (0 = pas de plafond spécial)

    public float crossBorderSpreadBonus = 0f;           // Cacastellaire N4B
    public float regionalSpreadBonus = 0f;              // Myco N4B
    public float infectionGrowthMultiplier = 0f;        // Nano N2B, Myco N2B: bonus multiplicateur spread

    // ─── VACCINE ENEMY SYSTEM ─────────────────────
    public bool isVaccineResearchActive = false;
    public float vaccinePreparationProgress = 0f;  // 0..100
    public float vaccineBasePrepRate = 0.8333f;    // % par tour (100% en ~120 tours)
    public float vaccineSpreadRate = 0.012f;       // 1.2% des vivants non-vaccinés par tour
    public float vaccineGlobalMultiplier = 1f;
    public float vaccineEventMultiplier = 1f;
    public float playerSabotageSlowdown = 1f;      // Skill joueur: petit ralentissement (ex: 0.97)
    public float vaccineCapacityMin = 0.25f;       // Vitesse mini si le monde s'effondre
    public float vaccineCapacityGamma = 1.4f;      // Courbe de ralentissement
    public float vaccineSpreadMultiplier = 1f;
    public float vaccineEndgameInfectedThreshold = 0.08f; // Boost si infectés <= 8% des vivants
    public float vaccineEndgameMaxBoost = 2.5f;            // Multiplicateur max en fin de campagne

    public Virus(string virusName, float infectivity, float lethality, float coldResistance = 0f, float heatResistance = 0f)
    {
        name = virusName;
        this.infectivity = Mathf.Clamp01(infectivity);
        this.lethality = Mathf.Clamp01(lethality);
        this.coldResistance = Mathf.Clamp01(coldResistance);
        this.heatResistance = Mathf.Clamp01(heatResistance);
    }


    public override string ToString()
    {
        return $"{name} - Infectivité: {infectivity:P0} | Létalité: {lethality:P0} | Cold: {coldResistance:P0} | Heat: {heatResistance:P0} | Vaccin: {vaccinePreparationProgress:F1}%";
    }
}
