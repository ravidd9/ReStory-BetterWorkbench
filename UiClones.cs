using Restory.UserInterface;
using UnityEngine;

namespace ReStoryBetterWorkbench;

internal static class UiClones
{
    internal static void DisableInheritedLocalization(GameObject clone)
    {
        foreach (GUI_LocalisedText localizedText in clone.GetComponentsInChildren<GUI_LocalisedText>(true))
        {
            localizedText.IsEnabled = false;
        }
    }
}
