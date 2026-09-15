using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Oskarrr.Yautja.Components;

public enum YautjaSmartDiscState : byte
{
    Idle = 0,
    Flying = 1,
    Orbiting = 2,
    Returning = 3,
}

/// <summary>
/// Розумний диск Яутжа: по Z летить до найближчої живої цілі, кружляє і завдає шкоди, потім повертається.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class YautjaSmartDiscComponent : Component
{
    [DataField, AutoNetworkedField]
    public YautjaSmartDiscState State;

    [DataField, AutoNetworkedField]
    public EntityUid? Thrower;

    [DataField, AutoNetworkedField]
    public EntityUid? Target;

    [DataField]
    public float Range = 12f;

    [DataField]
    public float ThrowSpeed = 18f;

    [DataField]
    public float ReturnSpeed = 16f;

    [DataField]
    public float ArrivalDistance = 1.25f;

    [DataField]
    public float PickupDistance = 1.5f;

    [DataField]
    public float OrbitDistance = 0.9f;

    /// <summary>
    /// Тривалість одного оберту (сек). За цей час наноситься шкода один раз.
    /// </summary>
    [DataField]
    public float OrbitPeriod = 0.4f;

    [DataField]
    public int MaxOrbits = 10;

    [DataField, AutoNetworkedField]
    public int CompletedOrbits;

    [DataField, AutoNetworkedField]
    public float OrbitAccumulator;

    [DataField]
    public float HomingRetargetInterval = 0.25f;

    [DataField, AutoNetworkedField]
    public float HomingTimer;

    [DataField]
    public DamageSpecifier OrbitDamage = new();
}