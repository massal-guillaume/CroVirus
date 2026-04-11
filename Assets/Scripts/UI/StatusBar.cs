using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infectedText;
    [SerializeField] private TextMeshProUGUI deadText;
    [SerializeField] private Image progressBarDead;    // La partie noire (morts)
    [SerializeField] private Image progressBarInfected; // La partie rouge (infectés)
    [SerializeField] private Image progressBarFill;    // La partie bleue (sains)
    [SerializeField] private TextMeshProUGUI healthyPercentText;
    [SerializeField] private TextMeshProUGUI infectedPercentText;
    [SerializeField] private TextMeshProUGUI deadPercentText;
    
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (gameManager == null)
            return;

        UpdateStatusBar();
    }

    private void UpdateStatusBar()
    {
        int totalPopulation = GetTotalPopulation();
        int totalInfected = GetTotalInfected();
        int totalDead = GetTotalDead();

        // FORCER LES COULEURS BLANCHES POUR TOUS LES TEXTES
        if (infectedText != null)
        {
            infectedText.color = Color.white;
            infectedText.fontStyle = FontStyles.Bold;
        }
        if (deadText != null)
        {
            deadText.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            deadText.fontStyle = FontStyles.Bold;
        }
        if (deadPercentText != null)
        {
            deadPercentText.color = Color.white;
            deadPercentText.fontStyle = FontStyles.Bold;
        }
        if (infectedPercentText != null)
        {
            infectedPercentText.color = Color.white;
            infectedPercentText.fontStyle = FontStyles.Bold;
        }
        if (healthyPercentText != null)
        {
            healthyPercentText.color = Color.white;
            healthyPercentText.fontStyle = FontStyles.Bold;
        }

        // Afficher les stats à gauche et droite
        infectedText.text = $"Infectés: {totalInfected:N0}";
        deadText.text = $"Morts: {totalDead:N0}";

        // Calculer les taux
        float infectionRate = totalPopulation > 0 
            ? (float)totalInfected / totalPopulation 
            : 0f;
        
        float deathRate = totalPopulation > 0 
            ? (float)totalDead / totalPopulation 
            : 0f;
        
        float healthyRate = 1f - infectionRate - deathRate; // Sains = pop - infectés - morts
        healthyRate = Mathf.Clamp01(healthyRate);

        // ORDRE: NOIR | ROUGE | BLEU (de gauche à droite)
        
        // Barre NOIRE (morts): 0% → deathRate
        if (progressBarDead != null)
        {
            var deadRect = progressBarDead.GetComponent<RectTransform>();
            deadRect.anchorMin = new Vector2(0, 0);
            deadRect.anchorMax = new Vector2(deathRate, 1);
            deadRect.offsetMin = Vector2.zero;
            deadRect.offsetMax = Vector2.zero;
        }

        // Barre ROUGE (infectés): deathRate → deathRate + infectionRate
        if (progressBarInfected != null)
        {
            var infectedRect = progressBarInfected.GetComponent<RectTransform>();
            infectedRect.anchorMin = new Vector2(deathRate, 0);
            infectedRect.anchorMax = new Vector2(Mathf.Min(deathRate + infectionRate, 1f), 1);
            infectedRect.offsetMin = Vector2.zero;
            infectedRect.offsetMax = Vector2.zero;
        }

        // Barre BLEUE (sains): deathRate + infectionRate → 1
        if (progressBarFill != null)
        {
            var fillRect = progressBarFill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(deathRate + infectionRate, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        // Afficher les pourcentages sur chaque barre
        if (deadPercentText != null)
        {
            var deadPercentRect = deadPercentText.GetComponent<RectTransform>();
            if (deathRate > 0.07f)
            {
                deadPercentText.text = $"{deathRate * 100f:F1}%";
                deadPercentRect.anchorMin = new Vector2(0, 0.3f);
                deadPercentRect.anchorMax = new Vector2(deathRate, 0.7f);
            }
            else
            {
                deadPercentText.text = "";
                deadPercentRect.anchorMin = new Vector2(-1, -1);
                deadPercentRect.anchorMax = new Vector2(-0.5f, -0.5f);
            }
            deadPercentRect.offsetMin = Vector2.zero;
            deadPercentRect.offsetMax = Vector2.zero;
        }
        
        if (infectedPercentText != null)
        {
            var infectedPercentRect = infectedPercentText.GetComponent<RectTransform>();
            if (infectionRate > 0.07f)
            {
                infectedPercentText.text = $"{infectionRate * 100f:F1}%";
                infectedPercentRect.anchorMin = new Vector2(deathRate, 0.3f);
                infectedPercentRect.anchorMax = new Vector2(Mathf.Min(deathRate + infectionRate, 1f), 0.7f);
            }
            else
            {
                infectedPercentText.text = "";
                infectedPercentRect.anchorMin = new Vector2(-1, -1);
                infectedPercentRect.anchorMax = new Vector2(-0.5f, -0.5f);
            }
            infectedPercentRect.offsetMin = Vector2.zero;
            infectedPercentRect.offsetMax = Vector2.zero;
        }
        
        if (healthyPercentText != null)
        {
            var healthyPercentRect = healthyPercentText.GetComponent<RectTransform>();
            if (healthyRate > 0.07f)
            {
                healthyPercentText.text = $"{healthyRate * 100f:F1}%";
                healthyPercentRect.anchorMin = new Vector2(deathRate + infectionRate, 0.3f);
                healthyPercentRect.anchorMax = new Vector2(1f, 0.7f);
            }
            else
            {
                healthyPercentText.text = "";
                healthyPercentRect.anchorMin = new Vector2(-1, -1);
                healthyPercentRect.anchorMax = new Vector2(-0.5f, -0.5f);
            }
            healthyPercentRect.offsetMin = Vector2.zero;
            healthyPercentRect.offsetMax = Vector2.zero;
        }
    }

    private int GetTotalPopulation()
    {
        int total = 0;
        foreach (CountryObject country in gameManager.countries)
        {
            total += country.population.total;
        }
        return total;
    }

    private int GetTotalInfected()
    {
        int total = 0;
        foreach (CountryObject country in gameManager.countries)
        {
            total += country.population.infected;
        }
        return total;
    }

    private int GetTotalDead()
    {
        int total = 0;
        foreach (CountryObject country in gameManager.countries)
        {
            total += country.population.dead;
        }
        return total;
    }
}
