using Robust.Shared.Configuration;

namespace Content.Shared.DeltaV.CCVars;

/// <summary>
/// DeltaV specific cvars.
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming - Shush you
public sealed class DCCVars
{
    /// <summary>
    /// Whether the no EORG popup is enabled.
    /// </summary>
    public static readonly CVarDef<bool> RoundEndNoEorgPopup =
        CVarDef.Create("game.round_end_eorg_popup_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Skip the no EORG popup.
    /// </summary>
    public static readonly CVarDef<bool> SkipRoundEndNoEorgPopup =
        CVarDef.Create("game.skip_round_end_eorg_popup", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// How long to display the EORG popup for.
    /// </summary>
    public static readonly CVarDef<float> RoundEndNoEorgPopupTime =
        CVarDef.Create("game.round_end_eorg_popup_time", 5f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Disables all vision filters for species like Vulpkanin or Harpies. There are good reasons someone might want to disable these.
    /// </summary>
    public static readonly CVarDef<bool> NoVisionFilters =
        CVarDef.Create("accessibility.no_vision_filters", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
