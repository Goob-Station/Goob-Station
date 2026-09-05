// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Goobstation.Server.Wizard.Components;

/// <summary>
/// This component is needed for accessing scale from server side. Required for HulkSystem
/// </summary>
[RegisterComponent]
public sealed partial class ScaleDataComponent : Component
{
    [DataField]
    public Vector2 Scale = Vector2.One;
}
