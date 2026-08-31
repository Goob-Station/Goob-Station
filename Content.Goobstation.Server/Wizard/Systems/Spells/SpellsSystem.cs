using System.Linq;
using Content.Goobstation.CommonShared.Wizard.Components;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Systems.Spells;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Hands.Systems;
using Content.Server.Spreader;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Damage;
using Content.Shared.Friction;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed partial class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly DivineInterventionSystem _divineIntervention = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly TileFrictionController _tileFrictionController = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<TimedDespawnComponent> _timedDespawnQuery;
    private EntityQuery<FadingTimedDespawnComponent> _fadingTimedDespawnQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _handsQuery = GetEntityQuery<HandsComponent>();
        _timedDespawnQuery = GetEntityQuery<TimedDespawnComponent>();
        _fadingTimedDespawnQuery = GetEntityQuery<FadingTimedDespawnComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
    }

    private IEnumerable<MapCoordinates> GetSpawnCoordinatesAroundPerformer(EntityUid performer,
        float range,
        int amount,
        Angle angle,
        int collisionMask)
    {
        var xform = Transform(performer);
        var (pos, rot) = _xform.GetWorldPositionRotation(xform);

        var positions = _gun.LinearSpread(rot - angle, rot + angle, amount)
            .Select(x => new MapCoordinates(pos + x.ToWorldVec() * range, xform.MapID));

        foreach (var position in positions)
        {
            var dir = (position.Position - pos).Normalized();

            var ray = new CollisionRay(pos, dir, collisionMask);

            var result = _physics.IntersectRay(xform.MapID, ray, range, performer).FirstOrNull();

            if (result != null)
                yield return new MapCoordinates(result.Value.HitPos, xform.MapID);
            else
                yield return position;
        }
    }
}