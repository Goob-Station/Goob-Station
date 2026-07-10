using Content.Server.Fluids.EntitySystems;
using Content.Shared._Pirate.Weapons.Melee;
using Content.Shared._Pirate.Weapons.Melee.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Content.Goobstation.Maths.FixedPoint;
using System.Linq;

namespace Content.Server._Pirate.Weapons.Melee;

public sealed class DarkShardSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DarkShardComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<DarkShardComponent, OrganAddedToBodyEvent>(OnOrganAdded);
        SubscribeLocalEvent<DarkShardComponent, OrganRemovedFromBodyEvent>(OnOrganRemoved);
        SubscribeLocalEvent<CallCursedKatanaEvent>(OnCallKatana);
    }


    private void OnUseInHand(Entity<DarkShardComponent> shard, ref UseInHandEvent args)
    {
        var user = args.User;
        args.Handled = true;

        // Already implanted — one shard per person
        if (HasComp<DarkKatanaUserComponent>(user))
        {
            _popup.PopupEntity(Loc.GetString("dark-shard-already-implanted"), user, user, PopupType.MediumCaution);
            return;
        }

        // Bloodless species cannot use it
        if (!HasComp<BloodstreamComponent>(user))
        {
            _popup.PopupEntity(Loc.GetString("dark-shard-no-blood"), user, user, PopupType.MediumCaution);
            return;
        }

        // Need a body with a chest to implant into
        if (!TryComp<OrganComponent>(shard, out var organComp))
            return;

        var chest = _body.GetBodyChildrenOfType(user, BodyPartType.Chest, symmetry: BodyPartSymmetry.None).FirstOrDefault();

        if (chest == default)
        {
            _popup.PopupEntity(Loc.GetString("dark-shard-no-chest"), user, user, PopupType.MediumCaution);
            return;
        }

        // The dark_shard slot doesn't exist on the chest by default — create it first.
        _body.TryCreateOrganSlot(chest.Id, organComp.SlotId, out _);
        if (!_body.InsertOrgan(chest.Id, shard, organComp.SlotId))
        {
            _popup.PopupEntity(Loc.GetString("dark-shard-insert-failed"), user, user, PopupType.MediumCaution);
            return;
        }
    }


    private void OnOrganAdded(Entity<DarkShardComponent> shard, ref OrganAddedToBodyEvent args)
    {
        var body = args.Body;

        if (!HasComp<BloodstreamComponent>(body))
        {
            _popup.PopupEntity(Loc.GetString("dark-shard-no-blood"), body, body, PopupType.MediumCaution);
            _body.TryRemoveOrgan(shard);
            return;
        }

        var comp = EnsureComp<DarkKatanaUserComponent>(body);
        comp.KatanaProto = shard.Comp.KatanaProto;
        comp.BloodCostPercent = shard.Comp.BloodCostPercent;
        comp.SummonSound = shard.Comp.SummonSound;
        comp.RetractSound = shard.Comp.RetractSound;

        _actions.AddAction(body, ref comp.ActionEntity, shard.Comp.ActionProto);

        _audio.PlayPvs(shard.Comp.ConsumeSound, body);
        _popup.PopupEntity(Loc.GetString("dark-shard-implanted"), body, body);
    }


    private void OnOrganRemoved(Entity<DarkShardComponent> shard, ref OrganRemovedFromBodyEvent args)
    {
        var body = args.OldBody;

        if (!TryComp<DarkKatanaUserComponent>(body, out var comp))
            return;

        if (comp.SummonedKatana is { } katana && EntityManager.EntityExists(katana))
            QueueDel(katana);

        _actions.RemoveAction(comp.ActionEntity);
        RemComp<DarkKatanaUserComponent>(body);
        _popup.PopupEntity(Loc.GetString("dark-shard-removed"), body, body);
    }


    private void OnCallKatana(CallCursedKatanaEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<DarkKatanaUserComponent>(args.Performer, out var comp))
            return;

        args.Handled = true;

        if (comp.SummonedKatana is { } existing && EntityManager.EntityExists(existing))
        {
            _audio.PlayPvs(comp.RetractSound, args.Performer);
            QueueDel(existing);
            comp.SummonedKatana = null;
            return;
        }

        // Summon.
        DrainAndSpillBlood(args.Performer, comp.BloodCostPercent);
        var katana = Spawn(comp.KatanaProto, _transform.GetMapCoordinates(args.Performer));
        if (!_hands.TryPickupAnyHand(args.Performer, katana))
        {
            _popup.PopupEntity(Loc.GetString("dark-shard-no-free-hand"), args.Performer, args.Performer);
            QueueDel(katana);
            return;
        }

        _audio.PlayPvs(comp.SummonSound, args.Performer);
        comp.SummonedKatana = katana;
    }


    private void DrainAndSpillBlood(EntityUid performer, float costPercent)
    {
        if (!TryComp<BloodstreamComponent>(performer, out var bloodstream))
            return;

        if (!_solution.ResolveSolution(performer,
                bloodstream.BloodSolutionName,
                ref bloodstream.BloodSolution,
                out var bloodSolution))
            return;

        var bloodMax = bloodstream.BloodReferenceSolution.Volume * bloodstream.MaxVolumeModifier;
        var drainAmount = FixedPoint2.New(bloodMax.Float() * costPercent);
        var currentVolume = bloodSolution.Volume;

        drainAmount = FixedPoint2.Min(drainAmount, currentVolume);
        if (drainAmount <= FixedPoint2.Zero)
            return;

        var spill = _solution.SplitSolution(bloodstream.BloodSolution.Value, drainAmount);
        _puddle.TrySpillAt(performer, spill, out _);
    }
}
