using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfectionClickBonus : MonoBehaviour
{
    public static InfectionClickBonus Instance { get; private set; }

    private Canvas hudCanvas;
    private RectTransform canvasRT;
    private Camera mainCam;

    private Camera Cam
    {
        get
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) mainCam = FindObjectOfType<Camera>();
            return mainCam;
        }
    }

    private readonly HashSet<string> notified = new HashSet<string>();
    private readonly List<ActiveBonus> activeBonuses = new List<ActiveBonus>();
    private bool firstInfectionSkipped = false;

    private const int   EXPIRE_TURNS = 20;
    private const float POPUP_W  = 100f;
    private const float POPUP_H  = 72f;   // corps du bubble
    private const float TAIL_SZ  = 14f;   // queue pointue en bas

    private static readonly Color colorBorder  = new Color(1.00f, 0.22f, 0.10f, 1.00f);
    private static readonly Color colorBody    = new Color(0.15f, 0.05f, 0.04f, 0.96f);
    private static readonly Color colorHeader  = new Color(0.85f, 0.12f, 0.08f, 1.00f);
    private static readonly Color colorBtnHov  = new Color(0.25f, 0.08f, 0.07f, 1.00f);
    private static readonly Color colorBtnPrs  = new Color(0.08f, 0.02f, 0.02f, 1.00f);
    private static readonly Color colorFloat   = new Color(1.00f, 0.90f, 0.20f, 1.00f);

    private class ActiveBonus
    {
        public string        countryName;
        public GameObject    go;
        public RectTransform rt;
        public Country       mapCountry;
        public Renderer      countryRenderer;
        public int           expiresAtTurn;
        public int           points;
    }

    // ── Init ────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildCanvas();
    }

    private void BuildCanvas()
    {
        var go = new GameObject("InfectionBonusCanvas");
        hudCanvas = go.AddComponent<Canvas>();
        hudCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 90;
        canvasRT = go.GetComponent<RectTransform>();
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();
    }

    // ── Update : repositionne les popups sur leurs pays ────────────────────

    void Update()
    {
        for (int i = activeBonuses.Count - 1; i >= 0; i--)
        {
            var b = activeBonuses[i];
            if (b.go == null) { activeBonuses.RemoveAt(i); continue; }
            if (Cam == null) continue;
            Vector3 worldPos = b.countryRenderer != null
                ? b.countryRenderer.bounds.center
                : b.mapCountry.transform.position;
            Vector3 screen = Cam.WorldToScreenPoint(worldPos);
            bool visible = screen.z > 0f;
            b.go.SetActive(visible);
            if (visible) b.rt.anchoredPosition = ScreenToCanvas(screen);
        }
    }

    // ── Appele par GameManager apres chaque tour ─────────────────────────────

    public void CheckNewInfections(List<CountryObject> countries, int turn)
    {
        for (int i = activeBonuses.Count - 1; i >= 0; i--)
        {
            if (activeBonuses[i].expiresAtTurn <= turn)
            {
                if (activeBonuses[i].go != null) Destroy(activeBonuses[i].go);
                activeBonuses.RemoveAt(i);
            }
        }
        foreach (var co in countries)
        {
            if (co.population.infected > 0 && notified.Add(co.name))
            {
                if (!firstInfectionSkipped) { firstInfectionSkipped = true; continue; }
                SpawnButton(co, turn);
            }
        }
    }

    // ── Spawn : bubble avec queue pointue ────────────────────────────────────

    private void SpawnButton(CountryObject co, int turn)
    {
        if (WorldMap.Instance == null) return;
        Country mapCountry = WorldMap.Instance.GetCountry(co.name);
        if (mapCountry == null) return;

        int pts = Mathf.Max(2, Mathf.RoundToInt(Mathf.Log10(co.population.total + 1)));

        // Conteneur racine — pivot bas-centre pour pointer sur le pays
        var root = new GameObject("InfBonus_" + co.name);
        root.transform.SetParent(hudCanvas.transform, false);
        var rt = root.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(POPUP_W, POPUP_H + TAIL_SZ);
        rt.pivot     = new Vector2(0.5f, 0f);   // bas-centre

        // Corps principal (fond sombre + bordure via décalage)
        // -- Bordure (léger rectangle plus grand derrière) --
        var border = SubRect(root.transform, "Border", Vector2.zero, Vector2.one,
            new Vector2(-2f, TAIL_SZ - 2f), new Vector2(2f, 2f), colorBorder);

        // -- Fond sombre --
        SubRect(border.transform, "Body", Vector2.zero, Vector2.one,
            new Vector2(2f, 2f), new Vector2(-2f, -2f), colorBody);

        // -- Header rouge (en haut) : nom du pays --
        var headerH = 30f;
        float headerFrac = headerH / POPUP_H;
        var header = SubRect(border.transform, "Header",
            new Vector2(0f, 1f - headerFrac), new Vector2(1f, 1f),
            new Vector2(2f, 0f), new Vector2(-2f, -2f), colorHeader);
        var nameLabel = AddTMP(header.transform, "NameLabel",
            Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -0f),
            co.name, 16f, FontStyles.Bold, TextAlignmentOptions.Midline, Color.white);
        nameLabel.enableWordWrapping = false;
        nameLabel.enableAutoSizing = true;
        nameLabel.fontSizeMin = 8f;
        nameLabel.fontSizeMax = 16f;
        nameLabel.overflowMode = TextOverflowModes.Ellipsis;

        // -- Points "+8" en grand au centre --
        AddTMP(border.transform, "PtsLabel",
            new Vector2(0f, 0f), new Vector2(1f, 1f - headerFrac),
            new Vector2(2f, 4f), new Vector2(-2f, -4f),
            "+" + pts, 20f, FontStyles.Bold | FontStyles.Italic, TextAlignmentOptions.Midline, colorFloat);

        // -- Queue : carré rotaté 45° en bas-centre --
        var tailGO = new GameObject("Tail");
        tailGO.transform.SetParent(root.transform, false);
        var tailRT = tailGO.AddComponent<RectTransform>();
        tailRT.sizeDelta        = new Vector2(TAIL_SZ, TAIL_SZ);
        tailRT.anchorMin        = new Vector2(0.5f, 0f);
        tailRT.anchorMax        = new Vector2(0.5f, 0f);
        tailRT.pivot            = new Vector2(0.5f, 0.5f);
        tailRT.anchoredPosition = new Vector2(0f, TAIL_SZ * 0.5f);
        tailRT.localRotation    = Quaternion.Euler(0f, 0f, 45f);
        tailGO.AddComponent<Image>().color = colorBorder;

        // Bouton invisible par-dessus tout (couvre le corps entier)
        var btnGO = new GameObject("BtnOverlay");
        btnGO.transform.SetParent(root.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0f, 0f);
        btnRT.anchorMax = new Vector2(1f, 1f);
        btnRT.offsetMin = new Vector2(0f, TAIL_SZ);
        btnRT.offsetMax = Vector2.zero;
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = Color.clear;
        var btn = btnGO.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = Color.clear;
        cb.highlightedColor = colorBtnHov;
        cb.pressedColor     = colorBtnPrs;
        cb.selectedColor    = Color.clear;
        btn.colors = cb;
        btn.targetGraphic = btnImg;

        Renderer countryRenderer = mapCountry.GetComponentInChildren<Renderer>();

        var bonus = new ActiveBonus
        {
            countryName     = co.name,
            go              = root,
            rt              = rt,
            mapCountry      = mapCountry,
            countryRenderer = countryRenderer,
            expiresAtTurn   = turn + EXPIRE_TURNS,
            points          = pts
        };
        activeBonuses.Add(bonus);
        btn.onClick.AddListener(() => OnBonusClicked(bonus));

        // Position immédiate
        if (Cam != null)
        {
            Vector3 worldPos = countryRenderer != null
                ? countryRenderer.bounds.center
                : mapCountry.transform.position;
            Vector3 screen = Cam.WorldToScreenPoint(worldPos);
            if (screen.z > 0f) rt.anchoredPosition = ScreenToCanvas(screen);
        }
    }

    // ── Clic ─────────────────────────────────────────────────────────────────

    private void OnBonusClicked(ActiveBonus bonus)
    {
        if (bonus.go == null) return;
        PointManager pm = PointManager.GetInstance();
        if (pm != null) pm.AddPoints(bonus.points);
        Debug.Log("[InfectionBonus] +" + bonus.points + " pts collectes - " + bonus.countryName);
        StartCoroutine(AnimateFloatingText(bonus.rt.anchoredPosition + new Vector2(0f, POPUP_H + TAIL_SZ), "+" + bonus.points));
        Destroy(bonus.go);
        bonus.go = null;
        activeBonuses.Remove(bonus);
    }

    // ── Texte flottant anime (grand -> petit + disparait) ────────────────────

    private IEnumerator AnimateFloatingText(Vector2 startPos, string text)
    {
        const float duration    = 1.4f;
        const float startSize   = 42f;
        const float endSize     = 12f;
        const float riseAmount  = 60f;

        var go = new GameObject("FloatText");
        go.transform.SetParent(hudCanvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(100f, 60f);
        rt.anchoredPosition = startPos;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize  = startSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Midline;
        tmp.enableWordWrapping = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t;                                    // ease in
            tmp.fontSize = Mathf.Lerp(startSize, endSize, eased);
            tmp.color    = new Color(colorFloat.r, colorFloat.g, colorFloat.b, 1f - eased);
            rt.anchoredPosition = startPos + new Vector2(0f, riseAmount * t);
            yield return null;
        }
        Destroy(go);
    }

    // ── Helpers UI ───────────────────────────────────────────────────────────

    private static RectTransform SubRect(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        go.AddComponent<Image>().color = color;
        return rt;
    }

    private static TextMeshProUGUI AddTMP(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        string text, float size, FontStyles style,
        TextAlignmentOptions align, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize   = size;
        tmp.fontStyle  = style;
        tmp.alignment  = align;
        tmp.color      = color;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    // ── Utilitaire ────────────────────────────────────────────────────────────

    private Vector2 ScreenToCanvas(Vector3 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT,
            new Vector2(screenPos.x, screenPos.y),
            null,
            out Vector2 local);
        return local;
    }
}
