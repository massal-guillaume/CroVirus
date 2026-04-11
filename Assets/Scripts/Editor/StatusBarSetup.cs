using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class StatusBarSetup
{
    [MenuItem("Tools/Setup Status Bar")]
    public static void SetupStatusBar()
    {
        // Get or create MainScene
        var mainScene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);
        
        // Get existing Canvas
        var canvasGO = GameObject.Find("UIRoot");
        if (canvasGO == null)
        {
            Debug.LogError("UIRoot Canvas not found! Create the Country Popup first.");
            return;
        }
        
        Canvas canvas = canvasGO.GetComponent<Canvas>();

        // Cleanup existing StatusBar if any
        var existingStatusBar = GameObject.Find("StatusBar");
        if (existingStatusBar != null)
        {
            // Supprimer aussi les children potentiels
            for (int i = existingStatusBar.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(existingStatusBar.transform.GetChild(i).gameObject);
            }
            Object.DestroyImmediate(existingStatusBar);
        }

        // Create StatusBar Panel - FIXE EN BAS
        var statusBarGO = new GameObject("StatusBar");
        statusBarGO.transform.SetParent(canvas.transform, false);
        statusBarGO.AddComponent<RectTransform>();
        
        var statusBarRect = statusBarGO.GetComponent<RectTransform>();
        statusBarRect.anchorMin = new Vector2(0, 0);  // En bas à gauche
        statusBarRect.anchorMax = new Vector2(1, 0);  // En bas à droite (stretch horizontal)
        statusBarRect.offsetMin = new Vector2(0, 0);
        statusBarRect.offsetMax = new Vector2(0, 80); // Hauteur 80px
        
        // Background image
        var bgImage = statusBarGO.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Gris foncé semi-transparent

        // Ajouter le script StatusBar
        var statusBarScript = statusBarGO.AddComponent<StatusBar>();

        // ─── LEFT PANEL (Infectés) ───────────────────────────
        var leftGO = new GameObject("LeftPanel");
        leftGO.transform.SetParent(statusBarGO.transform, false);
        
        var leftRect = leftGO.AddComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0, 0);
        leftRect.anchorMax = new Vector2(0.3f, 1);
        leftRect.offsetMin = Vector2.zero;
        leftRect.offsetMax = Vector2.zero;

        var infectedText = leftGO.AddComponent<TextMeshProUGUI>();
        infectedText.text = "Infectés: 0";
        infectedText.fontSize = 28;
        infectedText.color = Color.white;
        infectedText.fontStyle = FontStyles.Bold;
        infectedText.alignment = TextAlignmentOptions.MidlineLeft;

        var infectedTextRect = infectedText.GetComponent<RectTransform>();
        infectedTextRect.anchorMin = Vector2.zero;
        infectedTextRect.anchorMax = Vector2.one;
        infectedTextRect.offsetMin = new Vector2(20, 0);
        infectedTextRect.offsetMax = new Vector2(-20, 0);

        // ─── RIGHT PANEL (Morts) ────────────────────────────
        var rightGO = new GameObject("RightPanel");
        rightGO.transform.SetParent(statusBarGO.transform, false);
        
        var rightRect = rightGO.AddComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.7f, 0);
        rightRect.anchorMax = new Vector2(1, 1);
        rightRect.offsetMin = Vector2.zero;
        rightRect.offsetMax = Vector2.zero;

        var deadText = rightGO.AddComponent<TextMeshProUGUI>();
        deadText.text = "Morts: 0";
        deadText.fontSize = 28;
        deadText.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Gris foncé pour les morts
        deadText.fontStyle = FontStyles.Bold;
        deadText.alignment = TextAlignmentOptions.MidlineRight;

        var deadTextRect = deadText.GetComponent<RectTransform>();
        deadTextRect.anchorMin = Vector2.zero;
        deadTextRect.anchorMax = Vector2.one;
        deadTextRect.offsetMin = new Vector2(20, 0);
        deadTextRect.offsetMax = new Vector2(-20, 0);

        // ─── CENTER PROGRESS BAR ─────────────────────────────
        var centerContainerGO = new GameObject("ProgressContainer");
        centerContainerGO.transform.SetParent(statusBarGO.transform, false);
        
        var centerRect = centerContainerGO.AddComponent<RectTransform>();
        centerRect.anchorMin = new Vector2(0.3f, 0.25f);
        centerRect.anchorMax = new Vector2(0.7f, 0.75f);
        centerRect.offsetMin = Vector2.zero;
        centerRect.offsetMax = Vector2.zero;

        // Background de la barre ROUGE (infectés - visible par-dessous)
        var barBgGO = new GameObject("BarBackground");
        barBgGO.transform.SetParent(centerContainerGO.transform, false);
        barBgGO.AddComponent<RectTransform>();
        
        var barBgRect = barBgGO.GetComponent<RectTransform>();
        barBgRect.anchorMin = Vector2.zero;
        barBgRect.anchorMax = Vector2.one;
        barBgRect.offsetMin = Vector2.zero;
        barBgRect.offsetMax = Vector2.zero;

        var barBgImage = barBgGO.AddComponent<Image>();
        barBgImage.color = new Color(1f, 0.2f, 0.2f, 1f); // ROUGE = infectés

        // Fill de la barre BLEUE (sains - se réduit de gauche à droite)
        var barFillGO = new GameObject("BarFill");
        barFillGO.transform.SetParent(centerContainerGO.transform, false);
        
        var barFillRect = barFillGO.AddComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = Vector2.one; // Commence 100% bleu
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;

        var barFillImage = barFillGO.AddComponent<Image>();
        barFillImage.color = new Color(0.2f, 0.4f, 1f, 1f); // BLEU = sains par-dessus

        // Barre NOIRE (morts) - overlay par-dessus le rouge ET le bleu
        var barDeadGO = new GameObject("BarDead");
        barDeadGO.transform.SetParent(centerContainerGO.transform, false);
        
        var barDeadRect = barDeadGO.AddComponent<RectTransform>();
        barDeadRect.anchorMin = Vector2.zero;
        barDeadRect.anchorMax = new Vector2(0, 1); // Commence invisible
        barDeadRect.offsetMin = Vector2.zero;
        barDeadRect.offsetMax = Vector2.zero;

        var barDeadImage = barDeadGO.AddComponent<Image>();
        barDeadImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // NOIR = morts

        // Texte % BLEU (sains)
        var healthyPercentGO = new GameObject("HealthyPercent");
        healthyPercentGO.transform.SetParent(centerContainerGO.transform, false);
        
        var healthyPercentText = healthyPercentGO.AddComponent<TextMeshProUGUI>();
        healthyPercentText.text = "100%";
        healthyPercentText.fontSize = 20;
        healthyPercentText.color = Color.white;
        healthyPercentText.fontStyle = FontStyles.Bold;
        healthyPercentText.alignment = TextAlignmentOptions.Center;

        var healthyPercentRect = healthyPercentGO.GetComponent<RectTransform>();
        healthyPercentRect.anchorMin = new Vector2(0.6f, 0.3f);
        healthyPercentRect.anchorMax = new Vector2(1, 0.7f);
        healthyPercentRect.offsetMin = Vector2.zero;
        healthyPercentRect.offsetMax = Vector2.zero;

        // Texte % ROUGE (infectés)
        var infectedPercentGO = new GameObject("InfectedPercent");
        infectedPercentGO.transform.SetParent(centerContainerGO.transform, false);
        
        var infectedPercentText = infectedPercentGO.AddComponent<TextMeshProUGUI>();
        infectedPercentText.text = "0%";
        infectedPercentText.fontSize = 20;
        infectedPercentText.color = Color.white;
        infectedPercentText.fontStyle = FontStyles.Bold;
        infectedPercentText.alignment = TextAlignmentOptions.Center;

        var infectedPercentRect = infectedPercentGO.GetComponent<RectTransform>();
        infectedPercentRect.anchorMin = new Vector2(0, 0.3f);
        infectedPercentRect.anchorMax = new Vector2(0.4f, 0.7f);
        infectedPercentRect.offsetMin = Vector2.zero;
        infectedPercentRect.offsetMax = Vector2.zero;

        // Texte % NOIR (morts)
        var deadPercentGO = new GameObject("DeadPercent");
        deadPercentGO.transform.SetParent(centerContainerGO.transform, false);
        
        var deadPercentText = deadPercentGO.AddComponent<TextMeshProUGUI>();
        deadPercentText.text = "0%";
        deadPercentText.fontSize = 20;
        deadPercentText.color = Color.white;
        deadPercentText.fontStyle = FontStyles.Bold;
        deadPercentText.alignment = TextAlignmentOptions.Center;

        var deadPercentRect = deadPercentGO.GetComponent<RectTransform>();
        deadPercentRect.anchorMin = new Vector2(0.25f, 0.3f);
        deadPercentRect.anchorMax = new Vector2(0.75f, 0.7f);
        deadPercentRect.offsetMin = Vector2.zero;
        deadPercentRect.offsetMax = Vector2.zero;

        // Assigner les références au script StatusBar via reflection
        var infectedField = typeof(StatusBar).GetField("infectedText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        infectedField?.SetValue(statusBarScript, infectedText);
        
        var deadField = typeof(StatusBar).GetField("deadText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        deadField?.SetValue(statusBarScript, deadText);
        
        var fillField = typeof(StatusBar).GetField("progressBarFill",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fillField?.SetValue(statusBarScript, barFillImage);

        var deadBarField = typeof(StatusBar).GetField("progressBarDead",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        deadBarField?.SetValue(statusBarScript, barDeadImage);
        
        var infectedBarField = typeof(StatusBar).GetField("progressBarInfected",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        infectedBarField?.SetValue(statusBarScript, barBgImage);
        
        var healthyPercentField = typeof(StatusBar).GetField("healthyPercentText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        healthyPercentField?.SetValue(statusBarScript, healthyPercentText);
        
        var infectedPercentField = typeof(StatusBar).GetField("infectedPercentText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        infectedPercentField?.SetValue(statusBarScript, infectedPercentText);
        
        var deadPercentField = typeof(StatusBar).GetField("deadPercentText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        deadPercentField?.SetValue(statusBarScript, deadPercentText);

        // Save scene
        EditorSceneManager.SaveScene(mainScene);
        Debug.Log("✅ Status Bar created! It will show global infection statistics.");
    }
}
