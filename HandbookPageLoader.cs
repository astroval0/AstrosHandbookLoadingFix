using System.Diagnostics;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace AstrosHandbookLoadingFix;

public class HandbookPageLoader
{
    private const double HandOverIntervalMs = 200;

    private static readonly AccessTools.FieldRef<GuiDialogHandbook, List<GuiHandbookPage>> AllPagesField =
        AccessTools.FieldRefAccess<GuiDialogHandbook, List<GuiHandbookPage>>("allHandbookPages");

    private static readonly AccessTools.FieldRef<GuiDialogHandbook, Dictionary<string, int>> PageNumbersField =
        AccessTools.FieldRefAccess<GuiDialogHandbook, Dictionary<string, int>>("pageNumberByPageCode");

    private static readonly AccessTools.FieldRef<GuiDialogHandbook, bool> LoadingField =
        AccessTools.FieldRefAccess<GuiDialogHandbook, bool>("loadingPagesAsync");

    private static readonly ConditionalWeakTable<GuiDialogHandbook, StrongBox<int>> LoadCounts = new();

    [ThreadStatic] private static HandbookPageLoader? current;

    private readonly GuiDialogHandbook handbook;
    private readonly ICoreClientAPI capi;
    private readonly int loadCount;
    private readonly List<GuiHandbookPage> finishedPages = [];
    private readonly List<GuiHandbookPage> handedOverPages = [];
    private GuiHandbookPage? pageBeingMade;
    private long lastHandOver;
    private int customPageCount = -1;

