using Content.Server._Goobstation.Implants.Components;
using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared.Implants;

namespace Content.Server._Goobstation.Implants.Systems;
public sealed class KravMagaImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KravMagaImplantComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<KravMagaComponent, ImplantRemovedFromEvent>(OnUnimplanted);
    }
    public void OnImplanted(EntityUid uid, KravMagaImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted.HasValue)
            EnsureComp<KravMagaComponent>(ev.Implanted.Value);
    }

    public void OnUnimplanted(Entity<KravMagaComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (HasComp<KravMagaImplantComponent>(args.Implant))
            RemComp<KravMagaComponent>(ent);
    }
}
