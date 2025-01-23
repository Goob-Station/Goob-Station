using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared._Goobstation.Wizard.Projectiles;
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

        SubscribeLocalEvent<WizardJauntComponent, PolymorphedIntoEvent>(OnPolymorph);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var trailQuery = GetEntityQuery<TrailComponent>();

        var query = EntityQueryEnumerator<WizardJauntComponent, PolymorphedEntityComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var jaunt, out var polymorphed, out var xform))
        {
            if (jaunt.JauntEndEffectSpawned)
                continue;

            jaunt.DurationBetweenEffects -= frameTime;

            if (jaunt.DurationBetweenEffects > 0f)
                continue;

            jaunt.JauntEndEffectSpawned = true;
            var ent = Spawn(jaunt.JauntEndEffect,
                _transform.GetMapCoordinates(uid, xform),
                rotation: _transform.GetWorldRotation(xform));
            _audio.PlayEntity(jaunt.JauntEndSound, Filter.Pvs(ent), ent, true);
            _transform.SetParent(ent, Transform(ent), uid, xform);

            if (!trailQuery.TryComp(ent, out var trail))
                continue;

            trail.RenderedEntity = polymorphed.Parent;
            Dirty(ent, trail);
        }
    }

    private void OnPolymorph(Entity<WizardJauntComponent> ent, ref PolymorphedIntoEvent args)
    {
        var (uid, comp) = ent;

        if (args.Reverted)
        {
            _transform.ReparentChildren(uid, args.Parent);
            return;
        }

        var startEffect = Spawn(comp.JauntStartEffect,
            _transform.GetMapCoordinates(uid),
            rotation: _transform.GetWorldRotation(uid));
        _audio.PlayPvs(comp.JauntStartSound, startEffect);

        if (!TryComp(startEffect, out TrailComponent? trail))
            return;

        trail.RenderedEntity = args.Parent;
        Dirty(startEffect, trail);
    }
}
