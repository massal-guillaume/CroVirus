using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { Playing, Victory, Defeat }

public class GameManager : MonoBehaviour
{
    public List<CountryObject> countries = new List<CountryObject>();
    public Virus virus;
    public int currentTurn = 0;
    private float timeSinceLastTurn = 0f;
    private float simulationSpeed = 1f;  // Tours par seconde
    private bool isPaused = false;  // État de pause de la simulation
    private GameState gameState = GameState.Playing;  // État du jeu
    private PointManager pointManager;
    private SkillTreeManager skillTreeManager;


    void Start()
    {
        // Initialiser PointManager
        InitializePointManager();
        // Initialiser SkillTreeManager
        InitializeSkillTreeManager();
        // Initialiser le jeu
        InitializeGame();
        CommandManager.Initialize(this);
    }

    private void InitializePointManager()
    {
        pointManager = FindObjectOfType<PointManager>();
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
        skillTreeManager = FindObjectOfType<SkillTreeManager>();
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

    public void InitializeGame()
    {
        // Initialiser le CountryManager avec les 3 pays de départ
        CountryManager.Initialize();
        
        // Initialiser les frontières et liaisons entre pays
        BorderManager.Initialize();

        // Créer le virus
        // Paramètres: nom, infectivity, lethality, coldResistance, heatResistance
        // infectivity 0.08 = R de 0.8 (progression contrôlée)
        virus = new Virus("COVID-20", 0.10f, 0.05f, 0.8f, 0.8f);  // Bon en froid, faible en chaud
    
        // Récupérer les pays depuis le CountryManager
        countries = CountryManager.GetAllCountries();

        // Infecter le premier pays
        countries[0].population.infected = 1;

        Debug.Log("=== JEU COMMENCÉ ===");
        PrintStatus();
    }


    //On simule un tour de jeu : 1) infections locales 2) voyages inter-pays 3) morts
    public void SimulateTurn()
    {
        currentTurn++;
        Debug.Log($"\n>>> SIMULATION DU TOUR {currentTurn} <<<");
        TransmissionService.SimulateSpread(countries, virus);
        TransmissionService.SimulateInterCountrySpread(countries);
        TransmissionService.SimulateMortality(countries, virus);
        PrintStatus();
        
        // Générer les points passifs basés sur infections et morts
        int totalInfected = TransmissionService.GetTotalInfected(countries);
        int totalDead = TransmissionService.GetTotalDead(countries);
        if (pointManager != null)
        {
            pointManager.GeneratePoints(totalInfected, totalDead);
        }
        
        // Vérifier les conditions de victoire/défaite
        CheckGameConditions();
    }

    public void PrintStatus()
    {
        Debug.Log($"\n--- TOUR {currentTurn} ---");
        Debug.Log($"Virus: {virus}");
        
        foreach (CountryObject country in countries)
        {
            Debug.Log(country);
        }

        int totalInfected = TransmissionService.GetTotalInfected(countries);
        int totalDead = TransmissionService.GetTotalDead(countries);
        Debug.Log($"\nTOTAL: Infectés = {totalInfected} | Morts = {totalDead}");
    }

    // === CONDITIONS DE VICTOIRE/DÉFAITE ===

    private void CheckGameConditions()
    {
        if (gameState != GameState.Playing)
            return;  // Déjà fini, ne rien faire

        int totalInfected = TransmissionService.GetTotalInfected(countries);
        int totalDead = TransmissionService.GetTotalDead(countries);
        int totalPopulation = GetTotalPopulation();
        int totalAlive = totalPopulation - totalDead;

        // VICTOIRE: Tout le monde est mort
        if (totalAlive == 0)
        {
            gameState = GameState.Victory;
            Debug.Log("\n=== VICTOIRE! Toute la population est décédée! ===");
            return;
        }

        // DÉFAITE: Plus d'infectés mais il y a des vivants
        if (totalInfected == 0 && totalAlive > 0)
        {
            gameState = GameState.Defeat;
            Debug.Log($"\n=== DÉFAITE! Le virus s'est éteint. {totalAlive} personnes ont survécu. ===");
            return;
        }
    }

    private int GetTotalPopulation()
    {
        int total = 0;
        foreach (CountryObject country in countries)
        {
            total += country.population.total;
        }
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
