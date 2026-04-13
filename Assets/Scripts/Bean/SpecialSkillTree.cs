using System.Collections.Generic;

/// <summary>
/// Static provider for all Special virus skill trees (24 skills, 6 per virus).
/// Structure per virus: N1 (L1, Active) â†’ N2A+N2B (L2, Passive) â†’ N3 (L3, Active) â†’ N4A+N4B (L4, Passive)
/// All costs are 0.
/// </summary>
public static class SpecialSkillTree
{
    public static List<Skill> GetAllSkills(VirusType virusType)
    {
        switch (virusType)
        {
            case VirusType.Classique:      return BuildClassiqueSkills();
            case VirusType.Cacastellaire:  return BuildCacastellaireSkills();
            case VirusType.NanoCaca:       return BuildNanoCacaSkills();
            case VirusType.FongiCaca:    return BuildMycoCloacqueSkills();
            default:                       return BuildClassiqueSkills();
        }
    }

    public static List<Skill> GetAllSkills()
    {
        var all = new List<Skill>();
        all.AddRange(BuildClassiqueSkills());
        all.AddRange(BuildCacastellaireSkills());
        all.AddRange(BuildNanoCacaSkills());
        all.AddRange(BuildMycoCloacqueSkills());
        return all;
    }

    public static Skill GetSkill(string skillId)
    {
        foreach (Skill s in GetAllSkills())
        {
            if (s.id == skillId)
                return s;
        }
        return null;
    }

