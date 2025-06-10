// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 youtissoum <51883137+youtissoum@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Harmony.BloodBrothers.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class BloodBrotherComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Brother;

    [DataField]
    public ProtoId<FactionIconPrototype> BloodBrotherIcon = "BloodBrotherFaction";

    public override bool SessionSpecific => true;
}
