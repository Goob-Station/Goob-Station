// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server.Shuttles.Components;

[RegisterComponent]
public sealed partial class DockingSignalControlComponent : Component
{
    /// <summary>
    /// Output port that is high while docked.
    /// </summary>
    [DataField]
    public ProtoId<SourcePortPrototype> DockStatusSignalPort = "DockStatus";
}