// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CloneProjector;

namespace Content.Goobstation.Server.CloneProjector;

[RegisterComponent]
public sealed partial class WearingCloneProjectorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<CloneProjectorComponent>? ConnectedProjector;
}
