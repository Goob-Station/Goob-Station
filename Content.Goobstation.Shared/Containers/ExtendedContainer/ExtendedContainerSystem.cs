// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Destructible;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Containers.ExtendedContainer;

public sealed partial class ExtendedContainerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistsystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExtendedContainerComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ExtendedContainerComponent, BreakageEventArgs>(OnBreak);
        SubscribeLocalEvent<ExtendedContainerComponent, DestructionEventArgs>(OnBreak);
        SubscribeLocalEvent<ExtendedContainerComponent, ContainerIsInsertingAttemptEvent>(OnContainerIsInsertingAttempt);
        SubscribeLocalEvent<ExtendedContainerComponent, ContainerIsRemovingAttemptEvent>(OnContainerIsRemovingAttempt);
    }

    /// <summary>
    /// Ensure the containers.
    /// </summary>
    private void OnComponentInit(EntityUid uid, ExtendedContainerComponent component, ComponentInit args)
    {
        component.Content = _containers.EnsureContainer<Container>(uid, component.ContainerName);
    }

    private void OnBreak(EntityUid uid, ExtendedContainerComponent component, EntityEventArgs args)
    {
        if (component.DeleteContentsOnBreak)
            return;

        if (component.Content == null)
            return;

        foreach (var entity in component.Content.ContainedEntities.ToArray())
        {
            _containers.Remove(entity, component.Content);
        }
    }

    /// <summary>
    /// Cancel removal from the container if the removed entity is not part of the whitelist then play a sound
    /// </summary>
    private void OnContainerIsRemovingAttempt(EntityUid uid, ExtendedContainerComponent component, ContainerIsRemovingAttemptEvent args)
    {
        if (args.Container.ID != component.ContainerName)
            return;

        // Cancel removal only if entity is NOT whitelisted
        if (component.RemoveWhitelist != null && _whitelistsystem.IsValid(component.RemoveWhitelist, args.EntityUid))
        {
            args.Cancel();
            return;
        }

        if (component.RemoveSound != null)
            _audioSystem.PlayPredicted(component.RemoveSound, Transform(uid).Coordinates, uid);
    }

    /// <summary>
    /// Cancel insertion into the container if the inserting entity does not pass the whitelist
    /// or the container is full, play a sound if successful
    /// </summary>
    private void OnContainerIsInsertingAttempt(EntityUid uid, ExtendedContainerComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (args.Container.ID != component.ContainerName)
            return;

        var isContainerFull = component.Content?.ContainedEntities.Count >= component.Capacity;

        if (isContainerFull ||
            component.InsertWhitelist != null &&
            _whitelistsystem.IsValid(component.InsertWhitelist, args.EntityUid))
        {
            args.Cancel();
            return;
        }

        if (component.InsertSound != null)
            _audioSystem.PlayPredicted(component.InsertSound, Transform(uid).Coordinates, uid);
    }
}