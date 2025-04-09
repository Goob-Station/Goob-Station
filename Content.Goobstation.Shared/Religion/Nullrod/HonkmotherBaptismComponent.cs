// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Goobstation.Shared.Bible;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Overlays;

[RegisterComponent, NetworkedComponent]
public sealed partial class HonkmotherBaptismComponent : Component
{
    [DataField]
    public EntProtoId BananaTouchAction = "ActionChaplainBananaTouch";

    [DataField]
    public EntityUid? BananaTouchActionEntity;


}
