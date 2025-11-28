using System.Linq;
using Content.Goobstation.Shared.Illusion;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Cloning;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Popups;
using Content.Server.Temperature.Components;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared.Body.Components;
using Content.Shared.Cloning;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Nutrition.AnimalHusbandry;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Spawners;

namespace Content.Goobstation.Server.Illusion;

public sealed class IllusionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    private static readonly ProtoId<CloningSettingsPrototype> Settings = "Illusion";
    private static readonly ProtoId<HTNCompoundPrototype> Compound = "IllusionCompound";

    public List<Type> ComponentsToRemove =
    [
        typeof(RespiratorComponent),
        typeof(BarotraumaComponent),
        typeof(HungerComponent),
        typeof(ThirstComponent),
        typeof(ReproductiveComponent),
        typeof(ReproductivePartnerComponent),
        typeof(TemperatureComponent),
        typeof(ConsciousnessComponent),
        typeof(PacifiedComponent),
        typeof(BloodstreamComponent),
    ];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IllusionOnMeleeHitComponent, MeleeHitEvent>(OnHit);

        SubscribeLocalEvent<IllusionComponent, MobStateChangedEvent>(OnStateChanged);
        SubscribeLocalEvent<IllusionComponent, TimedDespawnEvent>(OnDespawn);
    }

    private void OnDespawn(Entity<IllusionComponent> ent, ref TimedDespawnEvent args)
    {
        DeathPopup(ent);
    }

    private void OnStateChanged(Entity<IllusionComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
            return;

        DeathPopup(ent);
        QueueDel(ent);
    }

    private void DeathPopup(Entity<IllusionComponent> ent)
    {
        _popup.PopupCoordinates(Loc.GetString(ent.Comp.DeathMessage, ("ent", Identity.Entity(ent, EntityManager))),
            Transform(ent).Coordinates,
            PopupType.SmallCaution);
    }

    private void OnHit(Entity<IllusionOnMeleeHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || !ShouldSpawnIllusion(ent, args.User, args.HitEntities))
            return;

        SpawnIllusion(args.User, ent.Comp.Lifetime, ent.Comp.HealthMultiplier, args.HitEntities, ent.Comp.Components);
    }

    private void SpawnIllusion(EntityUid user,
        float lifetime,
        float healthMultiplier,
        IReadOnlyList<EntityUid> targets,
        ComponentRegistry components)
    {
        if (lifetime <= 0f || healthMultiplier <= 0f)
            return;

        var xform = Transform(user);
        if (!_cloning.TryCloning(user, _xform.GetMapCoordinates(xform), Settings, out var clone))
            return;

        if (TryComp(user, out DamageableComponent? damageable) &&
            TryComp(user, out MobThresholdsComponent? thresholds) &&
            (_threshold.TryGetThresholdForState(user, MobState.Critical, out var hp, thresholds) ||
             _threshold.TryGetThresholdForState(user, MobState.Dead, out hp, thresholds)))
        {
            var damage = damageable.TotalDamage;
            var totalHp = hp - damage;
            if (totalHp <= 0)
            {
                QueueDel(clone.Value);
                return;
            }

            _threshold.SetMobStateThreshold(clone.Value, totalHp.Value * healthMultiplier, MobState.Critical);
        }

        EnsureComp<TimedDespawnComponent>(clone.Value).Lifetime = lifetime;
        EnsureComp<IllusionComponent>(clone.Value);

        foreach (var comp in ComponentsToRemove)
        {
            RemComp(clone.Value, comp);
        }

        if (TryComp(user, out HandsComponent? hands))
        {
            foreach (var held in _hands.EnumerateHeld((user, hands)))
            {
                if (!HasComp<GunComponent>(held) && !HasComp<MeleeWeaponComponent>(held))
                    continue;

                if (_cloning.CopyItem(held, xform.Coordinates, copyStorage: false) is not { } weaponClone)
                    continue;

                if (!_hands.TryPickup(clone.Value, weaponClone, null, false, false, false))
                {
                    QueueDel(weaponClone);
                    continue;
                }

                EnsureComp<UnremoveableComponent>(weaponClone);

                if (!TryComp(weaponClone, out IllusionOnMeleeHitComponent? cloneIllusionOnHit) ||
                    !TryComp(held, out IllusionOnMeleeHitComponent? illusionOnHit))
                    continue;

                cloneIllusionOnHit.Chance = illusionOnHit.Chance * illusionOnHit.DegradationRateOnClone;
                cloneIllusionOnHit.HealthMultiplier =
                    illusionOnHit.HealthMultiplier * illusionOnHit.DegradationRateOnClone;
            }
        }

        var htn = EnsureComp<HTNComponent>(clone.Value);
        htn.RootTask = new HTNCompoundTask { Task = Compound };
        _npc.SetBlackboard(clone.Value, NPCBlackboard.FollowTarget, user.ToCoordinates(), htn);

        var exception = EnsureComp<FactionExceptionComponent>(clone.Value);
        _npcFaction.IgnoreEntity((clone.Value, exception), user);
        _npcFaction.AggroEntities((clone.Value, exception), targets);

        _htn.Replan(htn);

        EntityManager.AddComponents(clone.Value, components);
    }

    private bool ShouldSpawnIllusion(Entity<IllusionOnMeleeHitComponent> ent,
        EntityUid user,
        IReadOnlyList<EntityUid> hitMobs)
    {
        if (hitMobs.Count == 0)
            return false;

        if (!_random.Prob(ent.Comp.Chance))
            return false;

        if (ent.Comp.FactionWhitelist.Count > 0)
        {
            if (!TryComp(user, out NpcFactionMemberComponent? factionComp))
                return false;

            if (!factionComp.Factions.Any(x => ent.Comp.FactionWhitelist.Contains(x)))
                return false;
        }

        var mobStateQuery = GetEntityQuery<MobStateComponent>();
        var illusionQuery = GetEntityQuery<IllusionComponent>();
        foreach (var hit in hitMobs)
        {
            if (hit == user)
                continue;

            if (illusionQuery.HasComp(hit))
                continue;

            if (!mobStateQuery.TryComp(hit, out var mobState))
                continue;

            if (mobState.CurrentState is MobState.Alive or MobState.Critical)
                return true;
        }

        return false;
    }
}
