using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Plein-écran virus selection + patient 0 + loading bar.
/// Appelé via VirusSelectionPanel.Show(Action<VirusType, string> onReady).
/// </summary>
public class VirusSelectionPanel : MonoBehaviour
{
    // ─── Données des continents ───────────────────────────────
    private struct ContinentEntry
    {
        public string label; public string locKey; public Color color; public string[] countries;
        public ContinentEntry(string l, string lk, Color c, string[] ctrs) { label = l; locKey = lk; color = c; countries = ctrs; }
    }
    private static readonly ContinentEntry[] ContinentData = new ContinentEntry[]
    {
        new ContinentEntry("EUROPE", "continent_europe", new Color(0.35f, 0.55f, 1.00f, 1f), new[] {
            "France","Germany","United Kingdom","Spain","Italy","Poland","Netherlands",
            "Belgium","Sweden","Norway","Denmark","Finland","Switzerland","Austria",
            "Portugal","Czechia","Hungary","Romania","Ukraine","Greece","Russia",
            "Serbia","Croatia","Slovakia","Bulgaria","Belarus","Albania","Bosnia and Herz.",
            "North Macedonia","Montenegro","Moldova","Lithuania","Latvia","Estonia",
            "Luxembourg","Malta","Cyprus","Iceland","Ireland","Kosovo","San Marino",
            "Monaco","Andorra","Liechtenstein","N. Cyprus","Åland","Greenland",
            "Faeroe Is.","Guernsey","Jersey","Isle of Man" }),
        new ContinentEntry("ASIE", "continent_asia", new Color(1.00f, 0.65f, 0.15f, 1f), new[] {
            "China","India","Japan","South Korea","Indonesia","Pakistan","Bangladesh",
            "Vietnam","Thailand","Myanmar","Malaysia","Philippines","Uzbekistan",
            "Kazakhstan","Afghanistan","Tajikistan","Kyrgyzstan","Turkmenistan",
            "Azerbaijan","Armenia","Georgia","Mongolia","Singapore","Cambodia","Laos",
            "Nepal","Sri Lanka","North Korea","Taiwan","Hong Kong","Macao","Bhutan",
            "Timor-Leste","Brunei","Maldives",
            "Saudi Arabia","Turkey","Iran","Iraq","Israel","Jordan","Lebanon",
            "Syria","Yemen","Kuwait","United Arab Emirates","Qatar","Oman","Bahrain","Palestine" }),
        new ContinentEntry("AFRIQUE", "continent_africa", new Color(0.95f, 0.50f, 0.08f, 1f), new[] {
            "Nigeria","Ethiopia","Dem. Rep. Congo","Tanzania","South Africa","Kenya",
            "Algeria","Sudan","Uganda","Morocco","Angola","Mozambique","Ghana",
            "Madagascar","Cameroon","Côte d'Ivoire","Niger","Mali","Malawi","Burkina Faso",
            "Zambia","Senegal","Chad","Somalia","Zimbabwe","Guinea","Rwanda","Benin",
            "Tunisia","S. Sudan","Egypt","Togo","Sierra Leone","Libya","Liberia",
            "Central African Rep.","Congo","Eritrea","Mauritania","Gabon","Gambia",
            "Botswana","Namibia","Guinea-Bissau","Lesotho","Eq. Guinea","Mauritius",
            "Djibouti","eSwatini","Cabo Verde","Comoros","Seychelles",
            "São Tomé and Principe","Somaliland","W. Sahara","Saint Helena" }),
        new ContinentEntry("AMER. NORD", "continent_northam", new Color(0.30f, 0.88f, 0.30f, 1f), new[] {
            "United States of America","Canada","Mexico","Guatemala","Honduras",
            "El Salvador","Nicaragua","Costa Rica","Panama","Cuba","Haiti",
            "Dominican Rep.","Jamaica","Trinidad and Tobago","Bahamas","Barbados",
            "Belize","Grenada","Saint Lucia","St. Vin. and Gren.","Dominica",
            "St. Kitts and Nevis","Antigua and Barb.","Puerto Rico","U.S. Virgin Is.",
            "British Virgin Is.","Aruba","Curaçao","Sint Maarten","St-Barthélémy",
            "St-Martin","Turks and Caicos Is.","Cayman Is.","Bermuda",
            "Montserrat","Anguilla","St. Pierre and Miquelon" }),
        new ContinentEntry("AMER. SUD", "continent_southam", new Color(0.55f, 0.90f, 0.18f, 1f), new[] {
            "Brazil","Argentina","Colombia","Chile","Peru","Venezuela","Ecuador",
            "Bolivia","Paraguay","Uruguay","Guyana","Suriname","Falkland Is." }),
        new ContinentEntry("OCEANIE", "continent_oceania", new Color(0.20f, 0.78f, 0.95f, 1f), new[] {
            "Australia","New Zealand","Papua New Guinea","Fiji","Vanuatu","Solomon Is.",
            "Samoa","Tonga","Kiribati","Marshall Is.","Micronesia","Palau","Nauru",
            "Tuvalu","Cook Is.","New Caledonia","Fr. Polynesia","N. Mariana Is.","Guam",
            "American Samoa","Niue","Norfolk Island","Wallis and Futuna Is.","Pitcairn Is." }),
        new ContinentEntry("AUTRES", "continent_other", new Color(0.55f, 0.55f, 0.55f, 1f), new[] {
            "Antarctica","Vatican","Br. Indian Ocean Ter.","Fr. S. Antarctic Lands",
            "S. Geo. and the Is.","Ashmore and Cartier Is.","Heard I. and McDonald Is.",
            "Indian Ocean Ter.","Siachen Glacier" }),
    };

