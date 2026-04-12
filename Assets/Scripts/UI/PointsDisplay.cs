using UnityEngine;
using TMPro;

public class PointsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointsText;
    private PointManager pointManager;
    private bool pointManagerSearched = false;
    private bool outlineApplied = false;

    void Start()
    {
        if (pointsText == null)
        {
            Debug.LogWarning("PointsDisplay: pointsText not assigned in inspector!");
        }
    }

    void Update()
    {
        // Chercher PointManager si pas encore trouvé
        if (pointManager == null && !pointManagerSearched)
        {
            pointManager = FindObjectOfType<PointManager>();
            if (pointManager == null)
            {
                Debug.LogWarning("PointsDisplay: Searching for PointManager...");
                return;
            }
            else
            {
                Debug.Log("✅ PointsDisplay: PointManager found!");
                pointManagerSearched = true;
            }
        }

        if (pointManager == null || pointsText == null)
            return;

        // Appliquer outline une seule fois
        if (!outlineApplied)
        {
            pointsText.outlineWidth = 0.2f;
            pointsText.outlineColor = Color.black;
            outlineApplied = true;
        }

        UpdatePointsDisplay();
    }

    private void UpdatePointsDisplay()
    {
        int currentPoints = pointManager.GetCurrentPoints();
        pointsText.text = $"<b>Crotogènes: {currentPoints}</b>";
        pointsText.fontStyle = FontStyles.Bold;
    }
}

