using Content.Server.Flash;
using Content.Server.Hands.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Flockmind;
using Content.Server.Flockmind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Store.Components;
using Robust.Shared.Audio.Systems;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Content.Shared.Body.Systems;
using Content.Server.Medical;
using Robust.Server.GameObjects;
using Content.Shared.Stunnable;
using Robust.Shared.Map;
using Content.Shared.StatusEffect;
using Content.Server.Station.Systems;
using Content.Shared.Localizations;
using Robust.Shared.Audio;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using System.Linq;
using Robust.Server.Audio;

namespace Content.Server.Flockmind;

public sealed partial class FlockmindSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FlockmindComponent, EventSummonRift>(OnRiftSummon);
        SubscribeLocalEvent<FlockmindComponent, EventRadioStun>(OnRadioStun);
    }

    #region Flockmind abilities

    private void OnRiftSummon(EntityUid uid, FlockmindComponent comp, ref EventSummonRift args)
    {
        var transform = Transform(uid);
        var rift = EntityManager.SpawnEntity("Rift", transform.Coordinates);
        var audioSystem = EntityManager.System<AudioSystem>();
        audioSystem.PlayFromEntity("/Audio/Effects/teleport_activate.ogg", rift);
    }

    private void OnRadioStun(EntityUid uid, FlockmindComponent comp, ref OnRadioStun args)
    {
        var radius = 15.0f;
        var transform = Transform(uid);
        var lookup = _lookup.GetEntitiesInRange(transform.Coordinates, radius);

        foreach (var entity in lookup)
        {
            if (_inventory.TryGetSlotEntity(entity, "headset", out _))
            {
                _stun.TryStun(entity, TimeSpan.FromSeconds(5), true);
            }
        }
    }

    #endregion
}
