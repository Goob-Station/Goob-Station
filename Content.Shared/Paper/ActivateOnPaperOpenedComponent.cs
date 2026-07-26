// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Paper;

/// <summary>
/// Activates the item when used to write on paper, as if Z was pressed.
/// </summary>
[RegisterComponent]
[Access(typeof(PaperSystem))]
public sealed partial class ActivateOnPaperOpenedComponent : Component
{
}