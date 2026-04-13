using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public string id;                           // Unique identifier: "airborneL1", "seaL2", etc.
    public string name;                         // Display name: "Airborne Diarrhea"
    public string description;                  // Funny description
    public string variableModified;             // What variable changes: "Votre virus devient plus résistant au chaud"
    public int cost;                            // Point cost (0 for free, changeable later)
    public int level;                           // 1 or 2
    public string category;                     // "Transport", "Climate", "Bathroom", "Dog", "Pet", "Fly", "Urine", "DrugResistance", "VaccineBypass"
    public List<string> dependencies;           // Other skills that must be unlocked first (e.g., ["airborneL1"] for L2)
    public List<string> anyDependency;          // At least ONE of these must be unlocked (OR condition for N3 after N2 choice)
    public List<string> mutuallyExclusiveWith;  // Skills that block this one if already purchased (exclusive choice)
    public Dictionary<string, float> effects;   // What virus stats this skill affects
    
    // Runtime state
    public bool isUnlocked = false;
    
    public Skill(string id, string name, string description, string variableModified, int cost, int level, string category, 
                 List<string> dependencies, Dictionary<string, float> effects,
                 List<string> mutuallyExclusiveWith = null, List<string> anyDependency = null)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.variableModified = variableModified;
        this.cost = cost;
        this.level = level;
        this.category = category;
        this.dependencies = dependencies ?? new List<string>();
        this.effects = effects ?? new Dictionary<string, float>();
        this.mutuallyExclusiveWith = mutuallyExclusiveWith ?? new List<string>();
        this.anyDependency = anyDependency ?? new List<string>();
    }
    
    /// <summary>
    /// Check if player can afford this skill
    /// </summary>
    public bool CanAfford(int currentPoints)
    {
        return cost <= currentPoints;
    }
    
    /// <summary>
    /// Check if all dependencies are unlocked and no exclusive sibling is already purchased
    /// </summary>
    public bool CanUnlock(List<Skill> unlockedSkills)
    {
        foreach (string dependency in dependencies)
        {
            bool found = false;
            foreach (Skill unlockedSkill in unlockedSkills)
            {
                if (unlockedSkill.id == dependency)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return false;
        }
        // Blocked if any mutually exclusive sibling is already purchased
        foreach (string exclId in mutuallyExclusiveWith)
        {
            foreach (Skill unlockedSkill in unlockedSkills)
            {
                if (unlockedSkill.id == exclId)
                    return false;
            }
        }
        // OR-dependency: at least one of anyDependency must be unlocked
        if (anyDependency != null && anyDependency.Count > 0)
        {
            bool anyFound = false;
            foreach (string anyId in anyDependency)
            {
                foreach (Skill unlockedSkill in unlockedSkills)
                {
                    if (unlockedSkill.id == anyId) { anyFound = true; break; }
                }
                if (anyFound) break;
            }
            if (!anyFound) return false;
        }
        return true;
    }
    
    /// <summary>
    /// Check if skill is available for purchase
    /// </summary>
    public bool IsAvailable(List<Skill> unlockedSkills)
    {
        return !isUnlocked && CanUnlock(unlockedSkills);
    }
    
    /// <summary>
    /// Apply this skill's effects to a virus
    /// </summary>
    public void ApplyToVirus(Virus virus)
    {
        float GetNaturalLethalityBonusByLevel()
        {
            switch (level)
            {
                case 1: return 0.005f;
                case 2: return 0.015f;
                case 3: return 0.035f;
                case 4: return 0.075f;
                default: return 0f;
            }
        }

        bool hasMortalityEffect = false;

        foreach (var effect in effects)
        {
            switch (effect.Key)
            {
                case "airborneModifier":
                    virus.airborneModifier += effect.Value;
                    break;
                case "seaModifier":
                    virus.seaModifier += effect.Value;
                    break;
                case "landModifier":
                    virus.landModifier += effect.Value;
                    break;
                case "dogModifier":
                    virus.dogModifier += effect.Value;
                    break;
                case "petModifier":
                    virus.petModifier += effect.Value;
                    break;
                case "flyModifier":
                    virus.flyModifier += effect.Value;
                    break;
                case "urineModifier":
                    virus.urineModifier += effect.Value;
                    break;
                case "coldResistance":
                    virus.coldResistance = Mathf.Clamp01(virus.coldResistance + effect.Value);
                    break;
                case "heatResistance":
                    virus.heatResistance = Mathf.Clamp01(virus.heatResistance + effect.Value);
                    break;
                case "hygieneExploitation":
                    virus.hygieneExploitation += effect.Value;
                    break;
                case "wealthExploitation":
                    virus.wealthExploitation += effect.Value;
                    break;
                case "drugResistance":
                    virus.drugResistance = Mathf.Clamp01(virus.drugResistance + effect.Value);
                    break;
                case "immunityBypass":
                    virus.immunityBypass = Mathf.Clamp01(virus.immunityBypass + effect.Value);
                    break;
                case "vaccineResearchSlowdown":
                    virus.playerSabotageSlowdown = Mathf.Clamp(virus.playerSabotageSlowdown + effect.Value, 0.5f, 1.2f);
                    break;
                case "mortalityRate":
                    hasMortalityEffect = true;
                    virus.mortalityRate += effect.Value;
                    break;
                case "respiratoryDamage":
                    hasMortalityEffect = true;
                    virus.respiratoryDamage += effect.Value;
                    break;
                case "foodContamination":
                    hasMortalityEffect = true;
                    virus.foodContamination += effect.Value;
                    break;
                case "mentalDamage":
                    hasMortalityEffect = true;
                    virus.mentalDamage += effect.Value;
                    break;
                // ─── SPECIAL SKILL PASSIVES ──────────────────────────────
                case "lethality":
                    virus.lethality = Mathf.Clamp01(virus.lethality + effect.Value);
                    break;
                case "autoTransmissionSkillChance":
                    virus.autoTransmissionSkillChance = Mathf.Clamp01(virus.autoTransmissionSkillChance + effect.Value);
                    break;
                case "autoTransmissionSkillChanceTimed":
                    virus.autoTransmissionSkillChanceTimed = Mathf.Clamp01(virus.autoTransmissionSkillChanceTimed + effect.Value);
                    break;
                case "autoTransmissionSkillTurns":
                    virus.autoTransmissionSkillTurnsLeft += Mathf.RoundToInt(effect.Value);
                    break;
                case "autoLethalSymptomChance":
                    virus.autoLethalSymptomChance = Mathf.Clamp01(virus.autoLethalSymptomChance + effect.Value);
                    break;
                case "autoLethalSymptomChanceTimed":
                    virus.autoLethalSymptomChanceTimed = Mathf.Clamp01(virus.autoLethalSymptomChanceTimed + effect.Value);
                    break;
                case "autoLethalSymptomTurns":
                    virus.autoLethalSymptomTurnsLeft += Mathf.RoundToInt(effect.Value);
                    break;
                case "pointGainMultiplierPermanent":
                    virus.pointGainMultiplier += effect.Value;
                    break;
                case "vaccineMaxProgressMultiplier":
                    virus.vaccineMaxProgressMultiplier = effect.Value;
                    break;
                case "crossBorderSpreadBonus":
                    virus.crossBorderSpreadBonus += effect.Value;
                    break;
                case "regionalSpreadBonus":
                    virus.regionalSpreadBonus += effect.Value;
                    break;
                case "infectionGrowthMultiplier":
                    virus.infectionGrowthMultiplier += effect.Value;
                    break;
                // One-shot active effects (dispersalSpatiale, seedOneInNonInfectedCountry) are
                // handled directly in SkillTreeManager.UnlockSkill() after ApplyToVirus().
            }
        }

        if (hasMortalityEffect)
            virus.lethality = Mathf.Clamp01(virus.lethality + GetNaturalLethalityBonusByLevel());
    }
    
    public override string ToString()
    {
        return $"{name} (Lvl {level}) - Cost: {cost} pts - {description}";
    }
}
