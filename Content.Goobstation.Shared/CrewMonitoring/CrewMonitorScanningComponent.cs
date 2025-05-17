// SPDX-FileCopyrightText: 2025 Baptr0b0t <152836416+Baptr0b0t@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.CrewMonitoring;

[RegisterComponent]
public sealed partial class CrewMonitorScanningComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> ScannedEntities = [];

    [DataField]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(8);

    [DataField]
    public bool ApplyDeathrattle = true;

    [DataField]
    public EntityWhitelist Whitelist = new ();
}