    // â”€â”€â”€ CLASSIQUE â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Virus urbain nÃ© dans les crottes de chien â€” couleur: brun-rouge organique
    private static List<Skill> BuildClassiqueSkills()
    {
        return new List<Skill>
        {
            new Skill(
                id: "claN1",
                name: "Transmission Ã‰volutive",
                description: "Le virus dÃ©veloppe une affinitÃ© naturelle pour les vecteurs existants. Chaque tour, 7% de chance d'assimiler automatiquement un talent de transmission.",
                variableModified: "7% chance/tour: dÃ©blocage auto d'un skill Transmission",
                cost: 0, level: 1, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "autoTransmissionSkillChance", 0.04f } }
            ),
            new Skill(
                id: "claN2a",
                name: "Salive de Chenil",
                description: "La crotte originelle a imprÃ©gnÃ© la salive de tous les toutous. Propagation canine et animale amÃ©liorÃ©e.",
                variableModified: "+0.09 modificateur Chien et Animaux",
                cost: 0, level: 2, category: "Special",
                dependencies: new List<string> { "claN1" },
                effects: new Dictionary<string, float> { { "dogModifier", 0.09f }, { "petModifier", 0.09f } },
                mutuallyExclusiveWith: new List<string> { "claN2b" }
            ),
            new Skill(
                id: "claN2b",
                name: "Flaque de Trottoir",
                description: "Les flaques de la ville deviennent des autoroutes crottÃ©es. Propagation terrestre amÃ©liorÃ©e.",
                variableModified: "+0.07 modificateur Transport terrestre",
                cost: 0, level: 2, category: "Special",
                dependencies: new List<string> { "claN1" },
                effects: new Dictionary<string, float> { { "landModifier", 0.07f } },
                mutuallyExclusiveWith: new List<string> { "claN2a" }
            ),
            new Skill(
                id: "claN3",
                name: "Prime CoprogÃ¨ne",
                description: "Chaque infection rapporte dÃ©sormais un loyer biologique permanent. Bonus de points dÃ©finitif pour toute la partie.",
                variableModified: "+22% gain de points dÃ©finitif",
                cost: 0, level: 3, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "pointGainMultiplierPermanent", 0.22f } },
                mutuallyExclusiveWith: null,
                anyDependency: new List<string> { "claN2a", "claN2b" }
            ),
            new Skill(
                id: "claN4a",
                name: "Colique Invasive",
                description: "La derniÃ¨re mutation transforme les coliques en hÃ©morragie systÃ©mique. LÃ©talitÃ© augmentÃ©e.",
                variableModified: "+3.5% lÃ©talitÃ©",
                cost: 0, level: 4, category: "Special",
                dependencies: new List<string> { "claN3" },
                effects: new Dictionary<string, float> { { "lethality", 0.035f } },
                mutuallyExclusiveWith: new List<string> { "claN4b" }
            ),
            new Skill(
                id: "claN4b",
                name: "CroÃ»te Septique",
                description: "Dans les environnements insalubres, le virus dÃ©veloppe une couche de crasse biologique qui dÃ©cuple sa virulence.",
                variableModified: "+0.06 exploitation HygiÃ¨ne",
                cost: 0, level: 4, category: "Special",
                dependencies: new List<string> { "claN3" },
                effects: new Dictionary<string, float> { { "hygieneExploitation", 0.06f } },
                mutuallyExclusiveWith: new List<string> { "claN4a" }
            ),
        };
    }

    // â”€â”€â”€ CACASTELLAIRE â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Virus d'origine cosmique â€” couleur: noir/brun irisÃ© cosmique
    private static List<Skill> BuildCacastellaireSkills()
    {
        return new List<Skill>
        {
            new Skill(
                id: "casN1",
                name: "Dispersion Spatiale",
                description: "Par un phénomène de déjection orbitale, le virus se dissémine instantanément sur 5 pays aléatoires non infectés lors de l'achat.",
                variableModified: "Infecte immédiatement 5 pays aléatoires (1 personne chacun)",
                cost: 0, level: 1, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "dispersalSpatiale", 5f } }
            ),
            new Skill(
                id: "casN2a",
                name: "PoussiÃ¨re Orbitale",
                description: "Les particules de dÃ©jection cosmique infiltrent avions et bateaux sans effort.",
                variableModified: "+0.10 transport AÃ©rien | +0.08 transport Maritime",
                cost: 0, level: 2, category: "Special",
                dependencies: new List<string> { "casN1" },
                effects: new Dictionary<string, float> { { "airborneModifier", 0.10f }, { "seaModifier", 0.08f } },
                mutuallyExclusiveWith: new List<string> { "casN2b" }
            ),
            new Skill(
                id: "casN2b",
                name: "Plasma CoproÃ¯de",
                description: "L'origine extraterrestre confÃ¨re une rÃ©sistance aux tempÃ©ratures extrÃªmes.",
                variableModified: "+0.05 rÃ©sistance Chaleur | +0.05 rÃ©sistance Froid",
                cost: 0, level: 2, category: "Special",
                dependencies: new List<string> { "casN1" },
                effects: new Dictionary<string, float> { { "heatResistance", 0.05f }, { "coldResistance", 0.05f } },
                mutuallyExclusiveWith: new List<string> { "casN2a" }
            ),
            new Skill(
                id: "casN3",
                name: "Transmission Évolutive Astrale",
                description: "Le signal cosmique s'ancre définitivement dans le virus: 6% de chance permanente par tour d'assimiler automatiquement un skill de transmission.",
                variableModified: "+6% chance/tour permanent: déblocage auto d'un skill Transmission",
                cost: 0, level: 3, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "autoTransmissionSkillChance", 0.04f } },
                mutuallyExclusiveWith: null,
                anyDependency: new List<string> { "casN2a", "casN2b" }
            ),
            new Skill(
                id: "casN4a",
                name: "Halo NÃ©crotique",
                description: "L'aura de mort cosmique irradie les tissus, augmentant la lÃ©talitÃ© du virus.",
                variableModified: "+4% lÃ©talitÃ©",
                cost: 0, level: 4, category: "Special",
                dependencies: new List<string> { "casN3" },
                effects: new Dictionary<string, float> { { "lethality", 0.04f } },
                mutuallyExclusiveWith: new List<string> { "casN4b" }
            ),
            new Skill(
                id: "casN4b",
                name: "GravitÃ© FÃ©cale",
                description: "La masse gravitationnelle attire irrÃ©sistiblement le virus vers les frontiÃ¨res nationales.",
                variableModified: "+0.07 propagation transfrontaliÃ¨re",
                cost: 0, level: 4, category: "Special",
                dependencies: new List<string> { "casN3" },
                effects: new Dictionary<string, float> { { "crossBorderSpreadBonus", 0.07f } },
                mutuallyExclusiveWith: new List<string> { "casN4a" }
            ),
        };
    }

    // â”€â”€â”€ NANO CACA â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Virus robotique / IA â€” couleur: mÃ©tal sale / cyan clinique
    private static List<Skill> BuildNanoCacaSkills()
    {
        return new List<Skill>
        {
            new Skill(
                id: "nanN1",
                name: "Injection Patient ZÃ©ro",
                description: "Un nano-porteur autonome s'infiltre discrÃ¨tement dans un pays non infectÃ© et dÃ©pose prÃ©cisÃ©ment 1 porteur asymptomatique.",
                variableModified: "Infecte immÃ©diatement 1 personne dans un pays non infectÃ© alÃ©atoire",
                cost: 0, level: 1, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "seedOneInNonInfectedCountry", 1f } }
            ),
            new Skill(
                id: "nanN2a",
                name: "Blindage Pharmaceutique",
                description: "Les nano-coques renforcent la membrane virale contre les antiviraux. Les traitements perdent de leur efficacitÃ©.",
                variableModified: "+0.10 rÃ©sistance aux mÃ©dicaments",
                cost: 0, level: 2, category: "Special",
                dependencies: new List<string> { "nanN1" },
                effects: new Dictionary<string, float> { { "drugResistance", 0.10f } },
                mutuallyExclusiveWith: new List<string> { "nanN2b" }
            ),
            new Skill(
                id: "nanN2b",
                name: "Auto-RÃ©plication Sale",
                description: "Les nano-capteurs optimisent le taux de rÃ©plication en temps rÃ©el. La croissance des infections s'accÃ©lÃ¨re.",
                variableModified: "+0.08 multiplicateur de croissance d'infection",
                cost: 0, level: 2, category: "Special",
                dependencies: new List<string> { "nanN1" },
                effects: new Dictionary<string, float> { { "infectionGrowthMultiplier", 0.08f } },
                mutuallyExclusiveWith: new List<string> { "nanN2a" }
            ),
            new Skill(
                id: "nanN3",
                name: "Mutation Auto-Forge",
                description: "Les processeurs nano-viraux s'ancrent définitivement et forgent en permanence de nouveaux symptômes mortels.",
                variableModified: "+4% chance/tour permanent: déblocage auto d'un skill Mortalité",
                cost: 0, level: 3, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "autoLethalSymptomChance", 0.04f } },
                mutuallyExclusiveWith: null,
                anyDependency: new List<string> { "nanN2a", "nanN2b" }
            ),
            new Skill(
                id: "nanN4a",
                name: "Essaim de Colonisation",
                description: "Les nano-virus ciblent prioritairement les individus aisÃ©s, dont les rÃ©seaux sociaux assurent une dissÃ©mination maximale.",
                variableModified: "+0.06 exploitation Richesse",
                cost: 0, level: 4, category: "Special",
                dependencies: new List<string> { "nanN3" },
                effects: new Dictionary<string, float> { { "wealthExploitation", 0.06f } },
                mutuallyExclusiveWith: new List<string> { "nanN4b" }
            ),
            new Skill(
                id: "nanN4b",
                name: "Protocole Sphincter-0",
                description: "Le protocole final contourne les dÃ©fenses immunitaires gÃ©nÃ©tiques tout en augmentant la virulence lÃ©tale.",
                variableModified: "+4% lÃ©talitÃ© | +4% contournement ImmunitÃ©",
                cost: 0, level: 4, category: "Special",
                dependencies: new List<string> { "nanN3" },
                effects: new Dictionary<string, float> { { "lethality", 0.04f }, { "immunityBypass", 0.04f } },
                mutuallyExclusiveWith: new List<string> { "nanN4a" }
            ),
        };
    }

    // â”€â”€â”€ MYCO-CLOAQUE (Champignon) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Virus fongique des Ã©gouts â€” couleur: jaune-vert moisi / vert sombre
    private static List<Skill> BuildMycoCloacqueSkills()
    {
        return new List<Skill>
        {
            new Skill(
                id: "myoN1",
                name: "Mutation SporulÃ©e",
                description: "Le mycÃ©lium viral sporule spontanÃ©ment. Pendant 9 tours, 7% de chance par tour d'acquÃ©rir un symptÃ´me lÃ©tal.",
                variableModified: "7% chance/tour pendant 9 tours: dÃ©blocage auto d'un skill MortalitÃ©",
                cost: 0, level: 1, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "autoLethalSymptomChance", 0.90f } }
            ),
            new Skill(
                id: "myoN2a",
                name: "RÃ©seau d'Ã‰gouts",
                description: "Le champignon colonise les rÃ©seaux d'Ã©gouts. Les environnements Ã  faible hygiÃ¨ne deviennent de parfaits incubateurs.",
                variableModified: "+0.09 exploitation HygiÃ¨ne",
                cost: 0, level: 2, category: "Special",
                dependencies: new List<string> { "myoN1" },
                effects: new Dictionary<string, float> { { "hygieneExploitation", 0.09f } },
                mutuallyExclusiveWith: new List<string> { "myoN2b" }
            ),
            new Skill(
                id: "myoN2b",
                name: "Racines Intestinales",
                description: "Les hyphes virales s'ancrent profondÃ©ment dans les tissus intestinaux, accÃ©lÃ©rant la croissance de l'infection.",
                variableModified: "+0.07 croissance d'infection",
                cost: 0, level: 2, category: "Special",
                dependencies: new List<string> { "myoN1" },
                effects: new Dictionary<string, float> { { "infectionGrowthMultiplier", 0.07f } },
                mutuallyExclusiveWith: new List<string> { "myoN2a" }
            ),
            new Skill(
                id: "myoN3",
                name: "SynthÃ©tisation Difficile",
                description: "Le champignon produit des protÃ©ines mimant les anticorps naturels, rendant le dÃ©veloppement du vaccin extrÃªmement complexe. Plafond vaccin Ã  115%.",
                variableModified: "Plafond vaccin relevÃ© Ã  115%",
                cost: 0, level: 3, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "vaccineMaxProgressMultiplier", 1.15f } },
                mutuallyExclusiveWith: null,
                anyDependency: new List<string> { "myoN2a", "myoN2b" }
            ),
            new Skill(
                id: "myoN4a",
                name: "Floraison NÃ©cro-FÃ©cale",
                description: "Dans les environnements humides, les spores virales fleurissent en agents nÃ©crotiques. LÃ©talitÃ© augmentÃ©e.",
                variableModified: "+5% lÃ©talitÃ©",
                cost: 0, level: 4, category: "Special",
                dependencies: new List<string> { "myoN3" },
                effects: new Dictionary<string, float> { { "lethality", 0.05f } },
                mutuallyExclusiveWith: new List<string> { "myoN4b" }
            ),
            new Skill(
                id: "myoN4b",
                name: "Brouillard CopromycÃ¨te",
                description: "Un brouillard de spores fongiques enveloppe la rÃ©gion infectÃ©e, favorisant une propagation rapide dans les pays voisins.",
                variableModified: "+0.06 propagation rÃ©gionale",
                cost: 0, level: 4, category: "Special",
                dependencies: new List<string> { "myoN3" },
                effects: new Dictionary<string, float> { { "regionalSpreadBonus", 0.06f } },
                mutuallyExclusiveWith: new List<string> { "myoN4a" }
            ),
        };
    }
}
