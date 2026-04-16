using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main menu shown at startup. Hides itself and calls VirusSelectionPanel when "JOUER" is clicked.
/// Credit text is loaded from Assets/Resources/credits.txt.
/// Language buttons (FR / EN) are placeholders for a future localisation feature.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    // ── Singleton factory ─────────────────────────────────────────────────────
    public static void Show(Action onPlay)
    {
        var go = new GameObject("MainMenuUI");
        var menu = go.AddComponent<MainMenuUI>();
        menu.onPlayCallback = onPlay;
    }

    private Action onPlayCallback;

    // ── Localizable text references ───────────────────────────────────────────
    private TextMeshProUGUI taglineTMP;
    private Button playBtn;

    // ── Layout constants ──────────────────────────────────────────────────────
    private const float BTN_W     = 220f;
    private const float BTN_H     = 58f;
    private const float LANG_W    = 64f;
    private const float LANG_H    = 36f;
    private const float TITLE_FS  = 72f;
    private const float SUBTITLE_FS = 22f;

    // ── Colours ───────────────────────────────────────────────────────────────
    private static readonly Color BG_COLOR      = new Color(0.03f, 0.03f, 0.07f, 1f);
    private static readonly Color ACCENT        = new Color(0.78f, 0.18f, 0.18f, 1f);
    private static readonly Color BTN_NORMAL    = new Color(0.78f, 0.18f, 0.18f, 1f);
    private static readonly Color BTN_HOVER     = new Color(0.95f, 0.30f, 0.20f, 1f);
    private static readonly Color LANG_NORMAL   = new Color(0.15f, 0.15f, 0.22f, 1f);
    private static readonly Color LANG_ACTIVE   = new Color(0.25f, 0.48f, 0.80f, 1f);
    private static readonly Color TEXT_DIM      = new Color(0.65f, 0.65f, 0.65f, 1f);
    private static readonly Color CREDITS_COLOR = new Color(0.55f, 0.55f, 0.55f, 1f);

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        BuildUI();
    }

    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshTexts;
    }

    // ── Build ─────────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        // Root canvas — covers entire screen, always on top
        var cvGO = new GameObject("MainMenuCanvas");
        cvGO.transform.SetParent(transform, false);
        var canvas = cvGO.AddComponent<Canvas>();
        canvas.renderMode    = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder  = 200;
        var scaler = cvGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        cvGO.AddComponent<GraphicRaycaster>();

        // Full-screen background
        var bg = MakePanel(cvGO.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bg.AddComponent<Image>().color = BG_COLOR;

        // Subtle dark overlay gradient effect (simple vignette via nested image)
        var vignette = MakePanel(bg.transform, "Vignette", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        vignette.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        // ── Language buttons (top-right) ──────────────────────────────────────
        var langFR = MakeAnchoredButton(bg.transform, "LangFR", "FR",
            new Vector2(1f, 1f), new Vector2(-(LANG_W * 2f + 24f), -16f),
            new Vector2(LANG_W, LANG_H), LANG_ACTIVE, 16f);
        langFR.onClick.AddListener(() => OnLanguageSelected("FR", langFR, GetLangButton(bg, "LangEN")));

        var langEN = MakeAnchoredButton(bg.transform, "LangEN", "EN",
            new Vector2(1f, 1f), new Vector2(-LANG_W - 12f, -16f),
            new Vector2(LANG_W, LANG_H), LANG_NORMAL, 16f);
        langEN.onClick.AddListener(() => OnLanguageSelected("EN", langEN, langFR));

        // ── Centre column ───────────────────────────────────────────────────
        var colGO = MakePanel(bg.transform, "CentreCol",
            new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.85f),
            Vector2.zero, Vector2.zero);
        var col = colGO.transform;

        // Game title
        var titleGO = MakeText(col, "Title", "CROTTE VIRALE",
            new Vector2(0f, 0.62f), new Vector2(1f, 1f),
            Vector2.zero, Vector2.zero);
        var titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.fontSize     = TITLE_FS;
        titleTMP.fontStyle    = FontStyles.Bold;
        titleTMP.alignment    = TextAlignmentOptions.Center;
        titleTMP.color        = Color.white;
        titleTMP.enableAutoSizing = true;
        titleTMP.fontSizeMin  = 32f;
        titleTMP.fontSizeMax  = TITLE_FS;

        // Tagline
        var tagGO = MakeText(col, "Tagline", LocalizationManager.Get("menu_tagline"),
            new Vector2(0f, 0.52f), new Vector2(1f, 0.65f),
            Vector2.zero, Vector2.zero);
        taglineTMP = tagGO.GetComponent<TextMeshProUGUI>();
        taglineTMP.fontSize  = SUBTITLE_FS;
        taglineTMP.alignment = TextAlignmentOptions.Center;
        taglineTMP.color     = TEXT_DIM;
        taglineTMP.enableWordWrapping = true;

        // Accent line divider
        var divGO = new GameObject("Divider");
        divGO.transform.SetParent(col, false);
        var divRT = divGO.AddComponent<RectTransform>();
        divRT.anchorMin        = new Vector2(0.3f, 0.49f);
        divRT.anchorMax        = new Vector2(0.7f, 0.495f);
        divRT.offsetMin        = Vector2.zero;
        divRT.offsetMax        = Vector2.zero;
        divGO.AddComponent<Image>().color = ACCENT;

        // JOUER / PLAY button
        playBtn = MakeAnchoredButton(col, "PlayBtn", LocalizationManager.Get("menu_play_btn"),
            new Vector2(0.5f, 0.35f), Vector2.zero,
            new Vector2(BTN_W, BTN_H), BTN_NORMAL, 26f);
        playBtn.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        playBtn.onClick.AddListener(OnPlayClicked);

        // Subscribe for live language updates
        LocalizationManager.OnLanguageChanged += RefreshTexts;

        // ── Credits (bottom strip) ─────────────────────────────────────────────
        var creditsGO = MakeText(bg.transform, "Credits", LoadCredits(),
            new Vector2(0f, 0f), new Vector2(1f, 0.13f),
            new Vector2(20f, 8f), new Vector2(-20f, 0f));
        var creditsTMP = creditsGO.GetComponent<TextMeshProUGUI>();
        creditsTMP.fontSize         = 13f;
        creditsTMP.alignment        = TextAlignmentOptions.Center;
        creditsTMP.color            = CREDITS_COLOR;
        creditsTMP.enableWordWrapping = true;
        creditsTMP.overflowMode     = TextOverflowModes.Overflow;
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────
    private void OnPlayClicked()
    {
        Destroy(gameObject);
        onPlayCallback?.Invoke();
    }

    private void OnLanguageSelected(string lang, Button selected, Button other)
    {
        SetButtonColor(selected, LANG_ACTIVE);
        SetButtonColor(other, LANG_NORMAL);
        LocalizationManager.SetLanguage(lang);
    }

    private void RefreshTexts()
    {
        if (taglineTMP != null)
            taglineTMP.text = LocalizationManager.Get("menu_tagline");
        if (playBtn != null)
            playBtn.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Get("menu_play_btn");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static string LoadCredits()
    {
        var asset = Resources.Load<TextAsset>("credits");
        return asset != null ? asset.text : "CrotteViral";
    }

    private static Button GetLangButton(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        return t != null ? t.GetComponent<Button>() : null;
    }

    private static void SetButtonColor(Button btn, Color c)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = c;
        var cb = btn.colors;
        cb.normalColor      = c;
        cb.highlightedColor = new Color(c.r + 0.12f, c.g + 0.12f, c.b + 0.12f, c.a);
        cb.pressedColor     = new Color(c.r - 0.1f, c.g - 0.1f, c.b - 0.1f, c.a);
        cb.selectedColor    = c;
        btn.colors = cb;
    }

    /// <summary>Creates a GameObject with a full-stretch or anchored RectTransform.</summary>
    private static GameObject MakePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return go;
    }

    /// <summary>Creates a TMP label and returns the GameObject.</summary>
    private static GameObject MakeText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text  = text;
        tmp.color = Color.white;
        return go;
    }

    /// <summary>Creates a styled button anchored at a pivot point.</summary>
    private static Button MakeAnchoredButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 anchoredPos, Vector2 size, Color color, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchor;
        rt.anchorMax        = anchor;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;

        var img = go.AddComponent<Image>();
        img.color = color;

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.normalColor      = color;
        cb.highlightedColor = new Color(
            Mathf.Min(color.r + 0.12f, 1f),
            Mathf.Min(color.g + 0.12f, 1f),
            Mathf.Min(color.b + 0.12f, 1f), color.a);
        cb.pressedColor  = new Color(
            Mathf.Max(color.r - 0.1f, 0f),
            Mathf.Max(color.g - 0.1f, 0f),
            Mathf.Max(color.b - 0.1f, 0f), color.a);
        cb.selectedColor = color;
        btn.colors = cb;

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }
}
