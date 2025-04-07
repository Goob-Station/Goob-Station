using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;
public sealed class KravMagaImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Components.KravMagaImplantComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<KravMagaComponent, ImplantRemovedFromEvent>(OnUnimplanted);
    }
    public void OnImplanted(EntityUid uid, Components.KravMagaImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted.HasValue)
            EnsureComp<KravMagaComponent>(ev.Implanted.Value);
    }

    public void OnUnimplanted(Entity<KravMagaComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (HasComp<Components.KravMagaImplantComponent>(args.Implant))
            RemComp<KravMagaComponent>(ent);
    }
}
