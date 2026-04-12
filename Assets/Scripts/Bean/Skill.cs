using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public string id;                           // Unique identifier: "airborneL1", "seaL2", etc.
    public string name;                         // Display name: "Airborne Diarrhea"
    public string description;                  // Funny description
    public int cost;                            // Point cost (0 for free, changeable later)
    public int level;                           // 1 or 2
    public string category;                     // "Transport", "Climate", "Bathroom", "Dog", "Pet", "Fly", "Urine", "DrugResistance", "VaccineBypass"
    public List<string> dependencies;           // Other skills that must be unlocked first (e.g., ["airborneL1"] for L2)
    public Dictionary<string, float> effects;   // What virus stats this skill affects
    
    // Runtime state
    public bool isUnlocked = false;
    
    public Skill(string id, string name, string description, int cost, int level, string category, 
                 List<string> dependencies, Dictionary<string, float> effects)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.cost = cost;
        this.level = level;
        this.category = category;
        this.dependencies = dependencies ?? new List<string>();
        this.effects = effects ?? new Dictionary<string, float>();
    }
    
    /// <summary>
    /// Check if player can afford this skill
    /// </summary>
    public bool CanAfford(int currentPoints)
    {
        return cost <= currentPoints;
    }
    
    /// <summary>
    /// Check if all dependencies are unlocked
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
            }
        }
    }
    
    public override string ToString()
    {
        return $"{name} (Lvl {level}) - Cost: {cost} pts - {description}";
    }
}
