// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.CloneProjector.Clone;

[RegisterComponent]
public sealed partial class CloneComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<CloneProjectorComponent>? HostProjector;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? HostEntity;

}
