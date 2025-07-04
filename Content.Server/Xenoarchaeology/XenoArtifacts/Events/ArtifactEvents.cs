// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Events;

/// <summary>
///     Invokes when artifact was successfully activated.
///     Used to start attached effects.
/// </summary>
public sealed class ArtifactActivatedEvent : EntityEventArgs
{
    /// <summary>
    ///     Entity that activate this artifact.
    ///     Usually player, but can also be another object.
    /// </summary>
    public EntityUid? Activator;
}

/// <summary>
///     Force to randomize artifact triggers.
/// </summary>
public sealed class ArtifactNodeEnteredEvent : EntityEventArgs
{
    /// <summary>
    /// An entity-specific seed that can be used to
    /// generate random values.
    /// </summary>
    public readonly int RandomSeed;

    public ArtifactNodeEnteredEvent(int randomSeed)
    {
        RandomSeed = randomSeed;
    }
}