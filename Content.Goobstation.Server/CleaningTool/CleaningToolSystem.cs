// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Footprints;
using Content.Goobstation.Shared.CleaningTool;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.CleaningTool;

public sealed class CleaningToolSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CleaningToolComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CleaningToolComponent, CleaningToolDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<CleaningToolComponent> cleaningTool, ref AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Handled
            || args.Target != null)
            return;

        var user = args.User;
        var foundEntities = new HashSet<EntityUid>();

        _lookup.GetEntitiesInRange(args.ClickLocation,
            cleaningTool.Comp.Radius,
            foundEntities);

        foundEntities.RemoveWhere(ent =>
            !_interaction.InRangeUnobstructed(user, ent, cleaningTool.Comp.Radius)
            || !HasComp<FootprintComponent>(ent));

        if (foundEntities.Count == 0)
            return;

        args.Handled = TryStartCleaning(cleaningTool, args.User, foundEntities);
    }

    private bool TryStartCleaning(Entity<CleaningToolComponent> cleaningTool,
        EntityUid user,
        HashSet<EntityUid> targets)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            cleaningTool.Comp.CleanDelay,
            new CleaningToolDoAfterEvent(GetNetEntityList(targets)),
            cleaningTool,
            used: cleaningTool)
        {
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.01f,
        };

        _popup.PopupEntity(Loc.GetString("cleaning-tool-scrubbing-start", ("user", user)), user);
        return _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(Entity<CleaningToolComponent> cleaningTool, ref CleaningToolDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled)
            return;

        foreach (var ent in GetEntityList(args.Entities))
        {
            Spawn(cleaningTool.Comp.SparkleProto, Transform(ent).Coordinates);
            Del(ent);
        }

    }
}
