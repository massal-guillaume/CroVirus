using System;

[Serializable]
public class Event
{
    public enum EventType { Random, Trigger }

    public string name;
    public string description;
    public EventType type;
    // These are not serialized, must be rebound after load
    [NonSerialized]
    public Func<bool> condition;
    [NonSerialized]
    public Action effect;
    // For ordered triggers like Vaccine events
    public int orderIndex;
    public bool triggered;
    // For repeatable random events
    public bool repeatable = false;
    public int cooldownTurns = 0;
    public int lastTriggeredTurn = -999;

    public Event(string name, string description, EventType type, Func<bool> condition, Action effect, int orderIndex = -1)
    {
        this.name = name;
        this.description = description;
        this.type = type;
        this.condition = condition;
        this.effect = effect;
        this.orderIndex = orderIndex;
        this.triggered = false;
    }
}