    // ─── Données des virus ────────────────────────────────────
    private static readonly VirusEntry[] Entries = new VirusEntry[]
    {
        new VirusEntry(VirusType.Classique,    "classique",      new Color(0.60f, 0.85f, 0.40f, 1f)),
        new VirusEntry(VirusType.Cacastellaire,"cacastellaire",  new Color(0.90f, 0.65f, 0.15f, 1f)),
        new VirusEntry(VirusType.NanoCaca,     "nanocaca",       new Color(0.30f, 0.70f, 1.00f, 1f)),
        new VirusEntry(VirusType.FongiCaca,    "fungicaca",      new Color(0.80f, 0.35f, 0.90f, 1f)),
    };

    // ─── Référence statique pour le chargement ────────────────
    public static void Show(Action<VirusType, string> onReady)
    {
        GameObject go = new GameObject("VirusSelectionPanel");
        DontDestroyOnLoad(go);
        var panel = go.AddComponent<VirusSelectionPanel>();
        panel._onReady = onReady;
        panel.Build();
    }

    // ─── State ────────────────────────────────────────────────
    private Action<VirusType, string> _onReady;
    private VirusType? _selectedVirus;
    private string _selectedCountry;
    private Button _startButton;
    private TextMeshProUGUI _startButtonText;
    private Image[] _virusCardBorders;
    private Canvas _canvas;
    private readonly List<GameObject> _hiddenCanvases = new List<GameObject>();
    private GameObject _overlay;

    // Country selection
    private GameObject _countryGrid;
    private GameObject _countryContent;
    private Image[] _cardSelectionGlows;
    private TextMeshProUGUI _selectedCountryLabel;

    // ─── Build ────────────────────────────────────────────────
    private void Build()
    {
        // Canvas dédié toujours au-dessus de tout (sortingOrder 200)
        GameObject cvGO = new GameObject("SelectionCanvas");
        DontDestroyOnLoad(cvGO);
        _canvas = cvGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;
        var scaler = cvGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cvGO.AddComponent<GraphicRaycaster>();

        // Fond plein écran — rouge opaque, masque tout ce qui est derrière
        _overlay = new GameObject("VSP_Overlay");
        _overlay.transform.SetParent(_canvas.transform, false);
        Image overlayImg = _overlay.AddComponent<Image>();
        overlayImg.color = new Color(0.36f, 0.04f, 0.04f, 1f);
        RectTransform overlayRT = _overlay.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;

        // Masquer les autres éléments UI — stocker pour pouvoir les réactiver
        var allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var c in allCanvases)
        {
            if (c != _canvas && c.sortingOrder < 200)
            {
                _hiddenCanvases.Add(c.gameObject);
                c.gameObject.SetActive(false);
            }
        }

