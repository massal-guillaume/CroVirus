using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class UISetup
{
    [MenuItem("Tools/Setup Country Popup")]
    public static void SetupCountryPopup()
    {
        // Get or create MainScene
        var mainScene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);
        
        // Cleanup existing UI elements if any
        var existingPopup = GameObject.Find("CountryInfoPopupPanel");
        if (existingPopup != null)
            Object.DestroyImmediate(existingPopup);
        
        var existingCountriesPanel = GameObject.Find("CountriesPanel");
        if (existingCountriesPanel != null)
            Object.DestroyImmediate(existingCountriesPanel);

        // Create Canvas for UI
        var existingCanvas = GameObject.Find("UIRoot");
        Canvas canvas;
        RectTransform canvasRect;

        if (existingCanvas != null)
        {
            canvas = existingCanvas.GetComponent<Canvas>();
            canvasRect = existingCanvas.GetComponent<RectTransform>();
        }
        else
        {
            var canvasGO = new GameObject("UIRoot");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
        }

        // Create Country Info Popup Panel
        var popupPanelGO = new GameObject("CountryInfoPopupPanel");
        popupPanelGO.transform.SetParent(canvas.transform, false);
        popupPanelGO.AddComponent<RectTransform>();
        
        var popupRect = popupPanelGO.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.anchoredPosition = Vector2.zero;
        popupRect.sizeDelta = new Vector2(520, 450);
        
        var canvasGroupPopup = popupPanelGO.AddComponent<CanvasGroup>();
        canvasGroupPopup.alpha = 0f;
        canvasGroupPopup.interactable = false;
        canvasGroupPopup.blocksRaycasts = false;
        
        // Main background panel
        var popupImage = popupPanelGO.AddComponent<Image>();
        popupImage.color = new Color(0.98f, 0.98f, 0.98f, 1f); // Très léger gris-blanc
        
        // Red border outline - plus fin et élégant
        var popupOutline = popupPanelGO.AddComponent<Outline>();
        popupOutline.effectColor = new Color(0.9f, 0.1f, 0.1f, 1f); // Rouge un peu moins saturé
        popupOutline.effectDistance = new Vector2(8, 8); // Plus fin que 15

        // Create Title Bar with Red Background - PLUS GRAND
        var titleBarGO = new GameObject("TitleBar");
        titleBarGO.transform.SetParent(popupPanelGO.transform, false);
        titleBarGO.AddComponent<RectTransform>();
        
        var titleBarRect = titleBarGO.GetComponent<RectTransform>();
        titleBarRect.anchorMin = new Vector2(0, 1);
        titleBarRect.anchorMax = new Vector2(1, 1);
        titleBarRect.anchoredPosition = new Vector2(0, -35);
        titleBarRect.sizeDelta = new Vector2(0, 85); // Augmenté de 70 à 85
        
        var titleBarImage = titleBarGO.AddComponent<Image>();
        titleBarImage.color = new Color(0.9f, 0.1f, 0.1f, 1f); // Même rouge

        // Create Country Name Text in Title Bar - BIEN VISIBLE EN BLANC
        var countryNameText = CreatePopupText(titleBarGO, "CountryNameText", 
            Color.white, 52, Vector2.zero, TextAlignmentOptions.Center);
        var countryNameTextComponent = countryNameText;
        // Faire en sorte que le texte soit bold
        countryNameTextComponent.fontStyle = FontStyles.Bold;
        // Mettre le texte à remplir toute la hauteur de la barre
        var countryNameRect = countryNameText.GetComponent<RectTransform>();
        countryNameRect.anchorMin = Vector2.zero;
        countryNameRect.anchorMax = Vector2.one;
        countryNameRect.offsetMin = Vector2.zero;
        countryNameRect.offsetMax = Vector2.zero;
        
        // Create Info Text Label Container avec padding
        var infoContainerGO = new GameObject("InfoContainer");
        infoContainerGO.transform.SetParent(popupPanelGO.transform, false);
        infoContainerGO.AddComponent<RectTransform>();
        
        var containerRect = infoContainerGO.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.08f, 0.05f);
        containerRect.anchorMax = new Vector2(0.92f, 0.75f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        // Create Info Texts avec espacement généreux
        var populationText = CreatePopupText(infoContainerGO, "PopulationText",
            new Color(0.2f, 0.2f, 0.2f, 1f), 24, new Vector2(0, 100), TextAlignmentOptions.Left);
        
        var infectedText = CreatePopupText(infoContainerGO, "InfectedText",
            new Color(0.2f, 0.2f, 0.2f, 1f), 24, new Vector2(0, 55), TextAlignmentOptions.Left);
        
        var deathText = CreatePopupText(infoContainerGO, "DeathText",
            new Color(0.2f, 0.2f, 0.2f, 1f), 24, new Vector2(0, 10), TextAlignmentOptions.Left);
        
        var immunityText = CreatePopupText(infoContainerGO, "ImmunityText",
            new Color(0.2f, 0.2f, 0.2f, 1f), 24, new Vector2(0, -35), TextAlignmentOptions.Left);

        // Attach CountryInfoPopup script and assign references
        var popupScript = popupPanelGO.AddComponent<CountryInfoPopup>();
        
        // Use reflection to set private SerializeFields
        var popupCanvasGroupField = typeof(CountryInfoPopup).GetField("popupCanvasGroup", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        popupCanvasGroupField?.SetValue(popupScript, canvasGroupPopup);
        
        var countryNameTextField = typeof(CountryInfoPopup).GetField("countryNameText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        countryNameTextField?.SetValue(popupScript, countryNameText);
        
        var populationTextField = typeof(CountryInfoPopup).GetField("populationText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        populationTextField?.SetValue(popupScript, populationText);
        
        var infectedTextField = typeof(CountryInfoPopup).GetField("infectedText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        infectedTextField?.SetValue(popupScript, infectedText);
        
        var deathTextField = typeof(CountryInfoPopup).GetField("deathText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        deathTextField?.SetValue(popupScript, deathText);
        
        var immunityTextField = typeof(CountryInfoPopup).GetField("immunityText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        immunityTextField?.SetValue(popupScript, immunityText);

        // Save scene
        EditorSceneManager.SaveScene(mainScene);
        Debug.Log("✅ Country Popup created! Click on countries in the map to open the popup.");
    }

    private static TextMeshProUGUI CreatePopupText(GameObject parent, string name, Color color, int fontSize, Vector2 position, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        var textGO = new GameObject(name);
        textGO.transform.SetParent(parent.transform, false);
        
        var textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = "...";
        textComponent.alignment = alignment;
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(400, 60);
        
        return textComponent;
    }
}

