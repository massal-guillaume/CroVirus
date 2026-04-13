using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the skill tree state: which skills are unlocked, point spending, applying effects
/// </summary>
public class SkillTreeManager : MonoBehaviour
{
    private static SkillTreeManager instance;
    
    private List<Skill> unlockedSkills = new List<Skill>();
    private PointManager pointManager;
    private GameManager gameManager;
    
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
    
    public static SkillTreeManager GetInstance()
    {
        return instance;
    }
    
    public void Initialize(PointManager pointManager, GameManager gameManager)
    {
        this.pointManager = pointManager;
        this.gameManager = gameManager;
        unlockedSkills.Clear();
        
        Debug.Log("SkillTreeManager initialized");
    }
    
    /// <summary>
    /// Attempt to unlock a skill - check if affordable and dependencies met
    /// </summary>
    public bool UnlockSkill(string skillId)
    {
        Skill skill = TransmissionSkillTree.GetSkill(skillId);
        if (skill == null)
            skill = MortaliteSkillTree.GetSkill(skillId);
        if (skill == null)
            skill = SpecialSkillTree.GetSkill(skillId);

        if (skill == null)
        {
            Debug.LogError($"Skill not found: {skillId}");
            return false;
        }
        
        // Check if already unlocked
        if (skill.isUnlocked)
        {
            Debug.LogWarning($"Skill already unlocked: {skillId}");
            return false;
        }
        
        // Check if can afford (DISABLED: all skills are free for testing)
        // int currentPoints = pointManager.GetCurrentPoints();
        // if (!skill.CanAfford(currentPoints))
        // {
        //     Debug.LogWarning($"Cannot afford skill {skillId}. Cost: {skill.cost}, Have: {currentPoints}");
        //     return false;
        // }
        
        // Check if dependencies met
        if (!skill.CanUnlock(unlockedSkills))
        {
            Debug.LogWarning($"Dependencies not met for skill {skillId}");
            return false;
        }
        
        // Unlock the skill!
        skill.isUnlocked = true;
        unlockedSkills.Add(skill);
        
        // Spend points (DISABLED: all skills are free for testing)
        // pointManager.SpendPoints(skill.cost);
        
        // Apply effects to virus
        skill.ApplyToVirus(gameManager.virus);

        // Handle one-shot active effects that need game state access
        foreach (var effect in skill.effects)
        {
            switch (effect.Key)
            {
                case "dispersalSpatiale":
                    TriggerDispersal(Mathf.RoundToInt(effect.Value));
                    break;
                case "seedOneInNonInfectedCountry":
                    TriggerPatientZeroSeed();
                    break;
            }
        }

        // Apply permanent point multiplier to PointManager
        if (skill.effects.ContainsKey("pointGainMultiplierPermanent") && pointManager != null)
            pointManager.ApplyPointMultiplier(gameManager.virus.pointGainMultiplier);

        Debug.Log($"✅ Unlocked skill: {skill.name} (Cost: {skill.cost} pts)");

        return true;
    }
    
    /// <summary>
    /// Get list of currently unlocked skills
    /// </summary>
    public List<Skill> GetUnlockedSkills()
    {
        return new List<Skill>(unlockedSkills);
    }
    
    /// <summary>
    /// Get list of available skills that can be purchased
    /// </summary>
    public List<Skill> GetAvailableSkills()
    {
        return TransmissionSkillTree.GetAvailableSkills(unlockedSkills);
    }
    
