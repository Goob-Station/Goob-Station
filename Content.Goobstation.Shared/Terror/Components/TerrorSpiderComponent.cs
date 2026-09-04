using Content.Goobstation.Shared.Terror.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Terror spider.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TerrorSpiderComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<TerrorSpiderPrototype> SpiderType;

    /// <summary>
    /// The queen of this spider's hive, if there even is one.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Queen;

    /// <summary>
    /// Corpses this spider has wrapped. Affects scaling of hive and regen.
    /// </summary>
    [DataField]
    public int WrappedAmount;

    /// <summary>
    /// The base regen before being affected by hive scaling.
    /// </summary>
    [DataField]
    public DamageSpecifier? BaselineRegen;

    /// <summary>
    /// Wrap count at which the regen scaling caps out to avoid immortal spiders.
    /// </summary>
    [DataField]
    public float MaxRegenCorpses = 10f;

    /// <summary>
    /// Chance of this spider being gibbed upon the Queen dying. Default is a coin toss.
    /// </summary>
    [DataField]
    public float QueenDeathGibChance = 0.5f;
}
