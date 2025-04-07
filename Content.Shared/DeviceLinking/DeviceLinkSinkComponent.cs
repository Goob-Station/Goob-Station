// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeviceLinking;

[RegisterComponent]
[NetworkedComponent] // for interactions. Actual state isn't currently synced.
[Access(typeof(SharedDeviceLinkSystem))]
public sealed partial class DeviceLinkSinkComponent : Component
{
    /// <summary>
    /// The ports this sink has
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SinkPortPrototype>> Ports = new();

    /// <summary>
    /// Used for removing a sink from all linked sources when this component gets removed.
    /// This is not serialized to yaml as it can be inferred from source components.
    /// </summary>
    [ViewVariables]
    public HashSet<EntityUid> LinkedSources = new();

    /// <summary>
    /// Counts the amount of times a sink has been invoked for severing the link if this counter gets to high
    /// The counter is counted down by one every tick if it's higher than 0
    /// This is for preventing infinite loops
    /// </summary>
    [DataField]
    public int InvokeCounter;

    /// <summary>
    /// How high the invoke counter is allowed to get before the links to the sink are removed and the DeviceLinkOverloadedEvent gets raised
    /// If the invoke limit is smaller than 1 the sink can't overload
    /// </summary>
    [DataField]
    public int InvokeLimit = 10;
}