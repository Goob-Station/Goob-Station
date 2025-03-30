using Content.Goobstation.Server.Implants.Components;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class SpaceProofImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpaceProofImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<SpaceProofImplantComponent, ImplantRemovedFromEvent>(OnUnimplanted);
    }

    private void OnImplant(Entity<SpaceProofImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        var comp = ent.Comp;
        var user = args.Implanted.Value;

        if (HasComp<RespiratorComponent>(user))
        {
            RemComp<RespiratorComponent>(user);
            comp.NeededAir = true;
        }

        if (HasComp<BarotraumaComponent>(user))
        {
            RemComp<BarotraumaComponent>(user);
            comp.WasSpaceProof = true;
        }

    }

    private void OnUnimplanted(Entity<SpaceProofImplantComponent> ent, ref ImplantRemovedFromEvent args)
    {
        var user = args.Implanted;
        var comp = ent.Comp;

        if (comp.NeededAir)
            EnsureComp<RespiratorComponent>(user);
        if (comp.WasSpaceProof)
            EnsureComp<BarotraumaComponent>(user);

        if (HasComp<SpaceProofImplantComponent>(args.Implant))
            RemComp<SpaceProofImplantComponent>(ent);
    }
}
