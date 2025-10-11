using Content.Goobstation.Shared.Werewolf.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class WerewolfCursedBiteComponent : Component
{
    /// <summary>
    ///  The damage to deal to the target
    /// </summary>
    [DataField]
    public DamageSpecifier? Damage = new();

    /// <summary>
    ///  The bleeding to deal to the target
    /// </summary>
    [DataField]
    public float BleedingAmount = -50;

    /// <summary>
    ///  The forms to transfer to the target
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<WerewolfFormPrototype>> FormsToTransfer = new();

    /// <summary>
    ///  The form the target will start in
    /// </summary>
    [DataField(required: true)]
    public ProtoId<WerewolfFormPrototype> StartingForm;

    /// <summary>
    ///  The form the target will start in, if the target is pathlocked.
    /// Usually, it exists for mindshielded individuals.
    /// </summary>
    [DataField]
    public ProtoId<WerewolfFormPrototype> PathLockedForm;

    [ViewVariables]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    ///  How many seconds the user has to be in chokehold phase in order for the ability to be used.
    /// Once the duration reaches the seconds specified, the user will be able to use the ability on the victim
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5); // todo: make it 15

    /// <summary>
    ///  Whether the ability can be used, or not.
    /// </summary>
    [ViewVariables]
    public bool CanBeUsed;

    /// <summary>
    ///  Whether the ability's duration countdown has been activated, or not.
    /// Happens when user holds someone in chokehold state during grab.
    /// </summary>
    [ViewVariables]
    public bool Active;

    /// <summary>
    ///  The target that the ability will be used on.
    /// This ensures that you can only use this ability on the entity you are chokeholding.
    /// </summary>
    [ViewVariables]
    public EntityUid? Target;
}

/// <summary>
/// Raised on an entity once a user uses Cursed Bite on them
/// </summary>
[ByRefEvent]
public record struct CursedBiteUsedEvent(
    EntityUid? Target,
    EntityUid? Converter,
    bool Handled = false,
    bool Cancelled = false);
