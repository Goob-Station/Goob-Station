using Content.Server.Atmos.Rotting;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Forensics;
using Content.Server.Popups;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Gibbing;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Content.Shared._Impstation.Consume.Components;
using Content.Shared._Impstation.Consume;
using Content.Shared.Damage.Systems;
using Content.Shared.Gibbing.Systems;

namespace Content.Server._Impstation.Consume;

public sealed class ConsumeSystem : SharedConsumeSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly ForensicsSystem _forensics = default!;
    [Dependency] private readonly IngestionSystem _ingestion = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly RottingSystem _rotting = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;

    /// <summary>
    /// How far consumed the consumed must be before they gib
    /// </summary>
    private const float GibThreshold = 3.0f;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConsumeActionComponent, ConsumeEvent>(Consume);
        SubscribeLocalEvent<ConsumeActionComponent, ConsumeDoAfterEvent>(ConsumeDoafter);
    }

    public void Consume(Entity<ConsumeActionComponent> ent, ref ConsumeEvent args)
    {
        if (!_ingestion.HasMouthAvailable(args.Performer, args.Performer))
        {
            _popup.PopupClient(Loc.GetString("consume-fail-blocked"), ent, ent);
            return;
        }

        if (!_whitelist.CheckBoth(args.Target, ent.Comp.Blacklist, ent.Comp.Whitelist))
        {
            _popup.PopupEntity(Loc.GetString("consume-fail-inedible", ("target", Identity.Entity(args.Target, EntityManager))), ent, ent);
            return;
        }

        if (!_mobState.IsIncapacitated(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("consume-fail-incapacitated", ("target", Identity.Entity(args.Target, EntityManager))), ent, ent);
            return;
        }

        PlayMeatySound(ent);

        if (!TryComp<PhysicsComponent>(args.Target, out var targetPhysics))
            return;

        if (!TryComp<PhysicsComponent>(args.Performer, out var performerPhysics))
            return;

        var doargs = new DoAfterArgs(EntityManager, ent, targetPhysics.Mass / performerPhysics.Mass * ent.Comp.BaseConsumeSpeed, new ConsumeDoAfterEvent(), ent, args.Target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd
        };

        if (ent.Comp.PopupSelfStart != null)
        {
            var popupSelf = Loc.GetString(ent.Comp.PopupSelfStart,
                ("user", Identity.Entity(ent, EntityManager)),
                ("target", Identity.Entity(args.Target, EntityManager)));
            _popup.PopupEntity(popupSelf, ent, ent);
        }

        if (ent.Comp.PopupOthersStart != null)
        {
            var popupOthers = Loc.GetString(ent.Comp.PopupOthersStart,
                ("user", Identity.Entity(ent, EntityManager)),
                ("target", Identity.Entity(args.Target, EntityManager)));
            _popup.PopupEntity(popupOthers, ent, Filter.Pvs(ent).RemovePlayersByAttachedEntity(ent), true, PopupType.MediumCaution);
        }

        _doAfter.TryStartDoAfter(doargs);
        args.Handled = true;
    }

    public void ConsumeDoafter(Entity<ConsumeActionComponent> ent, ref ConsumeDoAfterEvent args)
    {
        if (args.Target == null || args.Cancelled || !TryComp<PhysicsComponent>(args.Target, out var targetPhysics))
            return;

        if (!_body.TryGetBodyOrganEntityComps<StomachComponent>(ent.Owner, out var stomachs))
            return;

        var highestAvailable = FixedPoint2.Zero;
        Entity<StomachComponent>? stomachToUse = null;
        foreach (var stomach in stomachs)
        {
            var owner = stomach.Owner;
            if (!_solutionContainer.ResolveSolution(owner, "stomach", ref stomach.Comp1.Solution, out var stomachSol))
                continue;

            if (stomachSol.AvailableVolume <= highestAvailable)
                continue;

            stomachToUse = stomach;
            highestAvailable = stomachSol.AvailableVolume;
        }

        // All stomachs are full or we have no stomachs
        if (stomachToUse == null)
        {
            _popup.PopupClient(Loc.GetString("ingestion-you-cannot-ingest-any-more", ("verb", "eat")), ent, ent);
            return;
        }

        // Drink Bloodstream
        _solutionContainer.TryGetSolution(args.Target.Value, ent.Comp.SolutionToDrinkFrom, out var targetSolutionComp, out var targetBloodstream);
        if (targetBloodstream != null && targetSolutionComp != null)
        {
            var foodReagentQuantity = targetPhysics.Mass * ent.Comp.MeatMultiplier;

            var consumedSolution = _solutionContainer.SplitSolution(targetSolutionComp.Value, targetBloodstream.Volume * ent.Comp.PortionDrunk);

            if (_rotting.IsRotten(args.Target.Value))
            {
                consumedSolution.AddReagent(ent.Comp.Toxin, foodReagentQuantity * ent.Comp.ToxinRatio);
                foodReagentQuantity *= 1 - ent.Comp.ToxinRatio; // this math is bad i just know it
            }

            consumedSolution.AddReagent(ent.Comp.FoodReagentPrototype, foodReagentQuantity);

            if (consumedSolution.Volume > highestAvailable)
            {
                var split = consumedSolution.SplitSolution(consumedSolution.Volume - highestAvailable);
                _puddle.TrySpillAt(ent.Owner, split, out _);
            }
            _stomach.TryTransferSolution(stomachToUse.Value.Owner, consumedSolution, stomachToUse);
        }

        // Transfer DNA
        _forensics.TransferDna(args.Target.Value, ent, false);

        // Deal Damage
        _damage.TryChangeDamage(args.Target.Value, ent.Comp.Damage, true, false);

        // Play Sound
        PlayMeatySound(ent);

        if (ent.Comp.PopupSelfEnd != null)
        {
            var popupSelf = Loc.GetString(ent.Comp.PopupSelfEnd,
                ("user", Identity.Entity(ent, EntityManager)),
                ("target", Identity.Entity(args.Target.Value, EntityManager)));
            _popup.PopupEntity(popupSelf, ent, ent);
        }

        if (ent.Comp.PopupOthersEnd != null)
        {
            var popupOthers = Loc.GetString(ent.Comp.PopupOthersEnd,
                ("user", Identity.Entity(ent, EntityManager)),
                ("target", Identity.Entity(args.Target.Value, EntityManager)));
            _popup.PopupEntity(popupOthers, ent, Filter.Pvs(ent).RemovePlayersByAttachedEntity(ent), true, PopupType.MediumCaution);
        }

        //Consumed Componentry Stuff lol
        EnsureComp<ConsumedComponent>(args.Target.Value, out var consumed);

        consumed.ConsumedValue += ent.Comp.PercentageConsumed;
        Dirty(args.Target.Value, consumed);

        if (consumed.ConsumedValue >= GibThreshold && TryComp<BodyComponent>(args.Target.Value, out var targetBody) && ent.Comp.CanGib)
            _body.GibBody(args.Target.Value,true,targetBody);
    }

    public void PlayMeatySound(Entity<ConsumeActionComponent> ent)
    {
        var soundPool = new SoundCollectionSpecifier("gib");
        _audio.PlayPvs(soundPool, ent, AudioParams.Default.WithVolume(-3f));
    }
}
