using Content.Shared.DoAfter;
using Content.Shared.Psionics;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Spells;

[RegisterComponent]
public sealed partial class BloodCultSpellsHolderComponent : Component
{
    [DataField]
    public int DefaultMaxSpells = 1;

    [DataField]
    public TimeSpan SpellCreationTime = TimeSpan.FromSeconds(2);

    [DataField]
    public ProtoId<PsionicPowerPoolPrototype> PowersPoolPrototype = "BloodCultPowers";

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> SelectedSpells = new();

    public int MaxSpells;

    public DoAfterId? DoAfterId;

    /// <summary>
    ///     Since radial selector menu doesn't have metadata, we use this to toggle between remove and
    ///     add spells modes.
    /// </summary>
    public bool AddSpellsMode = true;
}
