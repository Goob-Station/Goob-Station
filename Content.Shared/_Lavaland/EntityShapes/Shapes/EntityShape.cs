using System.Numerics;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Shapes;

/// <summary>
/// Represents a list of points that entities can be then spawned on.
/// </summary>
[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class EntityShape
{
    /// <summary>
    /// If specified, will add this shape into a shapes group,
    /// that can be customized via <see cref="GroupEntityShape"/>.
    /// That way you can change size or offset for groups of tiles
    /// instead of individually changing values.
    /// </summary>
    [DataField("group")]
    public string? OverrideGroup;

    [DataField("offset")]
    public Vector2 DefaultOffset;

    [DataField("size")]
    public int DefaultSize;

    [DataField("step")]
    public int DefaultStepSize;

    [ViewVariables]
    public Vector2 Offset;

    [ViewVariables]
    public int Size;

    [ViewVariables]
    public int StepSize;

    /// <summary>
    /// Calculates this shape and also lets you customize some parameters of shape's generation.
    /// </summary>
    public List<Vector2> GetShape(
        System.Random rand,
        IPrototypeManager proto,
        Vector2? center = null,
        int? size = null,
        int? stepSize = null)
    {
        Offset = DefaultOffset + (center ?? Vector2.Zero);
        Size = size ?? DefaultSize;
        StepSize = stepSize ?? DefaultStepSize;
        return GetShapeImplementation(rand, proto);
    }

    protected abstract List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto);
}
