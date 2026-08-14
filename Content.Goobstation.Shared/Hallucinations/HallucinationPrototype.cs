using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hallucinations;

/// <summary>
/// Hallucinations used by the hallucination system.
/// </summary>
[Prototype]
public sealed partial class HallucinationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public HallucinationEffect Effect = default!;
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class HallucinationEffect;
