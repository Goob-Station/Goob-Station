
using Content.Shared.Clumsy;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class ClumsyImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Components.ClumsyImplantComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<ClumsyComponent, ImplantRemovedFromEvent>(OnUnimplanted);
    }
    public void OnImplanted(EntityUid uid, Components.ClumsyImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted.HasValue)
            EnsureComp<ClumsyComponent>(ev.Implanted.Value);
    }

    public void OnUnimplanted(Entity<ClumsyComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (HasComp<Components.ClumsyImplantComponent>(args.Implant))
            RemComp<ClumsyComponent>(ent);
    }
}
