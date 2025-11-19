using System.Linq;
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.SmartLinkImplant;

public sealed class SmartLinkSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmartLinkArmComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SmartLinkArmComponent, BodyPartAddedEvent>(OnAttach);
        SubscribeLocalEvent<SmartLinkArmComponent, BodyPartRemovedEvent>(OnRemove);

        SubscribeLocalEvent<SmartLinkComponent, AmmoShotUserEvent>(OnShot);
    }

    private void OnInit(Entity<SmartLinkArmComponent> ent, ref ComponentInit args) => UpdateComp(ent);

    private void OnAttach(Entity<SmartLinkArmComponent> ent, ref BodyPartAddedEvent args) => UpdateComp(ent);

    private void OnRemove(Entity<SmartLinkArmComponent> ent, ref BodyPartRemovedEvent args) => UpdateComp(ent);

    private void UpdateComp(Entity<SmartLinkArmComponent> ent)
    {
        if (!TryComp<BodyPartComponent>(ent, out var part)
            || part.Body == null)
            return;

        var arms = _body.GetBodyChildrenOfType(part.Body.Value, BodyPartType.Arm);
        if (arms.Count() != arms.Where(x => HasComp<SmartLinkArmComponent>(x.Id)).Count())
        {
            RemComp<SmartLinkComponent>(part.Body.Value);
            return;
        }
        else
            EnsureComp<SmartLinkComponent>(part.Body.Value);
    }

    private void OnShot(Entity<SmartLinkComponent> ent, ref AmmoShotUserEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp(args.Gun, out GunComponent? gun) || gun.Target == null)
            return;

        if (gun.Target == Transform(uid).ParentUid)
            return;

        foreach (var projectile in args.FiredProjectiles)
        {
            if (HasComp<SmartLinkBlacklistComponent>(projectile))
                continue;

            var homing = EnsureComp<DelayedHomingProjectileComponent>(projectile);
            homing.HomingStart = _timing.CurTime + TimeSpan.FromSeconds(0.35f);
            homing.Target = gun.Target.Value;
            Dirty(projectile, homing);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<DelayedHomingProjectileComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.HomingStart)
                continue;

            var homing = EnsureComp<DelayedHomingProjectileComponent>(ent);
            homing.Target = comp.Target;
            RemCompDeferred<DelayedHomingProjectileComponent>(ent);
        }
    }
}
