using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    public float randomEventChance = 0.02f;
    public List<Event> randomEvents = new List<Event>();
    public List<Event> triggerEvents = new List<Event>();
    public int vaccineEventIndex = 0; // For ordered vaccine events
    public bool eventActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeEvents(GameManager gameManager)
    {
        randomEvents.Clear();
        triggerEvents.Clear();
        vaccineEventIndex = 0;
        eventActive = false;
        // Add Vaccine events in order
        AddVaccineEvents(gameManager);
        // Global trigger events
        AddGlobalEvents(gameManager);
        // Random events
        AddRandomEvents(gameManager);
    }

    private void AddRandomEvents(GameManager gameManager)
    {
        // ─── Réchauffement Climatique ─────────────────────────────────────────
        randomEvents.Add(new Event(
            "Réchauffement Climatique",
            "Les émissions de gaz à effet de serre atteignent un niveau record. La planète se réchauffe.",
            Event.EventType.Random,
            null,
            () =>
            {
                float increase = UnityEngine.Random.Range(1f, 2.5f);
                foreach (var c in gameManager.countries)
                    c.averageTemperature += increase;
                ShowEventNotification(
                    "Réchauffement Climatique",
                    $"Les émissions mondiales atteignent un sommet !\nChaque pays voit sa température augmenter de {increase:F1}°C.\nLes virus à résistance thermique vont prospérer."
                );
            }
        ));

        // ─── Fermeture des Frontières ─────────────────────────────────────────
        var borderClosureEvent = new Event(
            "Fermeture des Frontières",
            "Le pays le plus touché ferme ses frontières pour endiguer l'épidémie.",
            Event.EventType.Random,
            () => vaccineEventIndex >= 5,
            () =>
            {
                CountryObject target = null;
                float maxRate = -1f;
                foreach (var c in gameManager.countries)
                {
                    if (c.population.total == 0) continue;
                    float rate = (float)c.population.infected / c.population.total;
                    if (rate > maxRate) { maxRate = rate; target = c; }
                }
                if (target == null) return;
                int closedTurns = UnityEngine.Random.Range(5, 15);
                target.borderClosedTurns += closedTurns;
                ShowEventNotification(
                    "Fermeture des Frontières",
                    $"Submergé par l'épidémie, {target.name} décrète la fermeture totale de ses frontières.\nTous les voyages internationaux sont suspendus pendant {closedTurns} tours."
                );
            }
        );
        borderClosureEvent.repeatable = true;
        borderClosureEvent.cooldownTurns = 20;
        randomEvents.Add(borderClosureEvent);

        // ─── Don Pharmaceutique ───────────────────────────────────────────────
        // Détaillé : boost vitesse + avancement, conditionné à la recherche active
        randomEvents.Add(new Event(
            "Don Pharmaceutique",
            "Un consortium de laboratoires privés injecte des milliards dans la recherche vaccinale.",
            Event.EventType.Random,
            () => vaccineEventIndex >= 5 && gameManager.virus.isVaccineResearchActive,
            () =>
            {
                float rateBoost = UnityEngine.Random.Range(1.05f, 1.15f);
                float progressBoost = UnityEngine.Random.Range(3f, 8f);
                gameManager.virus.vaccineBasePrepRate *= rateBoost;
                gameManager.virus.vaccinePreparationProgress = Mathf.Min(99f,
                    gameManager.virus.vaccinePreparationProgress + progressBoost);
                ShowEventNotification(
                    "Don Pharmaceutique",
                    $"Un consortium pharmaceutique débloque des fonds d'urgence !\n" +
                    $"• Vitesse de recherche : +{(rateBoost - 1f) * 100f:F0}%\n" +
                    $"• Avancement immédiat : +{progressBoost:F0}% de progression"
                );
            }
        ));

        // ─── Mouvement Anti-Vaccin ────────────────────────────────────────────
        // Détaillé : ralentit la recherche, réduit le taux de vaccination ET boost infectivité
        var antiVaxEvent = new Event(
            "Mouvement Anti-Vaccin",
            "Une campagne de désinformation massive sème le doute sur le vaccin en développement.",
            Event.EventType.Random,
            () => vaccineEventIndex >= 5 && gameManager.virus.isVaccineResearchActive,
            () =>
            {
                float ratemalus = UnityEngine.Random.Range(0.82f, 0.93f);
                float spreadMalus = UnityEngine.Random.Range(0.82f, 0.93f);
                float infectBonus = UnityEngine.Random.Range(0.005f, 0.015f);
                gameManager.virus.vaccineBasePrepRate *= ratemalus;
                gameManager.virus.vaccineSpreadRate *= spreadMalus;
                gameManager.virus.infectivity = Mathf.Min(1f, gameManager.virus.infectivity + infectBonus);
                ShowEventNotification(
                    "Mouvement Anti-Vaccin",
                    $"Des millions rejettent le vaccin suite à de fausses informations !\n" +
                    $"• Vitesse de recherche : -{(1f - ratemalus) * 100f:F0}%\n" +
                    $"• Taux de vaccination : -{(1f - spreadMalus) * 100f:F0}%\n" +
                    $"• Contagiosité du virus : +{infectBonus * 100f:F1}% (moins de précautions)"
                );
            }
        );
        antiVaxEvent.repeatable = true;
        antiVaxEvent.cooldownTurns = 30;
        randomEvents.Add(antiVaxEvent);

        // ─── Fuite de Données de Recherche ────────────────────────────────────
        // Détaillé : recul de progression + ralentissement + flavour
        randomEvents.Add(new Event(
            "Fuite de Données de Recherche",
            "Des cyberterroristes s'infiltrent dans les serveurs des laboratoires vaccinaux.",
            Event.EventType.Random,
            () => vaccineEventIndex >= 5 && gameManager.virus.vaccinePreparationProgress >= 20f,
            () =>
            {
                float setback = UnityEngine.Random.Range(5f, 12f);
                float rateHit = UnityEngine.Random.Range(0.90f, 0.97f);
                gameManager.virus.vaccinePreparationProgress = Mathf.Max(0f,
                    gameManager.virus.vaccinePreparationProgress - setback);
                gameManager.virus.vaccineBasePrepRate *= rateHit;
                ShowEventNotification(
                    "Fuite de Données de Recherche",
                    $"Des hackers ont détruit des mois de travail scientifique !\n" +
                    $"• Régression de l'avancement : -{setback:F0}%\n" +
                    $"• Vitesse de recherche réduite : -{(1f - rateHit) * 100f:F0}%\n" +
                    $"(Les équipes doivent reconstruire leurs bases de données)"
                );
            }
        ));

        // ─── Vague de Froid ───────────────────────────────────────────────────
        randomEvents.Add(new Event(
            "Vague de Froid",
            "Une vague de froid polaire paralyse les pays nordiques.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.averageTemperature <= 10f),
            () =>
            {
                float drop = UnityEngine.Random.Range(1.5f, 3f);
                int affected = 0;
                foreach (var c in gameManager.countries)
                    if (c.averageTemperature <= 10f) { c.averageTemperature -= drop; affected++; }
                ShowEventNotification("Vague de Froid",
                    $"Des températures records s'abattent sur les pays nordiques !\n{affected} pays perdent {drop:F1}°C supplémentaires.");
            }
        ));

        // ─── Canicule Mondiale ────────────────────────────────────────────────
        randomEvents.Add(new Event(
            "Canicule Mondiale",
            "Des records de chaleur sont battus dans les pays tropicaux et méditerranéens.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.averageTemperature >= 25f),
            () =>
            {
                float rise = UnityEngine.Random.Range(1.5f, 3f);
                int affected = 0;
                foreach (var c in gameManager.countries)
                    if (c.averageTemperature >= 25f) { c.averageTemperature += rise; affected++; }
                ShowEventNotification("Canicule Mondiale",
                    $"Une chaleur extrême s'installe sur les pays chauds.\n{affected} pays subissent +{rise:F1}°C supplémentaires.");
            }
        ));

        // ─── Campagne de Santé OMS ────────────────────────────────────────────
        var omsEvent = new Event(
            "Campagne de Santé OMS",
            "L'OMS déploie des équipes médicales dans les pays les plus vulnérables.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.hygiene < 0.5f),
            () =>
            {
                float boost = UnityEngine.Random.Range(0.04f, 0.08f);
                int helped = 0;
                foreach (var c in gameManager.countries)
                    if (c.hygiene < 0.5f && helped < 3) { c.hygiene = Mathf.Min(1f, c.hygiene + boost); helped++; }
                ShowEventNotification("Campagne de Santé OMS",
                    $"L'OMS mobilise des ressources mondiales !\nL'hygiène s'améliore de {boost * 100f:F0}% dans {helped} pays défavorisés.");
            }
        );
        omsEvent.repeatable = true;
        omsEvent.cooldownTurns = 25;
        randomEvents.Add(omsEvent);

        // ─── Crise de l'Eau ───────────────────────────────────────────────────
        randomEvents.Add(new Event(
            "Crise de l'Eau",
            "Une pénurie d'eau potable frappe le pays avec les pires conditions sanitaires.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.hygiene < 0.6f),
            () =>
            {
                CountryObject target = null;
                float minHyg = float.MaxValue;
                foreach (var c in gameManager.countries)
                    if (c.hygiene < minHyg) { minHyg = c.hygiene; target = c; }
                if (target == null) return;
                float drop = UnityEngine.Random.Range(0.05f, 0.10f);
                target.hygiene = Mathf.Max(0f, target.hygiene - drop);
                ShowEventNotification("Crise de l'Eau",
                    $"L'accès à l'eau potable s'effondre à {target.name} !\nL'hygiène chute de {drop * 100f:F0}% — terrain idéal pour le virus.");
            }
        ));

        // ─── Grève des Hôpitaux ───────────────────────────────────────────────
        var hospitalStrikeEvent = new Event(
            "Grève des Hôpitaux",
            "Le personnel médical surchargé déclenche une grève dans le pays le plus riche.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.wealth >= 0.6f),
            () =>
            {
                CountryObject target = null;
                float maxWealth = -1f;
                foreach (var c in gameManager.countries)
                    if (c.wealth > maxWealth) { maxWealth = c.wealth; target = c; }
                if (target == null) return;
                float drop = UnityEngine.Random.Range(0.05f, 0.10f);
                target.drugResistance = Mathf.Max(0f, target.drugResistance - drop);
                ShowEventNotification("Grève des Hôpitaux",
                    $"Les médecins et infirmiers de {target.name} quittent leur poste !\nLa résistance aux traitements chute de {drop * 100f:F0}%.");
            }
        );
        hospitalStrikeEvent.repeatable = true;
        hospitalStrikeEvent.cooldownTurns = 20;
        randomEvents.Add(hospitalStrikeEvent);

        // ─── Aide Internationale ──────────────────────────────────────────────
        var aidEvent = new Event(
            "Aide Internationale",
            "Le G7 vote un plan d'aide d'urgence pour le pays le plus démuni.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.wealth < 0.4f),
            () =>
            {
                CountryObject target = null;
                float minWealth = float.MaxValue;
                foreach (var c in gameManager.countries)
                    if (c.wealth < minWealth) { minWealth = c.wealth; target = c; }
                if (target == null) return;
                float wBoost = UnityEngine.Random.Range(0.03f, 0.07f);
                float dBoost = UnityEngine.Random.Range(0.03f, 0.06f);
                target.wealth = Mathf.Min(1f, target.wealth + wBoost);
                target.drugResistance = Mathf.Min(1f, target.drugResistance + dBoost);
                ShowEventNotification("Aide Internationale",
                    $"Le G7 mobilise des fonds d'urgence pour {target.name} !\n• Richesse : +{wBoost * 100f:F0}%\n• Résistance médicale : +{dBoost * 100f:F0}%");
            }
        );
        aidEvent.repeatable = true;
        aidEvent.cooldownTurns = 25;
        randomEvents.Add(aidEvent);

        // ─── Révolution ───────────────────────────────────────────────────────
        randomEvents.Add(new Event(
            "Révolution",
            "Une insurrection populaire renverse le gouvernement du pays le plus riche.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.wealth >= 0.65f),
            () =>
            {
                CountryObject target = null;
                float maxWealth = -1f;
                foreach (var c in gameManager.countries)
                    if (c.wealth >= 0.65f && c.wealth > maxWealth) { maxWealth = c.wealth; target = c; }
                if (target == null) return;
                float wDrop = UnityEngine.Random.Range(0.07f, 0.14f);
                float dDrop = UnityEngine.Random.Range(0.05f, 0.10f);
                target.wealth = Mathf.Max(0f, target.wealth - wDrop);
                target.drugResistance = Mathf.Max(0f, target.drugResistance - dDrop);
                ShowEventNotification("Révolution",
                    $"Le gouvernement de {target.name} s'effondre sous la pression populaire !\n• Économie : -{wDrop * 100f:F0}%\n• Système de santé : -{dDrop * 100f:F0}%");
            }
        ));

        // ─── Mutation Virale ──────────────────────────────────────────────────
        var mutationEvent = new Event(
            "Mutation Virale",
            "Le virus développe de nouveaux mécanismes de transmission.",
            Event.EventType.Random,
            null,
            () =>
            {
                float boost = UnityEngine.Random.Range(0.01f, 0.03f);
                gameManager.virus.infectivity = Mathf.Min(1f, gameManager.virus.infectivity + boost);
                ShowEventNotification("Mutation Virale",
                    $"Une nouvelle souche plus virulente est détectée !\nContagiosité du virus : +{boost * 100f:F1}%\nLes autorités sanitaires sont en alerte.");
            }
        );
        mutationEvent.repeatable = true;
        mutationEvent.cooldownTurns = 15;
        randomEvents.Add(mutationEvent);

        // ─── Percée Scientifique ──────────────────────────────────────────────
        randomEvents.Add(new Event(
            "Percée Scientifique",
            "Une équipe de chercheurs fait une découverte majeure sur la structure du virus.",
            Event.EventType.Random,
            () => vaccineEventIndex >= 5 && gameManager.virus.isVaccineResearchActive && gameManager.virus.vaccinePreparationProgress < 85f,
            () =>
            {
                float progress = UnityEngine.Random.Range(5f, 12f);
                float rateBoost = UnityEngine.Random.Range(1.03f, 1.10f);
                gameManager.virus.vaccinePreparationProgress = Mathf.Min(99f,
                    gameManager.virus.vaccinePreparationProgress + progress);
                gameManager.virus.vaccineBasePrepRate *= rateBoost;
                ShowEventNotification("Percée Scientifique",
                    $"Un article majeur bouleverse la recherche vaccinale !\n" +
                    $"• Avancement immédiat : +{progress:F0}%\n" +
                    $"• Vitesse de recherche : +{(rateBoost - 1f) * 100f:F0}%");
            }
        ));

        // ─── Sabotage de Laboratoire ──────────────────────────────────────────
        randomEvents.Add(new Event(
            "Sabotage de Laboratoire",
            "Un acte de sabotage détruit des mois de travail dans un laboratoire clé.",
            Event.EventType.Random,
            () => vaccineEventIndex >= 5 && gameManager.virus.vaccinePreparationProgress >= 30f,
            () =>
            {
                float setback = UnityEngine.Random.Range(4f, 10f);
                float rateDrop = UnityEngine.Random.Range(0.88f, 0.96f);
                gameManager.virus.vaccinePreparationProgress = Mathf.Max(0f,
                    gameManager.virus.vaccinePreparationProgress - setback);
                gameManager.virus.vaccineBasePrepRate *= rateDrop;
                ShowEventNotification("Sabotage de Laboratoire",
                    $"Un incendie criminel ravage les installations de recherche !\n" +
                    $"• Régression : -{setback:F0}% de progression\n" +
                    $"• Vitesse de recherche : -{(1f - rateDrop) * 100f:F0}%");
            }
        ));

        // ─── Quarantaine Volontaire ────────────────────────────────────────────
        var quarantineEvent = new Event(
            "Quarantaine Volontaire",
            "Sous pression populaire, un pays décide de se confiner.",
            Event.EventType.Random,
            () => vaccineEventIndex >= 5,
            () =>
            {
                // Cible le 2ème pays le plus infecté (pas le premier, déjà visé par Fermeture)
                CountryObject target = null;
                float maxRate = -1f, secondMax = -1f;
                foreach (var c in gameManager.countries)
                {
                    if (c.population.total == 0) continue;
                    float rate = (float)c.population.infected / c.population.total;
                    if (rate > maxRate) { secondMax = maxRate; target = null; maxRate = rate; }
                    else if (rate > secondMax) { secondMax = rate; target = c; }
                }
                if (target == null)
                {
                    // Fallback: pays le plus infecté
                    foreach (var c in gameManager.countries)
                    {
                        if (c.population.total == 0) continue;
                        float rate = (float)c.population.infected / c.population.total;
                        if (rate > secondMax) { secondMax = rate; target = c; }
                    }
                }
                if (target == null) return;
                int turns = UnityEngine.Random.Range(3, 7);
                target.borderClosedTurns += turns;
                ShowEventNotification("Quarantaine Volontaire",
                    $"La population de {target.name} réclame un confinement immédiat !\nFrontières fermées pendant {turns} tours.");
            }
        );
        quarantineEvent.repeatable = true;
        quarantineEvent.cooldownTurns = 15;
        randomEvents.Add(quarantineEvent);

        // ─── Panique Boursière ────────────────────────────────────────────────
        var panicEvent = new Event(
            "Panique Boursière",
            "Les marchés s'effondrent face à l'ampleur de la pandémie.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.wealth >= 0.55f),
            () =>
            {
                float drop = UnityEngine.Random.Range(0.02f, 0.05f);
                int hit = 0;
                foreach (var c in gameManager.countries)
                    if (c.wealth >= 0.55f) { c.wealth = Mathf.Max(0f, c.wealth - drop); hit++; }
                ShowEventNotification("Panique Boursière",
                    $"Les bourses mondiales s'effondrent !\n{hit} pays riches perdent {drop * 100f:F0}% de richesse.\nLes budgets de santé sont revus à la baisse.");
            }
        );
        panicEvent.repeatable = true;
        panicEvent.cooldownTurns = 20;
        randomEvents.Add(panicEvent);

        // ─── Découverte d'un Traitement ───────────────────────────────────────
        randomEvents.Add(new Event(
            "Découverte d'un Traitement",
            "Un antiviral efficace est mis au point et distribué aux pays les plus pauvres.",
            Event.EventType.Random,
            null,
            () =>
            {
                float boost = UnityEngine.Random.Range(0.04f, 0.08f);
                int helped = 0;
                foreach (var c in gameManager.countries)
                    if (c.drugResistance < 0.5f && helped < 3) { c.drugResistance = Mathf.Min(1f, c.drugResistance + boost); helped++; }
                ShowEventNotification("Découverte d'un Traitement",
                    $"Un antiviral de nouvelle génération est distribué en urgence !\nRésistance médicale +{boost * 100f:F0}% dans {helped} pays défavorisés.");
            }
        ));

        // ─── Pollution Industrielle ───────────────────────────────────────────
        randomEvents.Add(new Event(
            "Pollution Industrielle",
            "Un scandale de pollution massive affecte les grandes puissances industrielles.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.wealth >= 0.6f),
            () =>
            {
                float drop = UnityEngine.Random.Range(0.04f, 0.08f);
                int hit = 0;
                foreach (var c in gameManager.countries)
                    if (c.wealth >= 0.6f && hit < 3) { c.hygiene = Mathf.Max(0f, c.hygiene - drop); hit++; }
                ShowEventNotification("Pollution Industrielle",
                    $"Des niveaux record de pollution sont détectés dans les grandes villes !\nHygiène -{drop * 100f:F0}% dans {hit} pays industrialisés.");
            }
        ));

        // ─── Conférence Mondiale sur la Pandémie ──────────────────────────────
        randomEvents.Add(new Event(
            "Conférence Mondiale sur la Pandémie",
            "Les chefs d'État se réunissent pour coordonner la réponse internationale.",
            Event.EventType.Random,
            () => vaccineEventIndex >= 5,
            () =>
            {
                float vaccMult = UnityEngine.Random.Range(0.05f, 0.12f);
                float hygieneBoost = UnityEngine.Random.Range(0.02f, 0.04f);
                gameManager.virus.vaccineGlobalMultiplier += vaccMult;
                foreach (var c in gameManager.countries)
                    c.hygiene = Mathf.Min(1f, c.hygiene + hygieneBoost);
                ShowEventNotification("Conférence Mondiale sur la Pandémie",
                    $"Un accord international historique est signé !\n" +
                    $"• Multiplicateur vaccin mondial : +{vaccMult * 100f:F0}%\n" +
                    $"• Hygiène dans tous les pays : +{hygieneBoost * 100f:F0}%");
            }
        ));

        // ─── Effondrement de l'Infrastructure ────────────────────────────────
        var collapseEvent = new Event(
            "Effondrement de l'Infrastructure",
            "Le système de santé du pays le plus infecté s'effondre sous la pression.",
            Event.EventType.Random,
            () => gameManager.countries.Exists(c => c.population.total > 0 && (float)c.population.infected / c.population.total >= 0.4f),
            () =>
            {
                CountryObject target = null;
                float maxRate = -1f;
                foreach (var c in gameManager.countries)
                {
                    if (c.population.total == 0) continue;
                    float rate = (float)c.population.infected / c.population.total;
                    if (rate > maxRate) { maxRate = rate; target = c; }
                }
                if (target == null) return;
                float dDrop = UnityEngine.Random.Range(0.07f, 0.14f);
                float wDrop = UnityEngine.Random.Range(0.04f, 0.08f);
                target.drugResistance = Mathf.Max(0f, target.drugResistance - dDrop);
                target.wealth = Mathf.Max(0f, target.wealth - wDrop);
                ShowEventNotification("Effondrement de l'Infrastructure",
                    $"Les hôpitaux de {target.name} sont débordés, le système craque !\n" +
                    $"• Résistance médicale : -{dDrop * 100f:F0}%\n" +
                    $"• Économie : -{wDrop * 100f:F0}% (coûts d'urgence)");
            }
        );
        collapseEvent.repeatable = true;
        collapseEvent.cooldownTurns = 15;
        randomEvents.Add(collapseEvent);
    }

    private void AddGlobalEvents(GameManager gameManager)
    {
        triggerEvents.Add(new Event(
            "Lockdown Mondial",
            "La pandémie est hors de contrôle. Tous les pays ferment leurs frontières définitivement.",
            Event.EventType.Trigger,
            () =>
            {
                int totalPop = 0, totalInfected = 0;
                foreach (var c in gameManager.countries) { totalPop += c.population.total; totalInfected += c.population.infected; }
                return totalPop > 0 && (float)totalInfected / totalPop >= 0.85f;
            },
            () =>
            {
                foreach (var c in gameManager.countries)
                    c.borderPermanentlyClosed = true;
                ShowEventNotification(
                    "Lockdown Mondial",
                    "85% de la population mondiale est infectée !\nTous les gouvernements ferment leurs frontières définitivement."
                );
            }
        ));
    }

    private void AddVaccineEvents(GameManager gameManager)
    {
        // ─── Palier 0 — Première détection ────────────────────────────────────
        // Se déclenche dès qu'un pays dépasse 0.1% d'infectés (tout début de partie)
        triggerEvents.Add(new Event(
            "Nouvelle découverte !",
            "Un scientifique a découvert par hasard votre virus en étudiant des échantillons.",
            Event.EventType.Trigger,
            () =>
            {
                if (vaccineEventIndex != 0) return false;
                foreach (var c in gameManager.countries)
                    if (c.population.total > 0 && (float)c.population.infected / c.population.total >= 0.01f)
                        return true;
                return false;
            },
            () =>
            {
                ShowEventNotification(
                    "Nouvelle découverte !",
                    "Un scientifique a découvert par hasard votre virus en étudiant des échantillons.\nLes autorités minimisent pour l'instant la menace."
                );
                vaccineEventIndex++;
            },
            0
        ));

        // ─── Palier 1 — Début des études ──────────────────────────────────────
        // Se déclenche quand 0.5% de la population mondiale est infectée
        triggerEvents.Add(new Event(
            "Début des études",
            "Une équipe de scientifiques inquiète a commencé à étudier le virus à part entière.",
            Event.EventType.Trigger,
            () =>
            {
                if (vaccineEventIndex != 1) return false;
                int totalPop = 0, totalInfected = 0;
                foreach (var c in gameManager.countries) { totalPop += c.population.total; totalInfected += c.population.infected; }
                return totalPop > 0 && (float)totalInfected / totalPop >= 0.05f;
            },
            () =>
            {
                ShowEventNotification(
                    "Début des études",
                    "Les cas se multiplient à grande vitesse.\nUne équipe de scientifiques a obtenu des financements pour étudier le virus à part entière."
                );
                vaccineEventIndex++;
            },
            1
        ));

        // ─── Palier 2 — Menace sérieuse ───────────────────────────────────────
        // Se déclenche quand 5% de la population mondiale est infectée OU 500 000 morts
        triggerEvents.Add(new Event(
            "Le virus semble être une menace sérieuse...",
            "Des investissements conséquents sont en cours pour potentiellement développer un vaccin.",
            Event.EventType.Trigger,
            () =>
            {
                if (vaccineEventIndex != 2) return false;
                int totalPop = 0, totalInfected = 0, totalDead = 0;
                foreach (var c in gameManager.countries) { totalPop += c.population.total; totalInfected += c.population.infected; totalDead += c.population.dead; }
                return totalPop > 0 && ((float)totalInfected / totalPop >= 0.20f || totalDead >= 10000000);
            },
            () =>
            {
                int totalDead = 0;
                foreach (var c in gameManager.countries) totalDead += c.population.dead;
                ShowEventNotification(
                    "Le virus semble être une menace sérieuse...",
                    $"Avec {totalDead:N0} morts recensés, les gouvernements ne peuvent plus ignorer la situation.\nDes investissements conséquents sont débloqués pour développer un vaccin."
                );
                vaccineEventIndex++;
            },
            2
        ));

        // ─── Palier 3 — Lancement de la recherche vaccinale ───────────────────
        // Se déclenche quand 15% de la population mondiale est infectée OU 5M de morts
        triggerEvents.Add(new Event(
            "Un espoir pour l'humanité ?",
            "Plusieurs laboratoires ont commencé à créer un vaccin.",
            Event.EventType.Trigger,
            () =>
            {
                if (vaccineEventIndex != 3) return false;
                int totalPop = 0, totalInfected = 0, totalDead = 0;
                foreach (var c in gameManager.countries) { totalPop += c.population.total; totalInfected += c.population.infected; totalDead += c.population.dead; }
                return totalPop > 0 && ((float)totalInfected / totalPop >= 0.40f || totalDead >= 100000000);
            },
            () =>
            {
                gameManager.virus.isVaccineResearchActive = true;
                ShowEventNotification(
                    "Un espoir pour l'humanité ?",
                    "La pandémie atteint une ampleur sans précédent.\nPlusieurs laboratoires ont débuté la conception d'un vaccin en urgence.\nLa course contre la montre commence."
                );
                vaccineEventIndex++;
            },
            3
        ));

        // ─── Palier 4 — Coopération Mondiale ─────────────────────────────────
        // Se déclenche quand la recherche a atteint 25% de progression
        triggerEvents.Add(new Event(
            "Coopération Mondiale",
            "Tous les laboratoires du monde travaillent ensemble pour développer un vaccin.",
            Event.EventType.Trigger,
            () => vaccineEventIndex == 4 && gameManager.virus.vaccinePreparationProgress >= 60f,
            () =>
            {
                gameManager.virus.vaccineBasePrepRate *= 1.10f;
                gameManager.virus.vaccineGlobalMultiplier += 0.05f;
                ShowEventNotification(
                    "Coopération Mondiale",
                    "Face à l'ampleur de la catastrophe, tous les pays suspendent leurs rivalités.\nTous les laboratoires du monde partagent leurs données.\n• Vitesse de recherche : +50%\n• Multiplicateur global : +20%"
                );
                vaccineEventIndex++;
            },
            4
        ));
    }

    public void ProcessTurn(int turn, List<CountryObject> countries, Virus virus)
    {
        // Safety: re-init if lists are empty (execution order issue)
        if (randomEvents.Count == 0 && triggerEvents.Count == 0)
        {
            var gm = FindAnyObjectByType<GameManager>();
            if (gm != null) { Debug.Log("[EventManager] Lists empty, re-initializing events."); InitializeEvents(gm); }
        }

        Debug.Log($"[EventManager] Turn {turn} | eventActive={eventActive} | randoms={randomEvents.Count} | triggers={triggerEvents.Count} | Notification={(Notification.Instance != null ? "OK" : "NULL")}");

        if (turn <= 5) { Debug.Log("[EventManager] Skipped: grace period (turn <= 5)"); return; }

        if (eventActive) { Debug.Log("[EventManager] Skipped: eventActive is true"); return; }

        // Try random events first
        var eligibleRandoms = randomEvents.FindAll(e =>
            (e.repeatable ? (turn - e.lastTriggeredTurn >= e.cooldownTurns) : !e.triggered)
            && (e.condition == null || e.condition()));
        float roll = UnityEngine.Random.value;
        Debug.Log($"[EventManager] Eligible randoms: {eligibleRandoms.Count} | roll={roll:F2} vs chance={randomEventChance}");
        if (eligibleRandoms.Count > 0 && roll < randomEventChance)
        {
            int idx = UnityEngine.Random.Range(0, eligibleRandoms.Count);
            var evt = eligibleRandoms[idx];
            Debug.Log($"[EventManager] Triggering random: {evt.name}");
            evt.effect?.Invoke();
            if (evt.repeatable) evt.lastTriggeredTurn = turn;
            else evt.triggered = true;
            eventActive = true;
            return;
        }
        // Then try trigger events (ordered)
        foreach (var evt in triggerEvents)
        {
            if (!evt.triggered && (evt.condition == null || evt.condition()))
            {
                Debug.Log($"[EventManager] Triggering trigger: {evt.name}");
                evt.effect?.Invoke();
                evt.triggered = true;
                eventActive = true;
                break;
            }
        }
    }

    public void OnEventNotificationClosed()
    {
        eventActive = false;
    }

    private void ShowEventNotification(string title, string message)
    {
        Notification.Instance.ShowEvent(title, message);
    }
}
