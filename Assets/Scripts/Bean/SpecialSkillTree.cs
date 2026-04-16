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

    // Virus urbain né dans les crottes de chien — couleur: brun-rouge organique
    private static List<Skill> BuildClassiqueSkills()
    {
        return new List<Skill>
        {
            new Skill(
                id: "claN1",
                name: "CrotteSpreading Evolution",
                description: "Le virus évolue en continue pour crotter un maximum d'humains. Chaque tour, 2% de chance d'assimiler automatiquement un talent de transmission.",
                variableModified: "2% chance/tour: déblocage auto d'un skill Transmission",
                cost: 14, level: 1, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "autoTransmissionSkillChance", 0.02f } }
            ),
            new Skill(
                id: "claN2a",
                name: "Bave Crottée",
                description: "Le virus a impregne la salive de tous les chiens. Propagation canine et animale améliorée.",
                variableModified: "+0.09 modificateur Chien et Animaux",
                cost: 20, level: 2, category: "Special",
                dependencies: new List<string> { "claN1" },
                effects: new Dictionary<string, float> { { "dogModifier", 0.09f }, { "petModifier", 0.09f } },
                mutuallyExclusiveWith: new List<string> { "claN2b" }
            ),
            new Skill(
                id: "claN2b",
                name: "Flaquacaca",
                description: "Meme se simples flaques d'eau inocente deviennent maintenant un moyen de propagation. Propagation terrestre améliorée.",
                variableModified: "+0.07 modificateur Transport terrestre",
                cost: 20, level: 2, category: "Special",
                dependencies: new List<string> { "claN1" },
                effects: new Dictionary<string, float> { { "landModifier", 0.07f } },
                mutuallyExclusiveWith: new List<string> { "claN2a" }
            ),
            new Skill(
                id: "claN3",
                name: "Prime Coprogène",
                description: "Chaque infection rapporte désormais un loyer biologique permanent. Bonus de gaints points définitif pour toute la partie.",
                variableModified: "+22% gain de points définitif",
                cost: 30, level: 3, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "pointGainMultiplierPermanent", 0.22f } },
                mutuallyExclusiveWith: null,
                anyDependency: new List<string> { "claN2a", "claN2b" }
            ),
            new Skill(
                id: "claN4a",
                name: "Colique Invasive",
                description: "La dernière mutation transforme les coliques en hémorragie systémique. Létalité augmentée.",
                variableModified: "+3.5% létalité",
                cost: 20, level: 4, category: "Special",
                dependencies: new List<string> { "claN3" },
                effects: new Dictionary<string, float> { { "lethality", 0.035f } },
                mutuallyExclusiveWith: new List<string> { "claN4b" }
            ),
            new Skill(
                id: "claN4b",
                name: "Croûte Septique",
                description: "Dans les environnements insalubres, le virus développe une couche de crasse biologique qui décuple sa virulence.",
                variableModified: "+0.06 exploitation Hygiène",
                cost: 20, level: 4, category: "Special",
                dependencies: new List<string> { "claN3" },
                effects: new Dictionary<string, float> { { "hygieneExploitation", 0.06f } },
                mutuallyExclusiveWith: new List<string> { "claN4a" }
            ),
        };
    }

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
                cost: 14, level: 1, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "dispersalSpatiale", 5f } }
            ),
            new Skill(
                id: "casN2a",
                name: "Poussière de Merde Orbitale",
                description: "Les particules de déjection cosmique infiltrent avions et bateaux sans effort.",
                variableModified: "+0.10 transport Aérien | +0.08 transport Maritime",
                cost: 20, level: 2, category: "Special",
                dependencies: new List<string> { "casN1" },
                effects: new Dictionary<string, float> { { "airborneModifier", 0.10f }, { "seaModifier", 0.08f } },
                mutuallyExclusiveWith: new List<string> { "casN2b" }
            ),
            new Skill(
                id: "casN2b",
                name: "Plasma Coproïde",
                description: "L'origine extraterrestre confère une résistance aux températures extrêmes.",
                variableModified: "+0.05 résistance Chaleur | +0.05 résistance Froid",
                cost: 20, level: 2, category: "Special",
                dependencies: new List<string> { "casN1" },
                effects: new Dictionary<string, float> { { "heatResistance", 0.05f }, { "coldResistance", 0.05f } },
                mutuallyExclusiveWith: new List<string> { "casN2a" }
            ),
            new Skill(
                id: "casN3",
                name: "Transmission Évolutive Astrale",
                description: "Le signal cosmique s'ancre définitivement dans le virus: 2% de chance permanente par tour d'assimiler automatiquement un skill de transmission.",
                variableModified: "+2% chance/tour permanent: déblocage auto d'un skill Transmission",
                cost: 30, level: 3, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "autoTransmissionSkillChance", 0.02f } },
                mutuallyExclusiveWith: null,
                anyDependency: new List<string> { "casN2a", "casN2b" }
            ),
            new Skill(
                id: "casN4a",
                name: "Halo Nécrotique",
                description: "L'aura de mort cosmique irradie les tissus, augmentant la létalité du virus.",
                variableModified: "+4% létalité",
                cost: 20, level: 4, category: "Special",
                dependencies: new List<string> { "casN3" },
                effects: new Dictionary<string, float> { { "lethality", 0.04f } },
                mutuallyExclusiveWith: new List<string> { "casN4b" }
            ),
            new Skill(
                id: "casN4b",
                name: "Gravité Fécale",
                description: "Un virus spaciale n'a aucune limite, les frontières ne peuvent le contenir.",
                variableModified: "+0.07 propagation transfrontalière",
                cost: 20, level: 4, category: "Special",
                dependencies: new List<string> { "casN3" },
                effects: new Dictionary<string, float> { { "crossBorderSpreadBonus", 0.07f } },
                mutuallyExclusiveWith: new List<string> { "casN4a" }
            ),
        };
    }

    // Virus robotique / IA — couleur: métal sale / cyan clinique
    private static List<Skill> BuildNanoCacaSkills()
    {
        return new List<Skill>
        {
            new Skill(
                id: "nanN1",
                name: "Injection Patient Zéro",
                description: "Un nano-porteur autonome s'infiltre discrètement dans un pays non infecté et dépose précisément 1 porteur asymptomatique.",
                variableModified: "Infecte immédiatement 1 personne dans un pays non infecté aléatoire",
                cost: 14, level: 1, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "seedOneInNonInfectedCountry", 1f } }
            ),
            new Skill(
                id: "nanN2a",
                name: "Blindage Pharmaceutique",
                description: "Les nano-coques renforcent la membrane virale contre les antiviraux. Les traitements perdent de leur efficacité.",
                variableModified: "+0.10 résistance aux médicaments",
                cost: 20, level: 2, category: "Special",
                dependencies: new List<string> { "nanN1" },
                effects: new Dictionary<string, float> { { "drugResistance", 0.10f } },
                mutuallyExclusiveWith: new List<string> { "nanN2b" }
            ),
            new Skill(
                id: "nanN2b",
                name: "Auto-Réplication Optimisée",
                description: "Les nano-capteurs optimisent le taux de réplication en temps réel. La croissance des infections s'accélère.",
                variableModified: "+0.08 multiplicateur de croissance d'infection",
                cost: 20, level: 2, category: "Special",
                dependencies: new List<string> { "nanN1" },
                effects: new Dictionary<string, float> { { "infectionGrowthMultiplier", 0.08f } },
                mutuallyExclusiveWith: new List<string> { "nanN2a" }
            ),
            new Skill(
                id: "nanN3",
                name: "Mutation CacaTueuse Autonome",
                description: "Les processeurs nano-viraux s'ancrent définitivement et forgent en permanence de nouveaux symptômes mortels.",
                variableModified: "+4% chance/tour permanent: déblocage auto d'un skill Mortalité",
                cost: 30, level: 3, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "autoLethalSymptomChance", 0.02f } },
                mutuallyExclusiveWith: null,
                anyDependency: new List<string> { "nanN2a", "nanN2b" }
            ),
            new Skill(
                id: "nanN4a",
                name: "Essaim de Colonisation",
                description: "Les nano-virus ciblent prioritairement les individus aisés, dont les réseaux sociaux assurent une dissémination maximale.",
                variableModified: "+0.06 exploitation Richesse",
                cost: 20, level: 4, category: "Special",
                dependencies: new List<string> { "nanN3" },
                effects: new Dictionary<string, float> { { "wealthExploitation", 0.06f } },
                mutuallyExclusiveWith: new List<string> { "nanN4b" }
            ),
            new Skill(
                id: "nanN4b",
                name: "Protocole Sphincter-0",
                description: "Le protocole final contourne les défenses immunitaires génétiques tout en augmentant la virulence létale.",
                variableModified: "+4% létalité | +4% contournement Immunité",
                cost: 20, level: 4, category: "Special",
                dependencies: new List<string> { "nanN3" },
                effects: new Dictionary<string, float> { { "lethality", 0.04f }, { "immunityBypass", 0.04f } },
                mutuallyExclusiveWith: new List<string> { "nanN4a" }
            ),
        };
    }

    // Virus fongique des égouts — couleur: jaune-vert moisi / vert sombre
    private static List<Skill> BuildMycoCloacqueSkills()
    {
        return new List<Skill>
        {
            new Skill(
                id: "myoN1",
                name: "Mutation Sporulée",
                description: "Le mycélium viral sporule spontanément. 2% de chance par tour d'acquérir un symptôme létal.",
                variableModified: "2% chance/tour: déblocage auto d'un skill Mortalité",
                cost: 14, level: 1, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "autoLethalSymptomChance", 0.02f } }
            ),
            new Skill(
                id: "myoN2a",
                name: "Réseau d'Égouts",
                description: "Le champignon colonise les réseaux d'égouts. Les environnements à faible hygiène deviennent de parfaits incubateurs.",
                variableModified: "+0.09 exploitation Hygiène",
                cost: 20, level: 2, category: "Special",
                dependencies: new List<string> { "myoN1" },
                effects: new Dictionary<string, float> { { "hygieneExploitation", 0.09f } },
                mutuallyExclusiveWith: new List<string> { "myoN2b" }
            ),
            new Skill(
                id: "myoN2b",
                name: "Racines Intestinales",
                description: "Les hyphes virales s'ancrent profondément dans les tissus intestinaux, accélérant la croissance de l'infection.",
                variableModified: "+0.07 croissance d'infection",
                cost: 20, level: 2, category: "Special",
                dependencies: new List<string> { "myoN1" },
                effects: new Dictionary<string, float> { { "infectionGrowthMultiplier", 0.07f } },
                mutuallyExclusiveWith: new List<string> { "myoN2a" }
            ),
            new Skill(
                id: "myoN3",
                name: "Synthétisation Difficile",
                description: "Le champignon produit des protéines mimant les anticorps naturels, rendant le développement du vaccin extrêmement complexe. Plafond vaccin à 115%.",
                variableModified: "Plafond vaccin relevé à 115%",
                cost: 30, level: 3, category: "Special",
                dependencies: new List<string>(),
                effects: new Dictionary<string, float> { { "vaccineMaxProgressMultiplier", 1.15f } },
                mutuallyExclusiveWith: null,
                anyDependency: new List<string> { "myoN2a", "myoN2b" }
            ),
            new Skill(
                id: "myoN4a",
                name: "Floraison Champigno-Fécale",
                description: "Dans les environnements humides, les spores virales fleurissent en agents nécrotiques. Létalité augmentée.",
                variableModified: "+5% létalité",
                cost: 20, level: 4, category: "Special",
                dependencies: new List<string> { "myoN3" },
                effects: new Dictionary<string, float> { { "lethality", 0.05f } },
                mutuallyExclusiveWith: new List<string> { "myoN4b" }
            ),
            new Skill(
                id: "myoN4b",
                name: "Brouillard Copromycète",
                description: "Un brouillard de spores fongiques enveloppe la région infectée, favorisant une propagation rapide dans les pays voisins.",
                variableModified: "+0.06 propagation régionale",
                cost: 20, level: 4, category: "Special",
                dependencies: new List<string> { "myoN3" },
                effects: new Dictionary<string, float> { { "regionalSpreadBonus", 0.06f } },
                mutuallyExclusiveWith: new List<string> { "myoN4a" }
            ),
        };
    }
}
