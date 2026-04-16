using System.Collections.Generic;

/// <summary>
/// Defines all Mortalite-related skills for the skill tree
/// Total: 21 skills — 5 chains (L1-L3) + 3 L4 fusions with 2 parents each
/// </summary>
public static class MortaliteSkillTree
{
    private static List<Skill> allSkills;

    public static List<Skill> GetAllSkills()
    {
        if (allSkills == null)
            InitializeSkills();
        return allSkills;
    }

    public static void Reset() { allSkills = null; }

    private static void InitializeSkills()
    {
        allSkills = new List<Skill>();

        // CHAIN 1: HEMORRAGIE RECTALE EXTREME
        allSkills.Add(new Skill(id: "anusL1", name: "Fissures Anales Spontanées", description: "L'épithélium rectal se déchire spontanément. Chaque passage provoque une hémorragie mineure. Le système immunitaire commence à échouer face à l'inflammation constante.", variableModified: "Mortalité augmentée, avec bonus renforcé dans les pays froids.", cost: 16, level: 1, category: "HemorragieRectale", dependencies: new List<string>(), effects: new Dictionary<string, float> { { "mortalityRate", 0.10f } }));
        allSkills.Add(new Skill(id: "anusL2", name: "Prolapsus Muqueux Complet", description: "La muqueuse rectale s'effondre et s'expulse. Les sphincters ne peuvent plus retenir quoi que ce soit. Les médecins ne savent plus quoi faire.", variableModified: "Mortalité encore plus élevée, surtout dans les pays froids.", cost: 35, level: 2, category: "HemorragieRectale", dependencies: new List<string> { "anusL1" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.18f } }));
        allSkills.Add(new Skill(id: "anusL3", name: "Nécrose du Sphincter", description: "Le tissu rectal meurt. Tout le système gastro-intestinal inférieur se transforme en plaie ouverte. La gangrène s'installe.", variableModified: "Hémorragie maximale: mortalité très forte en froid, excellente base de létalité partout.", cost: 55, level: 3, category: "HemorragieRectale", dependencies: new List<string> { "anusL2", "poumonL2" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.30f } }));

        // CHAIN 2: ASPHYXIE FECALE PROGRESSIVE
        allSkills.Add(new Skill(id: "poumonL1", name: "Reflux Méthanique Chronique", description: "Les gaz intestinaux remontent jusqu'aux poumons. Chaque respiration sent et brûle. La capacité vitale décline de 5% par jour.", variableModified: "Dégâts respiratoires renforcés en pays chauds ou climats arides.", cost: 16, level: 1, category: "AsphyxieFecale", dependencies: new List<string>(), effects: new Dictionary<string, float> { { "mortalityRate", 0.09f }, { "respiratoryDamage", 0.12f } }));
        allSkills.Add(new Skill(id: "poumonL2", name: "Solidification Bronchique", description: "La matière fécale se durcit dans les bronches. La toux produit des caillots marron. L'hypoxie commence. Les patients s'étouffent lentement.", variableModified: "Asphyxie plus violente, surtout en chaud/aride.", cost: 35, level: 2, category: "AsphyxieFecale", dependencies: new List<string> { "poumonL1" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.18f }, { "respiratoryDamage", 0.24f } }));
        allSkills.Add(new Skill(id: "poumonL3", name: "Inondation Pulmonaire Fécale", description: "Les alvéoles se remplissent de liquide intestinal. Le patient ne peut plus respirer normalement. La noyade interne est inévitable.", variableModified: "Asphyxie extrême: pic de mortalité en chaud/aride.", cost: 55, level: 3, category: "AsphyxieFecale", dependencies: new List<string> { "poumonL2", "bouffeL2" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.32f }, { "respiratoryDamage", 0.42f } }));

        // CHAIN 3: CONTAMINATION ALIMENTAIRE PLANETAIRE
        allSkills.Add(new Skill(id: "bouffeL1", name: "Infiltration des Cultures", description: "Les cultures agricoles sont contaminées par du fumier viral. Les récoltes deviennent des vecteurs passifs.", variableModified: "Contamination alimentaire surtout efficace en faible hygiène.", cost: 16, level: 1, category: "ContaminationAlimentaire", dependencies: new List<string>(), effects: new Dictionary<string, float> { { "mortalityRate", 0.08f }, { "foodContamination", 0.20f } }));
        allSkills.Add(new Skill(id: "bouffeL2", name: "Chaîne de Distribution Corrompue", description: "Les usines alimentaires sont devenues des incubateurs. Chaque étape de transformation amplifie la charge virale.", variableModified: "Contamination alimentaire amplifiée si hygiène basse, encore plus en pays chauds.", cost: 35, level: 2, category: "ContaminationAlimentaire", dependencies: new List<string> { "bouffeL1" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.16f }, { "foodContamination", 0.38f } }));
        allSkills.Add(new Skill(id: "bouffeL3", name: "Apocalypse du Marché Mondial", description: "Chaque aliment sur terre est infecté. Du champ à l'assiette, tout est contaminé. La famine tue avant la maladie.", variableModified: "Contamination maximale: très punissant en faible hygiène, avec surbonus en chaud.", cost: 55, level: 3, category: "ContaminationAlimentaire", dependencies: new List<string> { "bouffeL2", "folieL2" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.28f }, { "foodContamination", 0.60f } }));

        // CHAIN 4: CACAO CEREBRAL
        allSkills.Add(new Skill(id: "cerveauL1", name: "Encéphalite Fécale", description: "Les bactéries fécales franchissent la barrière hémato-encéphalique. Le cerveau marine doucement dans la crotte.", variableModified: "Dégâts mentaux actifs surtout dans les pays riches.", cost: 16, level: 1, category: "CerveauFecal", dependencies: new List<string>(), effects: new Dictionary<string, float> { { "mortalityRate", 0.08f }, { "mentalDamage", 0.15f } }));
        allSkills.Add(new Skill(id: "cerveauL2", name: "Pression Crânienne Brun Foncé", description: "Les cavités cérébrales se remplissent de matière brune. Les yeux commencent à suinter. La couleur est indescriptible.", variableModified: "Colonisation cérébrale renforcée en pays riches.", cost: 35, level: 2, category: "CerveauFecal", dependencies: new List<string> { "cerveauL1" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.16f }, { "mentalDamage", 0.28f } }));
        allSkills.Add(new Skill(id: "cerveauL3", name: "Excrétion Oculaire Terminale", description: "Le cerveau se vide par les orbites. Les larmes sont maintenant d'une nuance marron inquiétante. Les neurologues ont abandonné.", variableModified: "Dégâts mentaux majeurs: effet maximal dans les pays riches.", cost: 55, level: 3, category: "CerveauFecal", dependencies: new List<string> { "cerveauL2", "anusL2" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.28f }, { "mentalDamage", 0.48f } }));

        // CHAIN 5: FOLIE MERDIQUE
        allSkills.Add(new Skill(id: "folieL1", name: "Psychose Digestive", description: "Les neurotransmetteurs intestinaux envahissent le cerveau. La frontière entre envies et instincts s'efface.", variableModified: "Folie merdique: dégâts mentaux surtout visibles en pays riches.", cost: 16, level: 1, category: "FolieMerdique", dependencies: new List<string>(), effects: new Dictionary<string, float> { { "mortalityRate", 0.07f }, { "mentalDamage", 0.20f } }));
        allSkills.Add(new Skill(id: "folieL2", name: "Délire Stercoral", description: "Le patient voit des couleurs que les toilettes ne devraient pas avoir. Hallucinations garanties. Le calme ne sert à rien.", variableModified: "Folie merdique renforcée, surtout en pays riches.", cost: 35, level: 2, category: "FolieMerdique", dependencies: new List<string> { "folieL1" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.14f }, { "mentalDamage", 0.38f } }));
        allSkills.Add(new Skill(id: "folieL3", name: "Démence Terminale des Latrines", description: "Le patient est convaincu d'être une toilette. Phase finale : il essaie de s'utiliser lui-même. Les proches abandonnent.", variableModified: "Démence terminale: pression mentale maximale dans les pays riches.", cost: 55, level: 3, category: "FolieMerdique", dependencies: new List<string> { "folieL2", "cerveauL2" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.22f }, { "mentalDamage", 0.60f } }));

        // L4 FUSIONS - 2 parents each
        allSkills.Add(new Skill(id: "fusionLiquefactionL4", name: "Liquéfaction Fécale Totale", description: "Corps qui se désagrège par tous les vecteurs. Fèces hémorragiques, respiration d'excréments, aliments qui saignent. La désintégration est totale.", variableModified: "Fusion Anus+Poumon: létalité massive en pays froids et en zones chaudes/arides.", cost: 80, level: 4, category: "FusionLiquefaction", dependencies: new List<string> { "anusL3", "poumonL3" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.55f }, { "respiratoryDamage", 0.60f }, { "foodContamination", 0.50f } }));

        allSkills.Add(new Skill(id: "fusionApothéoseL4", name: "Apothéose Mentale Excrétoire", description: "Cerveau de caca qui rend fou. Le patient expulse ses pensées par l'anus. La conscience se dissout dans les fèces. Démence absolue.", variableModified: "Fusion Cerveau+Folie: dégâts mentaux extrêmes, actifs surtout dans les pays riches.", cost: 80, level: 4, category: "FusionApothéose", dependencies: new List<string> { "cerveauL3", "folieL3" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.52f }, { "mentalDamage", 0.85f } }));

        allSkills.Add(new Skill(id: "fusionContaminationL4", name: "Contamination Psycho-Respiratoire", description: "Noyade interne, empoisonnement alimentaire et démence simultanés. La folie aggrave chaque respiration, chaque mouthée devient délire. Triple asphyxie.", variableModified: "Fusion Poumon+Bouffe: énorme pression en chaud/aride, et en faible hygiène surtout si chaud.", cost: 80, level: 4, category: "FusionContamination", dependencies: new List<string> { "poumonL3", "bouffeL3" }, effects: new Dictionary<string, float> { { "mortalityRate", 0.58f }, { "respiratoryDamage", 0.70f }, { "mentalDamage", 0.65f } }));
    }

    public static Skill GetSkill(string skillId)
    {
        foreach (Skill skill in GetAllSkills())
            if (skill.id == skillId)
                return skill;
        return null;
    }

    public static List<Skill> GetAvailableSkills(List<Skill> unlockedSkills)
    {
        List<Skill> available = new List<Skill>();
        foreach (Skill skill in GetAllSkills())
            if (skill.IsAvailable(unlockedSkills))
                available.Add(skill);
        return available;
    }
}