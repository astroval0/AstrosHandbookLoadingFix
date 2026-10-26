using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace AstrosHandbookLoadingFix;

public class AstrosHandbookLoadingFixModSystem : ModSystem
{
    private Harmony? harmony;
    
    public static ILogger? Logger { get; private set; }
    
    /// <summary>
    /// only load on client
    /// </summary>
    /// <param name="forSide">the side that is loading the mods</param>
    /// <returns>true on the client</returns>
    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Client;
    }
    
    /// <summary>
    /// patches the handbook
    /// </summary>
    /// <param name="api">the client api</param>
    public override void StartClientSide(ICoreClientAPI api)
    {
        //Mod.Logger.Notification("woooooo im a ghooooost boooooo");
        Logger = Mod.Logger;
        harmony = new Harmony(Mod.Info.ModID);
        harmony.PatchAll(typeof(AstrosHandbookLoadingFixModSystem).Assembly);
    }

    /// <summary>
    /// takes the patches off again
    /// </summary>
    public override void Dispose()
    {
        harmony?.UnpatchAll(Mod.Info.ModID);
        Logger = null;
    }
}