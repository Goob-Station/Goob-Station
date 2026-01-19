using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Emag.Prototypes;

/// <summary>
/// Emag type prototype definitions.
/// Used to define different emag types for various emagged effects.
/// </summary>
[Prototype ("emagType")]
public sealed partial class EmagTypePrototype : IPrototype
{
    [IdDataField] 
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Optional display name for UI purposes.
    /// </summary>
    [DataField]
    public LocId? Name { get; set; }
}

/// How to use EmagTypePrototypes:
/// 
/// [Dependency] private readonly IPrototypeManager _prototype = default!; // goob edit
/// 
/// private readonly ProtoId<EmagTypePrototype> _emagIdInteraction = "Interaction"; // goob edit
/// EmagTypePrototype emagTypeInteraction = _prototype.Index(_emagIdInteraction); // goob edit

/// private readonly ProtoId<EmagTypePrototype> _emagIdAccess = "Access"; // goob edit
/// EmagTypePrototype emagTypeAccess = _prototype.Index(EmagIdAccess); // goob edit