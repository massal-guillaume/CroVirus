using System.Collections.Generic;
using System.Linq;
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
        public Graphic Background;
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
    [SerializeField] private TextMeshProUGUI infoImpactText;
    [SerializeField] private TextMeshProUGUI infoCostText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    [Header("Colors")]
    [SerializeField] private Color purchasedColor = new Color(0.1f, 0.8f, 0.1f, 1f);
    [SerializeField] private Color availableColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.55f);
    [SerializeField] private Color gridHexColor = new Color(1f, 1f, 1f, 0.08f);

    private readonly Dictionary<string, SkillNodeView> nodeViews = new Dictionary<string, SkillNodeView>();
    private readonly Dictionary<string, Vector2Int> nodeCells = new Dictionary<string, Vector2Int>();
    private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
    private int placementMaxCellX;
    private int placementMaxCellY;
    private bool hasPlacementBounds;

    private const float TransmissionHexSize = 78f;
    private const float MortaliteHexSize = 110f;
    // Flat-top hex grid spacing (truly contiguous, no gaps):
    // horizontal = 1.5 * radius, vertical = sqrt(3) * radius
    private float CurrentHexSize => (activeTab == SkillTab.Mortalite || activeTab == SkillTab.Special) ? MortaliteHexSize : TransmissionHexSize;
    private float CurrentHexRadius => CurrentHexSize * 0.5f;
    private float CurrentHexStepX => CurrentHexRadius * 1.5f;
    private float CurrentHexStepY => CurrentHexRadius * 1.7320508f;
    private const float GridStartX = 0f;
    private const float GridStartY = -64f;

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

        if (buyButton == null)
        {
            Transform buyTransform = transform.Find("Body/InfoPanel/BuyButton");
            if (buyTransform != null)
                buyButton = buyTransform.GetComponent<Button>();
        }

        if (buyButtonText == null && buyButton != null)
        {
            buyButtonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        ApplyBuyButtonStyle();

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

        if (activeTab == SkillTab.Mortalite)
        {
            BuildMortaliteTree();
            ShowDefaultInfo();
            return;
        }

        if (activeTab == SkillTab.Special)
        {
            BuildSpecialTree();
            ShowDefaultInfoForSpecial();
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
        nodeCells.Clear();
        occupiedCells.Clear();

        BuildHexBackdropGrid();

        List<Skill> allSkills = TransmissionSkillTree.GetAllSkills();
        if (allSkills == null || allSkills.Count == 0)
            return;

        List<Skill> unlocked = skillTreeManager != null ? skillTreeManager.GetUnlockedSkills() : new List<Skill>();

        List<Skill> level1Skills = allSkills.Where(s => s.level == 1).OrderBy(s => s.id).ToList();
        List<Skill> level2Skills = allSkills.Where(s => s.level == 2).OrderBy(s => s.id).ToList();
        List<Skill> level3Skills = allSkills.Where(s => s.level == 3).OrderBy(s => s.id).ToList();

        Dictionary<string, Vector2Int> positions = new Dictionary<string, Vector2Int>();

        float areaWidth = (skillsAreaRoot != null && skillsAreaRoot.rect.width > 1f) ? skillsAreaRoot.rect.width : 980f;
        float areaHeight = (skillsAreaRoot != null && skillsAreaRoot.rect.height > 1f) ? skillsAreaRoot.rect.height : 780f;
        int maxCellX = Mathf.Max(2, Mathf.FloorToInt((areaWidth - GridStartX - CurrentHexSize) / CurrentHexStepX));
        int maxCellY = Mathf.Max(2, Mathf.FloorToInt((areaHeight + Mathf.Abs(GridStartY) - CurrentHexSize) / CurrentHexStepY));
        placementMaxCellX = maxCellX;
        placementMaxCellY = maxCellY;
        hasPlacementBounds = true;

        Dictionary<string, string> l2ParentById = new Dictionary<string, string>();
        foreach (Skill l2 in level2Skills)
        {
            if (l2.dependencies != null && l2.dependencies.Count > 0)
                l2ParentById[l2.id] = l2.dependencies[0];
        }

        Dictionary<string, string> fusionPartnerByL2 = new Dictionary<string, string>();
        List<(string parentA, string parentB)> fusionParentPairs = new List<(string, string)>();
        HashSet<string> pairKeys = new HashSet<string>();

        foreach (Skill l3 in level3Skills)
        {
            if (l3.dependencies == null || l3.dependencies.Count < 2)
                continue;

            string l2A = l3.dependencies[0];
            string l2B = l3.dependencies[1];
            fusionPartnerByL2[l2A] = l2B;
            fusionPartnerByL2[l2B] = l2A;

            if (!l2ParentById.ContainsKey(l2A) || !l2ParentById.ContainsKey(l2B))
                continue;

            string parentA = l2ParentById[l2A];
            string parentB = l2ParentById[l2B];
            string pairKey = string.CompareOrdinal(parentA, parentB) <= 0 ? parentA + "|" + parentB : parentB + "|" + parentA;
            if (pairKeys.Add(pairKey))
                fusionParentPairs.Add((parentA, parentB));
        }

        // Place L1 parents that share an L3 in small local clusters.
        for (int i = 0; i < fusionParentPairs.Count; i++)
        {
            var pair = fusionParentPairs[i];
            int clusterCol = i % 2;
            int clusterRow = i / 2;
            Vector2Int clusterStart = ClampCellToVisible(new Vector2Int(2 + clusterCol * 8, 2 + clusterRow * 7), maxCellX, maxCellY);

            if (!positions.ContainsKey(pair.parentA))
            {
                Vector2Int aCell = FindNearestFreeCell(clusterStart, 10);
                positions[pair.parentA] = aCell;
                Skill aSkill = level1Skills.FirstOrDefault(s => s.id == pair.parentA);
                if (aSkill != null)
                    CreateSkillNode(aSkill, CellToPosition(aCell), unlocked);
            }

            if (!positions.ContainsKey(pair.parentB))
            {
                Vector2Int bTarget = ClampCellToVisible(new Vector2Int(clusterStart.x + 3, clusterStart.y + 1), maxCellX, maxCellY);
                Vector2Int bCell = FindNearestFreeCell(bTarget, 10);
                positions[pair.parentB] = bCell;
                Skill bSkill = level1Skills.FirstOrDefault(s => s.id == pair.parentB);
                if (bSkill != null)
                    CreateSkillNode(bSkill, CellToPosition(bCell), unlocked);
            }
        }

        // Place remaining L1 with a scattered layout across the map.
        List<Skill> remainingL1 = level1Skills.Where(s => !positions.ContainsKey(s.id)).ToList();

        // Force specific skills to the right side so important late-game branches are easier to spot.
        Dictionary<string, Vector2Int> forcedL1Cells = new Dictionary<string, Vector2Int>
        {
            { "vaccineSabotageL1", new Vector2Int(maxCellX - 4, 2) }
        };

        foreach (var forced in forcedL1Cells)
        {
            Skill forcedSkill = remainingL1.FirstOrDefault(s => s.id == forced.Key);
            if (forcedSkill == null)
                continue;

            Vector2Int wanted = ClampCellToVisible(forced.Value, maxCellX, maxCellY);
            Vector2Int cell = FindNearestFreeCell(wanted, 14);
            positions[forcedSkill.id] = cell;
            CreateSkillNode(forcedSkill, CellToPosition(cell), unlocked);
            remainingL1.Remove(forcedSkill);
        }

        int scatterCols = 3;
        for (int i = 0; i < remainingL1.Count; i++)
        {
            int row = i / scatterCols;
            int col = i % scatterCols;
            int zig = (row % 2 == 0) ? 0 : 2;

            // Keep remaining L1 nodes in the visible upper/middle area.
            Vector2Int wantedCell = ClampCellToVisible(new Vector2Int(1 + col * 7 + zig, 4 + row * 3 + (col % 2)), maxCellX, maxCellY);
            Vector2Int cell = FindNearestFreeCell(wantedCell, 12);

            positions[remainingL1[i].id] = cell;
            CreateSkillNode(remainingL1[i], CellToPosition(cell), unlocked);
        }

        // Level 2: MUST be adjacent to its level 1 parent
        for (int i = 0; i < level2Skills.Count; i++)
        {
            Skill skill = level2Skills[i];
            if (!ShouldShowSkill(skill, unlocked))
                continue;

            if (skill.dependencies == null || skill.dependencies.Count == 0)
                continue;

            string parentId = skill.dependencies[0];
            if (!positions.ContainsKey(parentId))
                continue;

            Vector2Int parentCell = positions[parentId];

            Vector2Int cell;
            string partnerId;
            if (fusionPartnerByL2.TryGetValue(skill.id, out partnerId))
            {
                if (positions.ContainsKey(partnerId))
                {
                    cell = FindAdjacentCellWithSharedNeighbor(parentCell, positions[partnerId]);
                    if (cell == parentCell)
                        cell = FindAdjacentFreeCell(parentCell);
                }
                else if (l2ParentById.ContainsKey(partnerId) && positions.ContainsKey(l2ParentById[partnerId]))
                {
                    cell = FindAdjacentCellClosestTo(parentCell, positions[l2ParentById[partnerId]]);
                }
                else
                {
                    cell = FindAdjacentFreeCell(parentCell);
                }
            }
            else
            {
                cell = FindAdjacentFreeCell(parentCell);
            }
            
            if (cell == parentCell)  // No free adjacent cell found, fallback
                continue;

            positions[skill.id] = cell;
            CreateSkillNode(skill, CellToPosition(cell), unlocked);
        }

        // Level 3 fusion: MUST be adjacent to both its L2 parents
        for (int i = 0; i < level3Skills.Count; i++)
        {
            Skill skill = level3Skills[i];
            if (!ShouldShowSkill(skill, unlocked))
                continue;

            if (skill.dependencies == null || skill.dependencies.Count < 2)
                continue;

            string depA = skill.dependencies[0];
            string depB = skill.dependencies[1];
            if (!positions.ContainsKey(depA) || !positions.ContainsKey(depB))
                continue;

            Vector2Int a = positions[depA];
            Vector2Int b = positions[depB];
            
            // Find adjacent cell that is free and adjacent to BOTH L2 parents
            Vector2Int cell = FindFusionAdjacentCell(a, b);
            if (!AreAdjacent(cell, a) || !AreAdjacent(cell, b))
                continue;

            positions[skill.id] = cell;
            CreateSkillNode(skill, CellToPosition(cell), unlocked);
        }

        // No dependency lines: user requested zero white links between hexagons.
    }

    private void BuildMortaliteTree()
    {
        ClearTreeVisuals();
        nodeViews.Clear();
        nodeCells.Clear();
        occupiedCells.Clear();

        BuildHexBackdropGrid();

        List<Skill> allSkills = MortaliteSkillTree.GetAllSkills();
        if (allSkills == null || allSkills.Count == 0)
            return;

        List<Skill> unlocked = skillTreeManager != null ? skillTreeManager.GetUnlockedSkills() : new List<Skill>();

        List<Skill> level1Skills = allSkills.Where(s => s.level == 1).OrderBy(s => s.id).ToList();
        List<Skill> level2Skills = allSkills.Where(s => s.level == 2).OrderBy(s => s.id).ToList();
        List<Skill> level3Skills = allSkills.Where(s => s.level == 3).OrderBy(s => s.id).ToList();
        List<Skill> level4Skills = allSkills.Where(s => s.level == 4).OrderBy(s => s.id).ToList();

        Dictionary<string, Vector2Int> positions = new Dictionary<string, Vector2Int>();

        float areaWidth = (skillsAreaRoot != null && skillsAreaRoot.rect.width > 1f) ? skillsAreaRoot.rect.width : 980f;
        float areaHeight = (skillsAreaRoot != null && skillsAreaRoot.rect.height > 1f) ? skillsAreaRoot.rect.height : 780f;
        int maxCellX = Mathf.Max(2, Mathf.FloorToInt((areaWidth - GridStartX - CurrentHexSize) / CurrentHexStepX));
        int maxCellY = Mathf.Max(2, Mathf.FloorToInt((areaHeight + Mathf.Abs(GridStartY) - CurrentHexSize) / CurrentHexStepY));
        placementMaxCellX = maxCellX;
        placementMaxCellY = maxCellY;
        hasPlacementBounds = true;

        // Use a deterministic fixed layout so placements stay stable between refreshes.
        BuildMortaliteTreeFixedLayout(allSkills, unlocked);
        return;

        Dictionary<string, string> l2ParentById = new Dictionary<string, string>();
        foreach (Skill l2 in level2Skills)
        {
            if (l2.dependencies != null && l2.dependencies.Count > 0)
                l2ParentById[l2.id] = l2.dependencies[0];
        }

        Dictionary<string, List<string>> fusionPartnersByL2 = new Dictionary<string, List<string>>();
        List<(string parentA, string parentB)> fusionParentPairs = new List<(string, string)>();
        HashSet<string> pairKeys = new HashSet<string>();

        foreach (Skill l3 in level3Skills)
        {
            if (l3.dependencies == null || l3.dependencies.Count < 2)
                continue;

            string l2A = l3.dependencies[0];
            string l2B = l3.dependencies[1];

            if (!fusionPartnersByL2.ContainsKey(l2A))
                fusionPartnersByL2[l2A] = new List<string>();
            if (!fusionPartnersByL2[l2A].Contains(l2B))
                fusionPartnersByL2[l2A].Add(l2B);

            if (!fusionPartnersByL2.ContainsKey(l2B))
                fusionPartnersByL2[l2B] = new List<string>();
            if (!fusionPartnersByL2[l2B].Contains(l2A))
                fusionPartnersByL2[l2B].Add(l2A);

            if (!l2ParentById.ContainsKey(l2A) || !l2ParentById.ContainsKey(l2B))
                continue;

            string parentA = l2ParentById[l2A];
            string parentB = l2ParentById[l2B];
            string pairKey = string.CompareOrdinal(parentA, parentB) <= 0 ? parentA + "|" + parentB : parentB + "|" + parentA;
            if (pairKeys.Add(pairKey))
                fusionParentPairs.Add((parentA, parentB));
        }

        // Place L1 parents that share an L3 in small local clusters.
        for (int i = 0; i < fusionParentPairs.Count; i++)
        {
            var pair = fusionParentPairs[i];
            int clusterCol = i % 2;
            int clusterRow = i / 2;
            Vector2Int clusterStart = ClampCellToVisible(new Vector2Int(2 + clusterCol * 8, 2 + clusterRow * 7), maxCellX, maxCellY);

            if (!positions.ContainsKey(pair.parentA))
            {
                Vector2Int aCell = FindNearestFreeCell(clusterStart, 10);
                positions[pair.parentA] = aCell;
                Skill aSkill = level1Skills.FirstOrDefault(s => s.id == pair.parentA);
                if (aSkill != null)
                    CreateSkillNode(aSkill, CellToPosition(aCell), unlocked);
            }

            if (!positions.ContainsKey(pair.parentB))
            {
                Vector2Int bTarget = ClampCellToVisible(new Vector2Int(clusterStart.x + 3, clusterStart.y + 1), maxCellX, maxCellY);
                Vector2Int bCell = FindNearestFreeCell(bTarget, 10);
                positions[pair.parentB] = bCell;
                Skill bSkill = level1Skills.FirstOrDefault(s => s.id == pair.parentB);
                if (bSkill != null)
                    CreateSkillNode(bSkill, CellToPosition(bCell), unlocked);
            }
        }

        // Place remaining L1 with a scattered layout across the map.
        List<Skill> remainingL1 = level1Skills.Where(s => !positions.ContainsKey(s.id)).ToList();

        int scatterCols = 3;
        for (int i = 0; i < remainingL1.Count; i++)
        {
            int row = i / scatterCols;
            int col = i % scatterCols;
            int zig = (row % 2 == 0) ? 0 : 2;

            Vector2Int wantedCell = ClampCellToVisible(new Vector2Int(1 + col * 7 + zig, 4 + row * 3 + (col % 2)), maxCellX, maxCellY);
            Vector2Int cell = FindNearestFreeCell(wantedCell, 12);

            positions[remainingL1[i].id] = cell;
            CreateSkillNode(remainingL1[i], CellToPosition(cell), unlocked);
        }

        // Level 2: MUST be adjacent to its level 1 parent
        for (int i = 0; i < level2Skills.Count; i++)
        {
            Skill skill = level2Skills[i];
            if (!ShouldShowSkill(skill, unlocked))
                continue;

            if (skill.dependencies == null || skill.dependencies.Count == 0)
                continue;

            string parentId = skill.dependencies[0];
            if (!positions.ContainsKey(parentId))
                continue;

            Vector2Int parentCell = positions[parentId];

            Vector2Int cell = parentCell;
            List<string> partnerIds;
            if (fusionPartnersByL2.TryGetValue(skill.id, out partnerIds) && partnerIds.Count > 0)
            {
                bool placedWithPartner = false;

                // Prefer a placement that leaves at least one strict shared-neighbor slot with an already placed partner.
                for (int p = 0; p < partnerIds.Count; p++)
                {
                    string partnerId = partnerIds[p];
                    if (!positions.ContainsKey(partnerId))
                        continue;

                    Vector2Int candidate = FindAdjacentCellWithSharedNeighbor(parentCell, positions[partnerId]);
                    if (candidate != parentCell)
                    {
                        cell = candidate;
                        placedWithPartner = true;
                        break;
                    }
                }

                if (!placedWithPartner)
                {
                    // If no partner is placed yet, bias toward the average of partner-parent L1 anchors.
                    int tx = parentCell.x;
                    int ty = parentCell.y;
                    int count = 1;
                    for (int p = 0; p < partnerIds.Count; p++)
                    {
                        string partnerId = partnerIds[p];
                        if (!l2ParentById.ContainsKey(partnerId))
                            continue;

                        string partnerL1 = l2ParentById[partnerId];
                        if (!positions.ContainsKey(partnerL1))
                            continue;

                        tx += positions[partnerL1].x;
                        ty += positions[partnerL1].y;
                        count++;
                    }

                    Vector2Int target = new Vector2Int(tx / count, ty / count);
                    cell = FindAdjacentCellClosestTo(parentCell, target);
                    if (cell == parentCell)
                        cell = FindAdjacentFreeCell(parentCell);
                }
            }
            else
            {
                cell = FindAdjacentFreeCell(parentCell);
            }
            
            if (cell == parentCell)
                continue;

            positions[skill.id] = cell;
            CreateSkillNode(skill, CellToPosition(cell), unlocked);
        }

        // Level 3: strict fusion placement between two L2 parents, with conflict-free assignment.
        Dictionary<string, Vector2Int> plannedL3Cells = new Dictionary<string, Vector2Int>();
        // L1 should not block fusion slots: reserve only already-placed L2 cells for strict L3 assignment.
        HashSet<Vector2Int> tempUsedCells = new HashSet<Vector2Int>();
        for (int i = 0; i < level2Skills.Count; i++)
        {
            Skill l2 = level2Skills[i];
            if (positions.ContainsKey(l2.id))
                tempUsedCells.Add(positions[l2.id]);
        }
        List<(Skill skill, List<Vector2Int> candidates)> l3Placements = new List<(Skill, List<Vector2Int>)>();

        for (int i = 0; i < level3Skills.Count; i++)
        {
            Skill skill = level3Skills[i];
            if (!ShouldShowSkill(skill, unlocked))
                continue;

            if (skill.dependencies == null || skill.dependencies.Count < 2)
                continue;

            string depA = skill.dependencies[0];
            string depB = skill.dependencies[1];
            if (!positions.ContainsKey(depA) || !positions.ContainsKey(depB))
                continue;

            Vector2Int a = positions[depA];
            Vector2Int b = positions[depB];
            List<Vector2Int> na = GetHexNeighbors(a);
            List<Vector2Int> nb = GetHexNeighbors(b);
            List<Vector2Int> candidates = new List<Vector2Int>();

            for (int n = 0; n < na.Count; n++)
            {
                Vector2Int cell = na[n];
                if (!IsInsidePlacementBounds(cell))
                    continue;
                if (!nb.Contains(cell))
                    continue;
                if (tempUsedCells.Contains(cell))
                    continue;
                candidates.Add(cell);
            }

            l3Placements.Add((skill, candidates));
        }

        l3Placements = l3Placements.OrderBy(x => x.candidates.Count).ToList();

        bool solved = TryAssignL3CellsRecursive(l3Placements, 0, tempUsedCells, plannedL3Cells);
        if (solved)
        {
            foreach (var kvp in plannedL3Cells)
            {
                Skill skill = level3Skills.FirstOrDefault(s => s.id == kvp.Key);
                if (skill == null)
                    continue;

                occupiedCells.Add(kvp.Value);
                positions[skill.id] = kvp.Value;
                CreateSkillNode(skill, CellToPosition(kvp.Value), unlocked);
            }
        }

        // Level 4 fusion: strict placement between two L3 parents
        for (int i = 0; i < level4Skills.Count; i++)
        {
            Skill skill = level4Skills[i];
            if (!ShouldShowSkill(skill, unlocked))
                continue;

            if (skill.dependencies == null || skill.dependencies.Count < 2)
                continue;

            string depA = skill.dependencies[0];
            string depB = skill.dependencies[1];
            if (!positions.ContainsKey(depA) || !positions.ContainsKey(depB))
                continue;

            Vector2Int a = positions[depA];
            Vector2Int b = positions[depB];
            
            // Must be adjacent to both parents. Ignore L1 occupancy so decorative chain roots never block fusions.
            Vector2Int cell = FindDoubleFusionAdjacentCellIgnoringL1(a, b, positions, level2Skills, level3Skills, level4Skills);
            if (!AreAdjacent(cell, a) || !AreAdjacent(cell, b))
                continue;

            positions[skill.id] = cell;
            CreateSkillNode(skill, CellToPosition(cell), unlocked);
        }
    }

    private void BuildMortaliteTreeFixedLayout(List<Skill> allSkills, List<Skill> unlocked)
    {
        Vector2Int layoutOffset = new Vector2Int(0, -2);

        Dictionary<string, Vector2Int> fixedCells = new Dictionary<string, Vector2Int>
        {
            { "anusL1", new Vector2Int(3, 5) },
            { "anusL2", new Vector2Int(4, 5) },
            { "poumonL1", new Vector2Int(5, 5) },
            { "poumonL2", new Vector2Int(6, 5) },
            { "bouffeL1", new Vector2Int(9, 5) },
            { "bouffeL2", new Vector2Int(8, 5) },
            { "folieL1", new Vector2Int(9, 7) },
            { "folieL2", new Vector2Int(8, 7) },
            { "cerveauL1", new Vector2Int(5, 7) },
            { "cerveauL2", new Vector2Int(6, 7) },

            { "anusL3", new Vector2Int(5, 4) },
            { "poumonL3", new Vector2Int(7, 4) },
            { "bouffeL3", new Vector2Int(8, 6) },
            { "folieL3", new Vector2Int(7, 7) },
            { "cerveauL3", new Vector2Int(6, 6) },

            { "fusionLiquefactionL4", new Vector2Int(6, 4) },
            { "fusionContaminationL4", new Vector2Int(7, 5) },
            { "fusionApothéoseL4", new Vector2Int(7, 6) }
        };

        List<Skill> orderedSkills = allSkills
            .OrderBy(s => s.level)
            .ThenBy(s => s.id)
            .ToList();

        for (int i = 0; i < orderedSkills.Count; i++)
        {
            Skill skill = orderedSkills[i];
            if (!fixedCells.ContainsKey(skill.id))
                continue;

            if (!ShouldShowSkill(skill, unlocked))
                continue;

            Vector2Int cell = fixedCells[skill.id];
            cell += layoutOffset;
            if (!IsInsidePlacementBounds(cell))
                continue;

            if (occupiedCells.Contains(cell))
                continue;

            occupiedCells.Add(cell);
            CreateSkillNode(skill, CellToPosition(cell), unlocked);
        }

        // Verify adjacency constraints at runtime for debugging and maintenance.
        for (int i = 0; i < orderedSkills.Count; i++)
        {
            Skill skill = orderedSkills[i];
            if (!fixedCells.ContainsKey(skill.id) || skill.dependencies == null)
                continue;

            Vector2Int childCell = fixedCells[skill.id] + layoutOffset;
            for (int d = 0; d < skill.dependencies.Count; d++)
            {
                string depId = skill.dependencies[d];
                if (!fixedCells.ContainsKey(depId))
                    continue;

                if (!AreAdjacent(childCell, fixedCells[depId] + layoutOffset))
                    Debug.LogWarning($"[MortaliteLayout] Non-adjacent dependency: {skill.id} -> {depId}");
            }
        }
    }

    private Vector2Int FindDoubleFusionAdjacentCellIgnoringL1(
        Vector2Int a,
        Vector2Int b,
        Dictionary<string, Vector2Int> positions,
        List<Skill> level2Skills,
        List<Skill> level3Skills,
        List<Skill> level4Skills)
    {
        HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();

        for (int i = 0; i < level2Skills.Count; i++)
        {
            Skill s = level2Skills[i];
            if (positions.ContainsKey(s.id))
                blocked.Add(positions[s.id]);
        }

        for (int i = 0; i < level3Skills.Count; i++)
        {
            Skill s = level3Skills[i];
            if (positions.ContainsKey(s.id))
                blocked.Add(positions[s.id]);
        }

        for (int i = 0; i < level4Skills.Count; i++)
        {
            Skill s = level4Skills[i];
            if (positions.ContainsKey(s.id))
                blocked.Add(positions[s.id]);
        }

        List<Vector2Int> na = GetHexNeighbors(a);
        List<Vector2Int> nb = GetHexNeighbors(b);

        for (int i = 0; i < na.Count; i++)
        {
            Vector2Int candidate = na[i];
            if (!IsInsidePlacementBounds(candidate))
                continue;
            if (!nb.Contains(candidate))
                continue;
            if (blocked.Contains(candidate))
                continue;

            return candidate;
        }

        return a;
    }

    private Vector2Int FindTripleFusionAdjacentCellIgnoringL1(
        Vector2Int a,
        Vector2Int b,
        Vector2Int c,
        Dictionary<string, Vector2Int> positions,
        List<Skill> level2Skills,
        List<Skill> level3Skills,
        List<Skill> level4Skills)
    {
        HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();

        for (int i = 0; i < level2Skills.Count; i++)
        {
            Skill s = level2Skills[i];
            if (positions.ContainsKey(s.id))
                blocked.Add(positions[s.id]);
        }

        for (int i = 0; i < level3Skills.Count; i++)
        {
            Skill s = level3Skills[i];
            if (positions.ContainsKey(s.id))
                blocked.Add(positions[s.id]);
        }

        for (int i = 0; i < level4Skills.Count; i++)
        {
            Skill s = level4Skills[i];
            if (positions.ContainsKey(s.id))
                blocked.Add(positions[s.id]);
        }

        List<Vector2Int> na = GetHexNeighbors(a);
        List<Vector2Int> nb = GetHexNeighbors(b);
        List<Vector2Int> nc = GetHexNeighbors(c);

        for (int i = 0; i < na.Count; i++)
        {
            Vector2Int candidate = na[i];
            if (!IsInsidePlacementBounds(candidate))
                continue;
            if (!nb.Contains(candidate) || !nc.Contains(candidate))
                continue;
            if (blocked.Contains(candidate))
                continue;

            return candidate;
        }

        return a;
    }

    private Vector2Int FindTripleFusionAdjacentCell(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        // Try to find a cell that is adjacent to all three parents
        List<Vector2Int> aNeighbors = GetHexNeighbors(a);
        List<Vector2Int> bNeighbors = GetHexNeighbors(b);
        List<Vector2Int> cNeighbors = GetHexNeighbors(c);
        
        // Find intersection: cells adjacent to all 3 parents
        HashSet<Vector2Int> intersection = new HashSet<Vector2Int>();
        foreach (Vector2Int n in aNeighbors)
        {
            if (bNeighbors.Contains(n) && cNeighbors.Contains(n))
                intersection.Add(n);
        }
        
        // Return first free cell from the triple intersection
        foreach (Vector2Int cell in intersection)
            if (!occupiedCells.Contains(cell))
                return cell;
        
        return a;
    }

            private bool TryAssignL3CellsRecursive(
                List<(Skill skill, List<Vector2Int> candidates)> l3Placements,
                int index,
                HashSet<Vector2Int> usedCells,
                Dictionary<string, Vector2Int> assignments)
            {
                if (index >= l3Placements.Count)
                    return true;

                var current = l3Placements[index];
                if (current.candidates == null || current.candidates.Count == 0)
                    return false;

                for (int i = 0; i < current.candidates.Count; i++)
                {
                    Vector2Int cell = current.candidates[i];
                    if (usedCells.Contains(cell))
                        continue;

                    usedCells.Add(cell);
                    assignments[current.skill.id] = cell;

                    if (TryAssignL3CellsRecursive(l3Placements, index + 1, usedCells, assignments))
                        return true;

                    assignments.Remove(current.skill.id);
                    usedCells.Remove(cell);
                }

                return false;
            }

    private void BuildHexBackdropGrid()
    {
        if (connectionsLayer == null || skillsAreaRoot == null)
            return;

        Rect areaRect = skillsAreaRoot.rect;
        float areaWidth = areaRect.width > 1f ? areaRect.width : 980f;
        float areaHeight = areaRect.height > 1f ? areaRect.height : 780f;

        // Render a contiguous hex map using the same cell-to-position conversion as skill nodes.
        // Extend grid beyond visible area to ensure full coverage and bottom visibility
        int maxCols = Mathf.CeilToInt((areaWidth + 400f) / CurrentHexStepX);
        int maxRows = Mathf.CeilToInt((areaHeight + 400f) / CurrentHexStepY);

        for (int row = -3; row < maxRows + 2; row++)
        {
            for (int col = -3; col < maxCols + 2; col++)
            {
                Vector2 pos = CellToPosition(new Vector2Int(col, row));
                float x = pos.x;
                float y = pos.y;

                // Render hexagons even slightly outside bounds to ensure full coverage
                // Use current hex size as threshold so edge cells are not culled.
                if (x < -CurrentHexSize || x > areaWidth + CurrentHexSize || y > CurrentHexSize || y < -areaHeight - CurrentHexSize)
                    continue;

                GameObject hexGO = new GameObject("GridHex");
                hexGO.transform.SetParent(connectionsLayer, false);

                RectTransform rect = hexGO.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(x, y);
                rect.sizeDelta = new Vector2(CurrentHexSize, CurrentHexSize);

                hexGO.AddComponent<CanvasRenderer>();
                HexagonGraphic hex = hexGO.AddComponent<HexagonGraphic>();
                hex.color = gridHexColor;
                hex.raycastTarget = false;
            }
        }

        // Keep the background grid behind dependency lines and skill nodes.
        for (int i = 0; i < connectionsLayer.childCount; i++)
        {
            Transform child = connectionsLayer.GetChild(i);
            if (child.name == "GridHex")
                child.SetAsFirstSibling();
        }
    }

    private Vector2 CellToPosition(Vector2Int cell)
    {
        float x = GridStartX + (cell.x * CurrentHexStepX);
        float y = GridStartY - (cell.y * CurrentHexStepY) - ((cell.x & 1) == 1 ? CurrentHexStepY * 0.5f : 0f);
        return new Vector2(x, y);
    }

    private Vector2Int ClampCellToVisible(Vector2Int cell, int maxCellX, int maxCellY)
    {
        return new Vector2Int(
            Mathf.Clamp(cell.x, 0, maxCellX),
            Mathf.Clamp(cell.y, 0, maxCellY)
        );
    }

    private Vector2Int FindNearestFreeCell(Vector2Int preferred, int maxRadius)
    {
        preferred = ClampToPlacementBounds(preferred);

        if (!occupiedCells.Contains(preferred))
        {
            occupiedCells.Add(preferred);
            return preferred;
        }

        for (int r = 1; r <= maxRadius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    Vector2Int candidate = new Vector2Int(preferred.x + dx, preferred.y + dy);
                    if (!IsInsidePlacementBounds(candidate))
                        continue;

                    if (occupiedCells.Contains(candidate))
                        continue;

                    occupiedCells.Add(candidate);
                    return candidate;
                }
            }
        }

        Vector2Int fallback = ClampToPlacementBounds(new Vector2Int(preferred.x + maxRadius + 1, preferred.y));
        occupiedCells.Add(fallback);
        return fallback;
    }

    private Vector2Int FindAdjacentFreeCell(Vector2Int parent)
    {
        // Find the first free neighbor of parent cell
        List<Vector2Int> neighbors = GetHexNeighbors(parent);
        foreach (var neighbor in neighbors)
        {
            if (!IsInsidePlacementBounds(neighbor))
                continue;

            if (!occupiedCells.Contains(neighbor))
            {
                occupiedCells.Add(neighbor);
                return neighbor;
            }
        }
        
        // No free adjacent cell found, return parent as fallback (will be skipped)
        return parent;
    }

    private Vector2Int FindAdjacentCellClosestTo(Vector2Int parent, Vector2Int target)
    {
        List<Vector2Int> neighbors = GetHexNeighbors(parent);
        Vector2Int best = parent;
        float bestDist = float.MaxValue;

        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector2Int candidate = neighbors[i];
            if (!IsInsidePlacementBounds(candidate))
                continue;

            if (occupiedCells.Contains(candidate))
                continue;

            float d = Vector2.Distance(CellToPosition(candidate), CellToPosition(target));
            if (d < bestDist)
            {
                bestDist = d;
                best = candidate;
            }
        }

        if (best != parent)
        {
            occupiedCells.Add(best);
            return best;
        }

        return FindAdjacentFreeCell(parent);
    }

    private Vector2Int FindAdjacentCellWithSharedNeighbor(Vector2Int parent, Vector2Int otherL2)
    {
        List<Vector2Int> neighbors = GetHexNeighbors(parent);
        Vector2Int best = parent;
        float bestDist = float.MaxValue;

        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector2Int candidate = neighbors[i];
            if (!IsInsidePlacementBounds(candidate))
                continue;

            if (occupiedCells.Contains(candidate))
                continue;

            if (!HasFreeSharedNeighbor(candidate, otherL2))
                continue;

            float d = Vector2.Distance(CellToPosition(candidate), CellToPosition(otherL2));
            if (d < bestDist)
            {
                bestDist = d;
                best = candidate;
            }
        }

        if (best != parent)
        {
            occupiedCells.Add(best);
            return best;
        }

        return parent;
    }

    private bool HasFreeSharedNeighbor(Vector2Int a, Vector2Int b)
    {
        List<Vector2Int> na = GetHexNeighbors(a);
        List<Vector2Int> nb = GetHexNeighbors(b);

        for (int i = 0; i < na.Count; i++)
        {
            if (!IsInsidePlacementBounds(na[i]))
                continue;

            if (nb.Contains(na[i]) && !occupiedCells.Contains(na[i]))
                return true;
        }

        return false;
    }

    private bool AreAdjacent(Vector2Int a, Vector2Int b)
    {
        return GetHexNeighbors(a).Contains(b);
    }

    private Vector2Int FindFusionAdjacentCell(Vector2Int a, Vector2Int b)
    {
        // Find a cell that is adjacent to BOTH a and b (common neighbor)
        List<Vector2Int> na = GetHexNeighbors(a);
        List<Vector2Int> nb = GetHexNeighbors(b);

        // Find common neighbors
        foreach (var neighbor in na)
        {
            if (!IsInsidePlacementBounds(neighbor))
                continue;

            if (nb.Contains(neighbor) && !occupiedCells.Contains(neighbor))
            {
                occupiedCells.Add(neighbor);
                return neighbor;
            }
        }

        // No free common neighbor, fallback to fusion cell logic
        return FindFusionCell(a, b);
    }

    private Vector2Int FindFusionCell(Vector2Int a, Vector2Int b)
    {
        // First try exact midpoint if integer cell coordinates.
        if (((a.x + b.x) & 1) == 0 && ((a.y + b.y) & 1) == 0)
        {
            Vector2Int mid = ClampToPlacementBounds(new Vector2Int((a.x + b.x) / 2, (a.y + b.y) / 2));
            if (!occupiedCells.Contains(mid))
            {
                occupiedCells.Add(mid);
                return mid;
            }
        }

        // Otherwise choose the nearest shared neighbor between both L2 cells.
        List<Vector2Int> na = GetHexNeighbors(a);
        List<Vector2Int> nb = GetHexNeighbors(b);

        Vector2 center = (CellToPosition(a) + CellToPosition(b)) * 0.5f;
        Vector2Int best = a;
        float bestDist = float.MaxValue;

        for (int i = 0; i < na.Count; i++)
        {
            if (!IsInsidePlacementBounds(na[i]))
                continue;

            if (!nb.Contains(na[i]) || occupiedCells.Contains(na[i]))
                continue;

            float d = Vector2.Distance(CellToPosition(na[i]), center);
            if (d < bestDist)
            {
                bestDist = d;
                best = na[i];
            }
        }

        if (bestDist < float.MaxValue)
        {
            occupiedCells.Add(best);
            return best;
        }

        // Fallback near midpoint snapped to free cell.
        Vector2Int midpointCell = ClampToPlacementBounds(new Vector2Int(Mathf.RoundToInt((a.x + b.x) * 0.5f), Mathf.RoundToInt((a.y + b.y) * 0.5f)));
        return FindNearestFreeCell(midpointCell, 5);
    }

    private bool IsInsidePlacementBounds(Vector2Int cell)
    {
        if (!hasPlacementBounds)
            return true;

        return cell.x >= 0 && cell.x <= placementMaxCellX && cell.y >= 0 && cell.y <= placementMaxCellY;
    }

    private Vector2Int ClampToPlacementBounds(Vector2Int cell)
    {
        if (!hasPlacementBounds)
            return cell;

        return new Vector2Int(
            Mathf.Clamp(cell.x, 0, placementMaxCellX),
            Mathf.Clamp(cell.y, 0, placementMaxCellY)
        );
    }

    private List<Vector2Int> GetHexNeighbors(Vector2Int cell)
    {
        // Vertical (odd-q) offset neighbors.
        bool odd = (cell.x & 1) == 1;
        if (odd)
        {
            return new List<Vector2Int>
            {
                new Vector2Int(cell.x + 1, cell.y + 1),
                new Vector2Int(cell.x + 1, cell.y),
                new Vector2Int(cell.x, cell.y - 1),
                new Vector2Int(cell.x - 1, cell.y),
                new Vector2Int(cell.x - 1, cell.y + 1),
                new Vector2Int(cell.x, cell.y + 1)
            };
        }

        return new List<Vector2Int>
        {
            new Vector2Int(cell.x + 1, cell.y),
            new Vector2Int(cell.x + 1, cell.y - 1),
            new Vector2Int(cell.x, cell.y - 1),
            new Vector2Int(cell.x - 1, cell.y - 1),
            new Vector2Int(cell.x - 1, cell.y),
            new Vector2Int(cell.x, cell.y + 1)
        };
    }

    private bool ShouldShowSkill(Skill skill, List<Skill> unlocked)
    {
        if (skill.level == 1)
            return true;

        if (skill.isUnlocked)
            return true;

        return skill.CanUnlock(unlocked);
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
        rect.sizeDelta = new Vector2(CurrentHexSize, CurrentHexSize);

        nodeGO.AddComponent<CanvasRenderer>();
        HexagonGraphic image = nodeGO.AddComponent<HexagonGraphic>();
        Button button = nodeGO.AddComponent<Button>();
        button.targetGraphic = image;

        SkillVisualState state = GetVisualState(skill, unlocked);
        image.color = GetColorForState(state);
        // Keep purchased skills clickable so players can still read their descriptions.
        button.interactable = state != SkillVisualState.Locked;

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
        label.text = $"L{skill.level}";

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
            List<Skill> unlocked = skillTreeManager.GetUnlockedSkills();
            int points = pointManager != null ? pointManager.GetCurrentPoints() : 0;
            bool canUnlock = selectedSkill.CanUnlock(unlocked);
            bool canAfford = selectedSkill.CanAfford(points);

            infoTitleText.text = $"{selectedSkill.name} (Achat impossible)";
            if (infoImpactText != null) infoImpactText.text = "";
            if (infoCostText != null) infoCostText.text = $"Cout: {selectedSkill.cost} | Points: {points}";
            if (!canUnlock)
            {
                infoDescriptionText.text = "Prerequis non remplis pour ce skill.";
            }
            else if (!canAfford)
            {
                infoDescriptionText.text = $"Points insuffisants: cout {selectedSkill.cost}, tu as {points}.";
            }
            else
            {
                infoDescriptionText.text = "Achat impossible pour une raison inconnue.";
            }

            UpdateBuyButtonState();
            return;
        }

        RefreshCurrentTab();
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
        infoTitleText.alignment = TMPro.TextAlignmentOptions.Center;
        infoTitleText.color = Color.white;

        infoDescriptionText.text = selectedSkill.description;
        infoDescriptionText.color = Color.white;

        if (infoImpactText != null)
        {
            infoImpactText.text = selectedSkill.variableModified;
            infoImpactText.color = new Color(1f, 0.40f, 0.40f, 1f);
        }

        int points = pointManager != null ? pointManager.GetCurrentPoints() : 0;
        if (infoCostText != null)
            infoCostText.text = $"Cout: {selectedSkill.cost} | Points: {points}";

        UpdateBuyButtonState();
    }

    private void ApplyBuyButtonStyle()
    {
        if (buyButton == null)
            return;

        RectTransform rect = buyButton.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.06f, 0.07f);
            rect.anchorMax = new Vector2(0.94f, 0.23f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        Image image = buyButton.GetComponent<Image>();
        if (image != null)
            image.color = new Color(0.50f, 0.04f, 0.04f, 1f);

        Outline outline = buyButton.GetComponent<Outline>();
        if (outline == null)
            outline = buyButton.gameObject.AddComponent<Outline>();

        outline.effectColor = new Color(1f, 0.22f, 0.22f, 1f);
        outline.effectDistance = new Vector2(3f, 3f);

        if (buyButtonText != null)
        {
            buyButtonText.color = Color.white;
            buyButtonText.fontStyle = FontStyles.Bold;
        }
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
        if (skill.isUnlocked || (unlocked != null && unlocked.Exists(u => u.id == skill.id)))
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
            infoTitleText.alignment = TMPro.TextAlignmentOptions.Center;
            infoTitleText.color = Color.white;
            infoDescriptionText.text = "Clique sur un skill rouge pour voir ses details puis achete-le avec tes Crotogenes. Les skills L2 gris se debloquent apres achat du L1.";
            infoDescriptionText.color = Color.white;
            if (infoImpactText != null) infoImpactText.text = "";
            if (infoCostText != null) infoCostText.text = "";
            buyButton.interactable = false;
            buyButtonText.text = "ACHETER";
            return;
        }

        ShowComingSoon(activeTab);
    }

    private void ShowDefaultInfoForSpecial()
    {
        VirusType vt = gameManager != null ? gameManager.currentVirusType : VirusType.Classique;
        string virusName = GetVirusDisplayName(vt);

        infoTitleText.text = virusName;
        infoTitleText.alignment = TMPro.TextAlignmentOptions.Center;
        infoTitleText.color = GetVirusThemeColor(vt, SkillVisualState.Available);

        infoDescriptionText.text = "Clique sur un skill pour voir ses details. Les skills N2 et plus se debloquent apres achat des prerequis. Tous les skills Special sont gratuits.";
        infoDescriptionText.color = Color.white;

        if (infoImpactText != null) infoImpactText.text = "";
        if (infoCostText != null) infoCostText.text = "";
        buyButton.interactable = false;
        buyButtonText.text = "ACHETER";
    }

    private void BuildSpecialTree()
    {
        ClearTreeVisuals();
        nodeViews.Clear();
        nodeCells.Clear();
        occupiedCells.Clear();

        BuildHexBackdropGrid();

        if (gameManager == null)
            return;

        VirusType virusType = gameManager.currentVirusType;
        List<Skill> allSkills = SpecialSkillTree.GetAllSkills(virusType);
        if (allSkills == null || allSkills.Count == 0)
            return;

        List<Skill> unlocked = skillTreeManager != null ? skillTreeManager.GetUnlockedSkills() : new List<Skill>();

        float areaWidth  = (skillsAreaRoot != null && skillsAreaRoot.rect.width  > 1f) ? skillsAreaRoot.rect.width  : 980f;
        float areaHeight = (skillsAreaRoot != null && skillsAreaRoot.rect.height > 1f) ? skillsAreaRoot.rect.height : 780f;
        placementMaxCellX = Mathf.Max(2, Mathf.FloorToInt((areaWidth  - GridStartX - CurrentHexSize) / CurrentHexStepX));
        placementMaxCellY = Mathf.Max(2, Mathf.FloorToInt((areaHeight + Mathf.Abs(GridStartY) - CurrentHexSize) / CurrentHexStepY));
        hasPlacementBounds = true;

        BuildSpecialTreeFixedLayout(allSkills, unlocked, virusType, areaWidth, areaHeight);
    }

    // Fixed top-to-bottom layout for Special trees.
    // N1 at top-centre, N2A left / N2B right, N3 centre, N4A left / N4B right.
    // All six skills are always rendered (locked/available/purchased states only).
    private void BuildSpecialTreeFixedLayout(List<Skill> allSkills, List<Skill> unlocked, VirusType virusType, float areaWidth, float areaHeight)
    {
        Dictionary<string, Vector2Int> fixedCells = BuildSpecialFixedCells(virusType);

        // Bounding box of the tree in cell space
        int minCellX = int.MaxValue, maxCellX = int.MinValue;
        int minCellY = int.MaxValue, maxCellY = int.MinValue;
        foreach (var kvp in fixedCells)
        {
            minCellX = Mathf.Min(minCellX, kvp.Value.x);
            maxCellX = Mathf.Max(maxCellX, kvp.Value.x);
            minCellY = Mathf.Min(minCellY, kvp.Value.y);
            maxCellY = Mathf.Max(maxCellY, kvp.Value.y);
        }
        // Tree bounding box centre cell (keep even column to avoid y half-step offset)
        int treeMidX = (minCellX + maxCellX) / 2;
        int treeMidY = (minCellY + maxCellY) / 2;
        if ((treeMidX & 1) == 1) treeMidX++;

        // Find the grid cell closest to the area centre
        float areaCenterX = areaWidth  * 0.5f - CurrentHexSize * 0.5f;
        float areaCenterY = -(areaHeight * 0.5f) + CurrentHexSize * 0.5f;
        int nearCol = Mathf.RoundToInt((areaCenterX - GridStartX) / CurrentHexStepX);
        int nearRow = Mathf.RoundToInt((GridStartY  - areaCenterY) / CurrentHexStepY);
        if ((nearCol & 1) == 1) nearCol++;

        int dx = nearCol - treeMidX;
        int dy = nearRow - treeMidY;

        var placedCells = new Dictionary<string, Vector2Int>();
        foreach (var kvp in fixedCells)
            placedCells[kvp.Key] = new Vector2Int(kvp.Value.x + dx, kvp.Value.y + dy);

        List<Skill> ordered = new List<Skill>(allSkills);
        ordered.Sort((a, b) => a.level.CompareTo(b.level));

        foreach (Skill skill in ordered)
        {
            if (!placedCells.ContainsKey(skill.id)) continue;
            Vector2Int cell = placedCells[skill.id];
            occupiedCells.Add(cell);
            CreateSpecialSkillNode(skill, CellToPosition(cell), unlocked, virusType);
        }

        DrawSpecialBranchLines(allSkills, placedCells, Vector2.zero, unlocked);
    }

    private void DrawSpecialBranchLines(List<Skill> allSkills, Dictionary<string, Vector2Int> cells, Vector2 pixelOffset, List<Skill> unlocked)
    {
        if (connectionsLayer == null) return;

        float R = CurrentHexSize * 0.5f;
        Vector2 hexCenter = new Vector2(R, -R);

        foreach (Skill skill in allSkills)
        {
            if (!cells.ContainsKey(skill.id)) continue;

            Vector2 childCenter = CellToPosition(cells[skill.id]) + hexCenter + pixelOffset;
            bool childBought = unlocked != null && unlocked.Exists(u => u.id == skill.id);

            foreach (string depId in skill.dependencies)
            {
                if (!cells.ContainsKey(depId)) continue;
                Vector2 parentCenter = CellToPosition(cells[depId]) + hexCenter + pixelOffset;
                bool parentBought = unlocked != null && unlocked.Exists(u => u.id == depId);
                Color c = (childBought && parentBought)
                    ? new Color(0.15f, 0.90f, 0.20f, 0.95f)
                    : new Color(1f, 1f, 1f, 0.55f);
                DrawSpecialLine(parentCenter, childCenter, c, R);
            }

            if (skill.anyDependency != null)
            {
                foreach (string depId in skill.anyDependency)
                {
                    if (!cells.ContainsKey(depId)) continue;
                    Vector2 parentCenter = CellToPosition(cells[depId]) + hexCenter + pixelOffset;
                    bool parentBought = unlocked != null && unlocked.Exists(u => u.id == depId);
                    Color c = (childBought && parentBought)
                        ? new Color(0.15f, 0.90f, 0.20f, 0.65f)
                        : new Color(1f, 1f, 1f, 0.35f);
                    DrawSpecialLine(parentCenter, childCenter, c, R);
                }
            }
        }
    }

    private void DrawSpecialLine(Vector2 from, Vector2 to, Color color, float stopRadius)
    {
        GameObject lineGO = new GameObject("SpecialLine");
        lineGO.transform.SetParent(connectionsLayer, false);

        RectTransform lineRect = lineGO.AddComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0, 1);
        lineRect.anchorMax = new Vector2(0, 1);
        lineRect.pivot = new Vector2(0f, 0.5f);

        Image lineImage = lineGO.AddComponent<Image>();
        lineImage.color = color;
        lineImage.raycastTarget = false;

        Vector2 dir = (to - from).normalized;
        // Pull endpoints inside the hex border (inradius ≈ R*0.866)
        Vector2 start = from + dir * (stopRadius * 0.87f);
        Vector2 end   = to   - dir * (stopRadius * 0.87f);
        Vector2 diff  = end - start;
        float len = diff.magnitude;

        lineRect.sizeDelta = new Vector2(Mathf.Max(0f, len), 5f);
        lineRect.anchoredPosition = start;
        lineRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg);
    }

    private Dictionary<string, Vector2Int> BuildSpecialFixedCells(VirusType virusType)
    {
        // One set of positions per virus — always the same shape.
        string prefix;
        switch (virusType)
        {
            case VirusType.Classique:     prefix = "cla"; break;
            case VirusType.Cacastellaire: prefix = "cas"; break;
            case VirusType.NanoCaca:      prefix = "nan"; break;
            case VirusType.FongiCaca:   prefix = "myo"; break;
            default:                      prefix = "cla"; break;
        }

        return new Dictionary<string, Vector2Int>
        {
            // Use even x columns to avoid odd-column half-step vertical offset.
            // With MortaliteHexSize=110: StepY≈95px, StepX≈82px, GridStartY=-64
            // Row 0: y ≈ -64  | Row 2: y ≈ -254 | Row 4: y ≈ -445 | Row 5: y ≈ -540
            // Area height ~780px so row 5 bottom edge ≈ -540-110 = -650 → visible
            { prefix + "N1",  new Vector2Int(4, 0) },  // Top centre
            { prefix + "N2a", new Vector2Int(2, 2) },  // Upper-mid left
            { prefix + "N2b", new Vector2Int(6, 2) },  // Upper-mid right
            { prefix + "N3",  new Vector2Int(4, 4) },  // Mid centre
            { prefix + "N4a", new Vector2Int(2, 6) },  // Lower left  (2 rows below N3, same gap as others)
            { prefix + "N4b", new Vector2Int(6, 6) },  // Lower right
        };
    }

    private void CreateSpecialSkillNode(Skill skill, Vector2 anchoredPos, List<Skill> unlocked, VirusType virusType)
    {
        GameObject nodeGO = new GameObject($"Node_{skill.id}");
        nodeGO.transform.SetParent(nodesLayer, false);

        RectTransform rect = nodeGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(CurrentHexSize, CurrentHexSize);

        nodeGO.AddComponent<CanvasRenderer>();
        HexagonGraphic image = nodeGO.AddComponent<HexagonGraphic>();
        Button button = nodeGO.AddComponent<Button>();
        button.targetGraphic = image;

        SkillVisualState state = GetVisualState(skill, unlocked);
        image.color = GetVirusThemeColor(virusType, state);
        // Purchased nodes are clickable so players can read their description (buy button will be disabled).
        button.interactable = state != SkillVisualState.Locked;

        button.onClick.AddListener(() => OnSkillSelected(skill));

        // Inner label showing skill tier
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(nodeGO.transform, false);

        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(2, 2);
        labelRect.offsetMax = new Vector2(-2, -2);

        TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 14;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;

        // Show "N1"/"N2"/"N3"/"N4" prefix + active badge for activation skills
        bool isActive = skill.effects.ContainsKey("dispersalSpatiale")
            || skill.effects.ContainsKey("seedOneInNonInfectedCountry")
            || skill.effects.ContainsKey("autoTransmissionSkillChance")
            || skill.effects.ContainsKey("autoTransmissionSkillChanceTimed")
            || skill.effects.ContainsKey("autoLethalSymptomChanceTimed")
            || skill.effects.ContainsKey("pointGainMultiplierPermanent")
            || skill.effects.ContainsKey("vaccineMaxProgressMultiplier");
        label.text = isActive ? $"N{skill.level}\n[ACT]" : $"N{skill.level}";

        nodeViews[skill.id] = new SkillNodeView
        {
            Skill = skill,
            Button = button,
            Background = image,
            Rect = rect
        };
    }

    private Color GetVirusThemeColor(VirusType virusType, SkillVisualState state)
    {
        switch (virusType)
        {
            case VirusType.Classique:
                // Brun-rouge organique
                if (state == SkillVisualState.Purchased)  return new Color(0.10f, 0.75f, 0.20f, 1f); // vert
                if (state == SkillVisualState.Available)  return new Color(0.65f, 0.28f, 0.08f, 1f);
                return new Color(0.28f, 0.13f, 0.04f, 1f);

            case VirusType.Cacastellaire:
                // Noir cosmique / brun irisé vert-toxique
                if (state == SkillVisualState.Purchased)  return new Color(0.10f, 0.75f, 0.20f, 1f); // vert
                if (state == SkillVisualState.Available)  return new Color(0.06f, 0.50f, 0.50f, 1f);
                return new Color(0.04f, 0.18f, 0.22f, 1f);

            case VirusType.NanoCaca:
                // Métal sale / cyan clinique
                if (state == SkillVisualState.Purchased)  return new Color(0.10f, 0.75f, 0.20f, 1f); // vert
                if (state == SkillVisualState.Available)  return new Color(0.12f, 0.42f, 0.55f, 1f);
                return new Color(0.06f, 0.16f, 0.24f, 1f);

            case VirusType.FongiCaca:
                // Jaune-vert moisi
                if (state == SkillVisualState.Purchased)  return new Color(0.10f, 0.75f, 0.20f, 1f); // vert
                if (state == SkillVisualState.Available)  return new Color(0.28f, 0.52f, 0.08f, 1f);
                return new Color(0.10f, 0.22f, 0.04f, 1f);

            default:
                return GetColorForState(state);
        }
    }

    private string GetVirusDisplayName(VirusType virusType)
    {
        switch (virusType)
        {
            case VirusType.Classique:     return "La crottance";
            case VirusType.Cacastellaire: return "Cacastellaire";
            case VirusType.NanoCaca:      return "Nano Caca";
            case VirusType.FongiCaca:   return "Fongi-Caca";
            default:                      return "Special";
        }
    }

    private void ShowComingSoon(SkillTab tab)
    {
        string tabName = tab == SkillTab.Mortalite ? "Mortalite" : "Special";
        infoTitleText.text = tabName;
        infoTitleText.alignment = TMPro.TextAlignmentOptions.Center;
        infoTitleText.color = Color.white;
        infoDescriptionText.text = "Contenu bientot disponible. Cet onglet sera implemente dans une prochaine phase.";
        infoDescriptionText.color = Color.white;
        if (infoImpactText != null) infoImpactText.text = "";
        if (infoCostText != null) infoCostText.text = "";
        buyButton.interactable = false;
        buyButtonText.text = "ACHETER";
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
