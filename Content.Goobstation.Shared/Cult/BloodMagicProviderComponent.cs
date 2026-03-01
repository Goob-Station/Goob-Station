using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cult.Magic;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodMagicProviderComponent : Component
{
    [DataField] public EntProtoId SpellsProviderActionId = "ActionCultPrepareBloodMagic";

    /// <summary>
    ///     If the magic has been enhanced in any way. This is likely due to an empowering rune.
    /// </summary>
    [DataField] public bool Enhanced = false;

    [DataField] public int MaxSpells = 1;

    [DataField] public int MaxEnhancedSpells = 5;

    // either this can be defined somewhere in yaml
    // or it's staying this way and i pray that all entprotoids get past linter
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<BloodCultTier, List<EntProtoId>> Spells = new()
    {
        {
            BloodCultTier.None,
            new List<EntProtoId>()
            {
                "ActionCultTouchSpellStun",
                "ActionCultTouchSpellTeleport",
                "ActionCultEmp",
                "ActionCultTouchSpellShackles"
            }
        },
        {
            BloodCultTier.Eyes,
            new List<EntProtoId>()
            {
                "ActionCultTouchSpellConstruction",
                "ActionCultTouchSpellEquipment",
                "ActionCultDagger",
            }
        },
        {
            BloodCultTier.Halos,
            new List<EntProtoId>()
            {
                // just the blood rites because it's usually lategame and cool
                "ActionCultTouchSpellManipulation"
            }
        },
    };

    /// <summary>
    ///     Automatically gets added and removed on leader gain / loss.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<EntProtoId> LeaderSpells = new()
    {
        // todo
    };
}
