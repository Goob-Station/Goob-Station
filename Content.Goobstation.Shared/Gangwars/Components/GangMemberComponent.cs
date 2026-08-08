using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Added to any player entity that belongs to a gang.
/// Tracks the gang colour and whether this member is currently standing in their gang's territory.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GangMemberComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "GangMember";

    [DataField, AutoNetworkedField]
    public Color? Gang;

    [DataField, AutoNetworkedField]
    public string? GangName;

    [DataField, AutoNetworkedField]
    public bool IsInTerritory;

    [DataField, AutoNetworkedField]
    public bool WearingGangClothes;


    /// <summary>
    /// Max range in tiles to check for a gang territory.
    /// Should be the largest territory radius in use (We reallllyyyy need zones).
    /// </summary>
    [DataField]
    public int MaxTerritoryRangeCheck = 10;

    /// <summary>
    /// How often the territory range check runs.
    /// </summary>
    [DataField]
    public float CheckInterval = 2f;

    [DataField, AutoNetworkedField]
    public TimeSpan NextCheck;

    [DataField, AutoNetworkedField]
    public int GangPoints = 1500;

    [DataField, AutoNetworkedField]
    public ProtoId<AlertPrototype> TerritoryAlert = "GangBonus";

    [DataField]
    public EntProtoId ToggleOverlayAction = "ActionGangMemberToggleOverlay";

    [DataField]
    public EntityUid? ToggleOverlayActionEnt;

    [DataField, AutoNetworkedField]
    public bool OverlayVisible = true;

    #region Territory Buffs / Healing

    // All of them require them to have all their gang clothes

    /// <summary>
    /// Territory buff, 0.8 = 20% less stamina damage taken
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StaminaBuff = 0.8f;

    /// <summary>
    /// Territory buff, 0.75 = 25% more defense
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DefenseBuff = 0.75f;

    /// <summary>
    /// Passive healing per tick
    /// </summary>
    [DataField]
    public DamageSpecifier HealAmount = new()
    {
        DamageDict = new()
        {
            { "Blunt", -0.4 },
            { "Slash", -0.4 },
            { "Piercing", -0.4 },
            { "Heat", -0.4 },
        }
    };

    /// <summary>
    /// Healing per tick applied when the BuffedHealing is active (near their locker).
    /// </summary>
    [DataField]
    public DamageSpecifier BuffedHealAmount = new()
    {
        DamageDict = new()
        {
            { "Blunt", -1 },
            { "Slash", -1 },
            { "Piercing", -1 },
            { "Heat", -1 },
            { "Poison", -1 },
        }
    };

    /// <summary>
    /// Probability per tick of reducing bleed when in territory and wearing gang clothes.
    /// 0.2 = 20%
    /// </summary>
    [DataField]
    public float BleedReductionChance = 0.2f;

    /// <summary>
    /// How much bleed to reduce when the bleed reduction proc fires.
    /// </summary>
    [DataField]
    public float BleedReductionAmount = -1f;

    #endregion
}
