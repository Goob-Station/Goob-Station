// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Network;

namespace Content.Shared.Administration.Logs;

/// <summary>
/// Interface implemented by admin log values that contain player references.
/// </summary>
public interface IAdminLogsPlayerValue
{
    IEnumerable<NetUserId> Players { get; }
}
