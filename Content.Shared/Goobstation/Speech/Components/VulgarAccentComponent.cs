// CREATED BY Goldminermac
// https://github.com/space-wizards/space-station-14/pull/31149
// LICENSED UNDER THE MIT LICENSE
// SEE README.MD AND LICENSE.TXT IN THE ROOT OF THIS REPOSITORY FOR MORE INFORMATION
using Robust.Shared.GameStates;
using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Shared.Speech.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VulgarAccentComponent : Component
{
    [DataField]
    public ProtoId<LocalizedDatasetPrototype> Pack = "SwearWords";

    [DataField]
    public float SwearProb = 0.5f;
}
