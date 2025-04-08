// SPDX-FileCopyrightText: 2024 ActiveMammmoth <140334666+ActiveMammmoth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Storage;
using Robust.Shared.Audio;

namespace Content.Shared.Magic.Events;

public sealed partial class RandomGlobalSpawnSpellEvent : InstantActionEvent, ISpeakSpell
{
    /// <summary>
    /// The list of prototypes this spell can spawn, will select one randomly
    /// </summary>
    [DataField]
    public List<EntitySpawnEntry> Spawns = new();

    /// <summary>
    /// Sound that will play globally when cast
    /// </summary>
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Magic/staff_animation.ogg");

    [DataField]
    public string? Speech { get; private set; }
}