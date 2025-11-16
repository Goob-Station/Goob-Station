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

    /// <summary>
    /// Which entities are allowed to get rallied
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = new();

    [ViewVariables]
    public EntProtoId StatusEffectRally = "StatusEffectRally";

    /// <summary>
    /// How long the status effect lasts
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(25);
}
