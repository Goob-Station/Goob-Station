using System.Linq;
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Goobstation.Shared.SmartLinkImplant;

public sealed class SmartLinkSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmartLinkArmComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SmartLinkArmComponent, BodyPartAddedEvent>(OnAttach);

        SubscribeLocalEvent<SmartLinkComponent, AmmoShotUserEvent>(OnShot);
    }

    private void OnInit(Entity<SmartLinkArmComponent> ent, ref ComponentInit args) => UpdateComp(ent);

    private void OnAttach(Entity<SmartLinkArmComponent> ent, ref BodyPartAddedEvent args) => UpdateComp(ent);

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
            var homing = EnsureComp<HomingProjectileComponent>(projectile);

            homing.Target = gun.Target.Value;
            Dirty(projectile, homing);
        }
    }
}
