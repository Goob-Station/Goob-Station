using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.StationRadio.Components;

/// <summary>
/// Component that allows a vinyl disk to spawn a game rule when it finishes playing.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class VinylSummonRuleComponent : Component
{
    /// <summary>
    /// The game rule prototype to spawn when the vinyl finishes playing.
    /// </summary>
    [DataField(required: true)]
    public string GameRule = string.Empty;

    /// <summary>
    /// Sound played when the vinyl burns to ash.
    /// </summary>
    [DataField]
    public SoundSpecifier BurnSound = new SoundPathSpecifier("/Audio/Effects/cig_snuff.ogg");
}
