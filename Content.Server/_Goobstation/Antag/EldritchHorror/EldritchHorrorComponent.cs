// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 OnsenCapy <101037138+OnsenCapy@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Antag.EldritchHorror;
using Content.Shared._Goobstation.EldritchHorror.EldritchHorrorEvents;

[RegisterComponent]
public sealed partial class EldritchHorrorComponent : Component
{

    [ViewVariables]
    public EntityUid? SpawnProphetActionEntity;

    [DataField]
    public EntProtoId SpawnProphetAction = "ActionRiseProphet";

    [DataField]
    public EntProtoId ProphetProtoId = "MobGhoulProphet";

    [DataField]
    public int ProphetAmount = 2;
}
