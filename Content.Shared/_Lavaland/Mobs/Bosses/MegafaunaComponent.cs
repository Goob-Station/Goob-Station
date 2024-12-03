using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Mobs.Bosses;

[Virtual, RegisterComponent]
public partial class MegafaunaComponent : Component
{
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
