using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Affiche l'écran de fin de partie (victoire, défaite, vaccin).
/// Appeler EndScreenUI.Show(state, virusName, turns, totalDead) depuis GameManager.
/// </summary>
public class EndScreenUI : MonoBehaviour
{
    public static EndScreenUI Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void Show(GameState state, string virusName, int turns, long totalDead, long totalInfected)
    {
        var go = new GameObject("EndScreenUI_Host");
        DontDestroyOnLoad(go);
        go.AddComponent<EndScreenUI>().StartCoroutine(
            go.GetComponent<EndScreenUI>().Build(state, virusName, turns, totalDead, totalInfected)
        );
    }

    private IEnumerator Build(GameState state, string virusName, int turns, long totalDead, long totalInfected)
    {
        // Pause game
        var gm = FindAnyObjectByType<GameManager>();
        if (gm != null) gm.PauseGame();

        // Canvas opaque couvrant tout
        GameObject cvGO = new GameObject("EndCanvas");
        cvGO.transform.SetParent(transform, false);  // parent au host pour pouvoir détruire ensemble
        Canvas cv = cvGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 1000;
        cvGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ((CanvasScaler)cvGO.GetComponent<CanvasScaler>()).referenceResolution = new Vector2(1920, 1080);
        cvGO.AddComponent<GraphicRaycaster>();

        bool isVictory  = state == GameState.Victory;
        bool isHealedL  = state == GameState.Healed;

        // Couleurs du thème
        Color bgColor      = isVictory  ? new Color(0.04f, 0.06f, 0.02f, 1f)
                           : isHealedL  ? new Color(0.02f, 0.04f, 0.12f, 1f)
                                        : new Color(0.08f, 0.02f, 0.02f, 1f);
        Color accentColor  = isVictory  ? new Color(0.40f, 0.90f, 0.20f, 1f)
                           : isHealedL  ? new Color(0.20f, 0.55f, 0.95f, 1f)
                                        : new Color(0.90f, 0.20f, 0.10f, 1f);
        Color dimAccent    = new Color(accentColor.r * 0.5f, accentColor.g * 0.5f, accentColor.b * 0.5f, 0.35f);

        string headlineText = isVictory ? LocalizationManager.Get("end_headline_victory")
                            : isHealedL ? LocalizationManager.Get("end_headline_healed")
                                        : LocalizationManager.Get("end_headline_defeat");
        string subText      = isVictory ? LocalizationManager.Get("end_sub_victory")
                            : isHealedL ? LocalizationManager.Get("end_sub_healed")
                                        : LocalizationManager.Get("end_sub_defeat");

        // Fond
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(cvGO.transform, false);
        Image bgImg = bg.AddComponent<Image>(); bgImg.color = bgColor;
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

        // Bande d'accent en haut
        MakePanelRT(cvGO, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -8f), new Vector2(0f, 0f), accentColor);
        // Bande d'accent en bas
        MakePanelRT(cvGO, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 8f), accentColor);

        // Titre principal
        MakeTMP(cvGO, headlineText, 72f, FontStyles.Bold, accentColor,
            new Vector2(0f, 0.60f), new Vector2(1f, 0.80f), Vector2.zero, Vector2.zero,
            TextAlignmentOptions.Center);

        // Sous-titre
        MakeTMP(cvGO, subText, 22f, FontStyles.Italic, new Color(0.75f, 0.75f, 0.75f, 1f),
            new Vector2(0.1f, 0.50f), new Vector2(0.9f, 0.62f), Vector2.zero, Vector2.zero,
            TextAlignmentOptions.Center);

        // Séparateur
        MakePanelRT(cvGO, new Vector2(0.2f, 0.485f), new Vector2(0.8f, 0.485f),
            new Vector2(0f, -1f), new Vector2(0f, 1f), dimAccent);

        // Nom du virus
        MakeTMP(cvGO, string.Format(LocalizationManager.Get("end_stat_virus"), virusName), 32f, FontStyles.Bold,
            new Color(accentColor.r, accentColor.g, accentColor.b, 0.9f),
            new Vector2(0.15f, 0.35f), new Vector2(0.85f, 0.47f), Vector2.zero, Vector2.zero,
            TextAlignmentOptions.Center);

        // Stats
        string statsText =
            string.Format(LocalizationManager.Get("end_stat_turns"), turns) + "\n" +
            string.Format(LocalizationManager.Get("end_stat_dead"), FormatBig(totalDead)) + "\n" +
            string.Format(LocalizationManager.Get("end_stat_infected"), FormatBig(totalInfected));
        MakeTMP(cvGO, statsText, 24f, FontStyles.Normal, new Color(0.85f, 0.85f, 0.85f, 1f),
            new Vector2(0.25f, 0.18f), new Vector2(0.75f, 0.36f), Vector2.zero, Vector2.zero,
            TextAlignmentOptions.Left);

        // Bouton recommencer
        MakeButton(cvGO, LocalizationManager.Get("end_btn_replay"), accentColor, bgColor,
            new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.16f),
            () => {
                var gm = Object.FindAnyObjectByType<GameManager>();
                Object.Destroy(gameObject);  // détruit le host et le canvas enfant
                if (gm != null) gm.Restart();
            });

        // Fade-in
        CanvasGroup cg = cvGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        float t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime / 0.6f; cg.alpha = Mathf.Clamp01(t); yield return null; }
    }

    // ── Helpers ──────────────────────────────────────────────

    private static string FormatBig(long n)
    {
        if (n >= 1_000_000_000) return $"{n / 1_000_000_000f:F1}G";
        if (n >= 1_000_000)     return $"{n / 1_000_000f:F1}M";
        if (n >= 1_000)         return $"{n / 1_000f:F0}K";
        return n.ToString();
    }

    private static void MakePanelRT(GameObject parent, Vector2 ancMin, Vector2 ancMax,
        Vector2 offMin, Vector2 offMax, Color color)
    {
        var go = new GameObject("Panel");
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = offMin; rt.offsetMax = offMax;
    }

    private static void MakeTMP(GameObject parent, string text, float size, FontStyles style,
        Color color, Vector2 ancMin, Vector2 ancMax, Vector2 offMin, Vector2 offMax,
        TextAlignmentOptions align)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.color = color; tmp.alignment = align;
        tmp.enableWordWrapping = true;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = offMin; rt.offsetMax = offMax;
    }

    private static void MakeButton(GameObject parent, string label, Color bgCol, Color textCol,
        Vector2 ancMin, Vector2 ancMax, System.Action onClick)
    {
        var go = new GameObject("Button");
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>(); img.color = bgCol;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor  = bgCol;
        colors.highlightedColor = new Color(bgCol.r + 0.15f, bgCol.g + 0.15f, bgCol.b + 0.15f, 1f);
        colors.pressedColor = new Color(bgCol.r - 0.1f, bgCol.g - 0.1f, bgCol.b - 0.1f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick());
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        // Bordure (Outline via child panel)
        var border = new GameObject("Border");
        border.transform.SetParent(go.transform, false);
        border.AddComponent<Image>().color = new Color(bgCol.r * 1.5f + 0.1f, bgCol.g * 1.5f + 0.1f, bgCol.b * 1.5f + 0.1f, 1f);
        var brt = border.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = new Vector2(-2f, -2f); brt.offsetMax = new Vector2(2f, 2f);
        border.transform.SetAsFirstSibling();

        // Label
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 26f; tmp.fontStyle = FontStyles.Bold;
        tmp.color = textCol; tmp.alignment = TextAlignmentOptions.Center;
        var lrt = lblGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
    }
}
