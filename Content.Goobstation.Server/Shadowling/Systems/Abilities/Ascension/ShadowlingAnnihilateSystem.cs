// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Explosion;
using Content.Shared.Gibbing.Events;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles the Annihilate abiltiy logic.
/// Gib from afar!
/// </summary>
public sealed class ShadowlingAnnihilateSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAnnihilateComponent, AnnihilateEvent>(OnAnnihilate);
        SubscribeLocalEvent<ShadowlingAnnihilateComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingAnnihilateComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingAnnihilateComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<ShadowlingAnnihilateComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    public ProtoId<ExplosionPrototype> ExplosionId = "Corpse";
    private void OnAnnihilate(EntityUid uid, ShadowlingAnnihilateComponent component, AnnihilateEvent args)
    {
        if (args.Handled
            || HasComp<ShadowlingComponent>(args.Target)
            || !HasComp<MobStateComponent>(args.Target))
            return;

        _chat.TryEmoteWithChat(uid, component.SnapEmote, ignoreActionBlocker: true, forceEmote: true);

        // The gibbening
        var target = args.Target;

        _explosionSystem.QueueExplosion(
            target,
            typeId: ExplosionId,
            totalIntensity: 1,
            slope: 1,
            maxTileIntensity: 1,
            canCreateVacuum: false);

        _body.GibBody(target, contents: GibContentsOption.Gib);
        args.Handled = true;
    }
}
