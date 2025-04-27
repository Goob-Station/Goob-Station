using Content.Server.Damage.Systems;
using Content.Server.Temperature.Components;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Server._Shitcode.Heretic.EntitySystems;

public sealed class IceSpearSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedProjectileSystem _projectile = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IceSpearComponent, ThrowDoHitEvent>(OnThrowDoHit,
            after: new[] { typeof(DamageOtherOnHitSystem), typeof(SharedProjectileSystem) });
    }

    private void OnThrowDoHit(Entity<IceSpearComponent> ent, ref ThrowDoHitEvent args)
    {
        if (!HasComp<MobStateComponent>(args.Target))
            return;

        if (!HasComp<HereticComponent>(args.Target) && !HasComp<GhostComponent>(args.Target) &&
            HasComp<TemperatureComponent>(args.Target))
            EnsureComp<IceCubeComponent>(args.Target);

        if (Exists(ent.Comp.ActionId))
            _action.SetIfBiggerCooldown(ent.Comp.ActionId, ent.Comp.ShatterCooldown);

        if (TryComp(ent, out EmbeddableProjectileComponent? embeddable))
            _projectile.EmbedDetach(ent, embeddable);

        var coords = Transform(ent).Coordinates;
        _audio.PlayPvs(ent.Comp.ShatterSound, coords);
        QueueDel(ent);
    }
}
