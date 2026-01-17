using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cult.Magic;

[RegisterComponent]
public sealed partial class BloodMagicProviderComponent : Component
{
    [DataField] public EntProtoId SpellsProviderActionId = "ActionCultPrepareBloodMagic";

    /// <summary>
    ///     If the magic has been enhanced in any way. This is likely due to an empowering rune.
    /// </summary>
    [DataField] public bool Enhanced = false;

    [DataField] public int MaxSpells = 1;

    [DataField] public int MaxEnhancedSpells = 5;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<EntProtoId> Spells = new()
    {
        "ActionCultTouchSpellStun",
        "ActionCultTouchSpellTeleport",
        "ActionCultTouchSpellConstruction",
        "ActionCultTouchSpellEquipment",
        "ActionCultDagger",
        "ActionCultTouchSpellManipulation",
        // todo add more here
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
