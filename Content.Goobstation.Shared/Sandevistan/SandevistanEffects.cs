using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Sandevistan;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class SandevistanEffect
{
    public abstract void Effect(EntityUid uid, SandevistanUserComponent comp, IEntityManager entityManager, float frameTime);
}

public sealed partial class SandevistanJitterEffect : SandevistanEffect
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    public override void Effect(EntityUid uid, SandevistanUserComponent comp, IEntityManager entityManager, float frameTime)
        => entityManager.System<SharedJitteringSystem>().DoJitter(uid, Duration, true);
}

public sealed partial class SandevistanStaminaDamageEffect : SandevistanEffect
{
    [DataField]
    public float Damage = 5f;

    public override void Effect(EntityUid uid, SandevistanUserComponent comp, IEntityManager entityManager, float frameTime)
        => entityManager.System<SharedStaminaSystem>().TakeStaminaDamage(uid, Damage * frameTime);
}

public sealed partial class SandevistanDamageEffect : SandevistanEffect
{
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Blunt", 6.5 },
        },
    };

    public override void Effect(EntityUid uid, SandevistanUserComponent comp, IEntityManager entityManager, float frameTime)
        => entityManager.System<DamageableSystem>().TryChangeDamage(uid, Damage * frameTime, ignoreResistances: true);
}

public sealed partial class SandevistanKnockdownEffect : SandevistanEffect
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    public override void Effect(EntityUid uid, SandevistanUserComponent comp, IEntityManager entityManager, float frameTime)
        => entityManager.System<SharedStunSystem>().TryKnockdown(uid, Duration, true);
}

public sealed partial class SandevistanDisableEffect : SandevistanEffect
{
    [DataField]
    public SoundSpecifier? OverloadSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_overload.ogg");

    [DataField]
    public TimeSpan GlitchDuration = TimeSpan.FromSeconds(3);

    public override void Effect(EntityUid uid, SandevistanUserComponent comp, IEntityManager entityManager, float frameTime)
    {
        if (IoCManager.Resolve<INetManager>().IsServer)
        {
            var glitchComp = entityManager.EnsureComponent<SandevistanGlitchComponent>(uid);
            glitchComp.ExpiresAt = IoCManager.Resolve<IGameTiming>().CurTime + GlitchDuration;
            entityManager.Dirty(uid, glitchComp);
            entityManager.System<SharedAudioSystem>().PlayPvs(OverloadSound, uid);
        }

        entityManager.System<SandevistanSystem>().Disable(uid, comp);
    }
}

public sealed partial class SandevistanDeathEffect : SandevistanEffect
{
    public override void Effect(EntityUid uid, SandevistanUserComponent comp, IEntityManager entityManager, float frameTime)
        => entityManager.System<MobStateSystem>().ChangeMobState(uid, MobState.Dead);
}

