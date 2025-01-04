using System.Numerics;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Goobstation.Wizard.TimeStop;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Wizard.SpellCards;

public sealed class SpellCardSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpellCardComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<SpellCardComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp(ent, out AppearanceComponent? appearance))
            return;

        if (!_appearance.TryGetData(ent, SpellCardVisuals.State, out _, appearance))
            _appearance.SetData(ent, SpellCardVisuals.State, 0, appearance);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<SpellCardComponent, PhysicsComponent, FixturesComponent, MetaDataComponent>();

        var xformQuery = GetEntityQuery<TransformComponent>();
        var homingQuery = GetEntityQuery<HomingProjectileComponent>();
        var trailQuery = GetEntityQuery<TrailComponent>();
        var appearanceQuery = GetEntityQuery<AppearanceComponent>();
        var frozenQuery = GetEntityQuery<FrozenComponent>();
        while (query.MoveNext(out var uid, out var card, out var physics, out var fix, out var meta))
        {
            if (card.Flipped)
            {
                if (frozenQuery.HasComp(uid))
                    continue;

                if (physics.AngularVelocity == 0f ||
                    homingQuery.TryComp(uid, out var homingComp) && homingComp.Target != null)
                    continue;

                var velocity = _transform.GetWorldRotation(uid, xformQuery).ToWorldVec() * card.TargetedSpeed;
                _physics.SetLinearVelocity(uid, velocity, false, true, fix, physics);
                continue;
            }

            AppearanceComponent? appearance;

            if (card.Targeted)
            {
                card.FlipAccumulator -= frameTime;

                if (card.FlipAccumulator > 0f)
                    continue;

                _physics.SetLinearDamping(uid, physics, 0f, false);
                var velocity = _transform.GetWorldRotation(uid, xformQuery).ToWorldVec() * card.TargetedSpeed;
                if (!frozenQuery.TryComp(uid, out var frozen))
                    _physics.SetLinearVelocity(uid, velocity, false, true, fix, physics);
                else
                    frozen.OldLinearVelocity = velocity;

                card.Flipped = true;

                Entity<SpellCardComponent, PhysicsComponent> entity = (uid, card, physics);
                Dirty(entity, meta);

                if (appearanceQuery.TryComp(uid, out appearance))
                    _appearance.SetData(uid, SpellCardVisuals.State, 2, appearance);

                if (trailQuery.TryComp(uid, out var trail))
                {
                    trail.Color = card.FlippedTrailColor;
                    Dirty(uid, trail, meta);
                }

                continue;
            }

            if (frozenQuery.HasComp(uid))
                continue;

            if (!Exists(card.Target) || TerminatingOrDeleted(card.Target))
            {
                _physics.SetLinearDamping(uid, physics, 0f, false);
                _physics.SetLinearVelocity(uid,
                    physics.LinearVelocity.Normalized() * card.TargetedSpeed,
                    false,
                    true,
                    fix,
                    physics);
                card.Targeted = true;
                card.Flipped = true;

                Entity<SpellCardComponent, PhysicsComponent> entity = (uid, card, physics);
                Dirty(entity, meta);

                if (appearanceQuery.TryComp(uid, out appearance))
                    _appearance.SetData(uid, SpellCardVisuals.State, 0, appearance);
                continue;
            }

            if (!physics.LinearVelocity.EqualsApprox(Vector2.Zero, card.Tolerance))
            {
                _physics.SetLinearVelocity(uid,
                    physics.LinearVelocity.Length() * _transform.GetWorldRotation(uid, xformQuery).ToWorldVec(),
                    false,
                    true,
                    fix,
                    physics);
                Dirty(uid, physics, meta);
                continue;
            }

            _physics.SetAngularVelocity(uid, 0f, false, fix, physics);

            if (appearanceQuery.TryComp(uid, out appearance))
                _appearance.SetData(uid, SpellCardVisuals.State, 1, appearance);

            var homing = EnsureComp<HomingProjectileComponent>(uid);
            homing.Target = card.Target.Value;
            card.Targeted = true;
            card.FlipAccumulator = card.FlipTime;
            if (card.FlipTime <= 0f)
                card.Flipped = true;
            Entity<SpellCardComponent, HomingProjectileComponent, PhysicsComponent> ent = (uid, card, homing, physics);
            Dirty(ent, meta);
        }
    }
}
