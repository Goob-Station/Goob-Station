using Content.Server.Stunnable;
using Content.Shared.Damage.Systems;
using Content.Shared.Dragon;
using Content.Shared.NPC.Components;
using Content.Shared.Sprite;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Dragon;

// GOOB TODO: many of this stuff can be goobmod
public sealed partial class DragonSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ISerializationManager _serManager = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    public void InitializeGoob()
    {
        SubscribeLocalEvent<DragonComponent, DragonRoarActionEvent>(OnDragonRoar);
        SubscribeLocalEvent<DragonComponent, DragonSpawnCarpHordeActionEvent>(OnRiseFish);
    }

    private void GoobOnInit(EntityUid uid, DragonComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.SpawnCarpsActionEntity, component.SpawnCarpsAction); // Goobstation
        _actions.AddAction(uid, ref component.RoarActionEntity, component.RoarAction); // Goobstation
    }

    // GOOB TODO: there is no reason to be doing these lookups in Update() with frametime
    // this probably causes unnecessary lag i think
    private void GoobHealDragonIfNearRift(Entity<DragonComponent> dragon, float frameTime)
    {
        var coords = Transform(dragon).Coordinates;
        var range = dragon.Comp.CarpRiftHealingRange;

        var rifts = _lookup.GetEntitiesInRange<DragonRiftComponent>(coords, range);

        if (rifts.Count > 0)
            _damage.TryChangeDamage(dragon.Owner, dragon.Comp.CarpRiftHealing * frameTime, true, false);
    }

    private void OnRiseFish(EntityUid uid, DragonComponent component, DragonSpawnCarpHordeActionEvent args)
    {
        if (args.Handled)
            return;

        Roar(uid, component);
        var xform = Transform(uid);
        for (int i = 0; i < component.CarpAmount; i++)
        {
            var ent = Spawn(component.CarpProtoId, xform.Coordinates);

            // Update their look to match the leader.
            if (TryComp<RandomSpriteComponent>(uid, out var randomSprite))
            {
                var spawnedSprite = EnsureComp<RandomSpriteComponent>(ent);
                _serManager.CopyTo(randomSprite, ref spawnedSprite, notNullableOverride: true);
                Dirty(ent, spawnedSprite);
            }
        }

        args.Handled = true;
    }

    private void OnDragonRoar(EntityUid uid, DragonComponent component, DragonRoarActionEvent args)
    {
        if (args.Handled)
            return;

        Roar(uid, component);

        // TODO: add pushing (like from push horn but stronger) after upstream is merged

        var xform = Transform(uid);
        var nearMobs = _lookup.GetEntitiesInRange<NpcFactionMemberComponent>(xform.Coordinates, component.RoarRange, LookupFlags.Uncontained);
        foreach (var mob in nearMobs)
        {
            if (_faction.IsEntityFriendly(uid, (mob.Owner, mob.Comp)))
                continue;

            _stun.TryUpdateStunDuration (mob, TimeSpan.FromSeconds(component.RoarStunTime));
        }

        args.Handled = true;
    }
}