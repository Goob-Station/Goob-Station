using Content.Goobstation.Shared.Shizophrenia;
using Content.Shared.Destructible.Thresholds;

namespace Content.Goobstation.Server.Shizophrenia;

[DataDefinition]
public sealed partial class AddAppearanceHallucinationsEvent : EntityEventArgs
{
    [DataField]
    public string Id = "";

    [DataField]
    public MinMax Delay = new();

    [DataField]
    public float Duration = -1f;

    [DataField]
    public List<HallucinationAppearanceData> Appearances = new();
}
