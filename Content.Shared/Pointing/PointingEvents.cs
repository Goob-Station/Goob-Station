// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Pointing;

// TODO just make pointing properly predicted?
// So true
/// <summary>
///     Event raised when someone runs the client-side pointing verb.
/// </summary>
[Serializable, NetSerializable]
public sealed class PointingAttemptEvent : EntityEventArgs
{
    public NetEntity Target;

    public PointingAttemptEvent(NetEntity target)
    {
        Target = target;
    }
}

/// <summary>
/// Raised on the entity who is pointing after they point at something.
/// </summary>
/// <param name="Pointed"></param>
[ByRefEvent]
public readonly record struct AfterPointedAtEvent(EntityUid Pointed);

/// <summary>
/// Raised on an entity after they are pointed at by another entity.
/// </summary>
/// <param name="Pointer"></param>
[ByRefEvent]
public readonly record struct AfterGotPointedAtEvent(EntityUid Pointer);