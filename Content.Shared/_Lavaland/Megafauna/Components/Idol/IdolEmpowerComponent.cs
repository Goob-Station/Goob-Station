using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components.Idol;

/// <summary>
/// Searches for any target component in the radius and empowers them. Unlike Rally, this goes on the scream effect.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IdolEmpowerComponent : Component
{
    /// <summary>
    /// Range at which empower can affect entities.
    /// </summary>
    [DataField]
    public float EmpowerRange = 5f;

    /// <summary>
    /// Which entities are allowed to get empowered.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = new();

    [ViewVariables]
    public EntProtoId StatusEffectRally = "StatusEffectRally";

    /// <summary>
    /// How long the status effect lasts
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);
}
