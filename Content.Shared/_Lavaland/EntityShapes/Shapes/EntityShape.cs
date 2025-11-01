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
    /// A weight used to pick between shapes.
    /// </summary>
    [DataField]
    public float Weight = 1;

    /// <summary>
    /// If specified, will add this shape into a shapes group,
    /// that can be customized via <see cref="AllEntityShape"/>.
    /// That way you can change size or offset for groups of tiles
    /// instead of individually changing values.
    /// </summary>
    [DataField("group")]
    public string? OverrideGroup;

    // All "DefaultX" are values that are specified in prototypes

    [DataField("offset")]
    public Vector2? DefaultOffset;

    [DataField("size")]
    public int? DefaultSize;

    [DataField("step")]
    public float? DefaultStepSize;

    [ViewVariables]
    public Vector2 Offset = Vector2.Zero;

    [ViewVariables]
    public int Size = 1;

    [ViewVariables]
    public float StepSize = 1;

    /// <summary>
    /// Calculates this shape and also lets you customize some parameters of shape's generation.
    /// </summary>
    public List<Vector2> GetShape(
        System.Random rand,
        IPrototypeManager proto,
        Vector2? center = null,
        int? size = null,
        float? stepSize = null)
    {
        // We take values by these priorities:
        // 1. YAML DataFields
        // 2. Arguments passed from the parent
        // 3. Default value.
        Offset = DefaultOffset ?? center ?? Offset;
        Size = DefaultSize ?? size ?? Size;
        StepSize = DefaultStepSize  ?? stepSize ?? StepSize;
        return GetShapeImplementation(rand, proto);
    }

    protected abstract List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto);
}
