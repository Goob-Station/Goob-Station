// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Stunnable;
using Content.Shared.Damage.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Stunnable;

public sealed class OvertimeStaminaDamageSystem : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OvertimeStaminaDamageComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<OvertimeStaminaDamageComponent> ent, ref ComponentInit args)
    {
        // UNDER NO CIRCUMSTANCES ALLOW THIS SHIT TO RUN ON CLIENT
        if (_net.IsClient)
        {
            RemComp<OvertimeStaminaDamageComponent>(ent);
            return;
        }

        ent.Comp.Timer = ent.Comp.Delay;
        ent.Comp.Damage = ent.Comp.Amount;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var ents in EntityQuery<TransformComponent, OvertimeStaminaDamageComponent>())
        {
            var xform = ents.Item1;
            var overtime = ents.Item2;
            overtime.Timer -= frameTime;

            if (overtime.Timer <= 0)
            {
                Update((xform.ParentUid, overtime));
                overtime.Timer = overtime.Delay;
            }
        }
    }

    private void Update(Entity<OvertimeStaminaDamageComponent> ent)
    {
        var damage = ent.Comp.Amount / ent.Comp.Delta;

        _stamina.TakeStaminaDamage(ent, damage, immediate: false, visual: false);

        ent.Comp.Damage -= damage;

        if (ent.Comp.Damage <= 0)
            RemComp<OvertimeStaminaDamageComponent>(ent);
    }
}
