using Content.Shared.ClothingAutoInjector;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Server.ClothingAutoInjector;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ClothingAutoInjectorComponent : Component
{
    /// <summary>
    /// Which prototype of chemicals to inject.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ClothingAutoInjectorConfigurationPrototype> Configuration;

}
