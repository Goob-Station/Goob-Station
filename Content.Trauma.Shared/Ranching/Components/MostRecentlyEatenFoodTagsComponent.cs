// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// Stores the most recently eaten food by an entity
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MostRecentlyEatenFoodTagsComponent : Component
{
    [DataField]
    public HashSet<ProtoId<TagPrototype>> Tag = new ();
}
