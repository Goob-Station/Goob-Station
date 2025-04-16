// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Server.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Events;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Tag;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Server.Chemistry.EntitySystems;

/// <summary>
/// System for handling injecting into an entity while a projectile is embedded.
/// </summary>
public sealed class SolutionInjectWhileEmbeddedSystem : EntitySystem
{
	[Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SolutionInjectWhileEmbeddedComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<SolutionInjectWhileEmbeddedComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _gameTiming.CurTime + ent.Comp.UpdateInterval;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SolutionInjectWhileEmbeddedComponent, EmbeddableProjectileComponent>();
        while (query.MoveNext(out var uid, out var injectComponent, out var projectileComponent))
        {
            if (_gameTiming.CurTime < injectComponent.NextUpdate)
                continue;

            injectComponent.NextUpdate += injectComponent.UpdateInterval;

            // <Goobstation> Goobstation - Shot syringes injecting over time
            if(projectileComponent.EmbeddedIntoUid == null) // check if we should reset state,
            {
                if (TryComp<PhysicsComponent>(uid, out var physics) && physics.BodyStatus != BodyStatus.InAir) // don't reset in-flight things
                    injectComponent.Injections = 0;
                continue;
            }

            if ((injectComponent.Injections ?? 0) >= injectComponent.MaxInjections)
                continue;

            if (injectComponent.Injections != null)
                injectComponent.Injections += 1;
            // </Goobstation>

            var ev = new InjectOverTimeEvent(projectileComponent.EmbeddedIntoUid.Value);
            RaiseLocalEvent(uid, ref ev);

        }
    }
}