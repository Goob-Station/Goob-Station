using Content.Shared.Chemistry.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects.Effects;

/// <summary>
///     Creates smoke similar to SmokeOnTrigger
/// </summary>
public sealed partial class DoSmokeEffect : EntityEffectBase<DoSmokeEffect>
{
    /// <summary>
    /// How long the smoke stays for, after it has spread.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How much the smoke will spread.
    /// </summary>
    [DataField(required: true)]
    public int SpreadAmount;

    /// <summary>
    /// Smoke entity to spawn.
    /// Defaults to smoke but you can use foam if you want.
    /// </summary>
    [DataField]
    public EntProtoId SmokePrototype = "Smoke";

    /// <summary>
    /// Solution to add to each smoke cloud.
    /// </summary>
    [DataField]
    public Solution Solution = new();

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null; // TODO
}
