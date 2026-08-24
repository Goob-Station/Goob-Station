// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Aggression;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AggressorComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Aggressives = new();
}
