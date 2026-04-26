using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

/// <summary>
/// Core component for the Bloodsucker.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodsuckerComponent : Component
{
    /// <summary>
    /// Blood drained per second just from existing.
    /// </summary>
    [DataField]
    public float PassiveBloodDrainPerSecond = 0.1f;

    [DataField]
    public float CoffinHealPerSecond = 1f;

    [DataField]
    public ProtoId<AlertPrototype> BloodAlert = "BloodsuckerBlood";

    [DataField]
    public ProtoId<AlertPrototype> RankAlert = "BloodsuckerRank";

    [DataField]
    public ProtoId<AlertPrototype> SolAlert = "BloodsuckerSol";

    /// <summary>
    /// How much blood the vampire currently has, used to update the UI element.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentBlood;

    /// <summary>
    /// Current vampire rank/level.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Rank = 1;

    /// <summary>
    /// Blood volume (units) at or below which frenzy triggers.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FrenzyThreshold = 150f;

    /// <summary>Blood volume (units) required to exit frenzy.</summary>
    [DataField]
    public float FrenzyExitThreshold = 300f;

    /// <summary>Multiplier applied to flash stun duration against this entity.</summary>
    [DataField]
    public float FlashDurationMultiplier = 3f;

    /// <summary>
    /// Entity UID of the coffin this bloodsucker has claimed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ClaimedCoffin;

    /// <summary>
    /// When true the entity passes medical scans as alive and loses
    /// vampire defensive buffs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsMasquerading;
}