    /// <summary>
    /// Check if a skill is unlocked
    /// </summary>
    public bool IsSkillUnlocked(string skillId)
    {
        foreach (Skill skill in unlockedSkills)
        {
            if (skill.id == skillId)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Get a specific skill from either tree
    /// </summary>
    public Skill GetSkill(string skillId)
    {
        Skill skill = TransmissionSkillTree.GetSkill(skillId);
        if (skill == null)
            skill = MortaliteSkillTree.GetSkill(skillId);
        if (skill == null)
            skill = SpecialSkillTree.GetSkill(skillId);
        return skill;
    }

    /// <summary>
    /// Called each turn to attempt auto-unlocking a Transmission skill (Classique N1 / Cacastellaire N3)
    /// </summary>
    public void TryAutoUnlockTransmissionSkill(Virus virus)
    {
        float chance = virus.autoTransmissionSkillChance;
        if (virus.autoTransmissionSkillTurnsLeft > 0)
        {
            chance = Mathf.Max(chance, virus.autoTransmissionSkillChanceTimed);
            virus.autoTransmissionSkillTurnsLeft--;
        }
        if (chance <= 0f) return;
        if (UnityEngine.Random.value > chance) return;

        List<Skill> available = TransmissionSkillTree.GetAvailableSkills(unlockedSkills);
        if (available.Count == 0) return;
        Skill toUnlock = available[UnityEngine.Random.Range(0, available.Count)];
        UnlockSkill(toUnlock.id);
        Debug.Log($"[AutoTransmission] Skill auto-débloqué: {toUnlock.name}");
        if (Notification.Instance != null)
            Notification.Instance.Show("MUTATION VIRALE AUTONOME", toUnlock.name);
    }

    /// <summary>
    /// Called each turn to attempt auto-unlocking a Mortalité skill (Nano N3 / Myco N1)
    /// </summary>
    public void TryAutoUnlockLethalSymptom(Virus virus)
    {
        float chance = virus.autoLethalSymptomChance;
        if (virus.autoLethalSymptomTurnsLeft > 0)
        {
            chance = Mathf.Max(chance, virus.autoLethalSymptomChanceTimed);
            virus.autoLethalSymptomTurnsLeft--;
        }
        if (chance <= 0f) return;
        if (UnityEngine.Random.value > chance) return;

        List<Skill> available = MortaliteSkillTree.GetAvailableSkills(unlockedSkills);
        if (available.Count == 0) return;
        Skill toUnlock = available[UnityEngine.Random.Range(0, available.Count)];
        UnlockSkill(toUnlock.id);
        Debug.Log($"[AutoLethal] Skill mortalité débloqué: {toUnlock.name}");
        if (Notification.Instance != null)
            Notification.Instance.Show("MUTATION VIRALE AUTONOME", toUnlock.name);
    }

    private void TriggerDispersal(int countryCount)
    {
        if (gameManager == null) return;
        var allCountries = gameManager.countries;
        var uninfected = new List<CountryObject>();
        foreach (var c in allCountries)
        {
            if (c.population.infected == 0 && c.population.GetHealthy() > 0)
                uninfected.Add(c);
        }
        int count = Mathf.Min(countryCount, uninfected.Count);
        // Fisher-Yates shuffle
        for (int i = 0; i < count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, uninfected.Count);
            CountryObject tmp = uninfected[i];
            uninfected[i] = uninfected[rnd];
            uninfected[rnd] = tmp;
        }

        var names = new System.Collections.Generic.List<string>();
        for (int i = 0; i < count; i++)
        {
            uninfected[i].population.infected += 1;
            names.Add(uninfected[i].name);
            Debug.Log($"[Dispersion Spatiale] {uninfected[i].name}: +1 infecté");
        }

        if (Notification.Instance != null && names.Count > 0)
        {
            string countries = string.Join(", ", names);
            Notification.Instance.Show(
                "☄ DISPERSION SPATIALE",
                countries,
                new UnityEngine.Color(0.20f, 0.60f, 1f, 1f)
            );
        }
    }

    private void TriggerPatientZeroSeed()
    {
        if (gameManager == null) return;
        var candidates = new List<CountryObject>();
        foreach (var c in gameManager.countries)
        {
            if (c.population.infected == 0 && c.population.GetHealthy() > 0)
                candidates.Add(c);
        }
        if (candidates.Count == 0) return;

        PatientZeroSelectionPanel.Show(candidates, target =>
        {
            target.population.infected = 1;
            Debug.Log($"[Patient Zéro] Injecté dans {target.name}");
            if (Notification.Instance != null)
                Notification.Instance.Show("💉 PATIENT ZÉRO", target.name);
        });
    }
    
    /// <summary>
    /// Print all available skills to console
    /// </summary>
    public void PrintAvailableSkills()
    {
        List<Skill> available = GetAvailableSkills();
        
        if (available.Count == 0)
        {
            Debug.Log("No skills available for purchase");
            return;
        }
        
        Debug.Log($"📊 Available Skills ({available.Count}):");
        foreach (Skill skill in available)
        {
            Debug.Log($"  - {skill.name} (Lvl {skill.level}) - Cost: {skill.cost} | {skill.description}");
        }
    }
    
    /// <summary>
    /// Print all unlocked skills to console
    /// </summary>
    public void PrintUnlockedSkills()
    {
        Debug.Log($"🔓 Unlocked Skills ({unlockedSkills.Count}):");
        foreach (Skill skill in unlockedSkills)
        {
            Debug.Log($"  ✅ {skill.name} (Lvl {skill.level})");
        }
    }
}
