using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.Core.Tokens;

namespace Content.Goobstation.Shared.Voidwalker;

[RegisterComponent, NetworkedComponent]
public sealed partial class VoidwalkerComponent : Component
{
    [DataField]
    public bool IsInSpace;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextSpacedCheck;

    [DataField]
    public TimeSpan SpacedCheckInterval = TimeSpan.FromSeconds(2);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextHealingTick;

    [DataField]
    public TimeSpan HealingTickInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How much to heal the voidwalker by when they're spaced.
    /// </summary>
    [DataField]
    public DamageSpecifier? HealingWhenSpaced;

    /// <summary>
    /// If the entity being pulled is space immune, this will be true so we don't remove it accidentally.
    /// </summary>
    [DataField]
    public bool EntityPulledWasSpaceImmune;

    /// <summary>
    /// What to multiply the voidwalker's speed by when they're in a non-spaced area.
    /// </summary>
    [DataField]
    public float NonSpacedSpeedModifier = 0.7f;

    /// <summary>
    /// Structures with this tag are convertable.
    /// </summary>
    [DataField]
    public ProtoId<TagPrototype> WallTag = "Wall";

    /// <summary>
    /// Structures that are converted are granted this tag.
    /// </summary>
    [DataField]
    public ProtoId<TagPrototype> VoidedStructureTag = "VoidedStructure";

    /// <summary>
    /// How long does it take to convert a wall?
    /// </summary>
    [DataField]
    public TimeSpan WallConvertTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long you need to stare at someone to stun em
    /// </summary>
    [DataField]
    public TimeSpan UnsettleDoAfterDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan UnsettleStunDuration= TimeSpan.FromSeconds(4);

    [DataField]
    public float UnsettleStaminaDamage = 80f;

    [ViewVariables(VVAccess.ReadOnly)]
    public ushort? UnsettleDoAfterId;

    /// <summary>
    /// How long does it take to send ya?
    /// </summary>
    [DataField]
    public TimeSpan KidnapDoAfterDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long are we sendin' ya fer?
    /// </summary>
    [DataField]
    public TimeSpan KidnapDuration = TimeSpan.FromSeconds(30);

    [DataField]
    public EntProtoId CosmicSkull = "CosmicSkull";

}
