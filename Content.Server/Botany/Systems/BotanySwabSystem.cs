// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Botany.Components;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Swab;

namespace Content.Server.Botany.Systems;

public sealed class BotanySwabSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly MutationSystem _mutationSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BotanySwabComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<BotanySwabComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BotanySwabComponent, BotanySwabDoAfterEvent>(OnDoAfter);
    }

    /// <summary>
    /// This handles swab examination text
    /// so you can tell if they are used or not.
    /// </summary>
    private void OnExamined(EntityUid uid, BotanySwabComponent swab, ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
        {
            if (swab.SeedData != null)
                args.PushMarkup(Loc.GetString("swab-used"));
            else
                args.PushMarkup(Loc.GetString("swab-unused"));
        }
    }

    /// <summary>
    /// Handles swabbing a plant.
    /// </summary>
    private void OnAfterInteract(EntityUid uid, BotanySwabComponent swab, AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !HasComp<PlantHolderComponent>(args.Target))
            return;

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, swab.SwabDelay, new BotanySwabDoAfterEvent(), uid, target: args.Target, used: uid)
        {
            Broadcast = true,
            BreakOnMove = true,
            NeedHand = true,
        });
    }

    /// <summary>
    /// Save seed data or cross-pollenate.
    /// </summary>
    private void OnDoAfter(EntityUid uid, BotanySwabComponent swab, DoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !TryComp<PlantHolderComponent>(args.Args.Target, out var plant))
            return;

        if (swab.SeedData == null)
        {
            // Pick up pollen
            swab.SeedData = plant.Seed;
            _popupSystem.PopupEntity(Loc.GetString("botany-swab-from"), args.Args.Target.Value, args.Args.User);
        }
        else
        {
            var old = plant.Seed;
            if (old == null)
                return;
            plant.Seed = _mutationSystem.Cross(swab.SeedData, old); // Cross-pollenate
            swab.SeedData = old; // Transfer old plant pollen to swab
            _popupSystem.PopupEntity(Loc.GetString("botany-swab-to"), args.Args.Target.Value, args.Args.User);
        }

        args.Handled = true;
    }
}
