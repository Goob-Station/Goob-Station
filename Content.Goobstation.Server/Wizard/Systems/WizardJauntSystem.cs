// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Wizard.Components;
using Content.Server.Polymorph.Components;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Polymorph;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Wizard.Systems;

/// <summary>
/// Handles effects for wizard jaunt.
/// TODO should be shared or probably client. Relies on PolymorphedEntityComponent though
/// </summary>
public sealed class WizardJauntSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    private EntityQuery<TrailComponent> _trailQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardJauntComponent, PolymorphedEvent>(OnPolymorph);

        _trailQuery = GetEntityQuery<TrailComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WizardJauntComponent, PolymorphedEntityComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var jaunt, out var polymorphed, out var xform))
        {
            if (jaunt.JauntEndEffectEntity is {} endEffect)
            {
                _xform.SetMapCoordinates(endEffect, _xform.GetMapCoordinates(xform));
                continue;
            }

            jaunt.DurationBetweenEffects -= frameTime;

            if (jaunt.DurationBetweenEffects > 0f)
                continue;

            var ent = Spawn(jaunt.JauntEndEffect,
                _xform.GetMapCoordinates(uid, xform),
                rotation: _xform.GetWorldRotation(xform));
            _audio.PlayEntity(jaunt.JauntEndSound, Filter.Pvs(ent), ent, true);
            jaunt.JauntEndEffectEntity = ent;

            if (!_trailQuery.TryComp(ent, out var trail))
                continue;

            trail.RenderedEntity = polymorphed.Parent;
            Dirty(ent, trail);
        }
    }

    private void OnPolymorph(Entity<WizardJauntComponent> ent, ref PolymorphedEvent args)
    {
        var (uid, comp) = ent;

        if (args.IsRevert)
            return;

        var startEffect = Spawn(comp.JauntStartEffect,
            _xform.GetMapCoordinates(uid),
            rotation: _xform.GetWorldRotation(uid));
        _audio.PlayPvs(comp.JauntStartSound, startEffect);

        if (!_trailQuery.TryComp(startEffect, out var trail))
            return;

        trail.RenderedEntity = args.OldEntity;
        Dirty(startEffect, trail);
    }
}