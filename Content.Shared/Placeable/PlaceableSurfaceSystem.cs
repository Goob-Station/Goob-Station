// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Storage;
using Content.Shared.Storage.Components;

namespace Content.Shared.Placeable;

public sealed class PlaceableSurfaceSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlaceableSurfaceComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        SubscribeLocalEvent<PlaceableSurfaceComponent, StorageInteractUsingAttemptEvent>(OnStorageInteractUsingAttempt);
        SubscribeLocalEvent<PlaceableSurfaceComponent, StorageAfterOpenEvent>(OnStorageAfterOpen);
        SubscribeLocalEvent<PlaceableSurfaceComponent, StorageAfterCloseEvent>(OnStorageAfterClose);
    }

    public void SetPlaceable(EntityUid uid, bool isPlaceable, PlaceableSurfaceComponent? surface = null)
    {
        if (!Resolve(uid, ref surface, false))
            return;

        if (surface.IsPlaceable == isPlaceable)
            return;

        surface.IsPlaceable = isPlaceable;
        Dirty(uid, surface);
    }

    public void SetPlaceCentered(EntityUid uid, bool placeCentered, PlaceableSurfaceComponent? surface = null)
    {
        if (!Resolve(uid, ref surface))
            return;

        surface.PlaceCentered = placeCentered;
        Dirty(uid, surface);
    }

    public void SetPositionOffset(EntityUid uid, Vector2 offset, PlaceableSurfaceComponent? surface = null)
    {
        if (!Resolve(uid, ref surface))
            return;

        surface.PositionOffset = offset;
        Dirty(uid, surface);
    }

    private void OnAfterInteractUsing(EntityUid uid, PlaceableSurfaceComponent surface, AfterInteractUsingEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (!surface.IsPlaceable)
            return;

        // 99% of the time they want to dump the stuff inside on the table, they can manually place with q if they really need to.
        // Just causes prediction CBT otherwise.
        if (HasComp<DumpableComponent>(args.Used))
            return;

        if (!_handsSystem.TryDrop(args.User, args.Used))
            return;

        _transformSystem.SetCoordinates(args.Used,
            surface.PlaceCentered ? Transform(uid).Coordinates.Offset(surface.PositionOffset) : args.ClickLocation);

        args.Handled = true;
    }

    private void OnStorageInteractUsingAttempt(Entity<PlaceableSurfaceComponent> ent, ref StorageInteractUsingAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnStorageAfterOpen(Entity<PlaceableSurfaceComponent> ent, ref StorageAfterOpenEvent args)
    {
        SetPlaceable(ent.Owner, true, ent.Comp);
    }

    private void OnStorageAfterClose(Entity<PlaceableSurfaceComponent> ent, ref StorageAfterCloseEvent args)
    {
        SetPlaceable(ent.Owner, false, ent.Comp);
    }
}