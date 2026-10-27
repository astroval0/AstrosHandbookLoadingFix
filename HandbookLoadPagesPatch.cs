using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace AstrosHandbookLoadingFix;

[HarmonyPatch(typeof(GuiDialogHandbook), "LoadPages_Async")]
public static class HandbookLoadPagesPatch
{
    /// <summary>
    /// loads the pages of the handbook on the same worker thread as the game, but hands them over while they get
    /// made instead of all at once at the end, so the handbook isnt empty for the first seconds after joining
    /// </summary>
    /// <param name="__instance">the handbook</param>
    /// <param name="___capi">the client api</param>
    /// <param name="___createPageHandlerAsync">the game's page maker</param>
    /// <returns>false to skip the game's version</returns>
    public static bool Prefix(GuiDialogHandbook __instance, ICoreClientAPI ___capi,
        OnCreatePagesDelegate ___createPageHandlerAsync)
    {
        HandbookPageLoader.Run(__instance, ___capi, ___createPageHandlerAsync);
        return false;
    }
}