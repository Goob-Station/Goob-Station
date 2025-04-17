using Content.Shared.Mobs.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Weapons.MissChance;

public sealed class MissChanceSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MissChanceComponent, PreventCollideEvent>(PreventCollide);
    }

    private void PreventCollide(Entity<MissChanceComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled
        || !HasComp<MobStateComponent>(args.OtherEntity)
        || !_random.Prob(ent.Comp.Chance))
            return;

        args.Cancelled = true;
    }

    public void ApplyMissChance(EntityUid? ent, float chance)
    {
        // GunShotEvent goes nuts with ammo uids on client so we tell it to stfu
        if (_netManager.IsClient || ent == null)
            return;

        var missComp = EnsureComp<MissChanceComponent>((EntityUid)ent);
        missComp.Chance = chance;
    }
}
