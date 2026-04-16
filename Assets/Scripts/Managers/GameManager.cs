using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { Playing, Victory, Defeat, Healed }

public class GameManager : MonoBehaviour
{
    public void PauseGame()
    {
        isPaused = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
    }
    public List<CountryObject> countries = new List<CountryObject>();
    public Virus virus;
    public VirusType currentVirusType = VirusType.Cacastellaire;
    public int currentTurn = 0;
    public long initialWorldPopulation = 0;
    private float timeSinceLastTurn = 0f;
    private float simulationSpeed = 1f;  // Tours par seconde
    private bool isPaused = false;  // État de pause de la simulation
    private GameState gameState = GameState.Playing;  // État du jeu
    private PointManager pointManager;
    private SkillTreeManager skillTreeManager;
    private int vaccineCompletedTurnsRemaining = -1; // -1 = pas encore déclenché


    void Start()
    {
        // Initialiser PointManager
        InitializePointManager();
        // Initialiser SkillTreeManager
        InitializeSkillTreeManager();
        // Afficher le menu principal, puis laisser le joueur choisir son virus
        MainMenuUI.Show(() =>
        {
            VirusSelectionPanel.Show((chosenType, chosenCountry) =>
            {
                currentVirusType = chosenType;
                InitializeGame(chosenCountry);
                CommandManager.Initialize(this);
            });
        });
    }

    public void Restart()
    {
        // Reset static services
        TransmissionService.Reset();
        VaccineService.Reset();
        CountryManager.Reset();
        TransmissionSkillTree.Reset();
        MortaliteSkillTree.Reset();

        // Reset game fields
        currentTurn = 0;
        timeSinceLastTurn = 0f;
        simulationSpeed = 1f;
        isPaused = false;
        gameState = GameState.Playing;
        vaccineCompletedTurnsRemaining = -1;
        countries.Clear();
        virus = null;

        // Reset managers
        pointManager.Initialize();
        skillTreeManager.Initialize(pointManager, this);

        // Reset EventManager
        if (EventManager.Instance != null)
            EventManager.Instance.InitializeEvents(this);

        // Show selection screen again
        VirusSelectionPanel.Show((chosenType, chosenCountry) =>
        {
            currentVirusType = chosenType;
            InitializeGame(chosenCountry);
            CommandManager.Initialize(this);
        });
    }

    private void InitializePointManager()
    {
        pointManager = FindAnyObjectByType<PointManager>();
        if (pointManager == null)
        {
            GameObject pointManagerObj = new GameObject("PointManager");
            pointManager = pointManagerObj.AddComponent<PointManager>();
        }
        pointManager.Initialize();
        Debug.Log("PointManager initialisé");
    }
    
    private void InitializeSkillTreeManager()
    {
        skillTreeManager = FindAnyObjectByType<SkillTreeManager>();
        if (skillTreeManager == null)
        {
            GameObject skillTreeObj = new GameObject("SkillTreeManager");
            skillTreeManager = skillTreeObj.AddComponent<SkillTreeManager>();
        }
        skillTreeManager.Initialize(pointManager, this);
        Debug.Log("SkillTreeManager initialisé");
    }

    void Update()
    {
        // Gestion des raccourcis clavier pour les commandes
        HandleKeyboardInput();

        // Si jeu fini, arrêter la simulation
        if (gameState != GameState.Playing)
            return;

        if (isPaused)
            return;  // Si en pause, ne rien simuler

        timeSinceLastTurn += Time.deltaTime;
        
        if (virus == null || countries == null || countries.Count == 0)
            return;

        if (timeSinceLastTurn >= 1f / simulationSpeed)
        {
            SimulateTurn();
            timeSinceLastTurn = 0f;
        }
    }

