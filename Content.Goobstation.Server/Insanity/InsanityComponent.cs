// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;

namespace Content.Goobstation.Server.Insanity;

[RegisterComponent]
public sealed partial class InsanityComponent : Component
{
    [DataField]
    public TimeSpan NextInsanityTick = TimeSpan.Zero;

    [DataField]
    public TimeSpan ExecutionInterval = TimeSpan.FromSeconds(15);

    [DataField]
    public bool IsBlinded;

    [DataField]
    public bool IsMuted;
}
