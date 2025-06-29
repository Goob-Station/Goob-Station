using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConsciousnessRequiredComponent : Component
{
    /// <summary>
    /// Identifier, basically. Must be unique.
    /// </summary>
    [AutoNetworkedField, DataField]
    public string Identifier = "requiredConsciousnessPart";

    /// <summary>
    /// Not having this part means death, or only unconsciousness.
    /// </summary>
    [AutoNetworkedField, DataField]
    public bool CausesDeath = true;
}
