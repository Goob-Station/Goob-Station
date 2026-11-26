using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bongle;

/// <summary>
/// Begins do-after. Upon finishing, charges forward and damages the first entity hit.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChargedHeadbuttComponent : Component
{
    [DataField]
    public float DashDistance = 5f;

    [DataField]
    public float DashSpeed = 6f;

    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public TimeSpan TargetStun = TimeSpan.Zero;

    [DataField]
    public TimeSpan TargetKnockdown = TimeSpan.FromSeconds(2);

    [DataField]
    public SoundSpecifier? ChargeSound;

    [DataField]
    public SoundSpecifier? ImpactSound = new SoundPathSpecifier("/Audio/Weapons/punch1.ogg");

    [ViewVariables, AutoNetworkedField]
    public bool IsCharging;

    [DataField]
    public TimeSpan ChargeTime = TimeSpan.FromSeconds(1.5);

    [DataField]
    public LocId? StartPopup = "charged-headbutt-start";

    [DataField]
    public bool TargetDropItems = true;
}

public sealed partial class ChargedHeadbuttEvent : WorldTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class ChargedHeadbuttDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetCoordinates TargetCoordinates;

    public override DoAfterEvent Clone() => this;
}
