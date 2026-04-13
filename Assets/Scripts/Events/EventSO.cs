using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CrotteViral/EventSO", fileName = "EventSO")]
public class EventSO : ScriptableObject
{
    [Tooltip("Unique id for the event (used for one-shot tracking)")]
    public string id;

    public string title;
    [TextArea]
    public string description;

    [Tooltip("Group id for sequenced events (e.g. 'vaccine_palier')")]
    public string groupId;

    [Tooltip("Sequence index within its group. Only the event whose index matches the group's nextSequence will be allowed to trigger.")]
    public int sequenceIndex = 0;

    public bool oneShot = true;
    public bool enabled = true;

    [Header("Random/Triggering")]
    public bool canTriggerRandomly = false;
    [Range(0f, 1f)]
    public float randomChance = 0.02f;

    [Tooltip("Percent thresholds (0..100). Set to -1 to disable a threshold.)")]
    public float infectedThresholdPct = -1f;
    public float deadThresholdPct = -1f;

    [Tooltip("List of effects to execute when the event triggers")]
    public List<EventEffectSO> effects = new List<EventEffectSO>();
}
