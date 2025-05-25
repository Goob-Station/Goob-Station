// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Storage.Visualizers;

[RegisterComponent]
[Access(typeof(EntityStorageVisualizerSystem))]
public sealed partial class EntityStorageVisualsComponent : Component
{
    /// <summary>
    /// The RSI state used for the base layer of the storage entity sprite while the storage is closed.
    /// </summary>
    [DataField("stateBaseClosed")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateBaseClosed;

    /// <summary>
    /// The RSI state used for the base layer of the storage entity sprite while the storage is open.
    /// </summary>
    [DataField("stateBaseOpen")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateBaseOpen;

    /// <summary>
    /// The RSI state used for the door/lid while the storage is open.
    /// </summary>
    [DataField("stateDoorOpen")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateDoorOpen;

    /// <summary>
    /// The RSI state used for the door/lid while the storage is closed.
    /// </summary>
    [DataField("stateDoorClosed")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateDoorClosed;

    /// <summary>
    /// The drawdepth the object has when it's open
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int? OpenDrawDepth;

    /// <summary>
    /// The drawdepth the object has when it's closed
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int? ClosedDrawDepth;
}