using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Systems;

namespace Content.Shared._F14.SCP;

// Blinking component for SCP-173
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlinkingComponent : Component
{
    [AutoNetworkedField] public bool IsBlinking = false;
    [AutoNetworkedField] public float CurrentTimer = 7f;
    [DataField("blinkInterval")] public float BlinkInterval = 7f;
    [DataField("blinkDuration")] public float BlinkDuration = 0.50f;
    [AutoNetworkedField] public bool IsAutoBlinking = false;
}

[Serializable, NetSerializable]
public sealed class BlinkChangedEvent : EntityEventArgs
{
    public bool IsBlinking { get; }
    public BlinkChangedEvent(bool isBlinking) => IsBlinking = isBlinking;
}

// SCP-173
[RegisterComponent]
public sealed partial class SCPFreezeOnSightComponent : Component
{
    [DataField("walkSpeed")] public float WalkSpeed = 8f;
    [DataField("sprintSpeed")] public float SprintSpeed = 12f;
    public bool IsWatched = false;
}

// SCP-096
[Serializable, NetSerializable]
public enum SCP096State : byte { Idle, Charging, Enraged, Dead }

[Serializable, NetSerializable]
public enum SCP096Visuals : byte { State }

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SCP096Component : Component
{
    [AutoNetworkedField] public SCP096State State = SCP096State.Idle;
    [AutoNetworkedField] public SCP096State PrevState = SCP096State.Idle;
    [AutoNetworkedField] public EntityUid? Target = null;
    [DataField("chargeTime")] public float ChargeTime = 20f;
    [AutoNetworkedField] public float CurrentChargeTimer = 0f;
    [DataField("calmSpeed")] public float CalmSpeed = 0f;
    [DataField("enragedSpeed")] public float EnragedSpeed = 15f;
    [DataField("idleSound")] public SoundSpecifier? IdleSound;
    [DataField("chargeSound")] public SoundSpecifier? ChargeSound;
    [DataField("enrageSound")] public SoundSpecifier? EnrageSound;
}

//scp scp scp 049
[RegisterComponent]
public sealed partial class SCP049Component : Component { }

//SCP-049-2
[RegisterComponent]
public sealed partial class SCP0492Component : Component { }
[Serializable, NetSerializable]
public sealed partial class SCP049CureDoAfterEvent : SimpleDoAfterEvent { }

// SCP-330
[RegisterComponent]
public sealed partial class SCP330Component : Component
{
    public Dictionary<EntityUid, int> TakenCount = new();
}

//scp-330 pink candy KABOOOOOM!
[RegisterComponent]
public sealed partial class SCP330PinkCandyComponent : Component { }

//scp-999 NYAAAAAAAA!!!!!!!!!
[RegisterComponent]
public sealed partial class SCP999Component : Component
{
    [DataField("cooldown")]
    public float Cooldown = 20f;

    public float Accumulator = 0f;

    [DataField("range")]
    public float Range = 2f;
}

//scp-113
[RegisterComponent]
public sealed partial class SCP113Component : Component
{
}

//scp-106
[Serializable, NetSerializable]
public enum SCP106Visuals : byte
{
    Submerged,
    FlashlightSlowed,
}

[Serializable, NetSerializable]
public enum SCP106VisualLayers : byte
{
    Base,
}

//main scp-106 components
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SCP106Component : Component
{
    [AutoNetworkedField]
    public bool IsSubmerged = false;

    [DataField]
    public float TileDecayChance = 0.40f;

    [DataField]
    public float FlashlightSpeedMult = 0.55f;

    [DataField]
    public float FlashlightLingerTime = 1.5f;

    public float FlashlightSlowTimer = 0f;

    [AutoNetworkedField]
    public bool IsFlashlightSlowed = false;

    [DataField]
    public string SCPPocketDimensionMap = "PocketDimension";

    [DataField] public string ActionToggleSubmerge = "ActionSCP106ToggleSubmerge";
    [DataField, AutoNetworkedField] public EntityUid? ActionToggleSubmergeEntity;

    [DataField] public string ActionMoveUp = "ActionSCP106MoveUp";
    [DataField, AutoNetworkedField] public EntityUid? ActionMoveUpEntity;

    [DataField] public string ActionMoveDown = "ActionSCP106MoveDown";
    [DataField, AutoNetworkedField] public EntityUid? ActionMoveDownEntity;
}

[RegisterComponent]
public sealed partial class SCP106SubmergedComponent : Component { }

[RegisterComponent]
public sealed partial class SCP106FlashlightSlowedComponent : Component
{
    public float SpeedMultiplier = 0.55f;
}
[RegisterComponent]
public sealed partial class SCP106BarrierComponent : Component
{
}

public sealed class SCP106FlashlightSlowSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SCP106FlashlightSlowedComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    private void OnRefresh(EntityUid uid,
        SCP106FlashlightSlowedComponent comp,
        RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(comp.SpeedMultiplier, comp.SpeedMultiplier);
    }
}

[RegisterComponent]
public sealed partial class SCPPocketDimensionComponent : Component { }

[RegisterComponent]
public sealed partial class AntiSCP106WallComponent : Component { }

//femure brake or evil ass rape machine
[Serializable, NetSerializable]
public enum FemurBreakerVisuals : byte
{
    State,
}

