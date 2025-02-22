using Robust.Shared.Prototypes;
using System.Threading;

namespace Content.Server._Lavaland.Mobs;

[Virtual, RegisterComponent]
public partial class MegafaunaComponent : Component
{
    /// <summary>
    ///     Used for all the timers that get assigned to the boss.
    ///     In theory all bosses should use it so i'll just leave it here.
    /// </summary>
    [NonSerialized] public CancellationTokenSource CancelToken = new();

    /// <summary>
    ///     Whether or not it should power trip aggressors or random locals
    /// </summary>
    [DataField] public bool Aggressive = false;

    /// <summary>
    ///     Should it drop guaranteed loot when dead? If so what exactly?
    /// </summary>
    [DataField] public EntProtoId? Loot = null;

    /// <summary>
    ///     Should it drop something besides the main loot as a crusher only reward?
    /// </summary>
    [DataField] public EntProtoId? CrusherLoot = null;

    /// <summary>
    ///     Check if the boss got damaged by crusher only.
    ///     True by default. Will immediately switch to false if anything else hit it. Even the environmental stuff.
    /// </summary>
    [DataField] public bool CrusherOnly = true;
}
