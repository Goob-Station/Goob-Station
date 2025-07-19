// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Server.Actions;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids.Components;
using Robust.Server.Audio;

namespace Content.Goobstation.Server.SlaughterDemon;


/// <summary>
/// This handles the blood crawl system.
/// Blood Crawl allows you to jaunt, as long as you activate it in a pool of blood.
/// To exit the jaunt, you must also stand on a poll of blood.
/// </summary>
public sealed class BloodCrawlSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    private EntityQuery<ActionsComponent> _actionQuery;
    private EntityQuery<PuddleComponent> _puddleQuery;
    private EntityQuery<PolymorphedEntityComponent> _polymorphedQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        _actionQuery = GetEntityQuery<ActionsComponent>();
        _puddleQuery = GetEntityQuery<PuddleComponent>();
        _polymorphedQuery = GetEntityQuery<PolymorphedEntityComponent>();

        SubscribeLocalEvent<BloodCrawlComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<BloodCrawlComponent, BloodCrawlEvent>(OnBloodCrawl);
    }

    private void OnStartup(EntityUid uid, BloodCrawlComponent component, ComponentStartup args)
    {
        if (!_actionQuery.TryGetComponent(uid, out var actions))
            return;

        _actions.AddAction(uid, ref component.ActionEntity, component.ActionId, component: actions);
    }

    private void OnBloodCrawl(EntityUid uid, BloodCrawlComponent component, BloodCrawlEvent args)
    {
        if (!IsStandingOnBlood(uid))
        {
            _actions.SetCooldown(component.ActionEntity, component.ActionCooldown);
            return;
        }

        component.IsCrawling = !component.IsCrawling;

        if (!component.IsCrawling && _polymorphedQuery.TryComp(uid, out var polymorph))
        {
            var reverted = _polymorph.Revert(uid);

            if (reverted != null)
                _audio.PlayPvs(component.ExitJauntSound, reverted.Value);

            var evExit = new BloodCrawlExitEvent();
            RaiseLocalEvent(polymorph.Parent, ref evExit);

            return;
        }

        var evAttempt = new BloodCrawlAttemptEvent();
        RaiseLocalEvent(uid, ref evAttempt);

        _audio.PlayPvs(component.EnterJauntSound, Transform(uid).Coordinates);

        _polymorph.PolymorphEntity(uid, component.Jaunt);
        _actions.StartUseDelay(component.ActionEntity);
    }

    #region Helper Functions

    /// <summary>
    /// Detects if an entity is standing on blood, or not.
    /// </summary>
    public bool IsStandingOnBlood(Entity<BloodCrawlComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return false;

        var ents = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRange);
        foreach (var entity in ents)
        {
            if (!_puddleQuery.TryComp(entity, out var puddle))
                continue;

            if (!_solutionContainerSystem.ResolveSolution(entity, puddle.SolutionName, ref puddle.Solution, out var solution))
                continue;

            foreach (var reagent in solution.Contents)
            {
                if (reagent.Reagent.Prototype == ent.Comp.Blood)
                    return true;
            }
        }
        return false;
    }

    #endregion
}


