using System.Collections.Generic;
using UnityEngine;

public abstract class EventEffectSO : ScriptableObject
{
    public abstract void Execute(List<CountryObject> countries, Virus virus);
}
