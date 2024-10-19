using Content.Shared.Damage.Systems;
using System.Text;

namespace Content.Shared.Stunnable;

public sealed partial class OvertimeStaminaDamageSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OvertimeStaminaDamageComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<OvertimeStaminaDamageComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Timer = ent.Comp.Delay;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var overtime in EntityQuery<OvertimeStaminaDamageComponent>())
        {
            overtime.Timer -= frameTime;

            if (overtime.Timer <= overtime.Delay)
            {
                var damage = overtime.Amount / overtime.Delta;

                _stamina.TakeStaminaDamage(overtime.Owner, damage, immediate: false, visual: false);

                overtime.Amount -= damage;

                overtime.Timer = overtime.Delay;
            }
        }
    }
}
