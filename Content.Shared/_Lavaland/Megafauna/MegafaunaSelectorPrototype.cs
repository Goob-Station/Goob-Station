using Content.Shared._Lavaland.Megafauna.Selectors;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna;

/// <summary>
/// Contains one or multiple TileShapes to create a pattern.
/// </summary>
[Prototype]
public sealed partial class MegafaunaSelectorPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public MegafaunaSelector Action = default!;
}
