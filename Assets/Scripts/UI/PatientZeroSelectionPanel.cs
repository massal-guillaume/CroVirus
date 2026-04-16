using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel de sélection du pays cible pour le skill "Injection Patient Zéro".
/// Affiche un menu déroulant des pays non infectés.
/// </summary>
public class PatientZeroSelectionPanel : MonoBehaviour
{
    public static void Show(List<CountryObject> uninfectedCountries, Action<CountryObject> onSelected)
    {
        if (uninfectedCountries == null || uninfectedCountries.Count == 0) return;

        GameObject go = new GameObject("PatientZeroPanel");
        DontDestroyOnLoad(go);
        go.AddComponent<PatientZeroSelectionPanel>().Build(uninfectedCountries, onSelected);
    }

    private void Build(List<CountryObject> countries, Action<CountryObject> onSelected)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cvGO = new GameObject("PZCanvas");
            canvas = cvGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;
            cvGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cvGO.AddComponent<GraphicRaycaster>();
        }

        // Fond semi-transparent
        GameObject overlay = new GameObject("PZ_Overlay");
        overlay.transform.SetParent(canvas.transform, false);
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.70f);
        RectTransform overlayRT = overlay.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;

        // Panel central
        GameObject panel = new GameObject("PZ_Panel");
        panel.transform.SetParent(overlay.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.13f, 0.13f, 0.13f, 0.97f);
        Outline panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor    = new Color(0.30f, 0.70f, 1.00f, 0.60f);
        panelOutline.effectDistance = new Vector2(2f, 2f);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax        = new Vector2(0.5f, 0.5f);
        panelRT.pivot            = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta        = new Vector2(420f, 180f);
        panelRT.anchoredPosition = Vector2.zero;

        // Titre
        GameObject titleGO = new GameObject("PZ_Title");
        titleGO.transform.SetParent(panel.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin        = new Vector2(0f, 1f);
        titleRT.anchorMax        = new Vector2(1f, 1f);
        titleRT.pivot            = new Vector2(0.5f, 1f);
        titleRT.sizeDelta        = new Vector2(0f, 44f);
        titleRT.anchoredPosition = new Vector2(0f, -12f);
        TextMeshProUGUI titleTxt = titleGO.AddComponent<TextMeshProUGUI>();
        titleTxt.text      = LocalizationManager.Get("pz_title");
        titleTxt.fontSize  = 14f;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color     = new Color(0.30f, 0.70f, 1.00f, 1f);

        // ── Dropdown ──────────────────────────────────────────────────────────
        GameObject ddGO = new GameObject("PZ_Dropdown");
        ddGO.transform.SetParent(panel.transform, false);
        RectTransform ddRT = ddGO.AddComponent<RectTransform>();
        ddRT.anchorMin        = new Vector2(0.5f, 1f);
        ddRT.anchorMax        = new Vector2(0.5f, 1f);
        ddRT.pivot            = new Vector2(0.5f, 1f);
        ddRT.sizeDelta        = new Vector2(380f, 40f);
        ddRT.anchoredPosition = new Vector2(0f, -62f);

        Image ddBg = ddGO.AddComponent<Image>();
        ddBg.color = new Color(0.20f, 0.20f, 0.20f, 1f);

        TMP_Dropdown dropdown = ddGO.AddComponent<TMP_Dropdown>();
        dropdown.targetGraphic = ddBg;

        // Label du dropdown
        GameObject ddLabel = new GameObject("Label");
        ddLabel.transform.SetParent(ddGO.transform, false);
        RectTransform ddLabelRT = ddLabel.AddComponent<RectTransform>();
        ddLabelRT.anchorMin = new Vector2(0f, 0f);
        ddLabelRT.anchorMax = new Vector2(1f, 1f);
        ddLabelRT.offsetMin = new Vector2(10f, 2f);
        ddLabelRT.offsetMax = new Vector2(-30f, -2f);
        TextMeshProUGUI ddLabelTxt = ddLabel.AddComponent<TextMeshProUGUI>();
        ddLabelTxt.fontSize  = 13f;
        ddLabelTxt.color     = Color.white;
        ddLabelTxt.alignment = TextAlignmentOptions.Left;
        dropdown.captionText = ddLabelTxt;

        // Flèche
        GameObject arrowGO = new GameObject("Arrow");
        arrowGO.transform.SetParent(ddGO.transform, false);
        RectTransform arrowRT = arrowGO.AddComponent<RectTransform>();
        arrowRT.anchorMin        = new Vector2(1f, 0.5f);
        arrowRT.anchorMax        = new Vector2(1f, 0.5f);
        arrowRT.pivot            = new Vector2(1f, 0.5f);
        arrowRT.sizeDelta        = new Vector2(24f, 24f);
        arrowRT.anchoredPosition = new Vector2(-6f, 0f);
        TextMeshProUGUI arrowTxt = arrowGO.AddComponent<TextMeshProUGUI>();
        arrowTxt.text      = "▼";
        arrowTxt.fontSize  = 11f;
        arrowTxt.color     = new Color(0.30f, 0.70f, 1.00f, 1f);
        arrowTxt.alignment = TextAlignmentOptions.Center;

        // Template (requis par TMP_Dropdown)
        GameObject templateGO = new GameObject("Template");
        templateGO.transform.SetParent(ddGO.transform, false);
        templateGO.SetActive(false);
        RectTransform templateRT = templateGO.AddComponent<RectTransform>();
        templateRT.anchorMin        = new Vector2(0f, 0f);
        templateRT.anchorMax        = new Vector2(1f, 0f);
        templateRT.pivot            = new Vector2(0.5f, 1f);
        templateRT.sizeDelta        = new Vector2(0f, 200f);
        templateRT.anchoredPosition = Vector2.zero;
        Image templateBg = templateGO.AddComponent<Image>();
        templateBg.color = new Color(0.18f, 0.18f, 0.18f, 0.98f);
        ScrollRect templateScroll = templateGO.AddComponent<ScrollRect>();
        templateScroll.horizontal = false;

        // Viewport du template
        GameObject vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(templateGO.transform, false);
        RectTransform vpRT = vpGO.AddComponent<RectTransform>();
        vpRT.anchorMin = new Vector2(0f, 0f);
        vpRT.anchorMax = new Vector2(1f, 1f);
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        Image vpImg = vpGO.AddComponent<Image>();
        vpImg.color = Color.white;
        Mask vpMask = vpGO.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;
        templateScroll.viewport = vpRT;

        // Content du template
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        RectTransform contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin        = new Vector2(0f, 1f);
        contentRT.anchorMax        = new Vector2(1f, 1f);
        contentRT.pivot            = new Vector2(0.5f, 1f);
        contentRT.sizeDelta        = new Vector2(0f, 28f);
        contentRT.anchoredPosition = Vector2.zero;
        templateScroll.content = contentRT;

        // Item template
        GameObject itemGO = new GameObject("Item");
        itemGO.transform.SetParent(contentGO.transform, false);
        RectTransform itemRT = itemGO.AddComponent<RectTransform>();
        itemRT.anchorMin  = new Vector2(0f, 0.5f);
        itemRT.anchorMax  = new Vector2(1f, 0.5f);
        itemRT.sizeDelta  = new Vector2(0f, 28f);
        Image itemBg = itemGO.AddComponent<Image>();
        itemBg.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        Toggle itemToggle = itemGO.AddComponent<Toggle>();
        itemToggle.targetGraphic = itemBg;
        ColorBlock tcb = itemToggle.colors;
        tcb.normalColor      = new Color(0.22f, 0.22f, 0.22f, 1f);
        tcb.highlightedColor = new Color(0.30f, 0.30f, 0.35f, 1f);
        tcb.selectedColor    = new Color(0.20f, 0.45f, 0.70f, 1f);
        itemToggle.colors    = tcb;

        GameObject itemLabelGO = new GameObject("Item Label");
        itemLabelGO.transform.SetParent(itemGO.transform, false);
        RectTransform itemLabelRT = itemLabelGO.AddComponent<RectTransform>();
        itemLabelRT.anchorMin = new Vector2(0f, 0f);
        itemLabelRT.anchorMax = new Vector2(1f, 1f);
        itemLabelRT.offsetMin = new Vector2(10f, 1f);
        itemLabelRT.offsetMax = new Vector2(-10f, -1f);
        TextMeshProUGUI itemLabelTxt = itemLabelGO.AddComponent<TextMeshProUGUI>();
        itemLabelTxt.fontSize  = 12f;
        itemLabelTxt.color     = Color.white;
        itemLabelTxt.alignment = TextAlignmentOptions.Left;

        dropdown.itemText = itemLabelTxt;
        dropdown.template = templateRT;

        // Remplir les options
        dropdown.options.Clear();
        foreach (var c in countries)
            dropdown.options.Add(new TMP_Dropdown.OptionData(c.name));
        dropdown.RefreshShownValue();

        // ── Bouton Confirmer ──────────────────────────────────────────────────
        GameObject confirmGO = new GameObject("PZ_Confirm");
        confirmGO.transform.SetParent(panel.transform, false);
        RectTransform confirmRT = confirmGO.AddComponent<RectTransform>();
        confirmRT.anchorMin        = new Vector2(0.5f, 0f);
        confirmRT.anchorMax        = new Vector2(0.5f, 0f);
        confirmRT.pivot            = new Vector2(0.5f, 0f);
        confirmRT.sizeDelta        = new Vector2(180f, 38f);
        confirmRT.anchoredPosition = new Vector2(0f, 18f);
        Image confirmImg = confirmGO.AddComponent<Image>();
        confirmImg.color = new Color(0.20f, 0.45f, 0.70f, 1f);
        Button confirmBtn = confirmGO.AddComponent<Button>();
        confirmBtn.targetGraphic = confirmImg;
        ColorBlock ccb = confirmBtn.colors;
        ccb.normalColor      = new Color(0.20f, 0.45f, 0.70f, 1f);
        ccb.highlightedColor = new Color(0.28f, 0.55f, 0.85f, 1f);
        ccb.pressedColor     = new Color(0.14f, 0.35f, 0.60f, 1f);
        confirmBtn.colors    = ccb;

        GameObject confirmLabelGO = new GameObject("Label");
        confirmLabelGO.transform.SetParent(confirmGO.transform, false);
        RectTransform confirmLabelRT = confirmLabelGO.AddComponent<RectTransform>();
        confirmLabelRT.anchorMin = Vector2.zero;
        confirmLabelRT.anchorMax = Vector2.one;
        confirmLabelRT.offsetMin = Vector2.zero;
        confirmLabelRT.offsetMax = Vector2.zero;
        TextMeshProUGUI confirmTxt = confirmLabelGO.AddComponent<TextMeshProUGUI>();
        confirmTxt.text      = LocalizationManager.Get("pz_btn_confirm");
        confirmTxt.fontSize  = 13f;
        confirmTxt.fontStyle = FontStyles.Bold;
        confirmTxt.color     = Color.white;
        confirmTxt.alignment = TextAlignmentOptions.Center;

        confirmBtn.onClick.AddListener(() =>
        {
            int idx = dropdown.value;
            if (idx >= 0 && idx < countries.Count)
            {
                onSelected(countries[idx]);
                Destroy(overlay);
                Destroy(gameObject);
            }
        });
    }
}
