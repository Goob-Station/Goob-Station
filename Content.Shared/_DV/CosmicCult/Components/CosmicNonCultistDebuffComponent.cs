


using Content.Shared.Damage;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Goobstation system. For non-cultist equipment debuff.
/// Makes the target take damage over time.
/// Meant to be used in conjunction with statusEffectSystem.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CosmicEntropyNonCultistComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan CheckTimer = default!;

    [DataField]
    public TimeSpan CheckWait = TimeSpan.FromSeconds(3);

    /// <summary>
    /// The chance to recieve a message popup while under the effects of Entropic Degen.
    /// </summary>
    [DataField]
    public float PopupChance = 0.00f;

    /// <summary>
    /// The debuff applied while the component is present.
    /// </summary>
    [DataField]
    public DamageSpecifier Degen = new()
    {
        DamageDict = new()
        {
            { "Cold", 10.0},
            { "Asphyxiation", 20.0},
            { "Ion", 20.0},
        }
    };
}