[Serializable, NetSerializable]
public enum FemurBreakerState : byte
{
    Idle,
    Activating,
    Used,
}

[RegisterComponent, NetworkedComponent]
public sealed partial class FemurBreakerComponent : Component
{
    [DataField]
    public SoundSpecifier ActivationSound =
        new SoundPathSpecifier("/Audio/SCP/femur_breaker.ogg");

    [DataField]
    public float VictimRange = 1.2f;

    [DataField]
    public float ActivationDelay = 1.5f;

    public bool Used = false;

    public float ActivationTimer = 0f;

    public bool Activating = false;
}

//scp-457 component
[Serializable, NetSerializable]
public enum SCP457Visuals : byte
{
    Attacking,
}

[Serializable, NetSerializable]
public enum SCP457VisualLayers : byte
{
    Base,
}

[RegisterComponent]
public sealed partial class SCP457Component : Component
{
    [DataField] public float IgniteRadius = 2.5f;
    [DataField] public float IgniteInterval = 1.0f;
    public float IgniteTimer = 0f;
    [DataField] public float FireDamagePerPulse = 8f;

    public bool IsAttacking = false;
}

// SCP-294 component

[RegisterComponent]
public sealed partial class SCP294Component : Component
{
    [DataField] public int QuartersRequired = 2;
    [DataField] public string ContainerPrototype = "Cup";
    [DataField] public float DispenserAmount = 30f;
    [ViewVariables(VVAccess.ReadWrite)] public int QuartersInserted = 0;
}

[Serializable, NetSerializable]
public sealed class SCP294BuiState : BoundUserInterfaceState
{
    public int QuartersRequired { get; }
    public int QuartersInserted { get; }
    public string? LastMessage { get; }

    public SCP294BuiState(int quartersRequired, int quartersInserted, string? lastMessage = null)
    {
        QuartersRequired = quartersRequired;
        QuartersInserted = quartersInserted;
        LastMessage = lastMessage;
    }
}

[Serializable, NetSerializable]
public sealed class SCP294RequestLiquidMessage : BoundUserInterfaceMessage
{
    public string LiquidName { get; }

    public SCP294RequestLiquidMessage(string liquidName)
    {
        LiquidName = liquidName;
    }
}

[Serializable, NetSerializable]
public sealed class SCP294InsertQuarterMessage : BoundUserInterfaceMessage { }

[Serializable, NetSerializable]
public enum SCP294UiKey : byte
{
    Key,
}

//  SCP-106 actions

public sealed partial class SCP106ToggleSubmersionEvent : InstantActionEvent { }

public sealed partial class SCP106MoveUpEvent : InstantActionEvent { }

public sealed partial class SCP106MoveDownEvent : InstantActionEvent { }

// KEYLOCK


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class KeylockComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Code = "0000";

    [DataField]
    public int MaxAttempts = 5;

    [AutoNetworkedField]
    public int FailedAttempts = 0;

    [AutoNetworkedField]
    public bool IsLocked = true;

    [AutoNetworkedField]
    public TimeSpan? LockedUntil = null;

    [DataField]
    public TimeSpan LockoutDuration = TimeSpan.FromSeconds(30);

    [DataField]
    public int CodeLength = 4;
}


[Serializable, NetSerializable]
public sealed class KeylockAttemptMessage : BoundUserInterfaceMessage
{
    public string AttemptedCode { get; }

    public KeylockAttemptMessage(string attemptedCode)
    {
        AttemptedCode = attemptedCode;
    }
}

[RegisterComponent]
public sealed partial class KeylockAccessComponent : Component
{
    [DataField("linkedDoor")]
    public EntityUid? LinkedDoor;
}

public sealed class KeylockAttemptEvent : EntityEventArgs
{
    public required string AttemptedCode { get; set; }
    public EntityUid User { get; set; }
}

public sealed class KeylockOpenEvent : EntityEventArgs
{
    public EntityUid User { get; set; }
}

public sealed class KeylockLockEvent : EntityEventArgs
{
    public EntityUid User { get; set; }
}
[Serializable, NetSerializable]
public sealed class KeylockBuiState : BoundUserInterfaceState
{
    public bool IsLocked { get; }
    public int FailedAttempts { get; }
    public int MaxAttempts { get; }

    public KeylockBuiState(bool isLocked, int failedAttempts, int maxAttempts)
    {
        IsLocked = isLocked;
        FailedAttempts = failedAttempts;
        MaxAttempts = maxAttempts;
    }
}
[Serializable, NetSerializable]
public enum KeylockUiKey : byte
{
    Key
}


// SCP-1499 component
[RegisterComponent]
public sealed partial class SCP1499Component : Component
{
    // saves coordinates
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityCoordinates? SavedLocation;

    // saves UID
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? CurrentUser;
}

// teleporter
[RegisterComponent]
public sealed partial class SCP1499DimensionComponent : Component { }


// SCP-458
[RegisterComponent]
public sealed partial class SCP458Component : Component
{
    //list of pizzas
    [DataField]
    public List<string> PizzaPrototypes = new()
    {
        "FoodPizzaMargherita",
        "FoodPizzaMeat",
        "FoodPizzaMushroom",
    };
}

