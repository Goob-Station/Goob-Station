using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Namespace has set accessors, leaving it on the default.
namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    [Dependency] private readonly SharedPopupSystem _popup = default!;
    private readonly string[] _severingDamageTypes = { "Slash", "Piercing", "Blunt" };
    private const double IntegrityJobTime = 0.005;
    private readonly JobQueue _integrityJobQueue = new(IntegrityJobTime);
    public sealed class IntegrityJob : Job<object>
    {
        private readonly SharedBodySystem _self;
        private readonly Entity<BodyPartComponent> _ent;
        public IntegrityJob(SharedBodySystem self, Entity<BodyPartComponent> ent, double maxTime, CancellationToken cancellation = default) : base(maxTime, cancellation)
        {
            _self = self;
            _ent = ent;
        }

        public IntegrityJob(SharedBodySystem self, Entity<BodyPartComponent> ent, double maxTime, IStopwatch stopwatch, CancellationToken cancellation = default) : base(maxTime, stopwatch, cancellation)
        {
            _self = self;
            _ent = ent;
        }

        protected override Task<object?> Process()
        {
            _self.ProcessIntegrityTick(_ent);

            return Task.FromResult<object?>(null);
        }
    }

    private EntityQuery<TargetingComponent> _queryTargeting;
    private void InitializeIntegrityQueue()
    {
        _queryTargeting = GetEntityQuery<TargetingComponent>();
        SubscribeLocalEvent<BodyComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
        SubscribeLocalEvent<BodyComponent, DamageModifyEvent>(OnBodyDamageModify);
        SubscribeLocalEvent<BodyPartComponent, DamageModifyEvent>(OnPartDamageModify);
        SubscribeLocalEvent<BodyPartComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void ProcessIntegrityTick(Entity<BodyPartComponent> entity)
    {
        if (!TryComp<DamageableComponent>(entity, out var damageable))
            return;

        var damage = damageable.TotalDamage;

        if (entity.Comp is { Body: { } body }
            && damage > entity.Comp.MinIntegrity
            && damage <= entity.Comp.IntegrityThresholds[TargetIntegrity.HeavilyWounded]
            && _queryTargeting.HasComp(body)
            && !_mobState.IsDead(body))
            _damageable.TryChangeDamage(entity, GetHealingSpecifier(entity), canSever: false, targetPart: GetTargetBodyPart(entity));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _integrityJobQueue.Process();

        if (!_timing.IsFirstTimePredicted)
            return;

        using var query = EntityQueryEnumerator<BodyPartComponent>();
        while (query.MoveNext(out var ent, out var part))
        {
            part.HealingTimer += frameTime;

            if (part.HealingTimer >= part.HealingTime)
            {
                part.HealingTimer = 0;
                _integrityJobQueue.EnqueueJob(new IntegrityJob(this, (ent, part), IntegrityJobTime));
            }
        }
    }

    private void OnBeforeDamageChanged(Entity<BodyComponent> ent, ref BeforeDamageChangedEvent args)
    {
        // If our target has a TargetingComponent, that means they will take limb damage
        // And if their attacker also has one, then we use that part.
        if (_queryTargeting.TryComp(ent, out var targetEnt))
        {
            var damage = args.Damage;
            TargetBodyPart? targetPart = null;

            if (args.TargetPart != null)
            {
                targetPart = args.TargetPart;
            }
            else if (args.Origin.HasValue && _queryTargeting.TryComp(args.Origin.Value, out var targeter))
            {
                targetPart = targeter.Target;
                // If the target is Torso then have a 33% chance to hit another part
                if (targetPart.Value == TargetBodyPart.Torso)
                {
                    var additionalPart = GetRandomPartSpread(_random, 10);
                    targetPart = targetPart.Value | additionalPart;
                }
            }
            else
            {
                // If there's an origin in this case, that means it comes from an entity without TargetingComponent,
                // such as an animal, so we attack a random part.
                if (args.Origin.HasValue)
                {
                    targetPart = GetRandomBodyPart(ent, targetEnt);
                }
                // Otherwise we damage all parts equally (barotrauma, explosions, etc).
                else if (damage != null)
                {
                    // Division by 2 cuz damaging all parts by the same damage by default is too much.
                    damage /= 2;
                    targetPart = TargetBodyPart.All;
                }
            }

            if (targetPart == null)
                return;

            if (!TryChangePartDamage(ent, args.Damage, args.CanSever, args.CanEvade, args.PartMultiplier, targetPart.Value)
                && args.CanEvade)
            {
                if (_net.IsServer)
                    _popup.PopupEntity(Loc.GetString("surgery-part-damage-evaded", ("user", Identity.Entity(ent, EntityManager))), ent);

                args.Evaded = true;
            }
        }
    }

    private void OnBodyDamageModify(Entity<BodyComponent> bodyEnt, ref DamageModifyEvent args)
    {
        if (args.TargetPart != null)
        {
            var (targetType, _) = ConvertTargetBodyPart(args.TargetPart.Value);
            args.Damage = args.Damage * GetPartDamageModifier(targetType);
        }
    }

    private void OnPartDamageModify(Entity<BodyPartComponent> partEnt, ref DamageModifyEvent args)
    {
        if (partEnt.Comp.Body != null
            && TryComp(partEnt.Comp.Body.Value, out DamageableComponent? damageable)
            && damageable.DamageModifierSetId != null
            && Prototypes.TryIndex<DamageModifierSetPrototype>(damageable.DamageModifierSetId, out var modifierSet))
            // TODO: We need to add a check to see if the given armor covers this part to cancel or not.
            args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifierSet);

        if (Prototypes.TryIndex<DamageModifierSetPrototype>("PartDamage", out var partModifierSet))
            args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, partModifierSet);

        args.Damage = args.Damage * GetPartDamageModifier(partEnt.Comp.PartType);
    }

    private bool TryChangePartDamage(EntityUid entity,
        DamageSpecifier damage,
        bool canSever,
        bool canEvade,
        float partMultiplier,
        TargetBodyPart targetParts)
    {
        var landed = false;
        var targets = SharedTargetingSystem.GetValidParts();
        foreach (var target in targets)
        {
            if (!targetParts.HasFlag(target))
                continue;

            var (targetType, targetSymmetry) = ConvertTargetBodyPart(target);
            if (GetBodyChildrenOfType(entity, targetType, symmetry: targetSymmetry) is { } part)
            {
                if (canEvade && TryEvadeDamage(entity, GetEvadeChance(targetType)))
                    continue;

                var damageResult = _damageable.TryChangeDamage(part.FirstOrDefault().Id, damage * partMultiplier, canSever: canSever);
                if (damageResult != null && damageResult.GetTotal() != 0)
                    landed = true;
            }
        }

        return landed;
    }

    private void OnDamageChanged(Entity<BodyPartComponent> partEnt, ref DamageChangedEvent args)
    {
        if (!TryComp<DamageableComponent>(partEnt, out var damageable))
            return;

        var severed = false;
        var partIdSlot = GetParentPartAndSlotOrNull(partEnt)?.Slot;
        var delta = args.DamageDelta;

        if (args.CanSever
            && partIdSlot is not null
            && delta != null
            && !HasComp<BodyPartReattachedComponent>(partEnt)
            && !partEnt.Comp.Enabled
            && damageable.TotalDamage >= partEnt.Comp.SeverIntegrity
            && _severingDamageTypes.Any(damageType => delta.DamageDict.TryGetValue(damageType, out var value) && value > 0))
            severed = true;

        CheckBodyPart(partEnt, GetTargetBodyPart(partEnt), severed, damageable);

        if (severed)
            DropPart(partEnt);

        Dirty(partEnt, partEnt.Comp);
    }

    /// <summary>
    /// Gets the random body part rolling a number between 1 and 9, and returns
    /// Torso if the result is 9 or more. The higher torsoWeight is, the higher chance to return it.
    /// By default, the chance to return Torso is 50%.
    /// </summary>
    private static TargetBodyPart GetRandomPartSpread(IRobustRandom random, ushort torsoWeight = 9)
    {
        const int targetPartsAmount = 9;
        // 5 = amount of target parts except Torso
        return random.Next(1, targetPartsAmount + torsoWeight) switch
        {
            1 => TargetBodyPart.Head,
            2 => TargetBodyPart.RightArm,
            3 => TargetBodyPart.RightHand,
            4 => TargetBodyPart.LeftArm,
            5 => TargetBodyPart.LeftHand,
            6 => TargetBodyPart.RightLeg,
            7 => TargetBodyPart.RightFoot,
            8 => TargetBodyPart.LeftLeg,
            9 => TargetBodyPart.LeftFoot,
            _ => TargetBodyPart.Torso,
        };
    }

    public TargetBodyPart? GetRandomBodyPart(EntityUid uid, TargetingComponent? target = null)
    {
        if (!Resolve(uid, ref target))
            return null;

        var totalWeight = target.TargetOdds.Values.Sum();
        var randomValue = _random.NextFloat() * totalWeight;

        foreach (var (part, weight) in target.TargetOdds)
        {
            if (randomValue <= weight)
                return part;
            randomValue -= weight;
        }

        return TargetBodyPart.Torso; // Default to torso if something goes wrong
    }

    /// <summary>
    /// This should be called after body part damage was changed.
    /// </summary>
    protected void CheckBodyPart(
        Entity<BodyPartComponent> partEnt,
        TargetBodyPart? targetPart,
        bool severed,
        DamageableComponent? damageable = null)
    {
        if (!Resolve(partEnt, ref damageable))
            return;

        var integrity = damageable.TotalDamage;

        // KILL the body part
        if (partEnt.Comp.Enabled && integrity >= partEnt.Comp.IntegrityThresholds[TargetIntegrity.CriticallyWounded])
        {
            var ev = new BodyPartEnableChangedEvent(false);
            RaiseLocalEvent(partEnt, ref ev);
        }

        // LIVE the body part
        if (!partEnt.Comp.Enabled && integrity <= partEnt.Comp.IntegrityThresholds[partEnt.Comp.EnableIntegrity] && !severed)
        {
            var ev = new BodyPartEnableChangedEvent(true);
            RaiseLocalEvent(partEnt, ref ev);
        }

        if (_queryTargeting.TryComp(partEnt.Comp.Body, out var targeting)
            && HasComp<MobStateComponent>(partEnt.Comp.Body))
        {
            var newIntegrity = GetIntegrityThreshold(partEnt.Comp, integrity.Float(), severed);
            // We need to check if the part is dead to prevent the UI from showing dead parts as alive.
            if (targetPart is not null &&
                targeting.BodyStatus.ContainsKey(targetPart.Value) &&
                targeting.BodyStatus[targetPart.Value] != TargetIntegrity.Dead)
            {
                targeting.BodyStatus[targetPart.Value] = newIntegrity;
                if (targetPart.Value == TargetBodyPart.Torso)
                    targeting.BodyStatus[TargetBodyPart.Groin] = newIntegrity;

                Dirty(partEnt.Comp.Body.Value, targeting);
            }
            // Revival events are handled by the server, so we end up being locked to a network event.
            // I hope you like the _net.IsServer, Remuchi :)
            if (_net.IsServer)
                RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(partEnt.Comp.Body.Value)), partEnt.Comp.Body.Value);
        }
    }

    /// <summary>
    /// Gets the integrity of all body parts in the entity.
    /// </summary>
    public Dictionary<TargetBodyPart, TargetIntegrity> GetBodyPartStatus(EntityUid entityUid)
    {
        var result = new Dictionary<TargetBodyPart, TargetIntegrity>();

        if (!TryComp<BodyComponent>(entityUid, out var body))
            return result;

        foreach (var part in SharedTargetingSystem.GetValidParts())
        {
            result[part] = TargetIntegrity.Severed;
        }

        foreach (var partComponent in GetBodyChildren(entityUid, body))
        {
            var targetBodyPart = GetTargetBodyPart(partComponent.Component.PartType, partComponent.Component.Symmetry);

            if (targetBodyPart != null && TryComp<DamageableComponent>(partComponent.Id, out var damageable))
                result[targetBodyPart.Value] = GetIntegrityThreshold(partComponent.Component, damageable.TotalDamage.Float(), false);
        }

        // Hardcoded shitcode for Groin :)
        result[TargetBodyPart.Groin] = result[TargetBodyPart.Torso];

        return result;
    }

    public TargetBodyPart? GetTargetBodyPart(Entity<BodyPartComponent> part) => GetTargetBodyPart(part.Comp.PartType, part.Comp.Symmetry);
    public TargetBodyPart? GetTargetBodyPart(BodyPartComponent part) => GetTargetBodyPart(part.PartType, part.Symmetry);

    /// <summary>
    /// Converts Enums from BodyPartType to their Targeting system equivalent.
    /// </summary>
    public TargetBodyPart? GetTargetBodyPart(BodyPartType type, BodyPartSymmetry symmetry)
    {
        return (type, symmetry) switch
        {
            (BodyPartType.Head, _) => TargetBodyPart.Head,
            (BodyPartType.Torso, _) => TargetBodyPart.Torso,
            (BodyPartType.Arm, BodyPartSymmetry.Left) => TargetBodyPart.LeftArm,
            (BodyPartType.Arm, BodyPartSymmetry.Right) => TargetBodyPart.RightArm,
            (BodyPartType.Hand, BodyPartSymmetry.Left) => TargetBodyPart.LeftHand,
            (BodyPartType.Hand, BodyPartSymmetry.Right) => TargetBodyPart.RightHand,
            (BodyPartType.Leg, BodyPartSymmetry.Left) => TargetBodyPart.LeftLeg,
            (BodyPartType.Leg, BodyPartSymmetry.Right) => TargetBodyPart.RightLeg,
            (BodyPartType.Foot, BodyPartSymmetry.Left) => TargetBodyPart.LeftFoot,
            (BodyPartType.Foot, BodyPartSymmetry.Right) => TargetBodyPart.RightFoot,
            _ => null
        };
    }

    /// <summary>
    /// Converts Enums from Targeting system to their BodyPartType equivalent.
    /// </summary>
    public (BodyPartType Type, BodyPartSymmetry Symmetry) ConvertTargetBodyPart(TargetBodyPart targetPart)
    {
        return targetPart switch
        {
            TargetBodyPart.Head => (BodyPartType.Head, BodyPartSymmetry.None),
            TargetBodyPart.Torso => (BodyPartType.Torso, BodyPartSymmetry.None),
            TargetBodyPart.Groin => (BodyPartType.Torso, BodyPartSymmetry.None), // TODO: Groin is not a part type yet
            TargetBodyPart.LeftArm => (BodyPartType.Arm, BodyPartSymmetry.Left),
            TargetBodyPart.LeftHand => (BodyPartType.Hand, BodyPartSymmetry.Left),
            TargetBodyPart.RightArm => (BodyPartType.Arm, BodyPartSymmetry.Right),
            TargetBodyPart.RightHand => (BodyPartType.Hand, BodyPartSymmetry.Right),
            TargetBodyPart.LeftLeg => (BodyPartType.Leg, BodyPartSymmetry.Left),
            TargetBodyPart.LeftFoot => (BodyPartType.Foot, BodyPartSymmetry.Left),
            TargetBodyPart.RightLeg => (BodyPartType.Leg, BodyPartSymmetry.Right),
            TargetBodyPart.RightFoot => (BodyPartType.Foot, BodyPartSymmetry.Right),
            _ => (BodyPartType.Torso, BodyPartSymmetry.None)
        };

    }

    public DamageSpecifier GetHealingSpecifier(BodyPartComponent part)
    {
        var damage = new DamageSpecifier()
        {
            DamageDict = new Dictionary<string, FixedPoint2>()
            {
                { "Blunt", -part.SelfHealingAmount },
                { "Slash", -part.SelfHealingAmount },
                { "Piercing", -part.SelfHealingAmount },
                { "Heat", -part.SelfHealingAmount },
                { "Cold", -part.SelfHealingAmount },
                { "Shock", -part.SelfHealingAmount },
                { "Caustic", -part.SelfHealingAmount * 0.1}, // not much caustic healing
            }
        };

        return damage;
    }

    /// <summary>
    /// Fetches the damage multiplier for part integrity based on part types.
    /// </summary>
    /// TODO: Serialize this per body part.
    public static float GetPartDamageModifier(BodyPartType partType)
    {
        return partType switch
        {
            BodyPartType.Head => 0.5f, // 50% damage, necks are hard to cut
            BodyPartType.Torso => 1.0f, // 100% damage
            BodyPartType.Arm => 0.7f, // 70% damage
            BodyPartType.Hand => 0.7f, // 70% damage
            BodyPartType.Leg => 0.7f, // 70% damage
            BodyPartType.Foot => 0.7f, // 70% damage
            _ => 0.5f
        };
    }

    /// <summary>
    /// Fetches the TargetIntegrity equivalent of the current integrity value for the body part.
    /// </summary>
    public static TargetIntegrity GetIntegrityThreshold(BodyPartComponent component, float integrity, bool severed)
    {
        if (severed)
            return TargetIntegrity.Severed;
        else if (!component.Enabled)
            return TargetIntegrity.Disabled;

        var targetIntegrity = TargetIntegrity.Healthy;
        foreach (var threshold in component.IntegrityThresholds)
        {
            if (integrity <= threshold.Value)
                targetIntegrity = threshold.Key;
        }

        return targetIntegrity;
    }

    /// <summary>
    /// Fetches the chance to evade integrity damage for a body part.
    /// Used when the entity is not dead, laying down, or incapacitated.
    /// </summary>
    public static float GetEvadeChance(BodyPartType partType)
    {
        return partType switch
        {
            BodyPartType.Head => 0.70f,  // 70% chance to evade
            BodyPartType.Arm => 0.20f,   // 20% chance to evade
            BodyPartType.Hand => 0.20f, // 20% chance to evade
            BodyPartType.Leg => 0.20f,   // 20% chance to evade
            BodyPartType.Foot => 0.20f, // 20% chance to evade
            BodyPartType.Torso => 0f, // 0% chance to evade
            _ => 0f
        };
    }

    public bool CanEvadeDamage(EntityUid uid)
    {
        if (!TryComp<MobStateComponent>(uid, out var mobState)
            || !TryComp<StandingStateComponent>(uid, out var standingState)
            || _mobState.IsCritical(uid, mobState)
            || _mobState.IsDead(uid, mobState)
            || standingState.CurrentState == StandingState.Lying)
            return false;

        return true;
    }

    public bool TryEvadeDamage(EntityUid uid, float evadeChance)
    {
        if (!CanEvadeDamage(uid))
            return false;

        return _random.NextFloat() < evadeChance;
    }

}
