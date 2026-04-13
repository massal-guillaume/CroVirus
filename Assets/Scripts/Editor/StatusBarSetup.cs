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

        // Create StatusBar Panel - FIXE EN BAS (avec bordure noire)
        var statusBarGO = new GameObject("StatusBar");
        statusBarGO.transform.SetParent(canvas.transform, false);
        statusBarGO.AddComponent<RectTransform>();
        
        var statusBarRect = statusBarGO.GetComponent<RectTransform>();
        statusBarRect.anchorMin = new Vector2(0, 0);  // En bas à gauche
        statusBarRect.anchorMax = new Vector2(1, 0);  // En bas à droite (stretch horizontal)
        statusBarRect.offsetMin = new Vector2(0, 0);
        statusBarRect.offsetMax = new Vector2(0, 85); // Hauteur 85px (+ 5px bordure)
        
        // Background image BORDURE (NOIR)
        var bgImage = statusBarGO.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.05f, 1f); // NOIR pour bordure

        // Ajouter le script StatusBar
        var statusBarScript = statusBarGO.AddComponent<StatusBar>();

        // ─── CONTENT PANEL (padding pour créer la bordure noire) ─────
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(statusBarGO.transform, false);
        
        var contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(3, 3);  // Padding 3px pour bordure noire
        contentRect.offsetMax = new Vector2(-3, -3);

        // Background du contenu (gris foncé comme avant)
        var contentImage = contentGO.AddComponent<Image>();
        contentImage.color = new Color(0.13f, 0.13f, 0.18f, 0.95f); // Gris foncé semi-transparent un peu plus clair

        // ─── LEFT PANEL (Infectés) ───────────────────────────
        var leftGO = new GameObject("LeftPanel");
        leftGO.transform.SetParent(contentGO.transform, false);
        
        var leftRect = leftGO.AddComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0, 0);
        leftRect.anchorMax = new Vector2(0.24f, 1);
        leftRect.offsetMin = Vector2.zero;
        leftRect.offsetMax = Vector2.zero;

        var infectedText = leftGO.AddComponent<TextMeshProUGUI>();
        infectedText.text = "Infectés: 0";
        infectedText.fontSize = 28;
        infectedText.color = Color.red;
        infectedText.fontStyle = FontStyles.Bold;
        infectedText.alignment = TextAlignmentOptions.MidlineLeft;

        var infectedTextRect = infectedText.GetComponent<RectTransform>();
        infectedTextRect.anchorMin = Vector2.zero;
        infectedTextRect.anchorMax = Vector2.one;
        infectedTextRect.offsetMin = new Vector2(20, 0);
        infectedTextRect.offsetMax = new Vector2(-20, 0);

        // ─── DEAD PANEL (Morts) - à droite de la barre centrale ─────
        var deadPanelGO = new GameObject("DeadPanel");
        deadPanelGO.transform.SetParent(contentGO.transform, false);

        var deadPanelRect = deadPanelGO.AddComponent<RectTransform>();
        deadPanelRect.anchorMin = new Vector2(0.62f, 0);
        deadPanelRect.anchorMax = new Vector2(0.76f, 1);
        deadPanelRect.offsetMin = Vector2.zero;
        deadPanelRect.offsetMax = Vector2.zero;

        var deadLabelGO = new GameObject("DeadLabel");
        deadLabelGO.transform.SetParent(deadPanelGO.transform, false);
        var deadLabelText = deadLabelGO.AddComponent<TextMeshProUGUI>();
        deadLabelText.text = "Mort";
        deadLabelText.fontSize = 25;
        deadLabelText.color = Color.black;
        deadLabelText.fontStyle = FontStyles.Bold;
        deadLabelText.alignment = TextAlignmentOptions.Bottom;
        var deadLabelRect = deadLabelGO.GetComponent<RectTransform>();
        deadLabelRect.anchorMin = new Vector2(0f, 0.52f);
        deadLabelRect.anchorMax = new Vector2(1f, 1f);
        deadLabelRect.offsetMin = new Vector2(4f, 0f);
        deadLabelRect.offsetMax = new Vector2(-4f, 0f);

        var deadValueGO = new GameObject("DeadValue");
        deadValueGO.transform.SetParent(deadPanelGO.transform, false);
        var deadText = deadValueGO.AddComponent<TextMeshProUGUI>();
        deadText.text = "0";
        deadText.fontSize = 22;
        deadText.color = Color.black;
        deadText.fontStyle = FontStyles.Bold;
        deadText.alignment = TextAlignmentOptions.Top;

        var deadTextRect = deadValueGO.GetComponent<RectTransform>();
        deadTextRect.anchorMin = new Vector2(0f, 0f);
        deadTextRect.anchorMax = new Vector2(1f, 0.52f);
        deadTextRect.offsetMin = new Vector2(4f, 0f);
        deadTextRect.offsetMax = new Vector2(-4f, 0f);

        // ─── CENTER PROGRESS BAR ─────────────────────────────
        var centerContainerGO = new GameObject("ProgressContainer");
        centerContainerGO.transform.SetParent(contentGO.transform, false);
        
        var centerRect = centerContainerGO.AddComponent<RectTransform>();
        centerRect.anchorMin = new Vector2(0.24f, 0.25f);
        centerRect.anchorMax = new Vector2(0.62f, 0.75f);
        centerRect.offsetMin = Vector2.zero;
        centerRect.offsetMax = Vector2.zero;

        // ─── RIGHT PANEL (Vaccin) ─────────────────────────────
        var vaccinePanelGO = new GameObject("VaccinePanel");
        vaccinePanelGO.transform.SetParent(contentGO.transform, false);

        var vaccinePanelRect = vaccinePanelGO.AddComponent<RectTransform>();
        vaccinePanelRect.anchorMin = new Vector2(0.76f, 0f);
        vaccinePanelRect.anchorMax = new Vector2(1f, 1f);
        vaccinePanelRect.offsetMin = Vector2.zero;
        vaccinePanelRect.offsetMax = new Vector2(-10f, 0f);

        var vaccineBarBgGO = new GameObject("VaccineBarBackground");
        vaccineBarBgGO.transform.SetParent(vaccinePanelGO.transform, false);
        var vaccineBarBgImage = vaccineBarBgGO.AddComponent<Image>();
        vaccineBarBgImage.color = new Color(0.08f, 0.16f, 0.32f, 1f);
        var vaccineBarBgRect = vaccineBarBgGO.GetComponent<RectTransform>();
        vaccineBarBgRect.anchorMin = new Vector2(0.05f, 0.2f);
        vaccineBarBgRect.anchorMax = new Vector2(0.95f, 0.8f);
        vaccineBarBgRect.offsetMin = Vector2.zero;
        vaccineBarBgRect.offsetMax = Vector2.zero;

        var vaccineBarFillGO = new GameObject("VaccineBarFill");
        vaccineBarFillGO.transform.SetParent(vaccineBarBgGO.transform, false);
        var vaccineBarFillImage = vaccineBarFillGO.AddComponent<Image>();
        vaccineBarFillImage.color = new Color(0.18f, 0.48f, 1f, 1f);
        vaccineBarFillImage.type = Image.Type.Filled;
        vaccineBarFillImage.fillMethod = Image.FillMethod.Horizontal;
        vaccineBarFillImage.fillOrigin = 0;
        vaccineBarFillImage.fillAmount = 0f;
        var vaccineBarFillRect = vaccineBarFillGO.GetComponent<RectTransform>();
        vaccineBarFillRect.anchorMin = Vector2.zero;
        vaccineBarFillRect.anchorMax = Vector2.one;
        vaccineBarFillRect.offsetMin = Vector2.zero;
        vaccineBarFillRect.offsetMax = Vector2.zero;

        var vaccinePercentGO = new GameObject("VaccinePercent");
        vaccinePercentGO.transform.SetParent(vaccineBarBgGO.transform, false);
        var vaccinePercentText = vaccinePercentGO.AddComponent<TextMeshProUGUI>();
        vaccinePercentText.text = "Vaccin: 0.0%";
        vaccinePercentText.fontSize = 20;
        vaccinePercentText.color = Color.white;
        vaccinePercentText.fontStyle = FontStyles.Bold;
        vaccinePercentText.alignment = TextAlignmentOptions.Center;
        var vaccinePercentRect = vaccinePercentGO.GetComponent<RectTransform>();
        vaccinePercentRect.anchorMin = Vector2.zero;
        vaccinePercentRect.anchorMax = Vector2.one;
        vaccinePercentRect.offsetMin = Vector2.zero;
        vaccinePercentRect.offsetMax = Vector2.zero;

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

        // Fill de la barre VERTE (sains - se réduit de gauche à droite)
        var barFillGO = new GameObject("BarFill");
        barFillGO.transform.SetParent(centerContainerGO.transform, false);
        
        var barFillRect = barFillGO.AddComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = Vector2.one; // Commence 100% bleu
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;

        var barFillImage = barFillGO.AddComponent<Image>();
        barFillImage.color = new Color(0.2f, 0.8f, 0.3f, 1f); // VERT = sains par-dessus

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

        var vaccineProgressTextField = typeof(StatusBar).GetField("vaccineProgressText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        vaccineProgressTextField?.SetValue(statusBarScript, vaccinePercentText);

        var vaccineProgressFillField = typeof(StatusBar).GetField("vaccineProgressFill",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        vaccineProgressFillField?.SetValue(statusBarScript, vaccineBarFillImage);

        // Save scene
        EditorSceneManager.SaveScene(mainScene);
        Debug.Log("✅ Status Bar created! It will show global infection statistics.");
    }
}
