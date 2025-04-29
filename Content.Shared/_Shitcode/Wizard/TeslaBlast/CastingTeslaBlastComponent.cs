// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.TeslaBlast;

[RegisterComponent, NetworkedComponent]
public sealed partial class CastingTeslaBlastComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public ushort DoAfterId;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Effect;

    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<AudioComponent>? Sound;
}