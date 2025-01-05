using Content.Server.Atmos.Components;
using Robust.Shared.Spawners;
using Content.Shared.Atmos;
using Content.Server.Atmos.EntitySystems;

namespace Content.Server.Atmos.EntitySystems;


/// <summary>
/// Assmos - Extinguisher Nozzle
/// Sets atmospheric temperature to 20C and removes all toxins. 
/// </summary>
public sealed class AtmosResinDespawnSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmo = default!;
    [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AtmosResinDespawnComponent, TimedDespawnEvent>(OnDespawn);
    }

    private void OnDespawn(EntityUid uid, AtmosResinDespawnComponent comp, ref TimedDespawnEvent args)
    {
        if (!TryComp(uid, out TransformComponent? xform))
            return;

        var mix = _atmo.GetContainingMixture(uid, true);

        if (mix is null) return;
        mix.AdjustMoles(Gas.CarbonDioxide, -mix.GetMoles(Gas.CarbonDioxide));
        mix.AdjustMoles(Gas.Plasma, -mix.GetMoles(Gas.Plasma));
        mix.AdjustMoles(Gas.Tritium, -mix.GetMoles(Gas.Tritium));
        mix.AdjustMoles(Gas.Ammonia, -mix.GetMoles(Gas.Ammonia));
        mix.AdjustMoles(Gas.NitrousOxide, -mix.GetMoles(Gas.NitrousOxide));
        mix.AdjustMoles(Gas.Frezon, -mix.GetMoles(Gas.Frezon));
        mix.AdjustMoles(Gas.BZ, -mix.GetMoles(Gas.BZ));
        mix.AdjustMoles(Gas.Healium, -mix.GetMoles(Gas.Healium));
        mix.AdjustMoles(Gas.Nitrium, -mix.GetMoles(Gas.Nitrium));
        mix.Temperature = Atmospherics.T20C;
        _gasOverlaySystem.UpdateSessions();
    }
}
