using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
public sealed partial class RallyComponent : Component
{
    /// <summary>
    /// Range at which rally can affect entities.
    /// </summary>
    [DataField]
    public float RallyRange = 10f;

    [DataField]
    public EntityWhitelist? Whitelist = new();

    [ViewVariables]
    public EntProtoId StatusEffectRally = "StatusEffectRally";

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(25);
}
