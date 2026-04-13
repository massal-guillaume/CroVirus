using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;

public class CrotteEvolutionButtonSetup
{
    [MenuItem("Tools/Setup CrotteEvolution Button")]
    public static void SetupCrotteEvolutionButton()
    {
        var mainScene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);

        var canvasGO = GameObject.Find("UIRoot");
        if (canvasGO == null)
        {
            Debug.LogError("UIRoot Canvas not found! Create the UI first.");
            return;
        }

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("UIRoot exists but has no Canvas component.");
            return;
        }

        var existingButton = GameObject.Find("CrotteEvolutionButton");
        if (existingButton != null)
        {
            Object.DestroyImmediate(existingButton);
        }

        var buttonGO = new GameObject("CrotteEvolutionButton");
        buttonGO.transform.SetParent(canvas.transform, false);

        var buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.pivot = new Vector2(1, 1);
        buttonRect.sizeDelta = new Vector2(390, 72);
        buttonRect.anchoredPosition = new Vector2(-20, -20);

        var borderImage = buttonGO.AddComponent<Image>();
        borderImage.color = new Color(1f, 0.1f, 0.1f, 1f);

        // Safety border so the red frame stays clearly visible at any scale.
        var borderOutline = buttonGO.AddComponent<Outline>();
        borderOutline.effectColor = new Color(1f, 0.1f, 0.1f, 1f);
        borderOutline.effectDistance = new Vector2(1f, 1f);

        var button = buttonGO.AddComponent<Button>();
        button.targetGraphic = borderImage;

        var buttonScript = buttonGO.AddComponent<CrotteEvolutionButton>();

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(buttonGO.transform, false);

        var contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(2, 2);
        contentRect.offsetMax = new Vector2(-2, -2);

        var contentImage = contentGO.AddComponent<Image>();
        contentImage.color = new Color(0.13f, 0.13f, 0.13f, 1f);

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(contentGO.transform, false);

        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(15, 0);
        labelRect.offsetMax = new Vector2(-15, 0);
        labelRect.anchoredPosition = new Vector2(0f, -4f);

        var label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "Menu\nCrotte Virus Evolution";
        label.enableAutoSizing = false;
        label.fontSize = 26;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        label.lineSpacing = 20;
        label.color = Color.white;
        label.outlineWidth = 0.10f;
        label.outlineColor = new Color(1f, 0.1f, 0.1f, 1f);

        // Keep same visual style as points display in all states.
        var colors = button.colors;
        colors.normalColor        = new Color(0.13f, 0.13f, 0.13f, 1f);
        colors.highlightedColor   = new Color(0.13f, 0.13f, 0.13f, 1f);
        colors.pressedColor       = new Color(0.13f, 0.13f, 0.13f, 1f);
        colors.selectedColor      = new Color(0.13f, 0.13f, 0.13f, 1f);
        colors.disabledColor      = new Color(0.3f,  0.3f,  0.3f,  0.5f);
        colors.colorMultiplier    = 1f;
        colors.fadeDuration       = 0.1f;
        button.colors = colors;
        button.targetGraphic = contentImage;


        button.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(button.onClick, buttonScript.OpenSkillTreeMenu);
        EditorUtility.SetDirty(button);

        EditorSceneManager.SaveScene(mainScene);
        Debug.Log("✅ CrotteEvolution button created next to Crotogenes.");
    }
}
