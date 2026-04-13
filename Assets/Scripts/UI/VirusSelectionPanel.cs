using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Popup de sélection du virus au démarrage.
/// Appelle onSelected(VirusType) puis se détruit.
/// </summary>
public class VirusSelectionPanel : MonoBehaviour
{
    private static readonly (VirusType type, string label, string desc, Color color)[] Entries =
    {
        (VirusType.Classique,     "La crottance",  "Virus classique, équilibré",           new Color(0.60f, 0.85f, 0.40f, 1f)),
        (VirusType.Cacastellaire, "Cacastellaire", "Dispersion spatiale et transmission",  new Color(0.90f, 0.65f, 0.15f, 1f)),
        (VirusType.NanoCaca,      "Nano Caca",     "Nano-pathogène furtif et létal",        new Color(0.30f, 0.70f, 1.00f, 1f)),
        (VirusType.FongiCaca,   "Fongi-Caca",  "Mycose fongique à croissance lente",   new Color(0.80f, 0.35f, 0.90f, 1f)),
    };

    public static void Show(Action<VirusType> onSelected)
    {
        GameObject go = new GameObject("VirusSelectionPanel");
        DontDestroyOnLoad(go);
        var panel = go.AddComponent<VirusSelectionPanel>();
        panel.Build(onSelected);
    }

    private void Build(Action<VirusType> onSelected)
    {
        // Canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cvGO = new GameObject("SelectionCanvas");
            canvas = cvGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            cvGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cvGO.AddComponent<GraphicRaycaster>();
        }

        // Fond noir semi-transparent sur tout l'écran
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(canvas.transform, false);
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform overlayRT = overlay.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;

        // Panel central
        GameObject panel = new GameObject("CenterPanel");
        panel.transform.SetParent(overlay.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.13f, 0.13f, 0.13f, 0.97f);
        Outline panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor    = new Color(0.85f, 0.22f, 0.22f, 0.55f);
        panelOutline.effectDistance = new Vector2(2f, 2f);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax        = new Vector2(0.5f, 0.5f);
        panelRT.pivot            = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta        = new Vector2(480f, 380f);
        panelRT.anchoredPosition = Vector2.zero;

        // Titre
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin        = new Vector2(0f, 1f);
        titleRT.anchorMax        = new Vector2(1f, 1f);
        titleRT.pivot            = new Vector2(0.5f, 1f);
        titleRT.sizeDelta        = new Vector2(0f, 50f);
        titleRT.anchoredPosition = new Vector2(0f, -12f);
        TextMeshProUGUI titleTxt = titleGO.AddComponent<TextMeshProUGUI>();
        titleTxt.text      = "CHOISISSEZ VOTRE VIRUS";
        titleTxt.fontSize  = 17f;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color     = Color.white;

        // Boutons (un par virus)
        float btnH   = 62f;
        float gap    = 10f;
        float startY = -72f;

        for (int i = 0; i < Entries.Length; i++)
        {
            var entry = Entries[i];

            GameObject btnGO = new GameObject($"Btn_{entry.type}");
            btnGO.transform.SetParent(panel.transform, false);
            RectTransform btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin        = new Vector2(0.5f, 1f);
            btnRT.anchorMax        = new Vector2(0.5f, 1f);
            btnRT.pivot            = new Vector2(0.5f, 1f);
            btnRT.sizeDelta        = new Vector2(420f, btnH);
            btnRT.anchoredPosition = new Vector2(0f, startY - i * (btnH + gap));

            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.20f, 0.20f, 0.20f, 1f);
            Outline btnOutline = btnGO.AddComponent<Outline>();
            btnOutline.effectColor    = new Color(entry.color.r, entry.color.g, entry.color.b, 0.55f);
            btnOutline.effectDistance = new Vector2(2f, 2f);

            Button btn = btnGO.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor      = new Color(0.20f, 0.20f, 0.20f, 1f);
            cb.highlightedColor = new Color(0.28f, 0.28f, 0.28f, 1f);
            cb.pressedColor     = new Color(0.15f, 0.15f, 0.15f, 1f);
            btn.colors = cb;
            btn.targetGraphic = btnImg;

            // Label + description dans le bouton
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            RectTransform labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin        = new Vector2(0f, 0.5f);
            labelRT.anchorMax        = new Vector2(1f, 1f);
            labelRT.offsetMin        = new Vector2(14f, 0f);
            labelRT.offsetMax        = new Vector2(-14f, -4f);
            TextMeshProUGUI labelTxt = labelGO.AddComponent<TextMeshProUGUI>();
            labelTxt.text      = entry.label;
            labelTxt.fontSize  = 15f;
            labelTxt.fontStyle = FontStyles.Bold;
            labelTxt.color     = entry.color;
            labelTxt.alignment = TextAlignmentOptions.Left;

            GameObject descGO = new GameObject("Desc");
            descGO.transform.SetParent(btnGO.transform, false);
            RectTransform descRT = descGO.AddComponent<RectTransform>();
            descRT.anchorMin        = new Vector2(0f, 0f);
            descRT.anchorMax        = new Vector2(1f, 0.5f);
            descRT.offsetMin        = new Vector2(14f, 4f);
            descRT.offsetMax        = new Vector2(-14f, 0f);
            TextMeshProUGUI descTxt = descGO.AddComponent<TextMeshProUGUI>();
            descTxt.text      = entry.desc;
            descTxt.fontSize  = 11f;
            descTxt.color     = new Color(0.75f, 0.75f, 0.75f, 1f);
            descTxt.alignment = TextAlignmentOptions.Left;

            VirusType captured = entry.type;
            btn.onClick.AddListener(() =>
            {
                onSelected(captured);
                Destroy(overlay);
                Destroy(gameObject);
            });
        }
    }
}
