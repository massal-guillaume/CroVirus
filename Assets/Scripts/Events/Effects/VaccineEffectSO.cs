using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CrotteViral/Effects/VaccineEffect", fileName = "VaccineEffect")]
public class VaccineEffectSO : EventEffectSO
{
    public enum VaccineEffectType
    {
        FirstDetection,
        FirstStudies,
        FirstInvestments,
        FirstVaccinStudies,
        GlobalCooperation
    }

    public VaccineEffectType effectType;
    public int ticks = 0;
    public float amount = 0f; // percent or multiplier depending on effect
    [Tooltip("Donation as proportion of target country population (0..1)")]
    public float donationFraction = 0.001f;

    public override void Execute(List<CountryObject> countries, Virus virus)
    {
        if (countries == null || countries.Count == 0 || virus == null)
            return;

        switch (effectType)
        {
            case VaccineEffectType.FirstDetection:
            
                if (Notification.Instance != null)
                {
                    string title = "Nouvelle découverte !";
                    string desc = "Un scientifique a découvert par hasard votre virus en étudiant des échantillons.";
                    string effect ="";
                    Notification.Instance.Show(title, desc + "\n\n" + effect);
                }

                break;
            
            case VaccineEffectType.FirstStudies:               
                if (Notification.Instance != null)
                {
                    string title = "Début des études";
                    string desc = "Une équipe de scientifiques inquiète a commencée à étudier le virus à part entière.";
                    string effect ="";

                    Notification.Instance.Show(title, desc + "\n\n" + effect);
                }
                break;

            case VaccineEffectType.FirstInvestments:
                if (Notification.Instance != null)
                {
                    string title = "Le virus semble etre une menace sérieuse...";
                    string desc = "Des investissement consequent sont en cours pour pour potientiellement développer un vaccin.";
                    string effect ="";
                    Notification.Instance.Show(title, desc + "\n\n" + effect);
                }
                break;

            case VaccineEffectType.FirstVaccinStudies:
                virus.isVaccineResearchActive = true;
                if (Notification.Instance != null)
                {
                    string title = "Un espoir pour l'humanité ?";
                    string desc = "Plusieurs laboratoire on commencé à crée un vaccin.";
                    string effect = "La préparation du vaccin a commencé, mais il est encore loin d'être prêt.";
                    Notification.Instance.Show(title, desc + "\n\n" + effect);
                }
                break;

            case VaccineEffectType.GlobalCooperation:
                if (Notification.Instance != null)
                {
                    string title = "Cooperation Mondiale";
                    string desc = "Tout les laboratoires du mondetravaillent ensemble pour développer un vaccin.";
                    string effect = "La vitesse de preparation du vaccin est grandement accélérée.";
                    Notification.Instance.Show(title, desc + "\n\n" + effect);
                }
                break;
        }
    }
}
