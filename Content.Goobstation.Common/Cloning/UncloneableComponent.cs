// SPDX-License-Identifier: AGPL-3.0-or-l ater

namespace Content.Goobstation.Common.Cloning;

/// <summary>
/// Make entity not be able to be cloned in cloning pod.. obvious from the name
/// </summary>
[RegisterComponent]
public sealed partial class UncloneableComponent : Component;
