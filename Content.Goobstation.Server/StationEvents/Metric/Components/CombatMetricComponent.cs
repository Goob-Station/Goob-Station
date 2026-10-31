// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(CombatMetricSystem))]
public sealed partial class CombatMetricComponent : Component
{
    [DataField]
    public double HostileScore = 10.0f;

    [DataField]
    public double FriendlyScore = 10.0f;

    [DataField]
    public double HostileNpcScore = 3.0f;

    [DataField]
    public HashSet<ProtoId<NpcFactionPrototype>> IgnoredHostileFactions = new()
    {
        "Mouse",
        "SimpleNeutral",
    };

    /// <summary>
    ///   Cost per point of medical damage for friendly entities
    /// </summary>
    [DataField]
    public double MedicalMultiplier = 0.05f;

    /// <summary>
    ///   Cost for friendlies who are in crit
    /// </summary>
    [DataField]
    public double CritScore = 10.0f;

    /// <summary>
    ///   Cost for friendlies who are dead
    /// </summary>
    [DataField]
    public double DeadScore = 20.0f;

    [DataField]
    public double MaxItemThreat = 15.0f;

    [DataField]
    public ProtoId<DepartmentPrototype> SecurityDepartment = "Security";

    [DataField]
    public TimeSpan DeathFadeTime = TimeSpan.FromMinutes(30);

    [DataField]
    public double MinDeathFactor = 0.25;

    [ViewVariables]
    public Dictionary<EntityUid, TimeSpan> DeathTimes = new();

    /// <summary>
    ///   ItemThreat - evaluate based on item tags how powerful a player is
    /// </summary>
    [DataField]
    public Dictionary<string, double> ItemThreat =
        new()
        {
            // TODO: PROTOTYPE
            { "Taser", 2.0f },
            { "Sidearm", 2.0f },
            { "Rifle", 5.0f },
            { "HighRiskItem", 2.0f },
            { "CombatKnife", 1.0f },
            { "Knife", 1.0f },
            { "Grenade", 2.0f },
            { "Bomb", 2.0f },
            { "MagazinePistol", 0.5f },
            { "Hacking", 1.0f },
            { "Jetpack", 1.0f },
        };
}
