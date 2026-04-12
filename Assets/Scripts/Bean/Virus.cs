using UnityEngine;

public class Virus
{
    public string name;
    public float infectivity;              // Contagiosité (0-1)
    public float lethality;                // Létalité (0-1)
    public float coldResistance;           // Résistance au froid (0-1)
    public float heatResistance;           // Résistance à la chaleur (0-1)
    
    // ─── TRANSPORT MODIFIERS ───────────────────────
    public float airborneModifier = 0.0f;  // Aviation transport
    public float seaModifier = 0.0f;       // Maritime transport
    public float landModifier = 0.0f;      // Land/Terrestre transport
    
    // ─── VECTOR MODIFIERS ──────────────────────────
    public float dogModifier = 0.0f;       // Dog feces vector
    public float petModifier = 0.0f;       // Exotic pets vector
    public float flyModifier = 0.0f;       // Fly vector
    public float urineModifier = 0.0f;     // Urine transmission
    
    // ─── EXPLOITATION MODIFIERS ────────────────────
    public float hygieneExploitation = 0.0f;  // Targets low-hygiene countries
    public float wealthExploitation = 0.0f;   // Targets rich countries
    public float drugResistance = 0.0f;       // Resists antiviral treatments (0-1: virus fights drugs)
    public float immunityBypass = 0.0f;       // Chance to bypass genetic immunity (0-1: probability)

    public Virus(string virusName, float infectivity, float lethality, float coldResistance = 0f, float heatResistance = 0f)
    {
        name = virusName;
        this.infectivity = Mathf.Clamp01(infectivity);
        this.lethality = Mathf.Clamp01(lethality);
        this.coldResistance = Mathf.Clamp01(coldResistance);
        this.heatResistance = Mathf.Clamp01(heatResistance);
    }


    public override string ToString()
    {
        return $"{name} - Infectivité: {infectivity:P0} | Létalité: {lethality:P0} | Cold: {coldResistance:P0} | Heat: {heatResistance:P0}";
    }
}
