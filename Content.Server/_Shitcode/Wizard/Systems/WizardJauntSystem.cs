// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Polymorph;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class WizardJauntSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardJauntComponent, PolymorphedEvent>(OnPolymorph);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var trailQuery = GetEntityQuery<TrailComponent>();

        var query = EntityQueryEnumerator<WizardJauntComponent, PolymorphedEntityComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var jaunt, out var polymorphed, out var xform))
        {
            if (jaunt.JauntEndEffectEntity is {} endEffect)
            {
                _transform.SetMapCoordinates(endEffect, _transform.GetMapCoordinates(xform));
                continue;
            }

            jaunt.DurationBetweenEffects -= frameTime;

            if (jaunt.DurationBetweenEffects > 0f)
                continue;

            var ent = Spawn(jaunt.JauntEndEffect,
                _transform.GetMapCoordinates(uid, xform),
                rotation: _transform.GetWorldRotation(xform));
            _audio.PlayEntity(jaunt.JauntEndSound, Filter.Pvs(ent), ent, true);
            jaunt.JauntEndEffectEntity = ent;

            if (!trailQuery.TryComp(ent, out var trail))
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
            _transform.GetMapCoordinates(uid),
            rotation: _transform.GetWorldRotation(uid));
        _audio.PlayPvs(comp.JauntStartSound, startEffect);

        if (!TryComp(startEffect, out TrailComponent? trail))
            return;

        trail.RenderedEntity = args.OldEntity;
        Dirty(startEffect, trail);
    }
}