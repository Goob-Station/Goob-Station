using System.ComponentModel.DataAnnotations;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Contract;

[Prototype("clause")]
public sealed partial class DevilContractProto : IPrototype
{
    [IdDataField]
    public string ID { get; private init; } = default!;

    [DataField]
    public LocId LocalizeClauseName { get; private init; } = default!;

    [DataField]
    public List<IComponent>? AddedComponents { get; set; }

    [DataField]
    public List<IComponent>? RemovedComponents { get; set; }

    [DataField]
    public DamageModifierSetPrototype? DamageModifierSet { get; set; }

}
