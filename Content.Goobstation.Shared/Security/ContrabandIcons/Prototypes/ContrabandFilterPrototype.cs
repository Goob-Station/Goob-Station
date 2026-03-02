using Content.Shared.Contraband;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Goobstation.Shared.Security.ContrabandIcons.Prototypes;

[Prototype("contrabandFilter")]
public sealed class ContrabandFilterPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<ContrabandFilterPrototype>))]
    public string[]? Parents { get; }

    /// <inheritdoc/>
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }
    
    [DataField]
    public List<ProtoId<ContrabandSeverityPrototype>> WhitelistedSeverity { get; } = new();
    
    [DataField]
    public List<ProtoId<ContrabandSeverityPrototype>> RequiresPermitSeverity { get; } = new();
    
    [DataField]
    public List<ProtoId<ContrabandSeverityPrototype>> BlacklistedSeverity { get; } = new();
}
