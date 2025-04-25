// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Dash;

[RegisterComponent]
public sealed partial class DashActionComponent : Component
{
    [DataField]
    public string? ActionProto;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionUid;
}

public sealed partial class DashActionEvent : WorldTargetActionEvent
{
    [DataField]
    public float Force = 2065;

    [DataField]
    public bool NeedsGravity = true; // I highly recommend not to change this

    [DataField]
    public bool MultiplyByMovementSpeed = true;
}