    /// <summary>
    /// starts loading the pages of a handbook
    /// </summary>
    /// <param name="handbook">the handbook</param>
    /// <param name="capi">the client api</param>
    private HandbookPageLoader(GuiDialogHandbook handbook, ICoreClientAPI capi)
    {
        this.handbook = handbook;
        this.capi = capi;
        loadCount = Volatile.Read(ref LoadCounts.GetOrCreateValue(handbook).Value);
        lastHandOver = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// notes that a handbook started loading its pages again, pages still coming from an older load get dropped
    /// </summary>
    /// <param name="handbook">the handbook</param>
    public static void Restart(GuiDialogHandbook handbook)
    {
        Interlocked.Increment(ref LoadCounts.GetOrCreateValue(handbook).Value);
    }

    /// <summary>
    /// makes the pages with the game's own page maker on this worker thread and hands them to the handbook while they
    /// get made, instead of all at once at the end
    /// </summary>
    /// <param name="handbook">the handbook</param>
    /// <param name="capi">the client api</param>
    /// <param name="makePages">the game's page maker</param>
    public static void Run(GuiDialogHandbook handbook, ICoreClientAPI capi, OnCreatePagesDelegate makePages)
    {
        HandbookPageLoader loader = new(handbook, capi);
        current = loader;
        List<GuiHandbookPage> pages;
        try
        {
            pages = makePages();
        }
        finally
        {
            current = null;
        }

        loader.Finish(pages);
    }

    /// <summary>
    /// notes a page that just started being made, when it is on a thread that is loading pages
    /// </summary>
    /// <param name="page">the page</param>
    public static void OnPageMade(GuiHandbookPage page)
    {
        current?.Add(page);
    }

    /// <summary>
    /// keeps a page until the next one starts being made, by then the game is done with it. meal pages get their
    /// recipe cache made right after the page, opening one before that would break
    /// </summary>
    /// <param name="page">the page that just started being made</param>
    private void Add(GuiHandbookPage page)
    {
        if (pageBeingMade != null)
        {
            finishedPages.Add(pageBeingMade);
        }

        pageBeingMade = page;
        if (finishedPages.Count > 0 && Stopwatch.GetElapsedTime(lastHandOver).TotalMilliseconds >= HandOverIntervalMs)
        {
            HandOver();
        }
    }

    /// <summary>
    /// hands the finished pages to the main thread. the handbook reads its page list on the main thread while it
    /// filters and draws, so only the main thread can add to it
    /// </summary>
    private void HandOver()
    {
        List<(GuiHandbookPage Page, string Code)> pages = finishedPages.Select(page => (page, page.PageCode)).ToList();
        finishedPages.Clear();
        lastHandOver = Stopwatch.GetTimestamp();
        capi.Event.EnqueueMainThreadTask(() => AddToHandbook(pages), "astroshandbookloadingfix-pages");
    }

    /// <summary>
    /// hands over the last pages once the game's page maker is done, then has the main thread check the result
    /// </summary>
    /// <param name="pages">every page the game's page maker made</param>
    private void Finish(List<GuiHandbookPage> pages)
    {
        if (pageBeingMade != null)
        {
            finishedPages.Add(pageBeingMade);
            pageBeingMade = null;
        }

        if (finishedPages.Count > 0)
        {
            HandOver();
        }

        capi.Event.EnqueueMainThreadTask(() => FinishInHandbook(pages), "astroshandbookloadingfix-pages");
    }

    /// <summary>
    /// adds pages to the end of the handbook with their numbers the same way the game numbers them, and shows them
    /// when the handbook is open
    /// </summary>
    /// <param name="pages">the pages with their page codes</param>
    private void AddToHandbook(List<(GuiHandbookPage Page, string Code)> pages)
    {
        if (IsOutdated())
        {
            return;
        }

        List<GuiHandbookPage> allPages = AllPagesField(handbook);
        Dictionary<string, int> pageNumbers = PageNumbersField(handbook);
        if (customPageCount < 0)
        {
            customPageCount = allPages.Count;
        }

        foreach ((GuiHandbookPage page, string code) in pages)
        {
            page.PageNumber = allPages.Count;
            pageNumbers[code] = page.PageNumber;
            allPages.Add(page);
            handedOverPages.Add(page);
        }

        LoadingField(handbook) = false;
        RefreshIfOpen();
    }

    /// <summary>
    /// makes sure the handbook ends up with exactly the pages the game made. normally they all got handed over
    /// already, but pages made somewhere else than this thread never got noticed
    /// </summary>
    /// <param name="pages">every page the game's page maker made</param>
    private void FinishInHandbook(List<GuiHandbookPage> pages)
    {
        if (IsOutdated())
        {
            return;
        }

        List<GuiHandbookPage> allPages = AllPagesField(handbook);
        if (customPageCount < 0)
        {
            customPageCount = allPages.Count;
        }

        if (handedOverPages.SequenceEqual(pages))
        {
            for (int i = 0; i < customPageCount; ++i)
            {
                allPages[i].PageNumber = i;
            }
        }
        else
        {
            allPages.RemoveRange(customPageCount, allPages.Count - customPageCount);
            allPages.AddRange(pages);
            NumberAllPages(allPages);
        }

        LoadingField(handbook) = false;
        RefreshIfOpen();
    }

    /// <summary>
    /// numbers every page the way the game does once its pages are loaded, a later page wins a page code
    /// </summary>
    /// <param name="allPages">the pages of the handbook</param>
    private void NumberAllPages(List<GuiHandbookPage> allPages)
    {
        Dictionary<string, int> pageNumbers = PageNumbersField(handbook);
        pageNumbers.Clear();
        for (int i = 0; i < allPages.Count; ++i)
        {
            pageNumbers[allPages[i].PageCode] = allPages[i].PageNumber = i;
        }
    }

    /// <summary>
    /// checks if the handbook started loading its pages again since this load started
    /// </summary>
    /// <returns>true if this load is outdated</returns>
    private bool IsOutdated()
    {
        return Volatile.Read(ref LoadCounts.GetOrCreateValue(handbook).Value) != loadCount;
    }

    /// <summary>
    /// fills the list of the handbook again when it is open
    /// </summary>
    private void RefreshIfOpen()
    {
        if (handbook.IsOpened())
        {
            handbook.FilterItems();
        }
    }
}