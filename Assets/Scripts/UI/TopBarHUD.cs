using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TopBarHUD : MonoBehaviour
{
    private GameManager gameManager;
    private TextMeshProUGUI daysText;
    private TextMeshProUGUI dateText;
    private Button slowButton;
    private Button normalButton;
    private Button pauseButton;
    private Button fastButton;
    private TextMeshProUGUI pauseButtonLabel;
    private RectTransform mainPanelRT;
    private TextMeshProUGUI toggleLabel;
    private bool isVisible = true;
    private Canvas hudCanvas;

    private static readonly DateTime startDate = DateTime.Today;
    private const float SPEED_SLOW   = 0.5f;
    private const float SPEED_NORMAL = 1.0f;
    private const float SPEED_FAST   = 3.0f;

    private const float PANEL_W        = 260f;
    private const float PANEL_H_FULL   = 148f;
    private const float PANEL_H_HEADER = 30f;

    private static readonly Color colorBg        = new Color(0.08f, 0.08f, 0.10f, 0.93f);
    private static readonly Color colorHeader    = new Color(0.11f, 0.11f, 0.15f, 1f);
    private static readonly Color colorText      = new Color(0.90f, 0.90f, 0.90f, 1f);
    private static readonly Color colorSubText   = new Color(0.55f, 0.55f, 0.65f, 1f);
    private static readonly Color colorBtnBase   = new Color(0.18f, 0.18f, 0.22f, 1f);
    private static readonly Color colorBtnHi     = new Color(0.25f, 0.55f, 0.90f, 1f);
    private static readonly Color colorBtnPause  = new Color(0.85f, 0.55f, 0.10f, 1f);
    private static readonly Color colorBtnResume = new Color(0.15f, 0.70f, 0.30f, 1f);

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        BuildUI();
        SkillTreeMenuUI.OnMenuOpened += OnSkillMenuOpened;
        SkillTreeMenuUI.OnMenuClosed += OnSkillMenuClosed;
    }

    void OnDestroy()
    {
        SkillTreeMenuUI.OnMenuOpened -= OnSkillMenuOpened;
        SkillTreeMenuUI.OnMenuClosed -= OnSkillMenuClosed;
    }

    void Update()
    {
        if (gameManager == null || !isVisible) return;
        UpdateDateDisplay();
        UpdateButtonStates();
    }

    // ─── Build UI ──────────────────────────────────────────────────────────

    private void BuildUI()
    {
        var canvasGO = new GameObject("TopBarCanvas");
        hudCanvas = canvasGO.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Compact rectangle — centered at top of screen
        var panelGO = new GameObject("HUDPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        mainPanelRT = panelGO.AddComponent<RectTransform>();
        mainPanelRT.anchorMin        = new Vector2(0.5f, 1f);
        mainPanelRT.anchorMax        = new Vector2(0.5f, 1f);
        mainPanelRT.pivot            = new Vector2(0.5f, 1f);
        mainPanelRT.sizeDelta        = new Vector2(PANEL_W, PANEL_H_FULL);
        mainPanelRT.anchoredPosition = new Vector2(0f, -8f);
        panelGO.AddComponent<Image>().color = colorBg;

        float headerFrac = PANEL_H_HEADER / PANEL_H_FULL; // ≈ 0.183

        // ── Étage 1 – Header : bouton cacher (pleine largeur) ──────────────
        var r1 = Row(panelGO.transform, "R1_Header", 0f, 1f, 1f - headerFrac, 1f, colorHeader);
        var toggleBtn = MakeButton(r1.transform, "ToggleBtn", LocalizationManager.Get("hud_btn_hide"),
            new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.95f),
            OnToggleClicked, colorBtnBase, 11f);
        toggleLabel = toggleBtn.GetComponentInChildren<TextMeshProUGUI>();

        // ── Étage 2 – Jour N ──────────────────────────────────────────────
        var r2 = Row(panelGO.transform, "R2_Day", 0f, 1f, 0.52f, 1f - headerFrac, Color.clear);
        daysText = MakeText(r2.transform, "DaysTxt",
            new Vector2(0.05f, 0f), new Vector2(0.95f, 1f),
            string.Format(LocalizationManager.Get("hud_day_label"), 0), 26, FontStyles.Bold, TextAlignmentOptions.Midline);
        daysText.color = colorText;

        // ── Étage 3 – Date calendrier ─────────────────────────────────────
        var r3 = Row(panelGO.transform, "R3_Date", 0f, 1f, 0.37f, 0.52f, Color.clear);
        dateText = MakeText(r3.transform, "DateTxt",
            new Vector2(0.04f, 0f), new Vector2(0.96f, 1f),
            "01 JAN 2024", 14, FontStyles.Normal, TextAlignmentOptions.Midline);

        // ── Étage 4 – Contrôles vitesse ───────────────────────────────────
        var r4 = Row(panelGO.transform, "R4_Speed", 0f, 1f, 0.02f, 0.37f, Color.clear);
        slowButton   = MakeButton(r4.transform, "SlowBtn",   LocalizationManager.Get("hud_btn_slow"),
            new Vector2(0.02f, 0.12f), new Vector2(0.25f, 0.88f), OnSlowClicked,   colorBtnBase,  11f);
        normalButton = MakeButton(r4.transform, "NormalBtn", LocalizationManager.Get("hud_btn_normal"),
            new Vector2(0.27f, 0.12f), new Vector2(0.50f, 0.88f), OnNormalClicked, colorBtnHi,    11f);
        pauseButton  = MakeButton(r4.transform, "PauseBtn",  LocalizationManager.Get("hud_btn_pause"),
            new Vector2(0.52f, 0.12f), new Vector2(0.74f, 0.88f), OnPauseClicked,  colorBtnPause, 11f);
        fastButton   = MakeButton(r4.transform, "FastBtn",   LocalizationManager.Get("hud_btn_fast"),
            new Vector2(0.76f, 0.12f), new Vector2(0.98f, 0.88f), OnFastClicked,   colorBtnBase,  11f);
        pauseButtonLabel = pauseButton.GetComponentInChildren<TextMeshProUGUI>();

        if (gameManager != null) gameManager.SetSimulationSpeed(SPEED_NORMAL);
    }

    // ─── Skill menu visibility ───────────────────────────────────────

    private void OnSkillMenuOpened() { if (hudCanvas != null) hudCanvas.enabled = false; }
    private void OnSkillMenuClosed()  { if (hudCanvas != null) hudCanvas.enabled = true;  }

    // ─── Toggle ────────────────────────────────────────────────────────────

    private void OnToggleClicked()
    {
        isVisible = !isVisible;
        foreach (Transform child in mainPanelRT)
        {
            if (child.name != "R1_Header")
                child.gameObject.SetActive(isVisible);
        }
        // Hide panel background when collapsed — only the blue button stays visible
        var bgImage = mainPanelRT.GetComponent<Image>();
        if (bgImage != null) bgImage.enabled = isVisible;

        mainPanelRT.sizeDelta = new Vector2(PANEL_W, isVisible ? PANEL_H_FULL : PANEL_H_HEADER);
        toggleLabel.text = isVisible ? LocalizationManager.Get("hud_btn_hide") : LocalizationManager.Get("hud_btn_show");
        toggleLabel.fontSize = isVisible ? 10f : 14f;
        SetButtonColor(toggleLabel.GetComponentInParent<Button>(),
            isVisible ? colorBtnBase : colorBtnHi);
    }

    // ─── Update display ────────────────────────────────────────────────────

    private void UpdateDateDisplay()
    {
        int turn = gameManager.currentTurn;
        DateTime current = startDate.AddDays(turn);
        daysText.text = string.Format(LocalizationManager.Get("hud_day_label"), turn);
        dateText.text = current.ToString("dd MMM yyyy").ToUpper();
    }

    private void UpdateButtonStates()
    {
        bool paused = gameManager.IsPaused();
        float speed = gameManager.GetSimulationSpeed();
        pauseButtonLabel.text = paused ? LocalizationManager.Get("hud_btn_resume") : LocalizationManager.Get("hud_btn_pause");
        SetButtonColor(pauseButton,  paused ? colorBtnResume : colorBtnPause);
        SetButtonColor(slowButton,   (!paused && Mathf.Approximately(speed, SPEED_SLOW))   ? colorBtnHi : colorBtnBase);
        SetButtonColor(normalButton, (!paused && Mathf.Approximately(speed, SPEED_NORMAL)) ? colorBtnHi : colorBtnBase);
        SetButtonColor(fastButton,   (!paused && Mathf.Approximately(speed, SPEED_FAST))   ? colorBtnHi : colorBtnBase);
    }

    // ─── Button callbacks ──────────────────────────────────────────────────

    private void OnSlowClicked()
    {
        if (gameManager == null) return;
        gameManager.SetSimulationSpeed(SPEED_SLOW);
        if (!gameManager.IsPaused()) gameManager.ResumeGame();
    }

    private void OnPauseClicked()
    {
        if (gameManager == null) return;
        if (gameManager.IsPaused()) gameManager.ResumeGame();
        else gameManager.PauseGame();
    }

    private void OnNormalClicked()
    {
        if (gameManager == null) return;
        gameManager.SetSimulationSpeed(SPEED_NORMAL);
        if (!gameManager.IsPaused()) gameManager.ResumeGame();
    }

    private void OnFastClicked()
    {
        if (gameManager == null) return;
        gameManager.SetSimulationSpeed(SPEED_FAST);
        if (!gameManager.IsPaused()) gameManager.ResumeGame();
    }

    // ─── UI helpers ────────────────────────────────────────────────────────

    private static GameObject Row(Transform parent, string name,
        float xMin, float xMax, float yMin, float yMax, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        if (bg.a > 0f) go.AddComponent<Image>().color = bg;
        return go;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, string text,
        float fontSize, FontStyles style, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        tmp.fontStyle = style; tmp.alignment = align;
        tmp.color = colorText;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private static Button MakeButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax,
        UnityEngine.Events.UnityAction onClick, Color bgColor, float fontSize = 14f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = bgColor + new Color(0.10f, 0.10f, 0.10f, 0f);
        cb.pressedColor     = bgColor - new Color(0.10f, 0.10f, 0.10f, 0f);
        cb.selectedColor    = bgColor;
        btn.colors = cb;
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var lrt = labelGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Midline;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 6f;
        tmp.fontSizeMax = fontSize;
        return btn;
    }

    private static void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
        var cb = btn.colors;
        cb.normalColor      = color;
        cb.highlightedColor = color + new Color(0.10f, 0.10f, 0.10f, 0f);
        cb.pressedColor     = color - new Color(0.10f, 0.10f, 0.10f, 0f);
        cb.selectedColor    = color;
        btn.colors = cb;
    }
}
