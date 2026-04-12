using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillTreeMenuUI : MonoBehaviour
{
    private enum SkillTab
    {
        Transmission,
        Mortalite,
        Special
    }

    private enum SkillVisualState
    {
        Locked,
        Available,
        Purchased
    }

    private class SkillNodeView
    {
        public Skill Skill;
        public Button Button;
        public Image Background;
        public RectTransform Rect;
    }

    public static SkillTreeMenuUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private Button closeButton;

    [Header("Tabs")]
    [SerializeField] private Button transmissionTabButton;
    [SerializeField] private Button mortaliteTabButton;
    [SerializeField] private Button specialTabButton;

    [Header("Skill Tree Area")]
    [SerializeField] private RectTransform skillsAreaRoot;
    [SerializeField] private RectTransform connectionsLayer;
    [SerializeField] private RectTransform nodesLayer;

    [Header("Info Panel")]
    [SerializeField] private TextMeshProUGUI infoTitleText;
    [SerializeField] private TextMeshProUGUI infoDescriptionText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    [Header("Colors")]
    [SerializeField] private Color purchasedColor = new Color(0.1f, 0.8f, 0.1f, 1f);
    [SerializeField] private Color availableColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.55f);

    private readonly Dictionary<string, SkillNodeView> nodeViews = new Dictionary<string, SkillNodeView>();

    private SkillTreeManager skillTreeManager;
    private PointManager pointManager;
    private GameManager gameManager;

    private Skill selectedSkill;
    private SkillTab activeTab = SkillTab.Transmission;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (menuCanvasGroup == null)
        {
            menuCanvasGroup = GetComponent<CanvasGroup>();
        }

        HideVisualOnly();
    }

    void Start()
    {
        skillTreeManager = SkillTreeManager.GetInstance();
        pointManager = FindObjectOfType<PointManager>();
        gameManager = FindObjectOfType<GameManager>();

        if (closeButton == null)
        {
            Transform closeTransform = transform.Find("CloseButton");
            if (closeTransform != null)
                closeButton = closeTransform.GetComponent<Button>();
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(HideMenu);

        if (transmissionTabButton != null)
            transmissionTabButton.onClick.AddListener(() => SwitchTab(SkillTab.Transmission));

        if (mortaliteTabButton != null)
            mortaliteTabButton.onClick.AddListener(() => SwitchTab(SkillTab.Mortalite));

        if (specialTabButton != null)
            specialTabButton.onClick.AddListener(() => SwitchTab(SkillTab.Special));

        if (buyButton != null)
            buyButton.onClick.AddListener(BuySelectedSkill);

        ShowDefaultInfo();
    }

    void Update()
    {
        if (menuCanvasGroup == null)
            return;

        if (menuCanvasGroup.alpha <= 0f)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HideMenu();
        }
    }

    public void ShowMenu()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null && gameManager.IsGameOver())
            return;

        if (CountryInfoPopup.Instance != null)
            CountryInfoPopup.Instance.HidePopup();

        if (skillTreeManager == null)
            skillTreeManager = SkillTreeManager.GetInstance();

        if (pointManager == null)
            pointManager = FindObjectOfType<PointManager>();

        activeTab = SkillTab.Transmission;
        selectedSkill = null;

        if (gameManager != null)
            gameManager.SetPaused(true);

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }

        RefreshCurrentTab();
    }

    public void HideMenu()
    {
        HideVisualOnly();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null && !gameManager.IsGameOver())
            gameManager.SetPaused(false);
    }

    private void HideVisualOnly()
    {
        if (menuCanvasGroup == null)
            return;

        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;
    }

    private void SwitchTab(SkillTab tab)
    {
        activeTab = tab;
        selectedSkill = null;
        RefreshCurrentTab();
    }

    private void RefreshCurrentTab()
    {
        UpdateTabVisuals();

        if (activeTab == SkillTab.Transmission)
        {
            BuildTransmissionTree();
            ShowDefaultInfo();
            return;
        }

        ClearTreeVisuals();
        ShowComingSoon(activeTab);
    }

    private void UpdateTabVisuals()
    {
        SetTabActive(transmissionTabButton, activeTab == SkillTab.Transmission);
        SetTabActive(mortaliteTabButton, activeTab == SkillTab.Mortalite);
        SetTabActive(specialTabButton, activeTab == SkillTab.Special);
    }

    private void SetTabActive(Button tabButton, bool isActive)
    {
        if (tabButton == null)
            return;

        var image = tabButton.GetComponent<Image>();
        if (image == null)
            return;

        image.color = isActive ? new Color(0.92f, 0.2f, 0.2f, 1f) : new Color(0.18f, 0.18f, 0.18f, 1f);
    }

    private void BuildTransmissionTree()
    {
        ClearTreeVisuals();
        nodeViews.Clear();

        List<Skill> allSkills = TransmissionSkillTree.GetAllSkills();
        if (allSkills == null || allSkills.Count == 0)
            return;

        List<Skill> unlocked = skillTreeManager != null ? skillTreeManager.GetUnlockedSkills() : new List<Skill>();

        List<string> categories = new List<string>();
        for (int i = 0; i < allSkills.Count; i++)
        {
            if (!categories.Contains(allSkills[i].category))
                categories.Add(allSkills[i].category);
        }

        float startY = -40f;
        float rowSpacing = 95f;
        float colL1 = 45f;
        float colL2 = 215f;

        for (int c = 0; c < categories.Count; c++)
        {
            string category = categories[c];
            float y = startY - (c * rowSpacing);

            Skill l1 = null;
            Skill l2 = null;

            for (int i = 0; i < allSkills.Count; i++)
            {
                if (allSkills[i].category != category)
                    continue;

                if (allSkills[i].level == 1 && l1 == null)
                    l1 = allSkills[i];
                else if (allSkills[i].level == 2 && l2 == null)
                    l2 = allSkills[i];
            }

            if (l1 != null)
                CreateSkillNode(l1, new Vector2(colL1, y), unlocked);

            if (l2 != null)
                CreateSkillNode(l2, new Vector2(colL2, y), unlocked);
        }

        DrawDependencyLines();
    }

    private void CreateSkillNode(Skill skill, Vector2 anchoredPos, List<Skill> unlocked)
    {
        GameObject nodeGO = new GameObject($"Node_{skill.id}");
        nodeGO.transform.SetParent(nodesLayer, false);

        RectTransform rect = nodeGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(70f, 70f);

        Image image = nodeGO.AddComponent<Image>();
        Button button = nodeGO.AddComponent<Button>();

        SkillVisualState state = GetVisualState(skill, unlocked);
        image.color = GetColorForState(state);
        button.interactable = state == SkillVisualState.Available;

        button.onClick.AddListener(() => OnSkillSelected(skill));

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(nodeGO.transform, false);

        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(2, 2);
        labelRect.offsetMax = new Vector2(-2, -2);

        TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 16;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;
        label.text = skill.level == 1 ? "L1" : "L2";

        nodeViews[skill.id] = new SkillNodeView
        {
            Skill = skill,
            Button = button,
            Background = image,
            Rect = rect
        };
    }

    private void DrawDependencyLines()
    {
        foreach (var kvp in nodeViews)
        {
            Skill child = kvp.Value.Skill;
            if (child.dependencies == null || child.dependencies.Count == 0)
                continue;

            for (int i = 0; i < child.dependencies.Count; i++)
            {
                if (!nodeViews.ContainsKey(child.dependencies[i]))
                    continue;

                RectTransform from = nodeViews[child.dependencies[i]].Rect;
                RectTransform to = kvp.Value.Rect;
                CreateLineBetweenNodes(from, to);
            }
        }
    }

    private void CreateLineBetweenNodes(RectTransform from, RectTransform to)
    {
        GameObject lineGO = new GameObject("Connection");
        lineGO.transform.SetParent(connectionsLayer, false);

        RectTransform lineRect = lineGO.AddComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0, 1);
        lineRect.anchorMax = new Vector2(0, 1);
        lineRect.pivot = new Vector2(0f, 0.5f);

        Image lineImage = lineGO.AddComponent<Image>();
        lineImage.color = lineColor;

        Vector2 start = from.anchoredPosition + new Vector2(from.sizeDelta.x, -from.sizeDelta.y * 0.5f);
        Vector2 end = to.anchoredPosition + new Vector2(0f, -to.sizeDelta.y * 0.5f);
        Vector2 diff = end - start;

        float length = diff.magnitude;
        lineRect.sizeDelta = new Vector2(length, 4f);
        lineRect.anchoredPosition = start;

        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnSkillSelected(Skill skill)
    {
        selectedSkill = skill;
        UpdateInfoForSelectedSkill();
    }

    private void BuySelectedSkill()
    {
        if (selectedSkill == null || skillTreeManager == null)
            return;

        bool success = skillTreeManager.UnlockSkill(selectedSkill.id);
        if (!success)
        {
            infoTitleText.text = $"{selectedSkill.name} (Achat impossible)";
            infoDescriptionText.text = "Prérequis non remplis ou points insuffisants.";
            UpdateBuyButtonState();
            return;
        }

        BuildTransmissionTree();
        selectedSkill = skillTreeManager.GetSkill(selectedSkill.id);
        UpdateInfoForSelectedSkill();
    }

    private void UpdateInfoForSelectedSkill()
    {
        if (selectedSkill == null)
        {
            ShowDefaultInfo();
            return;
        }

        infoTitleText.text = selectedSkill.name;
        infoDescriptionText.text = selectedSkill.description;

        UpdateBuyButtonState();
    }

    private void UpdateBuyButtonState()
    {
        if (buyButton == null || buyButtonText == null)
            return;

        if (selectedSkill == null)
        {
            buyButton.interactable = false;
            buyButtonText.text = "Acheter";
            return;
        }

        List<Skill> unlocked = skillTreeManager != null ? skillTreeManager.GetUnlockedSkills() : new List<Skill>();
        int points = pointManager != null ? pointManager.GetCurrentPoints() : 0;

        bool alreadyUnlocked = selectedSkill.isUnlocked;
        bool canUnlock = selectedSkill.CanUnlock(unlocked);
        bool canAfford = selectedSkill.CanAfford(points);

        buyButton.interactable = !alreadyUnlocked && canUnlock && canAfford;

        if (alreadyUnlocked)
        {
            buyButtonText.text = "Acheté";
        }
        else
        {
            buyButtonText.text = $"Acheter ({selectedSkill.cost})";
        }
    }

    private SkillVisualState GetVisualState(Skill skill, List<Skill> unlocked)
    {
        if (skill.isUnlocked)
            return SkillVisualState.Purchased;

        if (!skill.CanUnlock(unlocked))
            return SkillVisualState.Locked;

        return SkillVisualState.Available;
    }

    private Color GetColorForState(SkillVisualState state)
    {
        if (state == SkillVisualState.Purchased)
            return purchasedColor;

        if (state == SkillVisualState.Locked)
            return lockedColor;

        return availableColor;
    }

    private void ShowDefaultInfo()
    {
        if (activeTab == SkillTab.Transmission)
        {
            infoTitleText.text = "Transmission";
            infoDescriptionText.text = "Clique sur un skill rouge pour voir ses details puis achete-le avec tes Crotogenes. Les skills L2 gris se debloquent apres achat du L1.";
            buyButton.interactable = false;
            buyButtonText.text = "Acheter";
            return;
        }

        ShowComingSoon(activeTab);
    }

    private void ShowComingSoon(SkillTab tab)
    {
        string tabName = tab == SkillTab.Mortalite ? "Mortalite" : "Special";
        infoTitleText.text = tabName;
        infoDescriptionText.text = "Contenu bientot disponible. Cet onglet sera implemente dans une prochaine phase.";
        buyButton.interactable = false;
        buyButtonText.text = "Acheter";
    }

    private void ClearTreeVisuals()
    {
        if (connectionsLayer != null)
        {
            for (int i = connectionsLayer.childCount - 1; i >= 0; i--)
            {
                Destroy(connectionsLayer.GetChild(i).gameObject);
            }
        }

        if (nodesLayer != null)
        {
            for (int i = nodesLayer.childCount - 1; i >= 0; i--)
            {
                Destroy(nodesLayer.GetChild(i).gameObject);
            }
        }
    }
}
