using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.Yautja.Components;

public enum YautjaBracerSelfDestructPhase : byte
{
    None = 0,
    Arming = 1,
    Countdown = 2,
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class YautjaBracerComponent : Component
{
    public const string ClawsContainerId = "yautja-bracer-claws";
    public const string ShieldContainerId = "yautja-bracer-shield";

    [DataField]
    public EntProtoId ClawsPrototype = "GoobYautjaWristBlades";

    [DataField]
    public EntProtoId ShieldPrototype = "GoobYautjaBracerShield";

    /// <summary>
    /// Managed wrist-blade entity stored in <see cref="ClawsContainerId"/> or held when extended.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ClawsEntity;

    /// <summary>
    /// Managed bracer shield stored in <see cref="ShieldContainerId"/> or held when extended.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ShieldEntity;

    [DataField]
    public SoundSpecifier ClawsExtendSound =
        new SoundPathSpecifier("/Audio/_Oskarrr/Yautja/Equipment/pred_attach.wav");

    [DataField]
    public SoundSpecifier ShieldExtendSound =
        new SoundPathSpecifier("/Audio/_Oskarrr/Yautja/Equipment/pred_attach.wav");

    [DataField]
    public SoundSpecifier SelfDestructDoAfterSound =
        new SoundPathSpecifier("/Audio/_Oskarrr/Yautja/Equipment/self_destruct_doafter.wav");

    [DataField]
    public SoundSpecifier SelfDestructCountdownSound =
        new SoundPathSpecifier("/Audio/_Oskarrr/Yautja/Equipment/pred_countdown.ogg");

    [DataField]
    public TimeSpan SelfDestructCountdown = TimeSpan.FromSeconds(8);

    [DataField]
    public EntProtoId SelfDestructExplosionPrototype = "GoobYautjaBracerSelfDestructBurst";

    [DataField, AutoNetworkedField]
    public bool SelfDestructing;

    [DataField, AutoNetworkedField]
    public YautjaBracerSelfDestructPhase SelfDestructPhase;

    [DataField, AutoNetworkedField]
    public TimeSpan? SelfDestructAt;

    [DataField, AutoNetworkedField]
    public EntityUid? SelfDestructUser;

    public EntityUid? SelfDestructAction;

    [DataField]
    public SoundSpecifier CloakOnSound =
        new SoundPathSpecifier("/Audio/_Oskarrr/Yautja/Equipment/pred_cloakon.wav");

    [DataField]
    public SoundSpecifier CloakOffSound =
        new SoundPathSpecifier("/Audio/_Oskarrr/Yautja/Equipment/pred_cloakoff.wav");

    [DataField]
    public EntProtoId CloakDisappearEffect = "GoobYautjaDisappearEffect";

    [DataField, AutoNetworkedField]
    public bool Cloaked;

    [DataField, AutoNetworkedField]
    public EntityUid? CloakUser;
}

/// <summary>
/// Кігті, висунуті з наручника. Не знімаються вручну — лише через браслет.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class YautjaBracerClawsComponent : Component
{
    /// <summary>Runtime link to the owning bracer. Nullable so prototype save tests do not serialize invalid Uids.</summary>
    [DataField]
    public EntityUid? Bracer;
}

/// <summary>
/// Щит, висунутий з наручника. Не знімається вручну — лише через браслет.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class YautjaBracerShieldComponent : Component
{
    /// <summary>Runtime link to the owning bracer. Nullable so prototype save tests do not serialize invalid Uids.</summary>
    [DataField]
    public EntityUid? Bracer;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class YautjaBracerCloakTrackerComponent : Component
{
    /// <summary>Runtime link to the owning bracer. Nullable so prototype save tests do not serialize invalid Uids.</summary>
    [DataField]
    public EntityUid? Bracer;
}

/// <summary>
/// Плащ-пакунок Яутжа. Потрібен у слоті рюкзака для невидимості з наручника.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class YautjaCloakPackComponent : Component;

public sealed partial class ToggleYautjaClawsEvent : InstantActionEvent;

public sealed partial class ToggleYautjaShieldEvent : InstantActionEvent;

public sealed partial class ToggleYautjaCloakEvent : InstantActionEvent;

public sealed partial class YautjaBracerSelfDestructEvent : InstantActionEvent;
