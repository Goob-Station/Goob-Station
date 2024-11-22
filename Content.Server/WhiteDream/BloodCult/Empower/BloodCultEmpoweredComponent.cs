using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Empower;

[RegisterComponent]
public sealed partial class BloodCultEmpoweredComponent : Component
{
    /// <summary>
    ///     Changes the damage from drawing/using runes.
    /// </summary>
    [DataField]
    public float RuneDamageMultiplier = 0.5f;

    /// <summary>
    ///     Changes the drawing time of runes.
    /// </summary>
    [DataField]
    public float RuneTimeMultiplier = 0.5f;

    /// <summary>
    ///     Increases the amount of spells cultists can create at once.
    /// </summary>
    [DataField]
    public int ExtraSpells = 3;

    /// <summary>
    ///     The default duration of the empowering.
    /// </summary>
    [DataField]
    public TimeSpan DefaultTime = TimeSpan.FromSeconds(20);

    [DataField]
    public float NearbyCultTileRadius = 1f;

    [DataField]
    public string CultTile = "CultFloor";

    [DataField]
    public ProtoId<AlertPrototype> EmpoweredAlert = "CultEmpowered";

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan TimeRemaining = TimeSpan.Zero;
}
