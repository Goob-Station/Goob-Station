// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Megafauna.NumberSelectors;
using Content.Shared._Lavaland.Megafauna.Selectors;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaAiComponent : Component
{
    /// <summary>
    /// Selector that is added to the main thread
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public MegafaunaSelector Selector;

    /// <summary>
    /// Delay between picking new action selectors.
    /// Added to the delay that Selector returned after invocation.
    /// It's recommended to be always bigger than 0 to prevent errors.
    /// </summary>
    [DataField("actionDelay"), ViewVariables(VVAccess.ReadOnly)]
    public MegafaunaNumberSelector ActionDelaySelector = new MegafaunaConstantNumberSelector(0.5f);

    /// <summary>
    /// True if this megafauna can execute any attacks now.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool Active;

    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<TimeSpan, MegafaunaSelector> Schedule = new();

    /// <summary>
    /// Defines delay for the first megafauna's attack.
    /// </summary>
    [DataField]
    public float StartingDelay = 0.5f;
}
