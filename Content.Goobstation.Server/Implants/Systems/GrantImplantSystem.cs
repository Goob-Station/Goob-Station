using Content.Server.Implants.Components;
using Content.Shared.Implants;

namespace Content.Server.Implants.Systems;

/// <summary>
/// Adds implants on spawn to the entity
/// </summary>
public sealed class GrantImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedSubdermalImplantSystem _implantSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Goobstation.Server.Implants.Components.GrantImplantComponent, ComponentInit>(OnInit);
    }

    public void OnInit(EntityUid uid, Goobstation.Server.Implants.Components.GrantImplantComponent comp, ComponentInit args)
    {
        _implantSystem.AddImplants(uid, comp.Implants);
    }
}
