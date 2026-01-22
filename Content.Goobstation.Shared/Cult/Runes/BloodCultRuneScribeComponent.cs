using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cult.Runes;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultRuneScribeComponent : Component
{
    /// <summary>
    ///     What runes can this runescribe draw.
    ///     This is defined in a prototype.
    /// </summary>
    [DataField(required: true)] public List<EntProtoId> Runes;

    [DataField] public EntProtoId? MalfRune;

    [DataField] public DamageSpecifier DamagePerScribe = new();
}
