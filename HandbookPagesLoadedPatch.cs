using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace AstrosHandbookLoadingFix;

[HarmonyPatch(typeof(GuiDialogHandbook), "LoadPages_Async")]
public static class HandbookPagesLoadedPatch
{
    /// <summary>
    /// fills the list of an open handbook once all its pages are made. the game makes them on a worker thread after
    /// joining and shows nothing until they are all done, but it never fills the list when they are, so a handbook
    /// opened in that time stays empty until a tab gets clicked or it gets opened again
    /// </summary>
    /// <param name="__instance">the handbook</param>
    /// <param name="___capi">the client api</param>
    public static void Postfix(GuiDialogHandbook __instance, ICoreClientAPI ___capi)
    {
        ___capi.Event.EnqueueMainThreadTask(() => RefreshIfOpen(__instance), "astroshandbookloadingfix-refresh");
    }

    /// <summary>
    /// fills the list again when the handbook is open, this runs on the main thread
    /// </summary>
    /// <param name="handbook">the handbook</param>
    private static void RefreshIfOpen(GuiDialogHandbook handbook)
    {
        if (handbook.IsOpened())
        {
            handbook.FilterItems();
        }
    }
}