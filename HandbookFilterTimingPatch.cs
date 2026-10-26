using System.Diagnostics;
using HarmonyLib;
using Vintagestory.GameContent;

namespace AstrosHandbookLoadingFix;

[HarmonyPatch(typeof(GuiDialogHandbook), nameof(GuiDialogHandbook.OnGuiOpened))]
public class HandbookFilterTimingPatch
{
    public static void Prefix(out long __state)
    {
        __state = Stopwatch.GetTimestamp();
    }

    public static void Postfix(long __state, List<IFlatListItem> ___shownHandbookPages, List<GuiHandbookPage> ___allHandbookPages)
    {
        double ms = Stopwatch.GetElapsedTime(__state).TotalMilliseconds;
        AstrosHandbookLoadingFixModSystem.Logger?.Notification("filtering the handbook took {0:F1} ms, showing {1} of {2} pages", ms, ___shownHandbookPages.Count, ___allHandbookPages.Count);
    }
}