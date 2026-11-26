// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Botany;

/// <summary>
/// On a barrel that is sealed with crops in it and room left, so it's fermenting.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class ActiveFermentingBarrelComponent : Component
{
    /// <summary>
    /// When the next crop ferments.
    /// </summary>
    [ViewVariables, AutoPausedField]
    public TimeSpan NextFerment;
}
