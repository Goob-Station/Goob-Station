// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Server._Goobstation.Wizard.Components;

/// <summary>
/// This component is needed for accessing scale from server side. Required for HulkSystem
/// </summary>
[RegisterComponent]
public sealed partial class ScaleDataComponent : Component
{
    [DataField]
    public Vector2 Scale = Vector2.One;
}