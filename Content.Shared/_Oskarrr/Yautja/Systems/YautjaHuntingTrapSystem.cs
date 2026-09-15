using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Armable;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Inventory;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Pulling.Events;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

using Content.Shared._Oskarrr.Yautja.Components;

namespace Content.Shared._Oskarrr.Yautja.Systems;

public sealed class YautjaHuntingTrapSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StepTriggerSystem _stepTrigger = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly WoundSystem _wound = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<YautjaHuntingTrapComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<YautjaHuntingTrapComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<YautjaHuntingTrapComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<YautjaHuntingTrapComponent, BeingPulledAttemptEvent>(OnBeingPulled);
        SubscribeLocalEvent<YautjaHuntingTrapComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<YautjaHuntingTrapComponent, TriggerEvent>(OnTrigger);
        SubscribeLocalEvent<YautjaHuntingTrapComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStepTriggerAttempt(Entity<YautjaHuntingTrapComponent> ent, ref StepTriggerAttemptEvent args)
    {
        if (ent.Comp.Used)
            return;

        if (!TryComp<ItemToggleComponent>(ent, out var toggle))
            return;

        if (IsProtectedByYautjaGreaves(args.Tripper))
        {
            args.Cancelled = true;
            return;
        }

        args.Continue |= toggle.Activated;
    }

    private void OnActivateAttempt(Entity<YautjaHuntingTrapComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (!ent.Comp.Used)
            return;

        args.Cancelled = true;
        args.Popup = Loc.GetString("yautja-hunting-trap-already-used");
    }

    private void OnToggled(Entity<YautjaHuntingTrapComponent> ent, ref ItemToggledEvent args)
    {
        if (!args.Activated || ent.Comp.Used)
            return;

        if (args.User is { } user)
            _hands.TryDrop(user, ent.Owner, checkActionBlocker: false);

        if (TryComp<PullableComponent>(ent, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(ent, pullable);

        var xform = Transform(ent);
        if (!xform.Anchored)
            _transform.AnchorEntity(ent, xform);

        _physics.SetBodyType(ent, BodyType.Static);
    }

    private void OnBeingPulled(Entity<YautjaHuntingTrapComponent> ent, ref BeingPulledAttemptEvent args)
    {
        if (IsArmedActive(ent))
            args.Cancel();
    }

    private void OnPullAttempt(Entity<YautjaHuntingTrapComponent> ent, ref PullAttemptEvent args)
    {
        if (IsArmedActive(ent))
            args.Cancelled = true;
    }

    private void OnExamined(Entity<YautjaHuntingTrapComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || !ent.Comp.Used)
            return;

        args.PushMarkup(Loc.GetString("yautja-hunting-trap-examine-used"));
    }

    private void OnTrigger(Entity<YautjaHuntingTrapComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && args.Key != TriggerSystem.DefaultTriggerKey)
            return;

        if (ent.Comp.Used)
            return;

        if (args.User is not { } tripper || TerminatingOrDeleted(tripper))
            return;

        if (!_net.IsServer)
            return;

        _damageable.TryChangeDamage(
            tripper,
            ent.Comp.TriggerDamage,
            ignoreResistances: true,
            origin: ent.Owner,
            targetPart: TargetBodyPart.FullLegs,
            interruptsDoAfters: true);

        TryAmputateLowerLimb(tripper);
        MarkUsed(ent);
        args.Handled = true;
    }

    private void MarkUsed(Entity<YautjaHuntingTrapComponent> ent)
    {
        ent.Comp.Used = true;
        Dirty(ent);

        _stepTrigger.SetActive(ent, false);
        _itemToggle.SetOnActivate(ent.Owner, false);
        RemCompDeferred<ArmableComponent>(ent.Owner);

        var xform = Transform(ent);
        if (xform.Anchored)
            _transform.Unanchor(ent, xform);

        _physics.SetBodyType(ent, BodyType.Dynamic);

        _popup.PopupEntity(Loc.GetString("yautja-hunting-trap-sprung"), ent, PopupType.SmallCaution);
    }

    private void TryAmputateLowerLimb(EntityUid body)
    {
        var feet = _body.GetBodyChildrenOfType(body, BodyPartType.Foot).ToList();
        if (feet.Count > 0)
        {
            var foot = _random.Pick(feet);
            if (_body.TryGetParentBodyPart(foot.Id, out var parent, out _))
                _wound.AmputateWoundable(parent.Value, foot.Id);
            return;
        }

        var legs = _body.GetBodyChildrenOfType(body, BodyPartType.Leg).ToList();
        if (legs.Count == 0)
            return;

        var leg = _random.Pick(legs);
        if (_body.TryGetParentBodyPart(leg.Id, out var legParent, out _))
            _wound.AmputateWoundable(legParent.Value, leg.Id);
    }

    private bool IsArmedActive(Entity<YautjaHuntingTrapComponent> ent)
    {
        return !ent.Comp.Used
               && TryComp(ent, out ItemToggleComponent? toggle)
               && toggle.Activated;
    }

    private bool IsProtectedByYautjaGreaves(EntityUid tripper)
    {
        return _inventory.TryGetSlotEntity(tripper, "feet", out var feetItem)
               && HasComp<YautjaTrapProtectionComponent>(feetItem);
    }
}
