// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Goobstation.Shared.TableSlam;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PostTabledComponent : Component
{
    [DataField]
    public TimeSpan PostTabledShovableTime = TimeSpan.Zero;

    [DataField]
    public float ParalyzeChance = 0.35f;
}