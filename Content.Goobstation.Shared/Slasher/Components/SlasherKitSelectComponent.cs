using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Allows the Slasher to choose their kit (starting gear) when they first spawn.
/// </summary>
[RegisterComponent]
public sealed partial class SlasherKitSelectComponent : Component
{
    [DataField]
    public bool KitSelected;

    /// <summary>
    /// The kits on offer and the components every slasher gets after picking one.
    /// </summary>
    [DataField]
    public ProtoId<SlasherKitListPrototype> KitList = "SlasherKits";

    /// <summary>
    /// Offers every kit regardless of the player's ascensions. For debug and admin slashers.
    /// </summary>
    [DataField]
    public bool IgnoreAscensionLocks;

    /// <summary>
    /// Default song for the trailer music.
    /// </summary>
    [DataField]
    public SoundSpecifier DefaultThemeSong = new SoundPathSpecifier(
        "/Audio/_Goobstation/Slasher/Music/slasher_serial_killer_murder_frenzy_insane_horror_soundtrack.ogg");
}
