using Content.Server.Access.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;

namespace Content.Server._Shitcode.Heretic.EntitySystems.PathSpecific;

public sealed class EldritchIdCardSystem : SharedEldritchIdCardSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EldritchIdCardComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<EldritchIdCardComponent> ent, ref ComponentShutdown args)
    {
        if (!TerminatingOrDeleted(ent.Comp.PortalOne))
            QueueDel(ent.Comp.PortalOne);

        if (!TerminatingOrDeleted(ent.Comp.PortalTwo))
            QueueDel(ent.Comp.PortalTwo);
    }

    protected override void InitializeEldritchId(Entity<EldritchIdCardComponent> ent)
    {
        base.InitializeEldritchId(ent);

        RemCompDeferred<AgentIDCardComponent>(ent);
    }
}
