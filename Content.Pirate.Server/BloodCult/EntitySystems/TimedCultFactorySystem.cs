// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.BloodCult.Components;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared._White.RadialSelector;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class TimedCultFactorySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TimedCultFactoryComponent, ActivatableUIOpenAttemptEvent>(OnOpenAttempt);
        SubscribeLocalEvent<TimedCultFactoryComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpen);
        SubscribeLocalEvent<TimedCultFactoryComponent, RadialSelectorSelectedMessage>(OnSelected);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TimedCultFactoryComponent>();
        while (query.MoveNext(out var uid, out var factory))
        {
            if (factory.Active)
                continue;

            factory.CooldownRemaining -= frameTime;
            if (factory.CooldownRemaining > 0f)
                continue;

            factory.Active = true;
            factory.CooldownRemaining = 0f;
            _appearance.SetData(uid, BloodCultVisuals.Active, true);
        }
    }

    private void OnOpenAttempt(Entity<TimedCultFactoryComponent> factory, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!HasComp<BloodCultistComponent>(args.User) && !HasComp<BloodCultConstructComponent>(args.User))
        {
            args.Cancel();
            return;
        }

        if (factory.Comp.Active)
            return;

        _popup.PopupEntity(
            Loc.GetString("blood-cult-factory-cooldown", ("seconds", (int) Math.Ceiling(factory.Comp.CooldownRemaining))),
            factory,
            args.User);
        args.Cancel();
    }

    private void OnBeforeOpen(Entity<TimedCultFactoryComponent> factory, ref BeforeActivatableUIOpenEvent args)
    {
        _ui.SetUiState(
            factory.Owner,
            RadialSelectorUiKey.Key,
            new TrackedRadialSelectorState(factory.Comp.Entries));
    }

    private void OnSelected(Entity<TimedCultFactoryComponent> factory, ref RadialSelectorSelectedMessage args)
    {
        var selectedItem = args.SelectedItem;
        if (!factory.Comp.Active ||
            (!HasComp<BloodCultistComponent>(args.Actor) &&
             !HasComp<BloodCultConstructComponent>(args.Actor)) ||
            !_ui.IsUiOpen(factory.Owner, RadialSelectorUiKey.Key, args.Actor))
            return;

        var allowed = factory.Comp.Entries.Any(entry => entry.Prototype == selectedItem);
        if (!allowed || !_prototype.HasIndex<EntityPrototype>(selectedItem))
            return;

        var product = Spawn(selectedItem, Transform(args.Actor).Coordinates);
        _hands.TryPickupAnyHand(args.Actor, product);

        factory.Comp.Active = false;
        factory.Comp.CooldownRemaining = factory.Comp.Cooldown;
        _appearance.SetData(factory, BloodCultVisuals.Active, false);
        _ui.CloseUi(factory.Owner, RadialSelectorUiKey.Key, args.Actor);
    }
}
