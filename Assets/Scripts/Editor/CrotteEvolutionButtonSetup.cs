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
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(0, 1);
        buttonRect.pivot = new Vector2(0, 1);
        buttonRect.sizeDelta = new Vector2(300, 80);
        buttonRect.anchoredPosition = new Vector2(340, -20);

        PositionRightOfPointsDisplay(buttonRect);

        var borderImage = buttonGO.AddComponent<Image>();
        borderImage.color = new Color(1f, 0.1f, 0.1f, 1f);

        var button = buttonGO.AddComponent<Button>();
        button.targetGraphic = borderImage;

        var buttonScript = buttonGO.AddComponent<CrotteEvolutionButton>();

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(buttonGO.transform, false);

        var contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(3, 3);
        contentRect.offsetMax = new Vector2(-3, -3);

        var contentImage = contentGO.AddComponent<Image>();
        contentImage.color = Color.white;

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(contentGO.transform, false);

        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8, 0);
        labelRect.offsetMax = new Vector2(-8, 0);

        var label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "CrotteEvolution";
        label.fontSize = 28;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.black;

        button.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(button.onClick, buttonScript.OpenSkillTreeMenu);
        EditorUtility.SetDirty(button);

        EditorSceneManager.SaveScene(mainScene);
        Debug.Log("✅ CrotteEvolution button created next to Crotogenes.");
    }

    private static void PositionRightOfPointsDisplay(RectTransform buttonRect)
    {
        var pointsDisplayGO = GameObject.Find("PointsDisplay");
        if (pointsDisplayGO == null)
            return;

        var pointsRect = pointsDisplayGO.GetComponent<RectTransform>();
        if (pointsRect == null)
            return;

        float x = pointsRect.anchoredPosition.x + pointsRect.sizeDelta.x + 15f;
        float y = pointsRect.anchoredPosition.y;
        buttonRect.anchoredPosition = new Vector2(x, y);
    }
}
