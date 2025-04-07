// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._White.Collision.Blur;

[RegisterComponent]
public sealed partial class BlurOnCollideComponent : Component
{
    [DataField]
    public TimeSpan BlurTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan BlindTime = TimeSpan.Zero;
}