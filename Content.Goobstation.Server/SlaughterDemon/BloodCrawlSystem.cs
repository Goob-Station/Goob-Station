// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Server.Actions;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;


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
    [Dependency] private readonly SlaughterDemonSystem _slaughter = default!;

    private EntityQuery<ActionsComponent> _actionQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        _actionQuery = GetEntityQuery<ActionsComponent>();

        SubscribeLocalEvent<BloodCrawlComponent, BloodCrawlEvent>(OnBloodCrawl);

        SubscribeLocalEvent<BloodCrawlComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, BloodCrawlComponent component, ComponentStartup args)
    {
        if (!_actionQuery.TryGetComponent(uid, out var actions))
            return;

        _actions.AddAction(uid, ref component.ActionEntity, component.ActionId, component: actions);
    }

    private void OnBloodCrawl(EntityUid uid, BloodCrawlComponent component, BloodCrawlEvent args)
    {
        if (!_slaughter.IsStandingOnBlood(uid))
        {
            _actions.SetCooldown(component.ActionEntity, component.ActionCooldown);
            return;
        }

        component.IsCrawling = !component.IsCrawling;

        if (!component.IsCrawling && TryComp<PolymorphedEntityComponent>(uid, out var polymorph))
        {
            _polymorph.Revert(uid);

            var evExit = new BloodCrawlExitEvent();
            RaiseLocalEvent(polymorph.Parent, ref evExit);
            return;
        }

        var evAttempt = new BloodCrawlAttemptEvent();
        RaiseLocalEvent(uid, ref evAttempt);

        var ent = _polymorph.PolymorphEntity(uid, component.Jaunt);
        _actions.StartUseDelay(component.ActionEntity);
    }
}


