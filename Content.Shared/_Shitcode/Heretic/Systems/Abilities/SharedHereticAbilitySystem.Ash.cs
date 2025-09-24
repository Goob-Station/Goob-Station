using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.DoAfter;
using Content.Shared.Heretic;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeAsh()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticVolcanoBlast>(OnVolcanoBlast);
    }

    private void OnVolcanoBlast(Entity<HereticComponent> ent, ref EventHereticVolcanoBlast args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (!Status.TrySetStatusEffectDuration(ent,
                SharedFireBlastSystem.FireBlastStatusEffect,
                TimeSpan.FromSeconds(2)))
            return;

        var fireBlasted = EnsureComp<FireBlastedComponent>(ent);
        fireBlasted.Damage = -2f;

        if (ent.Comp is not { Ascended: true, CurrentPath: "Ash" })
            return;

        fireBlasted.MaxBounces *= 2;
        fireBlasted.BeamTime *= 0.66f;
    }
}
