using Content.Goobstation.Server.MalfunctionAi;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.MalfunctionAi;

/// <summary>
/// Game rule that turns the station's AI into a "Malfunction AI" antagonist.
/// The AI keeps its existing core entity but receives new laws, malf abilities and objectives.
/// </summary>
[RegisterComponent, Access(typeof(MalfunctionAiRuleSystem))]
public sealed partial class MalfunctionAiRuleComponent : Component
{
    /// <summary>
    /// Sound played to the AI player when they are made into a Malfunction AI.
    /// </summary>
    [DataField]
    public SoundSpecifier? GreetSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/malf.ogg");

    /// <summary>
    /// Announcement sound played station-wide when the Doomsday device is armed.
    /// </summary>
    [DataField]
    public SoundSpecifier DoomsdayArmedSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/aimalf.ogg");

    // --- Doomsday device ---

    /// <summary>
    /// AI entity that armed the Doomsday device; the explosion is centered here.
    /// </summary>
    [ViewVariables]
    public EntityUid? DoomsdayAi;

    /// <summary>
    /// True once the Doomsday countdown has started.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool DoomsdayArmed;

    /// <summary>
    /// True once the Doomsday explosion has actually fired (win condition).
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool DoomsdayDetonated;

    /// <summary>
    /// Seconds left on the Doomsday timer (TG uses 450s / 7.5 min).
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DoomsdayRemaining = 450f;

    /// <summary>
    /// Announcement thresholds (in seconds remaining) that have not yet been broadcast.
    /// </summary>
    [DataField]
    public List<int> DoomsdayAnnouncementsLeft = new() { 300, 180, 120, 60, 30, 10 };

    /// <summary>
    /// Alert level set on the station when the Doomsday device is armed.
    /// </summary>
    [DataField]
    public string DoomsdayAlertLevel = "delta";

    /// <summary>
    /// Core iconography forced on the AI when Doomsday is armed — the malfunctioning face.
    /// </summary>
    [DataField]
    public ProtoId<StationAiCustomizationPrototype> DoomsdayCoreIcon = "StationAiIconNotMalf";

    /// <summary>
    /// Total intensity for the Doomsday explosion.
    /// </summary>
    [DataField]
    public float DoomsdayExplosionIntensity = 150000f;

    /// <summary>
    /// Per-tile intensity cap for the Doomsday explosion.
    /// </summary>
    [DataField]
    public float DoomsdayMaxTileIntensity = 100f;

    /// <summary>
    /// Slope (falloff rate) of the Doomsday explosion.
    /// </summary>
    [DataField]
    public float DoomsdayExplosionSlope = 5f;

    /// <summary>
    /// Type of the Doomsday explosion.
    /// </summary>
    [DataField]
    public string DoomsdayExplosionType = "Default";
}