        // Titre principal
        GameObject titleGO = MakeText(_overlay, "VSP_Title", LocalizationManager.Get("vsp_title"), 28f, FontStyles.Bold, Color.white,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -20f), new Vector2(0f, -60f), TextAlignmentOptions.Center);

        // ── Moitié haute : cartes virus ──────────────────────
        GameObject topHalf = MakePanel(_overlay, "VSP_Top",
            new Vector2(0f, 0.45f), new Vector2(1f, 1f),
            new Vector4(20f, 60f, 20f, 0f),
            new Color(0.10f, 0.10f, 0.10f, 0f));

        _virusCardBorders = new Image[Entries.Length];
        _cardSelectionGlows = new Image[Entries.Length];
        float cardPad = 12f;

        for (int i = 0; i < Entries.Length; i++)
        {
            VirusEntry entry = Entries[i];
            int captured = i;

            GameObject card = new GameObject($"VSP_Card_{entry.type}");
            card.transform.SetParent(topHalf.transform, false);

            RectTransform cardRT = card.AddComponent<RectTransform>();
            float cardW = 1f / Entries.Length;
            cardRT.anchorMin = new Vector2(i * cardW, 0f);
            cardRT.anchorMax = new Vector2((i + 1) * cardW, 1f);
            cardRT.offsetMin = new Vector2(cardPad, cardPad);
            cardRT.offsetMax = new Vector2(-cardPad, -cardPad);

            Image cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.14f, 0.14f, 0.14f, 1f);
            _virusCardBorders[i] = cardImg;

            // Image glow de sélection (indépendante du bouton — pas animée par ColorBlock)
            GameObject glowGO = new GameObject("Glow");
            glowGO.transform.SetParent(card.transform, false);
            Image glowImg = glowGO.AddComponent<Image>();
            glowImg.color = new Color(entry.color.r, entry.color.g, entry.color.b, 0f);
            RectTransform glowRT = glowGO.GetComponent<RectTransform>();
            glowRT.anchorMin = Vector2.zero; glowRT.anchorMax = Vector2.one;
            glowRT.offsetMin = new Vector2(-4f, -4f); glowRT.offsetMax = new Vector2(4f, 4f);
            _cardSelectionGlows[i] = glowImg;

            Button cardBtn = card.AddComponent<Button>();
            cardBtn.transition = Selectable.Transition.None;
            cardBtn.targetGraphic = cardImg;

            // Nom du virus — grand, centré, tout en haut
            MakeText(card, "Name", LocalizationManager.Get("virus_name_" + entry.locKey), 28f, FontStyles.Bold, entry.color,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -70f), new Vector2(-8f, -10f),
                TextAlignmentOptions.Center);

            // Tagline
            MakeText(card, "Tagline", LocalizationManager.Get("virus_tagline_" + entry.locKey), 14f, FontStyles.Italic,
                new Color(0.75f, 0.75f, 0.75f, 1f),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(14f, -56f), new Vector2(-14f, -84f),
                TextAlignmentOptions.Left);

            // Séparateur
            GameObject sep = new GameObject("Sep");
            sep.transform.SetParent(card.transform, false);
            Image sepImg = sep.AddComponent<Image>();
            sepImg.color = new Color(entry.color.r, entry.color.g, entry.color.b, 0.35f);
            RectTransform sepRT = sep.GetComponent<RectTransform>();
            sepRT.anchorMin = new Vector2(0f, 1f);
            sepRT.anchorMax = new Vector2(1f, 1f);
            sepRT.offsetMin = new Vector2(14f, -88f);
            sepRT.offsetMax = new Vector2(-14f, -90f);

            // Description
            MakeText(card, "Desc", LocalizationManager.Get("virus_desc_" + entry.locKey), 17f, FontStyles.Normal,
                new Color(0.82f, 0.82f, 0.82f, 1f),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(14f, -310f), new Vector2(-14f, -98f),
                TextAlignmentOptions.TopLeft, overflow: TextOverflowModes.Ellipsis);

            // Capacités
            MakeText(card, "Caps", "►  " + LocalizationManager.Get("virus_caps_" + entry.locKey).Replace(" · ", "\n►  "), 13f, FontStyles.Normal,
                new Color(entry.color.r * 0.85f, entry.color.g * 0.85f, entry.color.b * 0.85f, 1f),
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(14f, 10f), new Vector2(-14f, 165f),
                TextAlignmentOptions.BottomLeft, overflow: TextOverflowModes.Truncate);

            cardBtn.onClick.AddListener(() => SelectVirus(captured));
        }

        // ── Séparateur horizontal ─────────────────────────────
        GameObject hSep = new GameObject("VSP_HSep");
        hSep.transform.SetParent(_overlay.transform, false);
        Image hSepImg = hSep.AddComponent<Image>();
        hSepImg.color = new Color(0.85f, 0.22f, 0.22f, 0.40f);
        RectTransform hSepRT = hSep.GetComponent<RectTransform>();
        hSepRT.anchorMin = new Vector2(0f, 0.45f);
        hSepRT.anchorMax = new Vector2(1f, 0.45f);
        hSepRT.offsetMin = new Vector2(20f, -1f);
        hSepRT.offsetMax = new Vector2(-20f, 1f);

        // ── Moitié basse : Patient 0 ──────────────────────────
        GameObject bottomHalf = MakePanel(_overlay, "VSP_Bottom",
            new Vector2(0f, 0f), new Vector2(1f, 0.45f),
            new Vector4(0f, 0f, 0f, 0f),
            new Color(0f, 0f, 0f, 0f));

        // Pays sélectionné (centré, au-dessus du titre)
        _selectedCountryLabel = MakeText(bottomHalf, "SelectedCountry", "", 20f, FontStyles.Bold,
            new Color(0.4f, 0.95f, 0.4f, 1f),
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -88f), new Vector2(0f, -44f),
            TextAlignmentOptions.Center).GetComponent<TextMeshProUGUI>();

        // Titre patient 0 (centré, grand)
        MakeText(bottomHalf, "P0Title", LocalizationManager.Get("vsp_patient0_title"), 24f, FontStyles.Bold, Color.white,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -38f), new Vector2(0f, -8f),
            TextAlignmentOptions.Center);

        // ── Ligne boutons continents ───────────────────────────
        GameObject continentRow = new GameObject("VSP_Continents");
        continentRow.transform.SetParent(bottomHalf.transform, false);
        RectTransform continentRowRT = continentRow.AddComponent<RectTransform>();
        continentRowRT.anchorMin = new Vector2(0f, 1f); continentRowRT.anchorMax = new Vector2(1f, 1f);
        continentRowRT.offsetMin = new Vector2(20f, -140f); continentRowRT.offsetMax = new Vector2(-20f, -96f);
        HorizontalLayoutGroup hlg = continentRow.AddComponent<HorizontalLayoutGroup>();
        hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true; hlg.spacing = 6f;

        for (int ci = 0; ci < ContinentData.Length; ci++)
        {
            var cd = ContinentData[ci];
            string[] captured = cd.countries;
            Color cc = cd.color;
            GameObject cbGO = new GameObject($"Cont_{ci}");
            cbGO.transform.SetParent(continentRow.transform, false);
            Image cbImg = cbGO.AddComponent<Image>();
            cbImg.color = new Color(cc.r * 0.28f, cc.g * 0.28f, cc.b * 0.28f, 1f);
            Button cbBtn = cbGO.AddComponent<Button>();
            ColorBlock cbCB = ColorBlock.defaultColorBlock;
            cbCB.normalColor   = new Color(cc.r * 0.28f, cc.g * 0.28f, cc.b * 0.28f, 1f);
            cbCB.highlightedColor = new Color(cc.r * 0.50f, cc.g * 0.50f, cc.b * 0.50f, 1f);
            cbCB.pressedColor  = new Color(cc.r * 0.18f, cc.g * 0.18f, cc.b * 0.18f, 1f);
            cbBtn.colors = cbCB; cbBtn.targetGraphic = cbImg;
            cbBtn.onClick.AddListener(() => ShowContinentCountries(captured, cc));
            GameObject cbTxtGO = new GameObject("T"); cbTxtGO.transform.SetParent(cbGO.transform, false);
            TextMeshProUGUI cbTxt = cbTxtGO.AddComponent<TextMeshProUGUI>();
            cbTxt.text = LocalizationManager.Get(cd.locKey); cbTxt.fontSize = 10f; cbTxt.fontStyle = FontStyles.Bold;
            cbTxt.color = new Color(Mathf.Min(cc.r + 0.4f, 1f), Mathf.Min(cc.g + 0.4f, 1f), Mathf.Min(cc.b + 0.4f, 1f), 1f);
            cbTxt.alignment = TextAlignmentOptions.Center;
            RectTransform cbTxtRT = cbTxtGO.GetComponent<RectTransform>();
            cbTxtRT.anchorMin = Vector2.zero; cbTxtRT.anchorMax = Vector2.one;
            cbTxtRT.offsetMin = Vector2.zero; cbTxtRT.offsetMax = Vector2.zero;
        }

        // ── Grille de pays scrollable (cachée, apparaît au clic continent) ──
        _countryGrid = new GameObject("VSP_CountryGrid");
        _countryGrid.transform.SetParent(bottomHalf.transform, false);
        RectTransform cgRT = _countryGrid.AddComponent<RectTransform>();
        cgRT.anchorMin = new Vector2(0f, 1f); cgRT.anchorMax = new Vector2(1f, 1f);
        cgRT.offsetMin = new Vector2(20f, -460f); cgRT.offsetMax = new Vector2(-20f, -148f);
        Image cgBg = _countryGrid.AddComponent<Image>();
        cgBg.color = new Color(0.07f, 0.07f, 0.07f, 0.97f);
        ScrollRect cgScroll = _countryGrid.AddComponent<ScrollRect>();
        cgScroll.horizontal = false; cgScroll.vertical = true;
        cgScroll.scrollSensitivity = 30f;
        cgScroll.movementType = ScrollRect.MovementType.Clamped;
        cgScroll.inertia = true; cgScroll.decelerationRate = 0.18f;
        // Viewport
        GameObject cgViewport = new GameObject("Viewport");
        cgViewport.transform.SetParent(_countryGrid.transform, false);
        RectTransform cgVpRT = cgViewport.AddComponent<RectTransform>();
        cgVpRT.anchorMin = Vector2.zero; cgVpRT.anchorMax = Vector2.one;
        cgVpRT.offsetMin = Vector2.zero; cgVpRT.offsetMax = Vector2.zero;
        cgViewport.AddComponent<RectMask2D>();
        // Content
        _countryContent = new GameObject("Content");
        _countryContent.transform.SetParent(cgViewport.transform, false);
        RectTransform cgConRT = _countryContent.AddComponent<RectTransform>();
        cgConRT.anchorMin = new Vector2(0f, 1f); cgConRT.anchorMax = new Vector2(1f, 1f);
        cgConRT.pivot = new Vector2(0.5f, 1f);
        cgConRT.offsetMin = Vector2.zero; cgConRT.offsetMax = Vector2.zero;
        GridLayoutGroup cgGlg = _countryContent.AddComponent<GridLayoutGroup>();
        cgGlg.cellSize = new Vector2(224f, 30f); cgGlg.spacing = new Vector2(4f, 4f);
        cgGlg.padding = new RectOffset(8, 8, 8, 8);
        cgGlg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        cgGlg.constraintCount = 8;
        ContentSizeFitter cgCsf = _countryContent.AddComponent<ContentSizeFitter>();
        cgCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        cgScroll.viewport = cgVpRT;
        cgScroll.content = cgConRT;
        _countryGrid.SetActive(false);

        // Bouton Démarrer
        GameObject startGO = new GameObject("VSP_StartBtn");
        startGO.transform.SetParent(_overlay.transform, false);
        RectTransform startRT = startGO.AddComponent<RectTransform>();
        startRT.anchorMin = new Vector2(0.3f, 0.42f); startRT.anchorMax = new Vector2(0.7f, 0.42f);
        startRT.offsetMin = new Vector2(0f, -24f); startRT.offsetMax = new Vector2(0f, 24f);
        Image startImg = startGO.AddComponent<Image>();
        startImg.color = new Color(0.55f, 0.10f, 0.10f, 1f);
        _startButton = startGO.AddComponent<Button>();
        ColorBlock sbcb = ColorBlock.defaultColorBlock;
        sbcb.normalColor    = new Color(0.75f, 0.12f, 0.12f, 1f);
        sbcb.highlightedColor = new Color(0.92f, 0.20f, 0.20f, 1f);
        sbcb.pressedColor   = new Color(0.50f, 0.08f, 0.08f, 1f);
        sbcb.disabledColor  = new Color(0.35f, 0.35f, 0.35f, 1f);
        _startButton.colors = sbcb; _startButton.targetGraphic = startImg;
        _startButton.interactable = false;
        _startButton.onClick.AddListener(OnStartClicked);

        GameObject startTextGO = new GameObject("Text");
        startTextGO.transform.SetParent(startGO.transform, false);
        _startButtonText = startTextGO.AddComponent<TextMeshProUGUI>();
        _startButtonText.text = LocalizationManager.Get("vsp_btn_start_hint");
        _startButtonText.fontSize = 15f; _startButtonText.fontStyle = FontStyles.Bold;
        _startButtonText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        _startButtonText.alignment = TextAlignmentOptions.Center;
        RectTransform stRT = startTextGO.GetComponent<RectTransform>();
        stRT.anchorMin = Vector2.zero; stRT.anchorMax = Vector2.one;
        stRT.offsetMin = Vector2.zero; stRT.offsetMax = Vector2.zero;
    }

    // ─── Sélection virus ──────────────────────────────────────
    private void SelectVirus(int index)
    {
        _selectedVirus = Entries[index].type;
        for (int i = 0; i < _virusCardBorders.Length; i++)
        {
            bool selected = i == index;
            Color ec = Entries[i].color;
            _virusCardBorders[i].color = selected
                ? new Color(0.22f, 0.22f, 0.22f, 1f)
                : new Color(0.14f, 0.14f, 0.14f, 1f);
            if (_cardSelectionGlows != null && i < _cardSelectionGlows.Length && _cardSelectionGlows[i] != null)
                _cardSelectionGlows[i].color = selected
                    ? new Color(ec.r, ec.g, ec.b, 0.22f)
                    : new Color(ec.r, ec.g, ec.b, 0f);
        }
        UpdateStartButton();
    }

    // ─── Grille continent ──────────────────────────────────────
    private void ShowContinentCountries(string[] countries, Color accentColor)
    {
        if (_countryContent == null) return;
        foreach (Transform child in _countryContent.transform)
            Destroy(child.gameObject);
        _countryGrid.SetActive(true);

        // Scroll back to top
        ScrollRect sr = _countryGrid.GetComponent<ScrollRect>();
        if (sr != null) sr.verticalNormalizedPosition = 1f;

        Color borderCol = accentColor;
        Color normalBg  = new Color(0.14f, 0.14f, 0.14f, 1f);
        Color hoverBg   = new Color(accentColor.r * 0.22f, accentColor.g * 0.22f, accentColor.b * 0.22f, 1f);

        foreach (string country in countries)
        {
            string cap = country;
            GameObject btnGO = new GameObject(country);
            btnGO.transform.SetParent(_countryContent.transform, false);
            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = normalBg;
            Button btn = btnGO.AddComponent<Button>();
            ColorBlock cb = ColorBlock.defaultColorBlock;
            cb.normalColor      = normalBg;
            cb.highlightedColor = hoverBg;
            cb.pressedColor     = new Color(0.08f, 0.08f, 0.08f, 1f);
            cb.selectedColor    = new Color(accentColor.r * 0.35f, accentColor.g * 0.35f, accentColor.b * 0.35f, 1f);
            btn.colors = cb; btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() => SelectCountry(cap));
            // Bande colorée gauche
            GameObject borderGO = new GameObject("Border");
            borderGO.transform.SetParent(btnGO.transform, false);
            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.color = borderCol;
            RectTransform borderRT = borderGO.GetComponent<RectTransform>();
            borderRT.anchorMin = new Vector2(0f, 0f); borderRT.anchorMax = new Vector2(0f, 1f);
            borderRT.pivot     = new Vector2(0f, 0.5f);
            borderRT.offsetMin = new Vector2(0f, 0f); borderRT.offsetMax = new Vector2(3f, 0f);
            // Label
            GameObject lblGO = new GameObject("L");
            lblGO.transform.SetParent(btnGO.transform, false);
            TextMeshProUGUI lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text = country; lbl.fontSize = 10f;
            lbl.color = new Color(0.90f, 0.90f, 0.90f, 1f);
            lbl.alignment = TextAlignmentOptions.Left;
            lbl.overflowMode = TextOverflowModes.Ellipsis;
            RectTransform lblRT = lblGO.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = new Vector2(8f, 2f); lblRT.offsetMax = new Vector2(-4f, -2f);
        }
    }

    private void SelectCountry(string country)
    {
        _selectedCountry = country;
        if (_countryGrid != null) _countryGrid.SetActive(false);
        if (_selectedCountryLabel != null)
            _selectedCountryLabel.text = $"{country}";
        UpdateStartButton();
    }


    // ─── Bouton Démarrer ──────────────────────────────────────
    private void UpdateStartButton()
    {
        bool ready = _selectedVirus.HasValue && !string.IsNullOrEmpty(_selectedCountry);
        _startButton.interactable = ready;
        if (_startButtonText != null)
            _startButtonText.text = ready ? LocalizationManager.Get("vsp_btn_start_ready") : LocalizationManager.Get("vsp_btn_start_hint");
        if (_startButton.targetGraphic is Image img)
            img.color = ready ? new Color(0.75f, 0.12f, 0.12f, 1f) : new Color(0.35f, 0.35f, 0.35f, 1f);
    }

    private void OnStartClicked()
    {
        if (!_selectedVirus.HasValue || string.IsNullOrEmpty(_selectedCountry)) return;
        VirusType virus = _selectedVirus.Value;
        string country = _selectedCountry;

        // Remplacer l'overlay par une barre de chargement
        StartCoroutine(ShowLoadingThenStart(virus, country));
    }

    // ─── Écran de chargement ──────────────────────────────────
    private IEnumerator ShowLoadingThenStart(VirusType virus, string country)
    {
        // Remplacer l'overlay par loading screen
        Destroy(_overlay);

        // Cacher TOUS les canvas existants pour que le fond opaque soit total
        var allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        var hiddenForLoading = new List<GameObject>();
        foreach (var c in allCanvases)
        {
            if (c.gameObject == _canvas.gameObject) continue; // le nôtre, géré séparément
            if (c.gameObject.activeSelf) { c.gameObject.SetActive(false); hiddenForLoading.Add(c.gameObject); }
        }

        // Canvas dédié loading, sortingOrder très haut
        GameObject loadingCanvasGO = new GameObject("VSP_LoadingCanvas");
        Canvas loadingCanvas = loadingCanvasGO.AddComponent<Canvas>();
        loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        loadingCanvas.sortingOrder = 999;
        loadingCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        loadingCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject loadingGO = new GameObject("VSP_Loading");
        loadingGO.transform.SetParent(loadingCanvasGO.transform, false);
        Image loadBg = loadingGO.AddComponent<Image>();
        loadBg.color = new Color(0.05f, 0.05f, 0.05f, 1f);
        RectTransform loadRT = loadingGO.GetComponent<RectTransform>();
        loadRT.anchorMin = Vector2.zero; loadRT.anchorMax = Vector2.one;
        loadRT.offsetMin = Vector2.zero; loadRT.offsetMax = Vector2.zero;

        // Titre
        MakeText(loadingGO, "LoadTitle", LocalizationManager.Get("vsp_loading_title"), 22f, FontStyles.Bold, Color.white,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 36f), new Vector2(0f, 70f), TextAlignmentOptions.Center);

        // Sous-titre
        MakeText(loadingGO, "LoadSub", LocalizationManager.Get("vsp_loading_sub"), 13f, FontStyles.Italic,
            new Color(0.6f, 0.6f, 0.6f, 1f),
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 8f), new Vector2(0f, 34f), TextAlignmentOptions.Center);

        // Barre de fond
        GameObject barBg = new GameObject("BarBg");
        barBg.transform.SetParent(loadingGO.transform, false);
        Image barBgImg = barBg.AddComponent<Image>();
        barBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform barBgRT = barBg.GetComponent<RectTransform>();
        barBgRT.anchorMin = new Vector2(0.1f, 0.5f);
        barBgRT.anchorMax = new Vector2(0.9f, 0.5f);
        barBgRT.offsetMin = new Vector2(0f, -28f);
        barBgRT.offsetMax = new Vector2(0f,  0f);

        // Barre de progression (fill)
        GameObject barFill = new GameObject("BarFill");
        barFill.transform.SetParent(barBg.transform, false);
        Image barFillImg = barFill.AddComponent<Image>();
        barFillImg.color = new Color(0.85f, 0.22f, 0.22f, 1f);
        barFillImg.type = Image.Type.Filled;
        barFillImg.fillMethod = Image.FillMethod.Horizontal;
        barFillImg.fillAmount = 0f;
        RectTransform fillRT = barFill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;

        // Label pourcentage — centré à l'intérieur de la barre
        TextMeshProUGUI pctLabel = MakeText(barBg, "Pct", "0%", 15f, FontStyles.Bold,
            Color.white,
            Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, TextAlignmentOptions.Center)
            .GetComponent<TextMeshProUGUI>();

        // Attendre que le chargement soit terminé
        GeoJsonLoader loader = FindAnyObjectByType<GeoJsonLoader>();
        // Déclencher le chargement maintenant (pas au Start)
        if (loader != null) loader.StartLoading();
        while (loader == null || !loader.IsLoaded)
        {
            float progress = loader != null ? loader.LoadProgress : 0f;
            barFillImg.fillAmount = progress;
            if (pctLabel != null) pctLabel.text = $"{Mathf.RoundToInt(progress * 100f)}%";
            yield return null;
        }

        barFillImg.fillAmount = 1f;
        if (pctLabel != null) pctLabel.text = "100%";

        // Courte pause pour que le joueur voit 100%
        yield return new WaitForSeconds(0.4f);

        Destroy(loadingCanvasGO);
        // Restaurer les canvas cachés pour le loading
        foreach (var go in hiddenForLoading)
            if (go != null) go.SetActive(true);
        // Re-activer exactement les canvas qu'on avait cachés (VirusSelectionPanel)
        foreach (var go in _hiddenCanvases)
            if (go != null) go.SetActive(true);
        _hiddenCanvases.Clear();
        if (_canvas != null) Destroy(_canvas.gameObject);
        Destroy(gameObject);
        _onReady?.Invoke(virus, country);
    }

    // ─── Helpers UI ───────────────────────────────────────────
    private static GameObject MakePanel(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector4 offsets, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(offsets.x, offsets.y);
        rt.offsetMax = new Vector2(-offsets.z, -offsets.w);
        return go;
    }

    private static GameObject MakeText(GameObject parent, string name, string text,
        float fontSize, FontStyles style, Color color,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        TextAlignmentOptions alignment,
        TextOverflowModes overflow = TextOverflowModes.Ellipsis)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.overflowMode = overflow;
        tmp.enableWordWrapping = true;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return go;
    }

    // ─── Data class ───────────────────────────────────────────
    private class VirusEntry
    {
        public VirusType type;
        public string locKey;
        public Color color;

        public VirusEntry(VirusType t, string lk, Color c)
        {
            type = t; locKey = lk; color = c;
        }
    }
}
