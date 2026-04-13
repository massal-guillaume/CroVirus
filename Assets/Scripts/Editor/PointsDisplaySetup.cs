using UnityEngine;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class PointsDisplaySetup
{
#if UNITY_EDITOR
    [MenuItem("Tools/Setup Points Display")]
    public static void SetupPointsDisplay()
    {
        // Get or create MainScene
        var mainScene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);
        
        // Get existing Canvas
        var canvasGO = GameObject.Find("UIRoot");
        if (canvasGO == null)
        {
            Debug.LogError("UIRoot Canvas not found! Create the Status Bar first.");
            return;
        }
        
        Canvas canvas = canvasGO.GetComponent<Canvas>();

        // Cleanup existing PointsDisplay if any
        var existingPointsDisplay = GameObject.Find("PointsDisplay");
        if (existingPointsDisplay != null)
        {
            Object.DestroyImmediate(existingPointsDisplay);
        }

        // Create PointsDisplay Panel - FIXE EN HAUT À GAUCHE
        var pointsDisplayGO = new GameObject("PointsDisplay");
        pointsDisplayGO.transform.SetParent(canvas.transform, false);
        pointsDisplayGO.AddComponent<RectTransform>();
        
        var pointsDisplayRect = pointsDisplayGO.GetComponent<RectTransform>();
        pointsDisplayRect.anchorMin = new Vector2(0, 1);    // Haut-gauche
        pointsDisplayRect.anchorMax = new Vector2(0, 1);    // Haut-gauche
        pointsDisplayRect.pivot = new Vector2(0, 1);        // Pivot haut-gauche
        pointsDisplayRect.anchoredPosition = new Vector2(20, -20);
        pointsDisplayRect.sizeDelta = new Vector2(300, 80);
        
        // Background image (BORDURE ROUGE INTENSE)
        var bgImage = pointsDisplayGO.AddComponent<Image>();
        bgImage.color = new Color(1f, 0.1f, 0.1f, 1f); // ROUGE INTENSE pour bordure — DA skill tree

        // Ajouter le script PointsDisplay
        var pointsDisplayScript = pointsDisplayGO.AddComponent<PointsDisplay>();

        // ─── CONTENT PANEL (padding pour créer la bordure) ─────
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(pointsDisplayGO.transform, false);
        
        var contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(3, 3);  // Padding 3px pour bordure rouge
        contentRect.offsetMax = new Vector2(-3, -3);

        // Background du contenu (DARK — DA skill tree)
        var contentImage = contentGO.AddComponent<Image>();
        contentImage.color = new Color(0.08f, 0.08f, 0.08f, 1f);

        // ─── POINTS TEXT ─────────────────────────────────────
        var pointsTextGO = new GameObject("PointsText");
        pointsTextGO.transform.SetParent(contentGO.transform, false);
        
        var pointsTextRect = pointsTextGO.AddComponent<RectTransform>();
        pointsTextRect.anchorMin = Vector2.zero;
        pointsTextRect.anchorMax = Vector2.one;
        pointsTextRect.offsetMin = new Vector2(15, 0);
        pointsTextRect.offsetMax = new Vector2(-15, 0);

        var pointsText = pointsTextGO.AddComponent<TextMeshProUGUI>();
        pointsText.text = "Crotogènes: Points";
        pointsText.fontSize = 32;
        pointsText.color = Color.white;  // BLANC — DA skill tree
        pointsText.fontStyle = FontStyles.Bold;
        pointsText.alignment = TextAlignmentOptions.MidlineLeft;
        pointsText.lineSpacing = 100;
        pointsText.outlineWidth = 0.15f;
        pointsText.outlineColor = new Color(1f, 0.1f, 0.1f, 1f);  // Outline rouge — DA skill tree

        // Assigner la référence au script via Reflection
        var pointsTextField = typeof(PointsDisplay).GetField("pointsText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        pointsTextField?.SetValue(pointsDisplayScript, pointsText);

        // Save scene
        EditorSceneManager.SaveScene(mainScene);
        Debug.Log("✅ Points Display créé! Il affichera les Crotogènes en haut-gauche.");
    }
#endif
}

