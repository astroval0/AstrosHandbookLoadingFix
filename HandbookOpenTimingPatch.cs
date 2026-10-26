using System.Diagnostics;
using HarmonyLib;
using Vintagestory.GameContent;

namespace AstrosHandbookLoadingFix;

[HarmonyPatch(typeof(GuiDialogHandbook), nameof(GuiDialogHandbook.OnGuiOpened))]
public class HandbookOpenTimingPatch
{
    public static void Prefix(out long __state)
    {
        __state = Stopwatch.GetTimestamp();
    }

    public static void Postfix(long __state)
    {
        double ms = Stopwatch.GetElapsedTime(__state).TotalMilliseconds;
        AstrosHandbookLoadingFixModSystem.Logger?.Notification("opening the handbook took {0:F1} ms", ms);
    }
}