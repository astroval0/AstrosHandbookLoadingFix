using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace AstrosHandbookLoadingFix;

public class AstrosHandbookLoadingFixModSystem : ModSystem
{
    /// <summary>
    /// only load on client
    /// </summary>
    /// <param name="forSide">the side that is loading the mods</param>
    /// <returns>true on the client</returns>
    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Client;
    }
    
    public override void StartClientSide(ICoreClientAPI api)
    {
        Mod.Logger.Notification("woooooo im a ghooooost boooooo");
    }
}