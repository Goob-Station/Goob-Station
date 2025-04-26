// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Clothing.ClothingAutoInjector;

[RegisterComponent]
public sealed partial class AutoInjectOnStateChangeComponent : Component
{
    /// <summary>
    /// The autoinjector this component is linked to.
    /// </summary>
    public EntityUid? ClothingAutoInjector;
}
