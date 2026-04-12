using System.Collections.Generic;

/// <summary>
/// Defines all Transmission-related skills for the skill tree
/// Total: 20 skills, organized by category
/// </summary>
public static class TransmissionSkillTree
{
    private static List<Skill> allSkills;
    
    public static List<Skill> GetAllSkills()
    {
        if (allSkills == null)
        {
            InitializeSkills();
        }
        return allSkills;
    }
    
    private static void InitializeSkills()
    {
        allSkills = new List<Skill>();
        
        // ═════════════════════════════════════════════════════════════════
        // TRANSPORT CATEGORY (6 skills)
        // ═════════════════════════════════════════════════════════════════
        
        // AIR TRANSPORT
        allSkills.Add(new Skill(
            id: "airborneL1",
            name: "Airborne Diarrhea",
            description: "Fecal aerosols reach cruising altitude. Even first-class passengers aren't safe.",
            cost: 0,
            level: 1,
            category: "Transport",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "airborneModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "airborneL2",
            name: "Shit Storm Skies",
            description: "Airplane bathrooms become flying petri dishes. All 300 passengers breathe the same air...",
            cost: 0,
            level: 2,
            category: "Transport",
            dependencies: new List<string> { "airborneL1" },
            effects: new Dictionary<string, float> { { "airborneModifier", 0.50f } }
        ));
        
        // SEA TRANSPORT
        allSkills.Add(new Skill(
            id: "seaL1",
            name: "Sewage Routes",
            description: "Cruise ships are just floating toilets. Your virus travels first class.",
            cost: 0,
            level: 1,
            category: "Transport",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "seaModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "seaL2",
            name: "Ocean of Filth",
            description: "The ship's water recycling system is now your personal postal service.",
            cost: 0,
            level: 2,
            category: "Transport",
            dependencies: new List<string> { "seaL1" },
            effects: new Dictionary<string, float> { { "seaModifier", 0.50f } }
        ));
        
        // LAND TRANSPORT (Terrestre)
        allSkills.Add(new Skill(
            id: "landL1",
            name: "Ground Contamination",
            description: "Rest stops and truck stops: your virus hitchhikes across the continent.",
            cost: 0,
            level: 1,
            category: "Transport",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "landModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "landL2",
            name: "Shit Highway",
            description: "Every bathroom break spreads the plague. Welcome to the fecal freeway.",
            cost: 0,
            level: 2,
            category: "Transport",
            dependencies: new List<string> { "landL1" },
            effects: new Dictionary<string, float> { { "landModifier", 0.50f } }
        ));
        
        // ═════════════════════════════════════════════════════════════════
        // CLIMATE CATEGORY (4 skills)
        // ═════════════════════════════════════════════════════════════════
        
        // COLD RESISTANCE
        allSkills.Add(new Skill(
            id: "coldL1",
            name: "Frozen Crotte Survivor",
            description: "Your virus evolves to thrive in Russian bathrooms. Vodka doesn't kill it.",
            cost: 0,
            level: 1,
            category: "Climate",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "coldResistance", 0.15f } }
        ));
        
        allSkills.Add(new Skill(
            id: "coldL2",
            name: "Arctic Poop Master",
            description: "Siberian strainsbecome your virus's new favorite climate. Penguin poop spreads it further.",
            cost: 0,
            level: 2,
            category: "Climate",
            dependencies: new List<string> { "coldL1" },
            effects: new Dictionary<string, float> { { "coldResistance", 0.30f } }
        ));
        
        // HEAT RESISTANCE
        allSkills.Add(new Skill(
            id: "heatL1",
            name: "Desert Diarrhea Adaptation",
            description: "Death Valley bathrooms can't kill this strain. Even scorpion poop carries your virus.",
            cost: 0,
            level: 1,
            category: "Climate",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "heatResistance", 0.15f } }
        ));
        
        allSkills.Add(new Skill(
            id: "heatL2",
            name: "Furnace of Filth",
            description: "Infections thrive at 50°C. Your virus loves a scorching toilet seat.",
            cost: 0,
            level: 2,
            category: "Climate",
            dependencies: new List<string> { "heatL1" },
            effects: new Dictionary<string, float> { { "heatResistance", 0.30f } }
        ));
        
        // ═════════════════════════════════════════════════════════════════
        // BATHROOM CATEGORY (2 skills)
        // ═════════════════════════════════════════════════════════════════
        
        allSkills.Add(new Skill(
            id: "bathroomL1",
            name: "No Shower Spray",
            description: "Countries without washing systems spread your glory. Dry hands = contaminated hands.",
            cost: 0,
            level: 1,
            category: "Bathroom",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "hygieneExploitation", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "bathroomL2",
            name: "Attack of the Japanese Toilet",
            description: "High-tech bidet malfunctions send germs everywhere. Rich people get rekt by sentient toilets.",
            cost: 0,
            level: 2,
            category: "Bathroom",
            dependencies: new List<string> { "bathroomL1" },
            effects: new Dictionary<string, float> { { "wealthExploitation", 0.30f } }
        ));
        
        // ═════════════════════════════════════════════════════════════════
        // DOG VECTOR (2 skills)
        // ═════════════════════════════════════════════════════════════════
        
        allSkills.Add(new Skill(
            id: "dogL1",
            name: "Poodle Pandemic",
            description: "Dogs develop infected diarrhea. Every public defecation spreads your masterpiece. Pooper scoopers? More like virus dispensers.",
            cost: 0,
            level: 1,
            category: "Dog",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "dogModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "dogL2",
            name: "Great Dane Devastation",
            description: "Giant dog turds contain viral loads that make laboratories jealous. Street dogs become your unwitting army.",
            cost: 0,
            level: 2,
            category: "Dog",
            dependencies: new List<string> { "dogL1" },
            effects: new Dictionary<string, float> { { "dogModifier", 0.50f } }
        ));
        
        // ═════════════════════════════════════════════════════════════════
        // HUMAN FARTS (2 skills)
        // ═════════════════════════════════════════════════════════════════
        
        allSkills.Add(new Skill(
            id: "petL1",
            name: "Silent But Deadly",
            description: "Infected humans become walking fart dispensers. Every office, elevator, and bus ride becomes a viral aerosol attack. The smell is your calling card.",
            cost: 0,
            level: 1,
            category: "Pet",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "petModifier", 0.20f } }
        ));
        
        allSkills.Add(new Skill(
            id: "petL2",
            name: "Methane Apocalypse",
            description: "Human digestive systems become biological weapons. Gas crowding, restaurant bathrooms, concert crowds—your virus travels literally on hot air. Humanity's own biology defeats them.",
            cost: 0,
            level: 2,
            category: "Pet",
            dependencies: new List<string> { "petL1" },
            effects: new Dictionary<string, float> { { "petModifier", 0.40f } }
        ));
        
        // ═════════════════════════════════════════════════════════════════
        // FLY VECTOR (2 skills)
        // ═════════════════════════════════════════════════════════════════
        
        allSkills.Add(new Skill(
            id: "flyL1",
            name: "Fly Courier Service",
            description: "Flies land on feces, then on food. Your virus travels lunch-to-lunch across villages.",
            cost: 0,
            level: 1,
            category: "Fly",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "flyModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "flyL2",
            name: "Insect Army",
            description: "Trillions of flies become your delivery network. Open sewers = fly breeding grounds = viral paradise.",
            cost: 0,
            level: 2,
            category: "Fly",
            dependencies: new List<string> { "flyL1" },
            effects: new Dictionary<string, float> { { "flyModifier", 0.50f } }
        ));
        
        // ═════════════════════════════════════════════════════════════════
        // URINE TRANSMISSION (2 skills)
        // ═════════════════════════════════════════════════════════════════
        
        allSkills.Add(new Skill(
            id: "urineL1",
            name: "Golden Stream",
            description: "Infected urine contaminates water supplies. Bathroom floors become skating rinks of doom.",
            cost: 0,
            level: 1,
            category: "Urine",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "urineModifier", 0.20f } }
        ));
        
        allSkills.Add(new Skill(
            id: "urineL2",
            name: "Bladder Blitzkrieg",
            description: "Urinary tract infections spread your virus with every toilet flush. Water systems worldwide bow to your supremacy.",
            cost: 0,
            level: 2,
            category: "Urine",
            dependencies: new List<string> { "urineL1" },
            effects: new Dictionary<string, float> { { "urineModifier", 0.40f } }
        ));
        
        // ═════════════════════════════════════════════════════════════════
        // DRUG RESISTANCE (2 skills)
        // ═════════════════════════════════════════════════════════════════
        
        allSkills.Add(new Skill(
            id: "drugL1",
            name: "Antibiotic Breaker",
            description: "Penicillin? Cute. Your virus laughs at antibiotics and keeps spreading.",
            cost: 0,
            level: 1,
            category: "DrugResistance",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "drugResistance", 0.20f } }
        ));
        
        allSkills.Add(new Skill(
            id: "drugL2",
            name: "Super Bug Evolution",
            description: "Every antibiotic makes you stronger. Hospitals are your gym. Medical science surrenders.",
            cost: 0,
            level: 2,
            category: "DrugResistance",
            dependencies: new List<string> { "drugL1" },
            effects: new Dictionary<string, float> { { "drugResistance", 0.40f } }
        ));
        
        // ═════════════════════════════════════════════════════════════════
        // VACCINE BYPASS (2 skills)
        // ═════════════════════════════════════════════════════════════════
        
        allSkills.Add(new Skill(
            id: "vaccineL1",
            name: "ViteMaDose",
            description: "Your virus mutates faster than vaccines are made. Immunity? Never heard of it.",
            cost: 0,
            level: 1,
            category: "VaccineBypass",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "immunityBypass", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "vaccineL2",
            name: "Immunité? Lol.",
            description: "Herd immunity is a myth. Your virus treats antibodies as appetizers.",
            cost: 0,
            level: 2,
            category: "VaccineBypass",
            dependencies: new List<string> { "vaccineL1" },
            effects: new Dictionary<string, float> { { "immunityBypass", 0.50f } }
        ));
    }
    
    /// <summary>
    /// Get a skill by ID
    /// </summary>
    public static Skill GetSkill(string skillId)
    {
        foreach (Skill skill in GetAllSkills())
        {
            if (skill.id == skillId)
                return skill;
        }
        return null;
    }
    
    /// <summary>
    /// Get all available (unlocked but purchaseable) skills
    /// </summary>
    public static List<Skill> GetAvailableSkills(List<Skill> unlockedSkills)
    {
        List<Skill> available = new List<Skill>();
        foreach (Skill skill in GetAllSkills())
        {
            if (skill.IsAvailable(unlockedSkills))
            {
                available.Add(skill);
            }
        }
        return available;
    }
}
