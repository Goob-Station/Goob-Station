using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.BerserkerImplant;

public abstract class SharedBerserkerImplantSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActivateBerserkerImplantActionEvent>(OnActivate);

        SubscribeLocalEvent<BerserkerImplantActiveComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<BerserkerImplantActiveComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
        SubscribeLocalEvent<BerserkerImplantActiveComponent, BeforeStaminaDamageEvent>(OnStaminaDamageModify);
        SubscribeLocalEvent<BerserkerImplantActiveComponent, GetUserMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<BerserkerImplantActiveComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnActivate(ActivateBerserkerImplantActionEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<BerserkerImplantActiveComponent>(args.Performer, out var berserker))
            return;

        args.Handled = true;

        var uid = args.Performer;
        berserker = EnsureComp<BerserkerImplantActiveComponent>(uid);
        berserker.EndTime = Timing.CurTime + TimeSpan.FromSeconds(berserker.Duration);
        if (!HasComp<TrailComponent>(uid))
        {
            var trail = AddComp<TrailComponent>(uid);
            trail.RenderedEntity = uid;
            trail.LerpTime = 0.1f;
            trail.LerpDelay = TimeSpan.FromSeconds(2);
            trail.Lifetime = 3;
            trail.Frequency = 0.07f;
            trail.AlphaLerpAmount = 0.2f;
            trail.MaxParticleAmount = 25;
            trail.Color = new(255, 47, 0, 180);
        }

        _jitter.DoJitter(uid, TimeSpan.FromSeconds(3), true);
    }

    private void OnDamageModify(Entity<BerserkerImplantActiveComponent> ent, ref DamageModifyEvent args)
    {
        if (!args.Damage.AnyPositive())
            return;

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, ent.Comp.DamageModifier);
    }

    private void OnBeforeDamageChanged(Entity<BerserkerImplantActiveComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (!args.Damage.AnyPositive())
            return;

        if (!_mobState.IsAlive(ent.Owner))
            return;

        if (!_mobThreshold.TryGetThresholdForState(ent.Owner, MobState.Critical, out var threshold))
            return;

        if (!TryComp<DamageableComponent>(ent, out var damageable))
            return;

        if (_mobThreshold.CheckVitalDamage(ent, damageable) + args.Damage.GetTotal() < threshold)
            return;

        args.Cancelled = true;

        if (ent.Comp.DelayedDamage.GetTotal() < 150)    // Prevent insta gib after berserker ends
            ent.Comp.DelayedDamage += args.Damage;
    }

    private void OnStaminaDamageModify(Entity<BerserkerImplantActiveComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        args.Value *= ent.Comp.StunModifier;
    }

    private void OnGetMeleeDamage(Entity<BerserkerImplantActiveComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        args.Damage *= ent.Comp.SelfDamageModifier;
    }

    private void OnShotAttempted(Entity<BerserkerImplantActiveComponent> ent, ref ShotAttemptedEvent args)
    {
        args.Cancel();
    }
}
