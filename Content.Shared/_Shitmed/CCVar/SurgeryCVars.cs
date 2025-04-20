using Content.Shared.FixedPoint;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._Shitmed.CCVar;

public sealed class SurgeryCVars : CVars
{
    /*
     * Medical CVars
     */

    /// <summary>
    /// Whether or not players can operate on themselves.
    /// </summary>
    public static readonly CVarDef<bool> CanOperateOnSelf =
        CVarDef.Create("surgery.can_operate_on_self", true, CVar.SERVERONLY);

    /// <summary>
    /// How many times per second do we want to heal wounds.
    /// </summary>
    public static readonly CVarDef<float> MedicalHealingTickrate =
        CVarDef.Create("medical.heal_tickrate", 0.5f, CVar.SERVER | CVar.REPLICATED);

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
    /// for every x units of distance, (tiles), chance for dodging is increased by x percents, look for it down here
    /// </summary>
    public static readonly CVarDef<float> DodgeDistanceChance =
        CVarDef.Create("targeting.dodge_chance_distance", 4f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// for every x units of distance, (tiles), chance for dodging is increased by x percents, look for it down here
    /// </summary>
    public static readonly CVarDef<float> DodgeDistanceChange =
        CVarDef.Create("targeting.dodge_change_distance", 0.05f, CVar.SERVER | CVar.REPLICATED);
}
