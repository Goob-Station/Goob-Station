// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.BloodCult.Components;
using Content.Server.Popups;
using Content.Shared._White.ListViewSelector;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Humanoid;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Trigger;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class TeleportRuneSystem : EntitySystem
{
    private const int MaxNameLength = 32;
    private const float InteractionRange = 2f;

    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeleportRuneComponent, BloodCultRuneDrawnEvent>(OnRuneDrawn);
        SubscribeLocalEvent<TeleportRuneComponent, TeleportRuneNameSelectedMessage>(OnNameSelected);
        SubscribeLocalEvent<TeleportRuneComponent, TriggerEvent>(OnTriggered);
        SubscribeLocalEvent<TeleportRuneComponent, ListViewItemSelectedMessage>(OnDestinationSelected);
    }

    private void OnRuneDrawn(Entity<TeleportRuneComponent> rune, ref BloodCultRuneDrawnEvent args)
    {
        if (!HasComp<BloodCultistComponent>(args.User))
            return;

        _ui.OpenUi(rune.Owner, TeleportRuneNameUiKey.Key, args.User);
    }

    private void OnNameSelected(Entity<TeleportRuneComponent> rune, ref TeleportRuneNameSelectedMessage args)
    {
        if (!HasComp<BloodCultistComponent>(args.Actor) ||
            !_ui.IsUiOpen(rune.Owner, TeleportRuneNameUiKey.Key, args.Actor) ||
            !_transform.InRange(Transform(args.Actor).Coordinates, Transform(rune).Coordinates, InteractionRange))
            return;

        var name = args.Name.Trim();
        rune.Comp.Name = name[..Math.Min(name.Length, MaxNameLength)];
        _ui.CloseUi(rune.Owner, TeleportRuneNameUiKey.Key, args.Actor);
    }

    private void OnTriggered(Entity<TeleportRuneComponent> rune, ref TriggerEvent args)
    {
        if (args.Handled || args.User is not { } user || !HasComp<BloodCultistComponent>(user))
            return;

        args.Handled = true;
        var destinations = GetDestinations(rune);
        if (destinations.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-teleport-not-found"), rune.Owner, user);
            return;
        }

        _ui.SetUiState(rune.Owner, ListViewSelectorUiKey.Key, new ListViewSelectorState(destinations));
        _ui.OpenUi(rune.Owner, ListViewSelectorUiKey.Key, user);
    }

    private void OnDestinationSelected(Entity<TeleportRuneComponent> origin, ref ListViewItemSelectedMessage args)
    {
        if (!HasComp<BloodCultistComponent>(args.Actor) ||
            !_ui.IsUiOpen(origin.Owner, ListViewSelectorUiKey.Key, args.Actor) ||
            !EntityUid.TryParse(args.SelectedItem.Id, out var destinationUid) ||
            destinationUid == origin.Owner ||
            !TryComp<TeleportRuneComponent>(destinationUid, out var destination) ||
            Transform(origin).MapID != Transform(destinationUid).MapID ||
            !_transform.InRange(Transform(args.Actor).Coordinates, Transform(origin).Coordinates, InteractionRange))
            return;

        var validDestination = false;
        foreach (var entry in GetDestinations(origin))
        {
            if (entry.Id != args.SelectedItem.Id)
                continue;

            validDestination = true;
            break;
        }

        if (!validDestination)
            return;

        var destinationCoordinates = Transform(destinationUid).Coordinates;
        var targets = _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(
            Transform(origin).Coordinates,
            origin.Comp.GatherRange);

        foreach (var target in targets)
        {
            StopPulling(target);
            _transform.SetCoordinates(target, destinationCoordinates);
        }

        _audio.PlayPvs(origin.Comp.TeleportOutSound, origin.Owner);
        _audio.PlayPvs(destination.TeleportInSound, destinationUid);
        _ui.CloseUi(origin.Owner, ListViewSelectorUiKey.Key, args.Actor);
    }

    private List<ListViewSelectorEntry> GetDestinations(Entity<TeleportRuneComponent> origin)
    {
        var destinations = new List<ListViewSelectorEntry>();
        var originMap = Transform(origin).MapID;
        var query = EntityQueryEnumerator<TeleportRuneComponent>();

        while (query.MoveNext(out var uid, out var teleport))
        {
            if (uid == origin.Owner || Transform(uid).MapID != originMap)
                continue;

            var name = string.IsNullOrWhiteSpace(teleport.Name)
                ? Loc.GetString("cult-teleport-unnamed")
                : teleport.Name;
            destinations.Add(new ListViewSelectorEntry(uid.ToString(), name));
        }

        return destinations;
    }

    private void StopPulling(EntityUid target)
    {
        _pulling.StopAllPulls(target);
    }
}
