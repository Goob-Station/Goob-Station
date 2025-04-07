// SPDX-FileCopyrightText: 2025 amogus <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Common.Pirates;

/// <summary>
///     Given to an entity that cargo buys as a ransom.
///     On spawn ends all pending pirate game rules.
/// </summary>
[RegisterComponent]
public sealed partial class RansomComponent : Component
{

}