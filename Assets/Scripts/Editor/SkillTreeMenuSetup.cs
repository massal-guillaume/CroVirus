using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeMenuSetup
{
    [MenuItem("Tools/Setup Skill Tree Menu")]
    public static void SetupSkillTreeMenu()
    {
        var mainScene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);

        var canvasGO = GameObject.Find("UIRoot");
        if (canvasGO == null)
        {
            Debug.LogError("UIRoot Canvas not found! Create UI first.");
            return;
        }

        var existingMenu = GameObject.Find("SkillTreeMenu");
        if (existingMenu != null)
            Object.DestroyImmediate(existingMenu);

        var root = new GameObject("SkillTreeMenu");
        root.transform.SetParent(canvasGO.transform, false);

        var rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        var rootImage = root.AddComponent<Image>();
        rootImage.color = new Color(0.06f, 0.06f, 0.06f, 0.96f);

        var rootCanvasGroup = root.AddComponent<CanvasGroup>();
        rootCanvasGroup.alpha = 0f;
        rootCanvasGroup.interactable = false;
        rootCanvasGroup.blocksRaycasts = false;

        var script = root.AddComponent<SkillTreeMenuUI>();

        var closeButton = CreateButton(root.transform, "CloseButton", "X", 52);
        var closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(90f, 90f);
        closeRect.anchoredPosition = new Vector2(-10f, -10f);

        var closeImage = closeButton.GetComponent<Image>();
        if (closeImage != null)
            closeImage.color = new Color(0.86f, 0.12f, 0.12f, 1f);

        var topTabs = new GameObject("TopTabs");
        topTabs.transform.SetParent(root.transform, false);
        var topTabsRect = topTabs.AddComponent<RectTransform>();
        topTabsRect.anchorMin = new Vector2(0f, 1f);
        topTabsRect.anchorMax = new Vector2(1f, 1f);
        topTabsRect.pivot = new Vector2(0.5f, 1f);
        topTabsRect.sizeDelta = new Vector2(0f, 90f);
        topTabsRect.anchoredPosition = new Vector2(0f, 0f);

        var transmissionTab = CreateButton(topTabs.transform, "TransmissionTab", "Transmission", 34);
        var mortaliteTab = CreateButton(topTabs.transform, "MortaliteTab", "Mortalite", 34);
        var specialTab = CreateButton(topTabs.transform, "SpecialTab", "Special", 34);

        StretchThreeColumns(transmissionTab.GetComponent<RectTransform>(), mortaliteTab.GetComponent<RectTransform>(), specialTab.GetComponent<RectTransform>());

        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        var bodyRect = body.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(20f, 20f);
        bodyRect.offsetMax = new Vector2(-20f, -110f);

        var skillsArea = new GameObject("SkillsArea");
        skillsArea.transform.SetParent(body.transform, false);
        var skillsAreaRect = skillsArea.AddComponent<RectTransform>();
        skillsAreaRect.anchorMin = new Vector2(0f, 0f);
        skillsAreaRect.anchorMax = new Vector2(0.63f, 1f);
        skillsAreaRect.offsetMin = new Vector2(0f, 0f);
        skillsAreaRect.offsetMax = new Vector2(-10f, 0f);

        var skillsBg = skillsArea.AddComponent<Image>();
        skillsBg.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        
        // Red border around skills area (more flashy and thicker)
        var skillsOutline = skillsArea.AddComponent<Outline>();
        skillsOutline.effectColor = new Color(1f, 0.1f, 0.1f, 1f);  // Brighter red
        skillsOutline.effectDistance = new Vector2(4f, 4f);  // Thicker border

        // Add RectMask2D to skillsArea to clip all children at the border
        var skillsAreaMask = skillsArea.AddComponent<RectMask2D>();
        skillsAreaMask.softness = Vector2Int.zero;

        var connectionsLayer = CreateLayer(skillsArea.transform, "ConnectionsLayer");
        // No inset: both layers share the same rect so backdrop and nodes stay aligned
        connectionsLayer.offsetMin = Vector2.zero;
        connectionsLayer.offsetMax = Vector2.zero;
        var nodesLayer = CreateLayer(skillsArea.transform, "NodesLayer");
        nodesLayer.offsetMin = Vector2.zero;
        nodesLayer.offsetMax = Vector2.zero;

        // Add RectMask2D to nodes layer for proper clipping at edges
        var nodesMask = nodesLayer.gameObject.AddComponent<RectMask2D>();
        nodesMask.softness = Vector2Int.zero;

        var infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(body.transform, false);
        var infoRect = infoPanel.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.63f, 0f);
        infoRect.anchorMax = new Vector2(1f, 1f);
        infoRect.offsetMin = new Vector2(10f, 0f);
        infoRect.offsetMax = Vector2.zero;

        // ── PANEL BACKGROUND ─────────────────────────────────────────
        var infoBg = infoPanel.AddComponent<Image>();
        infoBg.color = new Color(0.04f, 0.04f, 0.04f, 1f);

        var infoPanelOutline = infoPanel.AddComponent<Outline>();
        infoPanelOutline.effectColor = new Color(1f, 0.1f, 0.1f, 1f);
        infoPanelOutline.effectDistance = new Vector2(4f, 4f);

        // ── TITLE BANNER ─────────────────────────────────────────────
        var titleBanner = new GameObject("TitleBanner");
        titleBanner.transform.SetParent(infoPanel.transform, false);
        var titleBannerRect = titleBanner.AddComponent<RectTransform>();
        titleBannerRect.anchorMin = new Vector2(0f, 1f);
        titleBannerRect.anchorMax = new Vector2(1f, 1f);
        titleBannerRect.pivot = new Vector2(0.5f, 1f);
        titleBannerRect.offsetMin = new Vector2(0f, -56f);
        titleBannerRect.offsetMax = new Vector2(0f, 0f);
        var titleBannerBg = titleBanner.AddComponent<Image>();
        titleBannerBg.color = new Color(0.22f, 0.02f, 0.02f, 1f);

        var infoTitle = CreateText(titleBanner.transform, "SkillName", "Skill Name", 36, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        var infoTitleRect = infoTitle.GetComponent<RectTransform>();
        infoTitleRect.anchorMin = Vector2.zero;
        infoTitleRect.anchorMax = Vector2.one;
        infoTitleRect.offsetMin = new Vector2(10f, 4f);
        infoTitleRect.offsetMax = new Vector2(-10f, -4f);

        // Thin bright-red separator below title banner
        var titleSep = new GameObject("TitleSeparator");
        titleSep.transform.SetParent(infoPanel.transform, false);
        var titleSepRect = titleSep.AddComponent<RectTransform>();
        titleSepRect.anchorMin = new Vector2(0.03f, 1f);
        titleSepRect.anchorMax = new Vector2(0.97f, 1f);
        titleSepRect.pivot = new Vector2(0.5f, 1f);
        titleSepRect.offsetMin = new Vector2(0f, -59f);
        titleSepRect.offsetMax = new Vector2(0f, -56f);
        titleSep.AddComponent<Image>().color = new Color(0.9f, 0.1f, 0.1f, 1f);

        // ── DESCRIPTION FRAME ────────────────────────────────────────
        var descFrame = new GameObject("DescriptionFrame");
        descFrame.transform.SetParent(infoPanel.transform, false);
        var descFrameRect = descFrame.AddComponent<RectTransform>();
        descFrameRect.anchorMin = new Vector2(0f, 0.51f);
        descFrameRect.anchorMax = new Vector2(1f, 0.87f);
        descFrameRect.offsetMin = new Vector2(10f, 0f);
        descFrameRect.offsetMax = new Vector2(-10f, 0f);
        descFrame.AddComponent<Image>().color = new Color(0.09f, 0.09f, 0.09f, 1f);
        var descFrameOutline = descFrame.AddComponent<Outline>();
        descFrameOutline.effectColor = new Color(0.6f, 0.08f, 0.08f, 0.9f);
        descFrameOutline.effectDistance = new Vector2(2f, 2f);

        var descLabel = CreateText(descFrame.transform, "DescLabel", "— DESCRIPTION —", 15, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.75f, 0.18f, 0.18f, 1f));
        var descLabelRect = descLabel.GetComponent<RectTransform>();
        descLabelRect.anchorMin = new Vector2(0f, 1f);
        descLabelRect.anchorMax = new Vector2(1f, 1f);
        descLabelRect.pivot = new Vector2(0.5f, 1f);
        descLabelRect.offsetMin = new Vector2(0f, -26f);
        descLabelRect.offsetMax = new Vector2(0f, -6f);

        var infoDescription = CreateText(descFrame.transform, "InfoDescription", "Selectionne un skill.", 23, FontStyles.Normal, TextAlignmentOptions.TopLeft, Color.white);
        var infoDescRect = infoDescription.GetComponent<RectTransform>();
        infoDescRect.anchorMin = Vector2.zero;
        infoDescRect.anchorMax = Vector2.one;
        infoDescRect.offsetMin = new Vector2(12f, 8f);
        infoDescRect.offsetMax = new Vector2(-12f, -30f);

        // ── IMPACT FRAME ─────────────────────────────────────────────
        var impactFrame = new GameObject("ImpactFrame");
        impactFrame.transform.SetParent(infoPanel.transform, false);
        var impactFrameRect = impactFrame.AddComponent<RectTransform>();
        impactFrameRect.anchorMin = new Vector2(0f, 0.32f);
        impactFrameRect.anchorMax = new Vector2(1f, 0.50f);
        impactFrameRect.offsetMin = new Vector2(10f, 0f);
        impactFrameRect.offsetMax = new Vector2(-10f, 0f);
        impactFrame.AddComponent<Image>().color = new Color(0.14f, 0.02f, 0.02f, 1f);
        var impactFrameOutline = impactFrame.AddComponent<Outline>();
        impactFrameOutline.effectColor = new Color(1f, 0.15f, 0.15f, 1f);
        impactFrameOutline.effectDistance = new Vector2(2f, 2f);

        var impactLabel = CreateText(impactFrame.transform, "ImpactLabel", "— IMPACT —", 15, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.35f, 0.35f, 1f));
        var impactLabelRect = impactLabel.GetComponent<RectTransform>();
        impactLabelRect.anchorMin = new Vector2(0f, 1f);
        impactLabelRect.anchorMax = new Vector2(1f, 1f);
        impactLabelRect.pivot = new Vector2(0.5f, 1f);
        impactLabelRect.offsetMin = new Vector2(0f, -26f);
        impactLabelRect.offsetMax = new Vector2(0f, -6f);

        var infoImpact = CreateText(impactFrame.transform, "ImpactText", "", 22, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(1f, 0.40f, 0.40f, 1f));
        var infoImpactRect = infoImpact.GetComponent<RectTransform>();
        infoImpactRect.anchorMin = Vector2.zero;
        infoImpactRect.anchorMax = Vector2.one;
        infoImpactRect.offsetMin = new Vector2(12f, 8f);
        infoImpactRect.offsetMax = new Vector2(-12f, -30f);

        // ── COST BAR ─────────────────────────────────────────────────
        var infoCostGO = CreateText(infoPanel.transform, "CostText", "", 19, FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.55f, 0.55f, 0.55f, 1f));
        var infoCostRect = infoCostGO.GetComponent<RectTransform>();
        infoCostRect.anchorMin = new Vector2(0f, 0.25f);
        infoCostRect.anchorMax = new Vector2(1f, 0.31f);
        infoCostRect.offsetMin = new Vector2(10f, 0f);
        infoCostRect.offsetMax = new Vector2(-10f, 0f);

        // ── BUY BUTTON ───────────────────────────────────────────────
        var buyButton = CreateButton(infoPanel.transform, "BuyButton", "ACHETER", 30);
        var buyRect = buyButton.GetComponent<RectTransform>();
        buyRect.anchorMin = new Vector2(0.06f, 0.07f);
        buyRect.anchorMax = new Vector2(0.94f, 0.23f);
        buyRect.pivot = new Vector2(0.5f, 0.5f);
        buyRect.offsetMin = Vector2.zero;
        buyRect.offsetMax = Vector2.zero;

        var buyImage = buyButton.GetComponent<Image>();
        if (buyImage != null)
            buyImage.color = new Color(0.50f, 0.04f, 0.04f, 1f);

        var buyOutline = buyButton.gameObject.AddComponent<Outline>();
        buyOutline.effectColor = new Color(1f, 0.22f, 0.22f, 1f);
        buyOutline.effectDistance = new Vector2(3f, 3f);

        var buyLabel = buyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buyLabel != null)
        {
            buyLabel.color = Color.white;
            buyLabel.fontStyle = FontStyles.Bold;
        }

        BindFields(script, rootCanvasGroup, closeButton, transmissionTab, mortaliteTab, specialTab,
            skillsAreaRect, connectionsLayer, nodesLayer, infoTitle, infoDescription, infoImpact, infoCostGO, buyButton, buyLabel);

        // Fix TMP shared material face color to white so vertex colors display exactly as set.
        // Without this, a red face color in the font asset tints all text red regardless of .color value.
        // Apply to all distinct font materials used in the panel.
        var allPanelTexts = infoPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        var patchedMaterials = new System.Collections.Generic.HashSet<Material>();
        foreach (var t in allPanelTexts)
        {
            if (t.fontSharedMaterial != null && patchedMaterials.Add(t.fontSharedMaterial))
            {
                t.fontSharedMaterial.SetColor("_FaceColor", Color.white);
                EditorUtility.SetDirty(t.fontSharedMaterial);
            }
        }

        // Ensure close button stays above tabs and all other elements.
        closeButton.transform.SetAsLastSibling();

        root.transform.SetAsLastSibling();

        EditorSceneManager.SaveScene(mainScene);
        Debug.Log("✅ Skill Tree Menu setup complete.");
    }

    private static RectTransform CreateLayer(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10f, 10f);
        rect.offsetMax = new Vector2(-10f, -10f);
        return rect;
    }

    private static Button CreateButton(Transform parent, string name, string label, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(180f, 70f);

        var image = go.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(6f, 0f);
        textRect.offsetMax = new Vector2(-6f, 0f);

        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        return button;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string value, int fontSize, FontStyles style, TextAlignmentOptions alignment, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300f, 100f);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = true;

        return text;
    }

    private static void StretchThreeColumns(RectTransform first, RectTransform second, RectTransform third)
    {
        first.anchorMin = new Vector2(0f, 0f);
        first.anchorMax = new Vector2(1f / 3f, 1f);
        first.offsetMin = Vector2.zero;
        first.offsetMax = Vector2.zero;

        second.anchorMin = new Vector2(1f / 3f, 0f);
        second.anchorMax = new Vector2(2f / 3f, 1f);
        second.offsetMin = Vector2.zero;
        second.offsetMax = Vector2.zero;

        third.anchorMin = new Vector2(2f / 3f, 0f);
        third.anchorMax = new Vector2(1f, 1f);
        third.offsetMin = Vector2.zero;
        third.offsetMax = Vector2.zero;
    }

    private static void BindFields(
        SkillTreeMenuUI script,
        CanvasGroup rootCanvasGroup,
        Button closeButton,
        Button transmission,
        Button mortalite,
        Button special,
        RectTransform skillsArea,
        RectTransform connections,
        RectTransform nodes,
        TextMeshProUGUI infoTitle,
        TextMeshProUGUI infoDescription,
        TextMeshProUGUI infoImpact,
        TextMeshProUGUI infoCost,
        Button buyButton,
        TextMeshProUGUI buyLabel)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        typeof(SkillTreeMenuUI).GetField("menuCanvasGroup", flags)?.SetValue(script, rootCanvasGroup);
        typeof(SkillTreeMenuUI).GetField("closeButton", flags)?.SetValue(script, closeButton);

        typeof(SkillTreeMenuUI).GetField("transmissionTabButton", flags)?.SetValue(script, transmission);
        typeof(SkillTreeMenuUI).GetField("mortaliteTabButton", flags)?.SetValue(script, mortalite);
        typeof(SkillTreeMenuUI).GetField("specialTabButton", flags)?.SetValue(script, special);

        typeof(SkillTreeMenuUI).GetField("skillsAreaRoot", flags)?.SetValue(script, skillsArea);
        typeof(SkillTreeMenuUI).GetField("connectionsLayer", flags)?.SetValue(script, connections);
        typeof(SkillTreeMenuUI).GetField("nodesLayer", flags)?.SetValue(script, nodes);

        typeof(SkillTreeMenuUI).GetField("infoTitleText", flags)?.SetValue(script, infoTitle);
        typeof(SkillTreeMenuUI).GetField("infoDescriptionText", flags)?.SetValue(script, infoDescription);
        typeof(SkillTreeMenuUI).GetField("infoImpactText", flags)?.SetValue(script, infoImpact);
        typeof(SkillTreeMenuUI).GetField("infoCostText", flags)?.SetValue(script, infoCost);

        typeof(SkillTreeMenuUI).GetField("buyButton", flags)?.SetValue(script, buyButton);
        typeof(SkillTreeMenuUI).GetField("buyButtonText", flags)?.SetValue(script, buyLabel);
    }
}
