// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared._Trauma.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FavoriteFoodComponent : Component
{
    /// <summary>
    /// The tags that the favorite food has, when the entity eats anything with these tags it will gain <see cref="Amount"/> in happiness
    /// </summary>
    [DataField]
    public HashSet<ProtoId<TagPrototype>> Tag = new ();

    /// <summary>
    /// Happiness to increase by when favorite food eaten
    /// </summary>
    [DataField]
    public int Amount = 30;
}
