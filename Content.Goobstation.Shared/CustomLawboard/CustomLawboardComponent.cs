using Content.Shared.Silicons.Laws;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.CustomLawboard;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CustomLawboardComponent : Component
{
    /// <summary>
    /// The laws of this custom lawboard.
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public List<SiliconLaw> Laws = new();
}
