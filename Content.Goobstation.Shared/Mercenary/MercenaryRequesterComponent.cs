// SPDX-FileCopyrightText: 2025 IrisTheAmped <iristheamped@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Content.Shared.Roles;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Mercenary;

[RegisterComponent, NetworkedComponent]
public sealed partial class MercenaryRequesterComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Requester;
    [DataField]
    public bool BriefingSent = false;
    [DataField]
    public SoundSpecifier MindcontrolStartSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/mindcontrol_start.ogg");
}

[RegisterComponent]
public sealed partial class MercenaryRoleComponent : BaseMindRoleComponent
{
    [DataField("requester")]
    public EntityUid? Requester;
}
