using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CrotteEvolutionButton : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.RemoveListener(OpenSkillTreeMenu);
        button.onClick.AddListener(OpenSkillTreeMenu);
    }

    public void OpenSkillTreeMenu()
    {
        // Try to call an existing SkillTree menu script if present.
        if (TryInvokeSkillTreeMenuMethod("ShowMenu"))
            return;

        if (TryInvokeSkillTreeMenuMethod("OpenMenu"))
            return;

        if (TryInvokeSkillTreeMenuMethod("ToggleMenu"))
            return;

        Debug.LogWarning("CrotteEvolutionButton: Aucun menu skills trouve (SkillTreeMenuUI). Le bouton est pret, il sera branche automatiquement quand le menu existera.");
    }

    private bool TryInvokeSkillTreeMenuMethod(string methodName)
    {
        Type menuType = FindTypeByName("SkillTreeMenuUI");
        if (menuType == null)
            return false;

        Component menuComponent = FindAnyObjectByType(menuType) as Component;
        if (menuComponent == null)
            return false;

        MethodInfo method = menuType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        if (method == null || method.GetParameters().Length != 0)
            return false;

        method.Invoke(menuComponent, null);
        return true;
    }

    private Type FindTypeByName(string typeName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type type = assemblies[i].GetType(typeName);
            if (type != null)
                return type;

            Type[] allTypes;
            try
            {
                allTypes = assemblies[i].GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }

            for (int j = 0; j < allTypes.Length; j++)
            {
                if (allTypes[j].Name == typeName)
                    return allTypes[j];
            }
        }

        return null;
    }
}
