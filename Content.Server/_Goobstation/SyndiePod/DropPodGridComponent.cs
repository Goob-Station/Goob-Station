// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Server._Goobstation.DropPod;

[RegisterComponent]
public sealed partial class DropPodGridComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityCoordinates? TargetCoords;

    [DataField]
    public SoundSpecifier? ArriveSound;
}
