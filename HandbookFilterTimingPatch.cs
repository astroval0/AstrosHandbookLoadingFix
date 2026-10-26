using System.Diagnostics;
using HarmonyLib;
using Vintagestory.GameContent;

namespace AstrosHandbookLoadingFix;

[HarmonyPatch(typeof(GuiDialogHandbook), nameof(GuiDialogHandbook.FilterItems))]
public class HandbookFilterTimingPatch
{
    
    /// <summary>
    /// remembers when filtering the handbook started
    /// Priority.First so the time counts the other prefixes as well
    /// </summary>
    /// <param name="__state">the start time, harmony passes it to de postfix</param>
    [HarmonyPriority(Priority.First)]
    public static void Prefix(out long __state)
    {
        __state = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// logs how long filtering took and how many pages it kept
    /// </summary>
    /// <param name="__state">the start time from the prefix</param>
    /// <param name="___shownHandbookPages">the pages the list shows now</param>
    /// <param name="___allHandbookPages">every page in the handbook</param>
    public static void Postfix(long __state, List<IFlatListItem> ___shownHandbookPages, List<GuiHandbookPage> ___allHandbookPages)
    {
        double ms = Stopwatch.GetElapsedTime(__state).TotalMilliseconds;
        AstrosHandbookLoadingFixModSystem.Logger?.Notification("filtering the handbook took {0:F1} ms, showing {1} of {2} pages", ms, ___shownHandbookPages.Count, ___allHandbookPages.Count);
    }
}