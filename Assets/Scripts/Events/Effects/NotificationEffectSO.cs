using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CrotteViral/Effects/NotificationEffect", fileName = "NotificationEffect")]
public class NotificationEffectSO : EventEffectSO
{
    public string title;
    [TextArea]
    public string message;

    public override void Execute(List<CountryObject> countries, Virus virus)
    {
        if (Notification.Instance != null)
            Notification.Instance.Show(title, message);
    }
}
