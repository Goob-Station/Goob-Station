using Content.Pirate.Shared.Stains.Components;
using Content.Pirate.Shared.Stains.Systems;
using Content.Pirate.Shared.WashingMachine;
using Content.Pirate.Shared.Wetness.Components;
using Content.Pirate.Shared.Wetness.Systems;
using Content.Server.Forensics;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Destructible;
using Content.Shared.Forensics.Components;
using Content.Shared.Storage.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Pirate.Server.WashingMachine;

public sealed class WashingMachineSystem : SharedWashingMachineSystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;
    [Dependency] private readonly SharedStainSystem _stains = null!;
    [Dependency] private readonly SharedWetnessSystem _wetness = null!;
    [Dependency] private readonly ForensicsSystem _forensics = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly IPrototypeManager _proto = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly ReactiveSystem _reactive = null!;

    /// <summary>How long after a wash cycle wearing a laundered inner uniform grants the buff.</summary>
    private static readonly TimeSpan FreshLaundryWindow = TimeSpan.FromMinutes(5);
    private static readonly SoundSpecifier HitSound = new SoundCollectionSpecifier("MetalThud");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WashingMachineComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<WashingMachineComponent, BreakageEventArgs>(OnBreak);
    }

    private void OnMapInit(Entity<WashingMachineComponent> ent, ref MapInitEvent args)
    {
        Appearance.SetData(ent.Owner, WashingMachineVisuals.State, ent.Comp.State);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<WashingMachineComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.State != WashingMachineState.Washing)
                continue;

            if (Timing.CurTime >= comp.WashFinishTime)
            {
                FinishWash(uid, comp);
                continue;
            }

            ProcessWashingHazards(uid, comp, frameTime);
        }
    }

    private void ProcessWashingHazards(EntityUid uid, WashingMachineComponent comp, float frameTime)
    {
        if (!TryComp<EntityStorageComponent>(uid, out var storage) || storage.Contents.ContainedEntities.Count == 0)
            return;

        var bluntProto = _proto.Index<DamageTypePrototype>("Blunt");
        var damage = new DamageSpecifier(bluntProto, comp.BluntDamagePerSecond * frameTime);

        var waterSpray = new Solution();
        waterSpray.AddReagent(comp.WaterSprayReagent, comp.WaterSprayAmount);

        var sprayWater = _random.Prob(comp.WaterSprayChance * frameTime);
        var hasHeavyItems = false;

        foreach (var item in storage.Contents.ContainedEntities)
        {
            if (sprayWater)
                _reactive.DoEntityReaction(item, waterSpray, ReactionMethod.Touch);

            // Clothing does not take tumble damage.
            if (HasComp<ClothingComponent>(item))
                continue;

            hasHeavyItems = true;
            _damageable.TryChangeDamage(item, damage, true);
        }

        if (!hasHeavyItems)
            return;

        if (_random.Prob(comp.ThumpSoundChance * frameTime))
            Audio.PlayPvs(HitSound, uid);
    }

    protected override bool TryStartWash(Entity<WashingMachineComponent> ent, EntityUid user)
    {
        if (!base.TryStartWash(ent, user))
            return false;

        ent.Comp.AudioStream = Audio.PlayPvs(ent.Comp.WashLoopSound, ent.Owner)?.Entity;
        return true;
    }

    private void FinishWash(EntityUid uid, WashingMachineComponent comp)
    {
        comp.State = WashingMachineState.Idle;
        comp.WashFinishTime = null;
        comp.NextWashAllowed = Timing.CurTime + comp.Cooldown;

        Audio.Stop(comp.AudioStream);
        Audio.PlayPvs(comp.WashFinishedSound, uid);
        Appearance.SetData(uid, WashingMachineVisuals.State, WashingMachineState.Idle);

        HashSet<EntityUid> items = new();
        if (TryComp<EntityStorageComponent>(uid, out var storage))
        {
            items = storage.Contents.ContainedEntities.ToHashSet();
            foreach (var item in items)
            {
                if (!TryComp<StainableComponent>(item, out var stain) ||
                    !_solution.TryGetSolution(item, stain.SolutionName, out var sol))
                {
                    continue;
                }

                if (TryComp<ForensicsComponent>(uid, out var machineForensics))
                    machineForensics.DNAs.UnionWith(_forensics.GetSolutionsDNA(sol.Value.Comp.Solution));

                _solution.RemoveAllSolution(sol.Value);
                _stains.UpdateVisuals((item, stain));
            }

            // Finished laundry is clean and dry.
            foreach (var item in items)
            {
                if (TryComp<WettableComponent>(item, out var wettable))
                    _wetness.DryFully((item, wettable));

                // Inner uniforms can grant the fresh-laundry buff.
                if (TryComp<ClothingComponent>(item, out var clothing) && (clothing.Slots & SlotFlags.INNERCLOTHING) != 0)
                {
                    var fresh = EnsureComp<FreshLaundryComponent>(item);
                    fresh.Expiry = Timing.CurTime + FreshLaundryWindow;
                }
            }
        }

        var machineEv = new WashingMachineFinishedWashingEvent(items);
        RaiseLocalEvent(uid, machineEv);

        var itemEv = new WashingMachineWashedEvent(uid, items);
        foreach (var item in items)
        {
            RaiseLocalEvent(item, itemEv);
        }

        UpdateForensics((uid, comp), items);

        Dirty(uid, comp);
        Storage.OpenStorage(uid);
    }

    protected override void UpdateForensics(Entity<WashingMachineComponent> ent, HashSet<EntityUid> items)
    {
        if (!TryComp<ForensicsComponent>(ent.Owner, out var forensics))
            return;

        foreach (var item in items)
        {
            if (!TryComp<FiberComponent>(item, out var fiber))
                continue;

            var fiberText = fiber.FiberColor == null
                ? Loc.GetString("forensic-fibers", ("material", fiber.FiberMaterial))
                : Loc.GetString("forensic-fibers-colored", ("color", fiber.FiberColor), ("material", fiber.FiberMaterial));

            forensics.Fibers.Add(fiberText);
        }
    }

    private void OnBreak(Entity<WashingMachineComponent> ent, ref BreakageEventArgs args)
    {
        ent.Comp.State = WashingMachineState.Broken;
        ent.Comp.WashFinishTime = null;
        Audio.Stop(ent.Comp.AudioStream);
        Dirty(ent.Owner, ent.Comp);
        Appearance.SetData(ent.Owner, WashingMachineVisuals.State, WashingMachineState.Broken);
    }
}