    private void HandleKeyboardInput()
    {
        // P = Pause
        if (Keyboard.current.pKey.wasPressedThisFrame)
            CommandManager.ExecuteCommand("/pause");

        // R = Resume
        if (Keyboard.current.rKey.wasPressedThisFrame)
            CommandManager.ExecuteCommand("/resume");

        // F = Fast (4x)
        if (Keyboard.current.fKey.wasPressedThisFrame)
            CommandManager.ExecuteCommand("/fast");

        // S = Slow (0.25x)
        if (Keyboard.current.sKey.wasPressedThisFrame)
            CommandManager.ExecuteCommand("/slow");

        // N = Normal (1x)
        if (Keyboard.current.nKey.wasPressedThisFrame)
            CommandManager.ExecuteCommand("/normal");
    }

    public void InitializeGame(string patientZeroCountry = "France")
    {
        // Initialiser le CountryManager avec les 3 pays de départ
        CountryManager.Initialize();
        
        // Initialiser les frontières et liaisons entre pays
        BorderManager.Initialize();

        virus = currentVirusType switch
        {
            // La Crottance — virus polyvalent et équilibré, bonne résistance au froid
            VirusType.Classique => new Virus("La Crottance",
                infectivity:    0.12f,
                lethality:      0.00f,
                coldResistance: 0.70f,
                heatResistance: 0.45f)
            {
                hygieneExploitation = 0.08f,
                wealthExploitation  = 0.06f,
            },

            // Cacastellaire — excellent en aérien/international, mais fragile à la chaleur
            VirusType.Cacastellaire => new Virus("Cacastellaire",
                infectivity:    0.15f,
                lethality:      0.00f,
                coldResistance: 0.55f,
                heatResistance: 0.20f)
            {
                airborneModifier = 0.12f,
                seaModifier      = 0.06f,
                landModifier     = 0.04f,
            },

            // Nano Caca — très létal et furtif, mais se propage lentement
            VirusType.NanoCaca => new Virus("Nano Caca",
                infectivity:    0.07f,
                lethality:      0.1f,
                coldResistance: 0.50f,
                heatResistance: 0.50f)
            {
                drugResistance  = 0.15f,
                immunityBypass  = 0.10f,
            },

            // Fongi-Caca — propagation lente mais résistant aux médicaments, prospère dans les zones chaudes/humides
            VirusType.FongiCaca => new Virus("Fongi-Caca",
                infectivity:    0.08f,
                lethality:      0.00f,
                coldResistance: 0.30f,
                heatResistance: 0.80f)
            {
                drugResistance      = 0.25f,
                hygieneExploitation = 0.12f,
                infectionGrowthMultiplier = 0.05f,
            },

            _ => new Virus("La Crottance", 0.12f, 0.05f, 0.70f, 0.45f),
        };
    
        // Récupérer les pays depuis le CountryManager
        countries = CountryManager.GetAllCountries();

        // Mémoriser la population mondiale initiale (avant toute mort)
        initialWorldPopulation = 0;
        foreach (var c in countries) initialWorldPopulation += c.population.total;

        // Infecter le pays choisi comme patient 0
        var patientZero = countries.Find(c => c.name.Equals(patientZeroCountry, System.StringComparison.OrdinalIgnoreCase));
        if (patientZero != null)
            patientZero.population.infected = 1;
        else if (countries.Count > 0)
            countries[0].population.infected = 1;

        virus.isVaccineResearchActive = false;

        Debug.Log($"=== JEU COMMENCÉ === Patient 0 : {patientZeroCountry}");

        if (EventManager.Instance != null)
            EventManager.Instance.InitializeEvents(this);
    }


