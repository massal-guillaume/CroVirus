#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class CreateEventAssets
{
    [MenuItem("Tools/CrotteViral/Create Example Vaccine Events")]
    public static void CreateExampleVaccineEvents()
    {
        string resourcesFolder = "Assets/Resources";
        string eventsFolder = "Assets/Resources/Events";

        if (!AssetDatabase.IsValidFolder(resourcesFolder))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(eventsFolder))
            AssetDatabase.CreateFolder(resourcesFolder, "Events");

        // Palier 1: FirstDetection
        var eff1 = ScriptableObject.CreateInstance<VaccineEffectSO>();
        eff1.effectType = VaccineEffectSO.VaccineEffectType.FirstDetection;
        AssetDatabase.CreateAsset(eff1, Path.Combine(eventsFolder, "Effect_Vaccine_FirstDetection.asset"));

        var ev1 = ScriptableObject.CreateInstance<EventSO>();
        ev1.id = "vaccine_palier_1";
        ev1.title = "Palier 1 — Découverte";
        ev1.groupId = "vaccine_palier";
        ev1.sequenceIndex = 0;
        ev1.oneShot = true;
        ev1.canTriggerRandomly = true;
        ev1.randomChance = 0.02f;
        ev1.infectedThresholdPct = 0.10f;
        ev1.effects.Add(eff1);
        AssetDatabase.CreateAsset(ev1, Path.Combine(eventsFolder, "Event_Vaccine_Palier1.asset"));

        // Palier 2: FirstStudies
        var eff2 = ScriptableObject.CreateInstance<VaccineEffectSO>();
        eff2.effectType = VaccineEffectSO.VaccineEffectType.FirstStudies;
        AssetDatabase.CreateAsset(eff2, Path.Combine(eventsFolder, "Effect_Vaccine_FirstStudies.asset"));

        var ev2 = ScriptableObject.CreateInstance<EventSO>();
        ev2.id = "vaccine_palier_2";
        ev2.title = "Palier 2 — Études";
        ev2.groupId = "vaccine_palier";
        ev2.sequenceIndex = 1;
        ev2.oneShot = true;
        ev2.infectedThresholdPct = 0.50f;
        ev2.effects.Add(eff2);
        AssetDatabase.CreateAsset(ev2, Path.Combine(eventsFolder, "Event_Vaccine_Palier2.asset"));

        // Palier 3: FirstInvestments
        var eff3 = ScriptableObject.CreateInstance<VaccineEffectSO>();
        eff3.effectType = VaccineEffectSO.VaccineEffectType.FirstInvestments;
        eff3.amount = 5f; // default progress boost, editable later
        AssetDatabase.CreateAsset(eff3, Path.Combine(eventsFolder, "Effect_Vaccine_FirstInvestments.asset"));

        var ev3 = ScriptableObject.CreateInstance<EventSO>();
        ev3.id = "vaccine_palier_3";
        ev3.title = "Palier 3 — Investissements";
        ev3.groupId = "vaccine_palier";
        ev3.sequenceIndex = 2;
        ev3.oneShot = true;
        ev3.infectedThresholdPct = 2.0f;
        ev3.effects.Add(eff3);
        AssetDatabase.CreateAsset(ev3, Path.Combine(eventsFolder, "Event_Vaccine_Palier3.asset"));

        // Palier 4: FirstVaccinStudies
        var eff4 = ScriptableObject.CreateInstance<VaccineEffectSO>();
        eff4.effectType = VaccineEffectSO.VaccineEffectType.FirstVaccinStudies;
        eff4.ticks = 3;
        AssetDatabase.CreateAsset(eff4, Path.Combine(eventsFolder, "Effect_Vaccine_FirstVaccinStudies.asset"));

        var ev4 = ScriptableObject.CreateInstance<EventSO>();
        ev4.id = "vaccine_palier_4";
        ev4.title = "Palier 4 — Premiers essais vaccinaux";
        ev4.groupId = "vaccine_palier";
        ev4.sequenceIndex = 3;
        ev4.oneShot = true;
        ev4.infectedThresholdPct = 10.0f;
        ev4.deadThresholdPct = 1.0f;
        ev4.effects.Add(eff4);
        AssetDatabase.CreateAsset(ev4, Path.Combine(eventsFolder, "Event_Vaccine_Palier4.asset"));

        // Palier 5: GlobalCooperation
        var eff5 = ScriptableObject.CreateInstance<VaccineEffectSO>();
        eff5.effectType = VaccineEffectSO.VaccineEffectType.GlobalCooperation;
        AssetDatabase.CreateAsset(eff5, Path.Combine(eventsFolder, "Effect_Vaccine_GlobalCooperation.asset"));

        var ev5 = ScriptableObject.CreateInstance<EventSO>();
        ev5.id = "vaccine_palier_5";
        ev5.title = "Palier 5 — Coopération mondiale";
        ev5.groupId = "vaccine_palier";
        ev5.sequenceIndex = 4;
        ev5.oneShot = true;
        ev5.infectedThresholdPct = 50.0f;
        ev5.deadThresholdPct = 5.0f;
        ev5.effects.Add(eff5);
        AssetDatabase.CreateAsset(ev5, Path.Combine(eventsFolder, "Event_Vaccine_Palier5.asset"));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("CrotteViral", "Example vaccine event assets created in Assets/Resources/Events.", "OK");
    }
}
#endif
