// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Common.MartialArts;
using Robust.Shared.GameStates;

namespace Content.Goidastation.Server.Grab;

[RegisterComponent]
public sealed partial class GrabbingItemComponent : Component
{
    [DataField]
    public GrabStage GrabStageOverride = GrabStage.Hard;

    [DataField]
    public float EscapeAttemptModifier = 2f;
}