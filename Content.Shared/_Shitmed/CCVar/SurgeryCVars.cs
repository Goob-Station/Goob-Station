using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._Shitmed.CCVar;

public sealed class SurgeryCVars : CVars
{
    #region Medical CVars

    /// <summary>
    /// Whether or not players can operate on themselves.
    /// </summary>
    public static readonly CVarDef<bool> CanOperateOnSelf =
        CVarDef.Create("surgery.can_operate_on_self", true, CVar.SERVERONLY);

    /// <summary>
    /// How many times per second do we want to heal wounds.
    /// </summary>
    public static readonly CVarDef<float> MedicalHealingTickrate =
        CVarDef.Create("medical.heal_tickrate", 2f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// The name is self-explanatory
    /// </summary>
    public static readonly CVarDef<float> MaxWoundSeverity =
        CVarDef.Create("wounding.max_wound_severity", 200f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// The same as above
    /// </summary>
    public static readonly CVarDef<float> WoundScarChance =
        CVarDef.Create("wounding.wound_scar_chance", 0.10f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// What part of wounds will be transferred from a destroyed woundable to its parent?
    /// </summary>
    public static readonly CVarDef<float> WoundTransferPart =
        CVarDef.Create("wounding.wound_severity_transfer", 0.10f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// for every n units of distance, (tiles), chance for dodging is equal to n*x percents, look for it down here
    /// </summary>
    public static readonly CVarDef<float> DodgeDistanceChance =
        CVarDef.Create("targeting.dodge_chance_distance", 4f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// the said x amount of percents
    /// </summary>
    public static readonly CVarDef<float> DodgeDistanceChange =
        CVarDef.Create("targeting.dodge_change_distance", 0.05f, CVar.SERVER | CVar.REPLICATED);

    #endregion

    #region Trauma CVars

    /// <summary>
    /// The multiplier applied to the base paralyze time upon an infliction of organ trauma.
    /// </summary>
    public static readonly CVarDef<float> OrganTraumaSlowdownTimeMultiplier =
        CVarDef.Create("traumas.organ_slowdown_time", 2f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// The slowdown applied to the walk speed upon an infliction of organ trauma
    /// </summary>
    public static readonly CVarDef<float> OrganTraumaWalkSpeedSlowdown =
        CVarDef.Create("traumas.organ_walk_speed_slowdown", 0.6f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// The slowdown applied to the run speed upon an infliction of organ trauma
    /// </summary>
    public static readonly CVarDef<float> OrganTraumaRunSpeedSlowdown =
        CVarDef.Create("traumas.organ_run_speed_slowdown", 0.6f, CVar.SERVER | CVar.REPLICATED);

    #endregion

    #region Bleeding CVars

    /// <summary>
    /// The rate at which severity (wound) points get exchanged into bleeding; e.g., 50 severity would be 3.5 bleeding points.
    /// </summary>
    public static readonly CVarDef<float> BleedingSeverityTrade =
        CVarDef.Create("bleeds.wound_severity_trade", 0.07f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// How quick by default do bleeds grow to their full form?
    /// </summary>
    public static readonly CVarDef<float> BleedsScalingTime =
        CVarDef.Create("bleeds.bleeding_scaling_time", 60f, CVar.SERVER | CVar.REPLICATED);

    #endregion

    #region Pain CVars

    public static readonly CVarDef<bool> PainScreams =
        CVarDef.Create("pain.screams", true, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float> PainScreamChance =
        CVarDef.Create("pain.scream_chance", 0.20f, CVar.SERVER | CVar.REPLICATED);

    #endregion
}
