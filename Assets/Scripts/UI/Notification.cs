using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Generic singleton notification popup.
/// Usage: Notification.Instance.Show("TITRE", "Message");
/// Can be reused for any in-game event (mutations, events, alerts, etc.)
/// </summary>
public class Notification : MonoBehaviour
{
    public static Notification Instance { get; private set; }

    private Button panelButton;

    public void ShowEvent(string title, string message)
        => Show(title, message, new Color(0.22f, 0.55f, 0.85f, 0.85f), isEventNotification: true);

    private Button AddPanelButton()
    {
        var panel = canvasGroup.gameObject;
        var btn = panel.GetComponent<Button>();
        if (btn == null) btn = panel.AddComponent<Button>();
        return btn;
    }

    private struct Entry { public string title; public string message; public Color accentColor; public bool isEventNotification; }
    private readonly Queue<Entry> queue = new Queue<Entry>();
    private bool isShowing = false;

    private CanvasGroup canvasGroup;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI messageText;
    private Outline outline;

    private const float FadeTime = 0.25f;
    private const float HoldTime = 3.5f;
    private const float PanelW   = 520f;
    private const float PanelH   = 280f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Show a notification with a custom accent colour. Pauses the game and waits for click to close.</summary>
    public void Show(string title, string message, Color accentColor, bool isEventNotification = false)
    {
        queue.Enqueue(new Entry { title = title, message = message, accentColor = accentColor, isEventNotification = isEventNotification });
        if (!isShowing) StartCoroutine(ProcessQueue());
    }

    /// <summary>Show a notification with the default red accent.</summary>
    public void Show(string title, string message)
        => Show(title, message, new Color(0.85f, 0.22f, 0.22f, 0.65f));

    // ── Internal ──────────────────────────────────────────────────────────────

    private IEnumerator ProcessQueue()
    {
        isShowing = true;
        var gm = FindAnyObjectByType<GameManager>();
        panelButton ??= AddPanelButton();
        while (queue.Count > 0)
        {
            // Attendre que le menu Evolution soit fermé avant d'afficher
            while (SkillTreeMenuUI.IsOpen)
                yield return null;

            Entry e = queue.Dequeue();
            titleText.text   = e.title;
            messageText.text = e.message;
            titleText.color  = Color.white;
            if (outline != null) outline.effectColor = e.accentColor;

            if (gm != null) gm.PauseGame();

            yield return StartCoroutine(Fade(0f, 1f, FadeTime));

            // Attend le clic du joueur pour fermer
            bool closed = false;
            panelButton.onClick.RemoveAllListeners();
            panelButton.onClick.AddListener(() => closed = true);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            while (!closed) yield return null;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            yield return StartCoroutine(Fade(1f, 0f, FadeTime));

            if (gm != null) gm.ResumeGame();
            if (e.isEventNotification && EventManager.Instance != null)
                EventManager.Instance.OnEventNotificationClosed();
        }
        isShowing = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        canvasGroup.alpha = from;
        canvasGroup.interactable   = to > 0.5f;
        canvasGroup.blocksRaycasts = to > 0.5f;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    private void BuildUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cvGO = new GameObject("NotificationCanvas");
            canvas = cvGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            cvGO.AddComponent<CanvasScaler>();
            cvGO.AddComponent<GraphicRaycaster>();
        }

        GameObject panel = new GameObject("NotificationPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(PanelW, PanelH);
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.08f, 0.97f);

        outline = panel.AddComponent<Outline>();
        outline.effectColor    = new Color(0.85f, 0.22f, 0.22f, 0.65f);
        outline.effectDistance = new Vector2(2f, 2f);

        canvasGroup = panel.AddComponent<CanvasGroup>();

        // Title (top ~30%)
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.72f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.offsetMin = new Vector2(12f, 0f);
        titleRT.offsetMax = new Vector2(-12f, -8f);
        titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.fontSize           = 22f;
        titleText.fontStyle          = FontStyles.Bold;
        titleText.alignment          = TextAlignmentOptions.Center;
        titleText.color              = Color.white;
        titleText.enableAutoSizing   = true;
        titleText.fontSizeMin        = 14f;
        titleText.fontSizeMax        = 22f;
        titleText.enableWordWrapping = true;
        titleText.overflowMode       = TextOverflowModes.Overflow;

        // Message (bottom ~70%)
        GameObject msgGO = new GameObject("Message");
        msgGO.transform.SetParent(panel.transform, false);
        RectTransform msgRT = msgGO.AddComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0f, 0f);
        msgRT.anchorMax = new Vector2(1f, 0.72f);
        msgRT.offsetMin = new Vector2(14f, 8f);
        msgRT.offsetMax = new Vector2(-14f, -4f);
        messageText = msgGO.AddComponent<TextMeshProUGUI>();
        messageText.fontSize           = 15f;
        messageText.fontStyle          = FontStyles.Normal;
        messageText.alignment          = TextAlignmentOptions.Center;
        messageText.color              = Color.white;
        messageText.enableAutoSizing   = true;
        messageText.fontSizeMin        = 9f;
        messageText.fontSizeMax        = 15f;
        messageText.enableWordWrapping = true;
        messageText.overflowMode       = TextOverflowModes.Overflow;
    }
}
