using System.Diagnostics;
using HarmonyLib;
using Vintagestory.GameContent;

namespace AstrosHandbookLoadingFix;

[HarmonyPatch(typeof(GuiDialogHandbook), nameof(GuiDialogHandbook.OnGuiOpened))]
public class HandbookOpenTimingPatch
{
    /// <summary>
    /// remembers when the handbook started opening
    /// </summary>
    /// <param name="__state">the start time</param>
    public static void Prefix(out long __state)
    {
        __state = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// logs how long opening the handbook took
    /// </summary>
    /// <param name="__state">the start time from the prefix</param>
    public static void Postfix(long __state)
    {
        double ms = Stopwatch.GetElapsedTime(__state).TotalMilliseconds;
        AstrosHandbookLoadingFixModSystem.Logger?.Notification("opening the handbook took {0:F1} ms", ms);
    }
}