using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Heretic;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeAsh()
    {
        SubscribeLocalEvent<EventHereticVolcanoBlast>(OnVolcanoBlast);
    }

    private void OnVolcanoBlast(EventHereticVolcanoBlast args)
    {
        if (!TryUseAbility(args, false))
            return;

        var ent = args.Performer;

        if (!_statusNew.TrySetStatusEffectDuration(ent,
                SharedFireBlastSystem.FireBlastStatusEffect,
                TimeSpan.FromSeconds(2)))
            return;

        args.Handled = true;

        var fireBlasted = EnsureComp<FireBlastedComponent>(ent);
        fireBlasted.Damage = -2f;

        if (!Heretic.TryGetHereticComponent(ent, out var heretic, out _) ||
            heretic is not { Ascended: true, CurrentPath: "Ash" })
            return;

        fireBlasted.MaxBounces *= 2;
        fireBlasted.BeamTime *= 0.66f;
    }
}
