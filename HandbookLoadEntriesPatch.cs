using HarmonyLib;
using Vintagestory.GameContent;

namespace AstrosHandbookLoadingFix;

[HarmonyPatch(typeof(GuiDialogHandbook), "loadEntries")]
public static class HandbookLoadEntriesPatch
{
    /// <summary>
    /// clears pages still coming from an older load, the game clears the handbook and loads every page again when
    /// hotkeys change
    /// </summary>
    /// <param name="__instance">the handbook</param>
    public static void Prefix(GuiDialogHandbook __instance)
    {
        HandbookPageLoader.Restart(__instance);
    }
}