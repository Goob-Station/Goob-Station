// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.DoAfter;

/// <summary>
///     Added to entities that are currently performing any doafters.
/// </summary>
[RegisterComponent]
public sealed partial class ActiveDoAfterComponent : Component
{
}