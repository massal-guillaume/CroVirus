using System.Collections.Generic;

/// <summary>
/// Defines all Transmission-related skills for the skill tree
/// Total: 25 skills, organized by category
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

    public static void Reset() { allSkills = null; }
    
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
            variableModified: "Nouveau modificateur bonus pour la Transmission propagation aerienne.",
            cost: 14,
            level: 1,
            category: "Transport",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "airborneModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "airborneL2",
            name: "Chemtrails of Contamination",
            description: "Les chemtrails existent pour de bon..mais relâchent du caca au-dessus des populations",
            variableModified: "L'impact de la propagation aérienne est encore plus fort.",
            cost: 28,
            level: 2,
            category: "Transport",
            dependencies: new List<string> { "airborneL1" },
            effects: new Dictionary<string, float> { { "airborneModifier", 0.50f } }
        ));
        
        // SEA TRANSPORT
        allSkills.Add(new Skill(
            id: "seaL1",
            name: "Buffets Piege",
            description: "Les buffets à volonté dans les restaurants des bateaux de croisière deviennent un terrain de contamination.",
            variableModified: "Nouveau modificateur bonus pour la Transmission propagation maritime.",
            cost: 14,
            level: 1,
            category: "Transport",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "seaModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "seaL2",
            name: "Océan de Saleté",
            description: "Les bateaux de croisière ne sont plus que des toilettes flottantes géantes.",
            variableModified: "L'impact de la propagation maritime est encore plus fort.",
            cost: 28,
            level: 2,
            category: "Transport",
            dependencies: new List<string> { "seaL1" },
            effects: new Dictionary<string, float> { { "seaModifier", 0.50f } }
        ));
        
        // LAND TRANSPORT (Terrestre)
        allSkills.Add(new Skill(
            id: "landL1",
            name: "Pompe dans le cul",
            description: "Un infectée a mis la pompe à essence dans son cul, creant une reseau geant de propagation avec les statiosn services.",
            variableModified: "Nouveau modificateur bonus pour la Transmission propagation terrestre.",
            cost: 14,
            level: 1,
            category: "Transport",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "landModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "landL2",
            name: "Mario CaCart",
            description: "La fumée d'echapement transforme les routes en autoroutes de contamination.",
            variableModified: "L'impact de la propagation terrestre est encore plus fort.",
            cost: 28,
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
            variableModified: "Nouveau modificateur bonus pour la Transmission resistance au froid.",
            cost: 14,
            level: 1,
            category: "Climate",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "coldResistance", 0.15f } }
        ));
        
        allSkills.Add(new Skill(
            id: "coldL2",
            name: "Arctic Poop Master",
            description: "Siberian strainsbecome your virus's new favorite climate. Penguin poop spreads it further.",
            variableModified: "L'impact de la resistance au froid est encore plus fort.",
            cost: 28,
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
            variableModified: "Nouveau modificateur bonus pour la Transmission resistance a la chaleur.",
            cost: 14,
            level: 1,
            category: "Climate",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "heatResistance", 0.15f } }
        ));
        
        allSkills.Add(new Skill(
            id: "heatL2",
            name: "Furnace of Filth",
            description: "Infections thrive at 50°C. La merde chaude et liquide s'ecoule partout et contamine tout ce qui bouge.",
            variableModified: "L'impact de la resistance a la chaleur est encore plus fort.",
            cost: 28,
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
            description: "Les pays sans systèmes de lavage propagent votre gloire. Mains sèches = mains contaminées.",
            variableModified: "Nouveau modificateur bonus pour la Transmission exploitation du manque d'hygiene.",
            cost: 14,
            level: 1,
            category: "Bathroom",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "hygieneExploitation", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "bathroomL2",
            name: "Attack of the Japanese Toilet",
            description: "High-tech bidet malfunctions send germs everywhere. Rich people get rekt by sentient toilets.",
            variableModified: "L'impact de l'exploitation des differences de richesse est encore plus fort.",
            cost: 28,
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
            variableModified: "Nouveau modificateur bonus pour la Transmission propagation via les chiens.",
            cost: 14,
            level: 1,
            category: "Dog",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "dogModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "dogL2",
            name: "Great Dane Devastation",
            description: "Giant dog turds contain viral loads that make laboratories jealous. Street dogs become your unwitting army.",
            variableModified: "L'impact de la propagation via les chiens est encore plus fort.",
            cost: 28,
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
            variableModified: "Nouveau modificateur bonus pour la Transmission propagation via les gaz digestifs humains.",
            cost: 14,
            level: 1,
            category: "Pet",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "petModifier", 0.20f } }
        ));
        
        allSkills.Add(new Skill(
            id: "petL2",
            name: "Methane Apocalypse",
            description: "Human digestive systems become biological weapons. Gas crowding, restaurant bathrooms, concert crowds—your virus travels literally on hot air. Humanity's own biology defeats them.",
            variableModified: "L'impact de la propagation via les gaz digestifs humains est encore plus fort.",
            cost: 28,
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
            variableModified: "Nouveau modificateur bonus pour la Transmission propagation via les mouches.",
            cost: 14,
            level: 1,
            category: "Fly",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "flyModifier", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "flyL2",
            name: "Insect Army",
            description: "Trillions of flies become your delivery network. Open sewers = fly breeding grounds = viral paradise.",
            variableModified: "L'impact de la propagation via les mouches est encore plus fort.",
            cost: 28,
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
            variableModified: "Nouveau modificateur bonus pour la Transmission par l'urine contaminee.",
            cost: 14,
            level: 1,
            category: "Urine",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "urineModifier", 0.20f } }
        ));
        
        allSkills.Add(new Skill(
            id: "urineL2",
            name: "Bladder Blitzkrieg",
            description: "Urinary tract infections spread your virus with every toilet flush. Water systems worldwide bow to your supremacy.",
            variableModified: "L'impact de la transmission par l'urine contaminee est encore plus fort.",
            cost: 28,
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
            variableModified: "Nouveau modificateur bonus pour la Transmission resistance aux antibiotiques.",
            cost: 14,
            level: 1,
            category: "DrugResistance",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "drugResistance", 0.20f } }
        ));
        
        allSkills.Add(new Skill(
            id: "drugL2",
            name: "Super Bug Evolution",
            description: "Every antibiotic makes you stronger. Hospitals are your gym. Medical science surrenders.",
            variableModified: "L'impact de la resistance aux antibiotiques est encore plus fort.",
            cost: 28,
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
            description: "Le virus n'a peur de personne, même ceux naturellement immunisés.",
            variableModified: "Nouveau modificateur bonus pour la Transmission contournement de l'immunite vaccinale.",
            cost: 14,
            level: 1,
            category: "VaccineBypass",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "immunityBypass", 0.25f } }
        ));
        
        allSkills.Add(new Skill(
            id: "vaccineL2",
            name: "Immunité? Lol.",
            description: "L'immunité collective est un mythe. Votre virus traite les anticorps comme des amuse-gueules.",
            variableModified: "L'impact du contournement de l'immunite vaccinale est encore plus fort.",
            cost: 28,
            level: 2,
            category: "VaccineBypass",
            dependencies: new List<string> { "vaccineL1" },
            effects: new Dictionary<string, float> { { "immunityBypass", 0.50f } }
        ));

        allSkills.Add(new Skill(
            id: "vaccineSabotageL1",
            name: "Caca dans les Pipettes",
            description: "A mysterious fecal perfume invades the lab. Researchers keep sneezing and dropping samples in panic.",
            variableModified: "Nouveau modificateur bonus pour la Transmission ralentissement de la recherche vaccinale.",
            cost: 14,
            level: 1,
            category: "VaccineBypass",
            dependencies: new List<string>(),
            effects: new Dictionary<string, float> { { "vaccineResearchSlowdown", -0.03f } }
        ));

        allSkills.Add(new Skill(
            id: "vaccineSabotageL2",
            name: "Guerre Bactériologique des WC",
            description: "The scientists' coffee machine now smells like apocalypse. Between toilet emergencies and contaminated gloves, the vaccine team crawls at snail speed.",
            variableModified: "L'impact du ralentissement de la recherche vaccinale est encore plus fort.",
            cost: 28,
            level: 2,
            category: "VaccineBypass",
            dependencies: new List<string> { "vaccineSabotageL1" },
            effects: new Dictionary<string, float> { { "vaccineResearchSlowdown", -0.05f } }
        ));

        // ═════════════════════════════════════════════════════════════════
        // FUSION LEVEL 3 (3 skills)
        // ═════════════════════════════════════════════════════════════════

        allSkills.Add(new Skill(
            id: "fusionBioBidetL3",
            name: "Bio-Bidet Catastrophe",
            description: "Bidets, urine trails, and plumbing systems sync into one global contamination engine.",
            variableModified: "Les impacts de l'exploitation du manque d'hygiene et de la transmission par l'urine contaminee sont encore plus forts.",
            cost: 48,
            level: 3,
            category: "FusionBioBidet",
            dependencies: new List<string> { "bathroomL2", "urineL2" },
            effects: new Dictionary<string, float>
            {
                { "hygieneExploitation", 0.45f },
                { "urineModifier", 0.60f }
            }
        ));

        allSkills.Add(new Skill(
            id: "fusionMethaneBurstL3",
            name: "Methane Vector Burst",
            description: "Les pets humains ne se dispersent plus dans l'air, créant des nuages de contamination qui se propagent rapidement.",
            variableModified: "Les impacts de la propagation via les gaz digestifs humains et de la propagation aerienne sont encore plus forts.",
            cost: 48,
            level: 3,
            category: "FusionMethaneAir",
            dependencies: new List<string> { "petL2", "airborneL2" },
            effects: new Dictionary<string, float>
            {
                { "petModifier", 0.55f },
                { "airborneModifier", 0.55f }
            }
        ));

        allSkills.Add(new Skill(
            id: "fusionCanineAirliftL3",
            name: "Canine Fecal Airlift",
            description: "Les mouches transportent les excréments infectés des chiens à travers le monde, facilitant la propagation rapide du virus.",
            variableModified: "Les impacts de la propagation via les chiens, via les mouches et de l'exploitation du manque d'hygiene sont encore plus forts.",
            cost: 48,
            level: 3,
            category: "FusionDogFly",
            dependencies: new List<string> { "dogL2", "flyL2" },
            effects: new Dictionary<string, float>
            {
                { "dogModifier", 0.65f },
                { "flyModifier", 0.65f },
                { "hygieneExploitation", 0.20f }
            }
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
