using Content.Shared.Silicons.Laws;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.CustomLawboard;


[RegisterComponent, NetworkedComponent]
public sealed partial class CustomLawboardComponent : Component
{
    /// <summary>
    /// The laws of this custom lawboard.
    /// </summary>
    public List<SiliconLaw> Laws = new();
}
