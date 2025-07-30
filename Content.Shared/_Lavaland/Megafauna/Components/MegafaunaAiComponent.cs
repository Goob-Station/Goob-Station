// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Megafauna.Actions;
using Content.Shared._Lavaland.Megafauna.NumberSelectors;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaAiComponent : Component
{
    public const int MaxThreads = 8;

    /// <summary>
    /// Selector that is added to the main thread
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public MegafaunaActionSelector Selector;

    /// <summary>
    /// Delay between picking new action selectors.
    /// Added to the delay that Selector returned after invocation.
    /// It's recommended to be always bigger than 0 to prevent errors.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public MegafaunaNumberSelector ActionDelaySelector = new MegafaunaConstantNumberSelector(0.5f);

    /// <summary>
    /// True if this megafauna can execute any attacks now.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool Active;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<MegafaunaActionThread> Threads = new(MaxThreads);

    // TODO MEGAFAUNA move targets somewhere else when more use cases will appear (I don't know what to do now)
    [ViewVariables, AutoNetworkedField]
    public EntityUid? CurrentTarget;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? PreviousTarget;

    /// <summary>
    /// When the boss doesn't die, but for any reason stops attacking,
    /// if this bool is true, will rejuvenate the megafauna.
    /// </summary>
    [DataField]
    public bool RejuvenateOnShutdown = true; // TODO MEGAFAUNA move this into another component

    /// <summary>
    /// Defines delay for the first megafauna's attack.
    /// </summary>
    [DataField]
    public float StartingDelay = 0.5f;
}

/// <summary>
/// Represents a thread that contains sequence of megafauna actions within some timeframe.
/// </summary>
public record struct MegafaunaActionThread(Dictionary<TimeSpan, MegafaunaActionSelector> Actions, bool IsMain);
