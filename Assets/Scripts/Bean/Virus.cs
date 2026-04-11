using UnityEngine;

public class Virus
{
    public string name;
    public float infectivity;       // Contagiosité (0-1)
    public float lethality;         // Létalité (0-1)
    public float coldResistance;    // Résistance au froid (0-1)
    public float heatResistance;    // Résistance à la chaleur (0-1)

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
