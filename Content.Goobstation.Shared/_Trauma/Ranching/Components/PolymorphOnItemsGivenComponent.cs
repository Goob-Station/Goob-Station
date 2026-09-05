// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared._Trauma.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PolymorphOnItemsGivenComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist;

    /// <summary>
    /// The entities to polymorph into
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId> ReplacementEntities;

    /// <summary>
    /// The amount of items required
    /// </summary>
    [DataField(required: true)]
    public int Amount;
}
