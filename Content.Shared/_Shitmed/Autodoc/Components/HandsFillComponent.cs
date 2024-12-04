using Content.Shared._Shitmed.Autodoc.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Autodoc.Components;

/// <summary>
/// Creates a list of hands and spawns items to fill them.
/// </summary>
[RegisterComponent, Access(typeof(HandsFillSystem))]
public sealed partial class HandsFillComponent : Component
{
    /// <summary>
    /// The name of each hand and the item to fill it with, if any.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, EntProtoId?> Hands = new();
}