    //On simule un tour de jeu : 1) infections locales 2) voyages inter-pays 3) morts
    public void SimulateTurn()
    {
        currentTurn++;
        TransmissionService.SimulateSpread(countries, virus);
        TransmissionService.SimulateInterCountrySpread(countries);
        TransmissionService.SimulateMortality(countries, virus);
        VaccineService.SimulatePreparation(countries, virus);
        VaccineService.SimulateSpread(countries, virus);

        long totalInfected = TransmissionService.GetTotalInfected(countries);
        long totalDead     = TransmissionService.GetTotalDead(countries);

        // Compte à rebours vaccin 100%
        if (virus.isVaccineResearchActive && virus.vaccinePreparationProgress >= virus.VaccineMaxProgress)
        {
            if (vaccineCompletedTurnsRemaining < 0)
            {
                vaccineCompletedTurnsRemaining = 5;
                if (Notification.Instance != null)
                    Notification.Instance.Show("Vaccin finalisé !", "Le vaccin contre le virus est prêt. Il va être distribué en urgence à travers le monde entier pour sauver l'humanité.");
            }

            vaccineCompletedTurnsRemaining--;

            if (vaccineCompletedTurnsRemaining <= 0)
            {
                gameState = GameState.Healed;
                string virusName    = currentVirusType.ToString();
                EndScreenUI.Show(GameState.Healed, virusName, currentTurn, totalDead, totalInfected);
                return;
            }
        }

        // Event evaluation (generic event system)
        if (EventManager.Instance != null)
        {
            EventManager.Instance.ProcessTurn(currentTurn, countries, virus);
        }

        // Auto-unlock skills from Special virus bonuses
        if (skillTreeManager != null)
        {
            if (virus.autoTransmissionSkillChance > 0f || virus.autoTransmissionSkillTurnsLeft > 0)
                skillTreeManager.TryAutoUnlockTransmissionSkill(virus);

            if (virus.autoLethalSymptomChance > 0f || virus.autoLethalSymptomTurnsLeft > 0)
                skillTreeManager.TryAutoUnlockLethalSymptom(virus);
        }
        PrintStatus();
        
        // Générer les points passifs basés sur infections et morts
        if (pointManager != null)
        {
            pointManager.GeneratePoints(totalInfected, totalDead);

            long worldPopulation = 0;
            foreach (var c in countries) worldPopulation += c.population.total;
            pointManager.CheckMilestones(totalInfected, totalDead, worldPopulation, currentTurn);
        }

        // Boutons d'infection cliquables
        if (InfectionClickBonus.Instance != null)
            InfectionClickBonus.Instance.CheckNewInfections(countries, currentTurn);

        // Vérifier les conditions de victoire/défaite
        CheckGameConditions();
    }

    public void PrintStatus() { }

    // === CONDITIONS DE VICTOIRE/DÉFAITE ===

    private void CheckGameConditions()
    {
        if (gameState != GameState.Playing)
            return;

        long totalInfected  = TransmissionService.GetTotalInfected(countries);
        long totalDead       = TransmissionService.GetTotalDead(countries);
        long totalVaccinated = VaccineService.GetTotalVaccinated(countries);
        long totalPopulation = GetTotalPopulation();
        long totalAlive      = totalPopulation - totalDead;

        GameState newState = GameState.Playing;

        // Healed est géré par le compte à rebours vaccin dans SimulateTurn
        if (totalDead >= initialWorldPopulation)
        {
            newState = GameState.Victory;
        }
        else if (totalInfected == 0 && totalAlive > 0)
        {
            newState = GameState.Defeat;
        }

        if (newState != GameState.Playing)
        {
            gameState = newState;
            string virusName = currentVirusType.ToString();
            EndScreenUI.Show(newState, virusName, currentTurn, totalDead, totalInfected);
        }
    }

    private long GetTotalPopulation()
    {
        long total = 0;
        foreach (CountryObject country in countries)
            total += country.population.total;
        return total;
    }

    // === COMMANDES ===

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public void SetSimulationSpeed(float speed)
    {
        simulationSpeed = Mathf.Clamp(speed, 0.1f, 10f);  // Entre 0.1x et 10x
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public float GetSimulationSpeed()
    {
        return simulationSpeed;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public bool IsGameOver()
    {
        return gameState != GameState.Playing;
    }
}
