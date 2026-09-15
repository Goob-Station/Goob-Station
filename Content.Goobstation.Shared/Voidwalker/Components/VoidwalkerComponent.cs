using Content.Goobstation.Shared.SpecialAnimation;
using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Voidwalker.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VoidwalkerComponent : Component
{
    [DataField]
    public bool IsInSpace;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextSpacedCheck;

    /// <summary>
    /// If the voidwalker is within this much of a passed object, don't count it as being in space.
    /// This is to prevent being able to stand inside a passed object, since they have no atmosphere inside.
    /// If you can think of a better way to handle this, do tell me - delph
    /// </summary>
    public float PassedObjectGraceRange = 1.0f; //

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

    [DataField]
    public EntProtoId CosmicSkull = "CosmicSkull";

}
