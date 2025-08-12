using Content.Goobstation.Server.Implants.Components;
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
    public void OnImplanted(Entity<ClumsyImplantComponent> clumsyImplant, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted is not { } implanted)
            return;

        EnsureComp<ClumsyComponent>(implanted);
    }

    public void OnUnimplanted(Entity<ClumsyComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (HasComp<ClumsyImplantComponent>(args.Implant))
            RemComp<ClumsyComponent>(ent);
    }
}
