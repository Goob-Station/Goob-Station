using Content.Goobstation.Common.Flammability;
using Content.Goobstation.Common.Temperature.Components;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Server.Atmos.Components;
using Robust.Shared.GameObjects;

namespace Content.Server.Goobstation.Devil;

public sealed class DevilFireImmunitySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DevilComponent, DevilFireImmuneEvent>(OnDevilFireImmune);
    }

    private void OnDevilFireImmune(EntityUid uid, DevilComponent comp, ref DevilFireImmuneEvent ev)
    {
        EnsureComp<SpecialHighTempImmunityComponent>(uid);
        EnsureComp<FireImmunityComponent>(uid);
        var flammable = EnsureComp<FlammableComponent>(uid);
        flammable.Damage = new();

        Dirty(uid, flammable);
    }
}
