using UnityEngine;

public static class CommandManager
{
    private static GameManager gameManager;

    public static void Initialize(GameManager gameManagerRef)
    {
        gameManager = gameManagerRef;
    }

    public static void ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        string command = input.ToLower().Trim();

        switch (command)
        {
            case "/pause":
                Pause();
                break;
            case "/resume":
                Resume();
                break;
            case "/fast":
                SetFastSpeed();
                break;
            case "/slow":
                SetSlowSpeed();
                break;
            case "/normal":
                SetNormalSpeed();
                break;
            case "/borders":
                BorderManager.DebugPrintBorders();
                break;
            default:
                Debug.LogWarning($"Commande inconnue: {command}");
                break;
        }
    }

    private static void Pause()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager non initialisé!");
            return;
        }
        gameManager.SetPaused(true);
        Debug.Log("Simulation PAUSÉE");
    }

    private static void Resume()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager non initialisé!");
            return;
        }
        gameManager.SetPaused(false);
        Debug.Log("Simulation REPRISE");
    }

    private static void SetFastSpeed()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager non initialisé!");
            return;
        }
        gameManager.SetSimulationSpeed(10f);  // 10 tours/seconde
        Debug.Log("Vitesse: RAPIDE (10x)");
    }

    private static void SetSlowSpeed()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager non initialisé!");
            return;
        }
        gameManager.SetSimulationSpeed(0.5f);  // 0.5 tours/seconde
        Debug.Log("Vitesse: LENTE (0.5x)");
    }

    private static void SetNormalSpeed()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager non initialisé!");
            return;
        }
        gameManager.SetSimulationSpeed(1f);  // 1 tour/seconde (normal)
        Debug.Log("Vitesse: NORMALE (1x)");
    }
}
