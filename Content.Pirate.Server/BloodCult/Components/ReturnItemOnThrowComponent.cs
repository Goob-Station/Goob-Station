// SPDX-FileCopyrightText: 2024 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Server.BloodCult.Components;

[RegisterComponent]
public sealed partial class ReturnItemOnThrowComponent : Component
{
    [DataField]
    public float ReturnSpeed = 15f;

    [DataField]
    public EntityWhitelist? ThrowerWhitelist;

    [DataField]
    public EntityWhitelist? TargetWhitelist;

    [DataField]
    public EntityWhitelist? TargetBlacklist;

    [ViewVariables]
    public EntityUid? ReturningTo;
}
