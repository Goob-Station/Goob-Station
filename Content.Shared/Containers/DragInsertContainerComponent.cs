// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Booblesnoot42 <108703193+Booblesnoot42@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Containers;

/// <summary>
/// This is used for a container that can have entities inserted into it via a
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(DragInsertContainerSystem))]
public sealed partial class DragInsertContainerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string ContainerId;

    /// <summary>
    /// If true, there will also be verbs for inserting / removing objects from this container.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool UseVerbs = true;

    /// <summary>
    /// The delay in seconds before a drag will be completed.
    /// </summary>
    [DataField]
    public TimeSpan EntryDelay = TimeSpan.Zero;

    /// <summary>
    /// If entry delay isn't zero, this sets whether an entity dragging itself into the container should be delayed.
    /// </summary>
    [DataField]
    public bool DelaySelfEntry = false;

}

[Serializable, NetSerializable, ByRefEvent]
public sealed partial class InsertOnDragDoAfterEvent : SimpleDoAfterEvent
{
}