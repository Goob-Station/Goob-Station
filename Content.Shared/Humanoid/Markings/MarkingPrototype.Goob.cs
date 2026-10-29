using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid.Markings;

public sealed partial class MarkingPrototype
{
    [DataField]
    public ProtoId<MarkingPrototype>? Wagging { get; private set; }
}
