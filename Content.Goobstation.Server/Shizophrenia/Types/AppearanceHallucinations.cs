using Content.Goobstation.Shared.Shizophrenia;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shizophrenia;

public sealed partial class AppearanceHallucinations : BaseHallucinationsType
{
    [DataField]
    public List<HallucinationAppearanceData> Appearances = new();

    public override BaseHallucinationsEntry GetEntry()
    {
        return new AppearanceHallucinationsEntry()
        {
            Appearances = Appearances,
            Delay = Delay,
        };
    }
}
