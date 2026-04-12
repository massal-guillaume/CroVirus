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

    private const float HexSize = 78f;
    private const float HexRadius = HexSize * 0.5f;
    // Flat-top hex grid spacing (truly contiguous, no gaps):
    // horizontal = 1.5 * radius, vertical = sqrt(3) * radius
    private const float HexStepX = HexRadius * 1.5f;
    private const float HexStepY = HexRadius * 1.7320508f;
    private const float GridStartX = 48f;
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
        int maxCellX = Mathf.Max(2, Mathf.FloorToInt((areaWidth - GridStartX - HexSize) / HexStepX));
        int maxCellY = Mathf.Max(2, Mathf.FloorToInt((areaHeight + Mathf.Abs(GridStartY) - HexSize) / HexStepY));
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

    private void BuildHexBackdropGrid()
    {
        if (connectionsLayer == null || skillsAreaRoot == null)
            return;

        Rect areaRect = skillsAreaRoot.rect;
        float areaWidth = areaRect.width > 1f ? areaRect.width : 980f;
        float areaHeight = areaRect.height > 1f ? areaRect.height : 780f;

        // Render a contiguous hex map using the same cell-to-position conversion as skill nodes.
        // Extend grid beyond visible area to ensure full coverage and bottom visibility
        int maxCols = Mathf.CeilToInt((areaWidth + 400f) / HexStepX);
        int maxRows = Mathf.CeilToInt((areaHeight + 400f) / HexStepY);

        for (int row = -3; row < maxRows + 2; row++)
        {
            for (int col = -3; col < maxCols + 2; col++)
            {
                Vector2 pos = CellToPosition(new Vector2Int(col, row));
                float x = pos.x;
                float y = pos.y;

                // Render hexagons even slightly outside bounds to ensure full coverage
                if (x < -50f || x > areaWidth + 50f || y > 50f || y < -areaHeight - 50f)
                    continue;

                GameObject hexGO = new GameObject("GridHex");
                hexGO.transform.SetParent(connectionsLayer, false);

                RectTransform rect = hexGO.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(x, y);
                rect.sizeDelta = new Vector2(HexSize, HexSize);

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
        float x = GridStartX + (cell.x * HexStepX);
        float y = GridStartY - (cell.y * HexStepY) - ((cell.x & 1) == 1 ? HexStepY * 0.5f : 0f);
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
        rect.sizeDelta = new Vector2(HexSize, HexSize);

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
        infoTitleText.color = Color.white;  // Ensure title is white
        
        int points = pointManager != null ? pointManager.GetCurrentPoints() : 0;
        infoDescriptionText.text = $"{selectedSkill.description}\n\n<b>{selectedSkill.variableModified}</b>\n\nCout: {selectedSkill.cost} | Points: {points}";
        infoDescriptionText.color = Color.white;  // Ensure description is white

        UpdateBuyButtonState();
    }

    private void ApplyBuyButtonStyle()
    {
        if (buyButton == null)
            return;

        RectTransform rect = buyButton.GetComponent<RectTransform>();
        if (rect != null)
        {
            // Move up in the info panel.
            rect.anchorMin = new Vector2(0.08f, 0.18f);
            rect.anchorMax = new Vector2(0.92f, 0.30f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        Image image = buyButton.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.68f, 0.68f, 0.68f, 1f); // Gray background
        }

        Outline outline = buyButton.GetComponent<Outline>();
        if (outline == null)
            outline = buyButton.gameObject.AddComponent<Outline>();

        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, 2f);

        if (buyButtonText != null)
        {
            buyButtonText.color = new Color(0.85f, 0.05f, 0.05f, 1f); // Red text
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
            infoTitleText.color = Color.white;
            infoDescriptionText.text = "Clique sur un skill rouge pour voir ses details puis achete-le avec tes Crotogenes. Les skills L2 gris se debloquent apres achat du L1.";
            infoDescriptionText.color = Color.white;
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
        infoTitleText.color = Color.white;
        infoDescriptionText.text = "Contenu bientot disponible. Cet onglet sera implemente dans une prochaine phase.";
        infoDescriptionText.color = Color.white;
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
