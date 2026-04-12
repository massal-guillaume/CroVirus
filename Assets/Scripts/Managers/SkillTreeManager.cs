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
        
        // Check if can afford
        int currentPoints = pointManager.GetCurrentPoints();
        if (!skill.CanAfford(currentPoints))
        {
            Debug.LogWarning($"Cannot afford skill {skillId}. Cost: {skill.cost}, Have: {currentPoints}");
            return false;
        }
        
        // Check if dependencies met
        if (!skill.CanUnlock(unlockedSkills))
        {
            Debug.LogWarning($"Dependencies not met for skill {skillId}");
            return false;
        }
        
        // Unlock the skill!
        skill.isUnlocked = true;
        unlockedSkills.Add(skill);
        
        // Spend points
        pointManager.SpendPoints(skill.cost);
        
        // Apply effects to virus
        skill.ApplyToVirus(gameManager.virus);
        
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
    /// Get a specific skill
    /// </summary>
    public Skill GetSkill(string skillId)
    {
        return TransmissionSkillTree.GetSkill(skillId);
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
