using HarmonyLib;
using Vintagestory.GameContent;

namespace AstrosHandbookLoadingFix;

[HarmonyPatch(typeof(GuiHandbookPage), MethodType.Constructor)]
public static class HandbookPageMadePatch
{
    /// <summary>
    /// tells the page loader a page started being made, it only takes pages made while it is loading
    /// </summary>
    /// <param name="__instance">the page</param>
    public static void Postfix(GuiHandbookPage __instance)
    {
        HandbookPageLoader.OnPageMade(__instance);
    }
}