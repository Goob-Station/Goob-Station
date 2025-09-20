using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.DoAfter;
using Content.Shared.Heretic;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeAsh()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticVolcanoBlast>(OnVolcanoBlast);

        SubscribeLocalEvent<HereticComponent, EventHereticVolcanoBlastDoAfter>(OnVolcanoBlastDoAfter);

    }

    private void OnVolcanoBlastDoAfter(Entity<HereticComponent> ent, ref EventHereticVolcanoBlastDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (Status.TrySetStatusEffectDuration(ent, SharedFireBlastSystem.FireBlastStatusEffect, TimeSpan.FromSeconds(2)))
        {
            var fireBlasted = EnsureComp<FireBlastedComponent>(ent);
            fireBlasted.Damage = -2f;

            if (ent.Comp is { Ascended: true, CurrentPath: "Ash" })
            {
                fireBlasted.MaxBounces *= 2;
                fireBlasted.BeamTime *= 0.66f;
            }
        }

        args.Handled = true;
    }

    private void OnVolcanoBlast(Entity<HereticComponent> ent, ref EventHereticVolcanoBlast args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var dargs = new DoAfterArgs(EntityManager,
            ent,
            args.ChannelTime,
            new EventHereticVolcanoBlastDoAfter(args.Radius),
            ent)
        {
            MultiplyDelay = false,
        };

        if (DoAfter.TryStartDoAfter(dargs))
            args.Handled = true;
    }
}
