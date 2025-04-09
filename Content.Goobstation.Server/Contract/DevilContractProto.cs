using System.ComponentModel.DataAnnotations;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Contract;

[Prototype("devilClause")]
public sealed class DevilClauseProto : IPrototype
{
    [IdDataField]
    public string ID { get; private init; } = default!;

    [DataField]
    public int ClauseWeight = 5;

    [DataField]
    public ComponentRegistry? AddedComponents { get; set; }

    [DataField]
    public ComponentRegistry? RemovedComponents { get; set; }

    [DataField]
    public string? DamageModifierSet { get; set; }

    [DataField]
    public List<string>? SpecialActions { get; set; }

}
