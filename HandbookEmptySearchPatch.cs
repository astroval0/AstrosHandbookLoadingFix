using System.Text.RegularExpressions;
using HarmonyLib;
using Vintagestory.API.Client;

namespace AstrosHandbookLoadingFix;

[HarmonyPatch(typeof(GuiDialog), nameof(GuiDialog.CountMatches))]
public static class HandbookEmptySearchPatch
{
    private const string EmptySearchPattern = "()";

    /// <summary>
    /// counts the matches of an empty search without running the regex. with nothing typed, the handbook searches every
    /// page with the regex (). that matches once at every character and once at the end, and running it over thousands
    /// of pages freezes the game
    /// </summary>
    /// <param name="text">the text that gets searched</param>
    /// <param name="regex">the search</param>
    /// <param name="__result">how many matches there are</param>
    /// <returns>false to skip running the regex</returns>
    public static bool Prefix(string text, Regex regex, ref int __result)
    {
        if (regex.ToString() != EmptySearchPattern)
        {
            return true;
        }

        __result = text.Length + 1;
        return false;
    }
}