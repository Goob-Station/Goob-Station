using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Cult.Magic;

[RegisterComponent]
public sealed partial class BloodMagicProviderComponent : Component
{
    [DataField] public int MaxSpells = 1;
    [DataField] public int MaxEnhancedSpells = 5;

    [DataField, NonSerialized, ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> Spells = new();

    /// <summary>
    ///     If the magic has been enhanced in any way. This is likely due to an empowering rune.
    /// </summary>
    [DataField] public bool Enhanced = false;

    [DataField] public EntProtoId SpellsProviderActionId = "ActionCultPrepareBloodMagic";
}
