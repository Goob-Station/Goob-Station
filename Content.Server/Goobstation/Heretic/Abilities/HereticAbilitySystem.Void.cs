using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Heretic.Components;
using Content.Server.Temperature.Components;
using Content.Shared.Heretic;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    private void SubscribeVoid()
    {
        SubscribeLocalEvent<HereticComponent, HereticAristocratWayEvent>(OnAristocratWay);
        SubscribeLocalEvent<HereticComponent, HereticAscensionVoidEvent>(OnAscensionVoid);
    }

    private void OnAristocratWay(Entity<HereticComponent> ent, ref HereticAristocratWayEvent args)
    {
        RemComp<TemperatureComponent>(ent);
        RemComp<RespiratorComponent>(ent);
    }

    private void OnAscensionVoid(Entity<HereticComponent> ent, ref HereticAscensionVoidEvent args)
    {
        RemComp<BarotraumaComponent>(ent);
        EnsureComp<AristocratComponent>(ent);
    }
}
