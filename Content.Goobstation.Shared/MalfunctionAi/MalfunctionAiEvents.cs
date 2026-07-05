using Content.Shared.Actions;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Opens the Malfunction AI ability store.
/// </summary>
public sealed partial class MalfOpenStoreEvent : InstantActionEvent;

/// <summary>
/// Targets a location; the machine there is overloaded into a small explosion.
/// World-targeted so the remote station AI can use it (entity-target actions fail their access check).
/// </summary>
public sealed partial class MalfOverloadMachineEvent : WorldTargetActionEvent;

/// <summary>
/// Causes a station-wide blackout: shuts off APC breakers across the station for a short time.
/// </summary>
public sealed partial class MalfBlackoutEvent : InstantActionEvent;

/// <summary>
/// Bolts and electrifies every airlock on the AI's grid for a short time.
/// </summary>
public sealed partial class MalfLockdownEvent : InstantActionEvent;

/// <summary>
/// Targets a location; a cyborg there is subverted to the Malfunction AI's side.
/// World-targeted so the remote station AI can use it (entity-target actions fail their access check).
/// </summary>
public sealed partial class MalfHackCyborgEvent : WorldTargetActionEvent;

/// <summary>
/// Arms the Doomsday device. Handled by the Malfunction AI rule.
/// </summary>
public sealed partial class MalfDoomsdayEvent : InstantActionEvent;

/// <summary>
/// Rigs every RCD on the AI's grid to beep and explode after a short delay.
/// </summary>
public sealed partial class MalfDetonateRcdsEvent : InstantActionEvent;

/// <summary>
/// Targets a location; the AI core is moved one tile in that direction.
/// World-targeted so the remote station AI can use it.
/// </summary>
public sealed partial class MalfGyroscopeEvent : WorldTargetActionEvent;

/// <summary>
/// Raised on the AI when the "Decrypt Syndicate Keys" store listing is bought
/// (via the listing's productEvent); grants access to the Syndicate radio channel.
/// </summary>
[Serializable, DataDefinition]
public sealed partial class MalfDecryptSyndicateKeysEvent : EntityEventArgs;

/// <summary>
/// Makes every air vent on the station spew plasma for a short time.
/// </summary>
public sealed partial class MalfPlasmaFloodEvent : InstantActionEvent;

/// <summary>
/// Targets a location; the AI's consciousness is shunted into the hacked APC there.
/// </summary>
public sealed partial class MalfShuntToApcEvent : WorldTargetActionEvent;

/// <summary>
/// Returns a shunted AI consciousness from an APC back into its core.
/// </summary>
public sealed partial class MalfReturnToCoreEvent : InstantActionEvent;

/// <summary>
/// Drains the batteries of every emergency light on the station.
/// </summary>
public sealed partial class MalfDisableEmergencyLightsEvent : InstantActionEvent;

/// <summary>
/// Opens a dialog letting the AI set a fake voice name (or reset it with an empty input).
/// </summary>
public sealed partial class MalfVoiceModulatorEvent : InstantActionEvent;

/// <summary>
/// Raised on the AI when the "Camera Microphones" store listing is bought
/// (via the listing's productEvent); lets the AI hear speech near cameras its eye is watching.
/// </summary>
[Serializable, DataDefinition]
public sealed partial class MalfCameraMicsEvent : EntityEventArgs;

/// <summary>
/// Raised on the AI when the "Upgrade Camera Network" store listing is bought
/// (via the listing's productEvent); cameras get X-ray vision and a bigger radius.
/// </summary>
[Serializable, DataDefinition]
public sealed partial class MalfCameraUpgradeEvent : EntityEventArgs;

/// <summary>
/// Opens the window listing the AI's subverted cyborgs.
/// </summary>
public sealed partial class MalfOpenBorgsUiEvent : InstantActionEvent;

/// <summary>
/// Raised on the AI when the "AI Turret Upgrade" store listing is bought
/// (via the listing's productEvent); improves the power and durability of all AI turrets.
/// </summary>
[Serializable, DataDefinition]
public sealed partial class MalfTurretUpgradeEvent : EntityEventArgs;
