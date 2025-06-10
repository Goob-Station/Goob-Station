// SPDX-FileCopyrightText: 2025 IrisTheAmped <iristheamped@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Mercenary;

[RegisterComponent, NetworkedComponent]
public sealed partial class MercenaryRequesterComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Requester;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool BriefingSent;

    [DataField]
    public EntProtoId MindRole = "MindRoleMercenary";
}


