using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SpecialAnimation;

/// <summary>
/// Prototype for custom SpecialAnimationData.
/// </summary>
[Prototype]
public sealed partial class SpecialAnimationPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public SpecialAnimationData Animation = SpecialAnimationData.DefaultAnimation;
}
