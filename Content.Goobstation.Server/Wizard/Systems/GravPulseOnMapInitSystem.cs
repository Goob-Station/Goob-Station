using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Singularity.EntitySystems;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class GravPulseOnMapInitSystem : EntitySystem
{
    [Dependency] private readonly GravityWellSystem _gravityWell = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GravPulseOnMapInitComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<GravPulseOnMapInitComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;

        _gravityWell.GravPulse(uid,
            comp.MaxRange,
            comp.MinRange,
            comp.BaseRadialAcceleration,
            comp.BaseTangentialAcceleration);
    }
}
