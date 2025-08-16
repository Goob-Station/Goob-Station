using Content.Shared._Lavaland.EntityShapes.Shapes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes;

/// <summary>
/// Contains one or multiple EntityShapes to create a pattern.
/// </summary>
[Prototype]
public sealed partial class EntityShapePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public EntityShape Shape = default!;
}
