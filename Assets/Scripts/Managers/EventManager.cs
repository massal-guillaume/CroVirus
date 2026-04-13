using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Tooltip("Optional: assign EventSO assets here. Resources/Events will also be loaded at Awake.")]
    public EventSO[] assignedEvents;

    private List<EventSO> events = new List<EventSO>();
    private HashSet<string> triggeredIds = new HashSet<string>();
    private Dictionary<string, int> nextSequenceByGroup = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadEvents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadEvents()
    {
        events.Clear();
        if (assignedEvents != null && assignedEvents.Length > 0)
            events.AddRange(assignedEvents);

        var resources = Resources.LoadAll<EventSO>("Events");
        if (resources != null && resources.Length > 0)
            events.AddRange(resources);

        // Stable ordering: group then sequence index
        events = events
            .Where(e => e != null)
            .OrderBy(e => e.groupId ?? string.Empty)
            .ThenBy(e => e.sequenceIndex)
            .ToList();
    }

    public void ProcessTurn(int turn, List<CountryObject> countries, Virus virus)
    {
        if (events == null || events.Count == 0 || countries == null || virus == null)
            return;

        int totalPopulation = countries.Sum(c => c.population.total);
        if (totalPopulation <= 0)
            return;

        int totalInfected = TransmissionService.GetTotalInfected(countries);
        int totalDead = TransmissionService.GetTotalDead(countries);

        float infectedPct = (float)totalInfected / totalPopulation * 100f;
        float deadPct = (float)totalDead / totalPopulation * 100f;

        foreach (var ev in events)
        {
            if (ev == null || !ev.enabled)
                continue;

            if (ev.oneShot && !string.IsNullOrEmpty(ev.id) && triggeredIds.Contains(ev.id))
                continue;

            // Enforce group sequencing: only the event whose sequenceIndex matches nextSequence can trigger
            if (!string.IsNullOrEmpty(ev.groupId))
            {
                int next = 0;
                nextSequenceByGroup.TryGetValue(ev.groupId, out next);
                if (ev.sequenceIndex != next)
                    continue;
            }

            bool conditionMet = false;
            if (ev.infectedThresholdPct >= 0f && infectedPct >= ev.infectedThresholdPct)
                conditionMet = true;
            if (ev.deadThresholdPct >= 0f && deadPct >= ev.deadThresholdPct)
                conditionMet = true;

            bool triggered = false;
            if (conditionMet)
                triggered = true;
            else if (ev.canTriggerRandomly && Random.value <= ev.randomChance)
                triggered = true;

            if (!triggered)
                continue;

            // Execute effects
            if (ev.effects != null)
            {
                foreach (var eff in ev.effects)
                {
                    if (eff == null) continue;
                    try { eff.Execute(countries, virus); }
                    catch (System.Exception ex) { Debug.LogWarning($"Event effect execution failed: {ex}"); }
                }
            }

            // Mark one-shot triggered
            if (ev.oneShot && !string.IsNullOrEmpty(ev.id))
                triggeredIds.Add(ev.id);

            // Advance sequence for group
            if (!string.IsNullOrEmpty(ev.groupId))
            {
                int next = 0;
                nextSequenceByGroup.TryGetValue(ev.groupId, out next);
                nextSequenceByGroup[ev.groupId] = next + 1;
            }
        }
    }

    // Developer utility to force-trigger an event by id
    public void TriggerEventById(string id, List<CountryObject> countries, Virus virus)
    {
        var ev = events.FirstOrDefault(e => e != null && e.id == id);
        if (ev == null) return;
        if (ev.effects != null)
        {
            foreach (var eff in ev.effects)
                eff?.Execute(countries, virus);
        }
        if (ev.oneShot && !string.IsNullOrEmpty(ev.id)) triggeredIds.Add(ev.id);
    }
}
