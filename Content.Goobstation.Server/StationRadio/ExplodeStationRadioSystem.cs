using Content.Goobstation.Shared.StationRadio.Components;
using Content.Goobstation.Shared.StationRadio.Events;
using Content.Server.Explosion.EntitySystems;

namespace Content.Goobstation.Server.StationRadio;

public sealed class ExplodeStationRadioSystem : EntitySystem
{

    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StationRadioReceiverComponent, StationRadioExplodeEvent>(OnExplode);
    }

    private void OnExplode(EntityUid uid, StationRadioReceiverComponent component, StationRadioExplodeEvent args)
    {
        _explosionSystem.QueueExplosion(uid, "HardBomb", 50f, 1f, 10f, canCreateVacuum: false);
    }
}
