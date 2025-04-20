// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;

namespace Content.Goobstation.Server.Insanity;

[RegisterComponent]
public sealed partial class InsanityComponent : Component
{
    [ViewVariables]
    public TimeSpan NextInsanityTick = TimeSpan.Zero;

    [ViewVariables]
    public TimeSpan ExecutionInterval = TimeSpan.FromSeconds(15);

    [ViewVariables]
    public bool IsBlinded;

    [ViewVariables]
    public bool IsMuted;
}
