// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.CloneProjector.Clone;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HolographicCloneComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<CloneProjectorComponent>? HostProjector;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? HostEntity;

}
