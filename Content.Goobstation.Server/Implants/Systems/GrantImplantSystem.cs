using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

/// <summary>
/// Adds implants on spawn to the entity
/// </summary>
public sealed class GrantImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedSubdermalImplantSystem _implantSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GrantImplantComponent, ComponentInit>(OnInit);
    }

    public void OnInit(EntityUid uid, GrantImplantComponent comp, ComponentInit args)
    {
        _implantSystem.AddImplants(uid, comp.Implants);
    }
}
