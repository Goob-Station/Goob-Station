using Robust.Shared.Prototypes;

namespace Content.Shared.ClothingAutoInjector;

[RegisterComponent]
public sealed partial class EpinephrineAutoInjectComponent : Component
{

    public readonly List<EntProtoId> EpinephrineAutoInjector = new()
    {
        "ActionAutoInjectEpinephrine",
    };
}
