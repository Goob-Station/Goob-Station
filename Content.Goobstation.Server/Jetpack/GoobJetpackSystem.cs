using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Jetpack;

public sealed class GoobJetpackSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedJetpackSystem _jetpack = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private EntityQuery<WoundableComponent> _woundableQuery;

    private readonly Queue<(EntityUid Jetpack, EntityUid User)> _pendingFlings = new();

    public override void Initialize()
    {
        base.Initialize();
        _woundableQuery = GetEntityQuery<WoundableComponent>();

        SubscribeLocalEvent<ActiveJetpackComponent, ComponentStartup>(OnActiveEquip);
        SubscribeLocalEvent<ActiveJetpackComponent, GotEquippedHandEvent>(OnActiveEquip);
    }

    private void OnActiveEquip<T>(EntityUid uid, ActiveJetpackComponent comp, T args)
    {
        HandleJetpackInhands(uid);
    }

    private void HandleJetpackInhands(EntityUid jetpack)
    {
        if (!TryComp<JetpackComponent>(jetpack, out var jetpackComp) || jetpackComp.JetpackUser == null)
            return;

        var user = jetpackComp.JetpackUser.Value;

        if (!_hands.IsHolding(user, jetpack, out _))
            return;

        _pendingFlings.Enqueue((jetpack, user));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        while (_pendingFlings.TryDequeue(out var pending))
        {
            var (jetpack, user) = pending;

            if (TerminatingOrDeleted(jetpack) || TerminatingOrDeleted(user))
                continue;

            if (!TryComp<JetpackComponent>(jetpack, out var jetpackComp))
                continue;

            if (!_hands.IsHolding(user, jetpack, out var handId))
                continue;

            if (HasComp<UnremoveableComponent>(jetpack))
                SmartassCountermeasure(jetpack, jetpackComp, user, handId);
            else
                Fling(jetpack, jetpackComp, user);
        }
    }

    private void Fling(EntityUid jetpack, JetpackComponent _, EntityUid user)
    {
        _popup.PopupEntity(Loc.GetString("jetpack-inhand-fly-away"), user, user, PopupType.SmallCaution);

        _hands.TryDrop(user, jetpack);
        _throwing.TryThrow(jetpack, _random.NextAngle().ToVec(), 8f, user);
    }

    // You're not getting away with gluing your jetpack
    private void SmartassCountermeasure(EntityUid jetpack, JetpackComponent jetpackComp, EntityUid user, string handId)
    {
        EntityUid? handPart = null;
        foreach (var part in _body.GetBodyChildrenOfType(user, BodyPartType.Hand))
        {
                // NOTE: Open git blame on the function below and see who made it static
            if (SharedBodySystem.GetPartSlotContainerId(part.Component.ParentSlot?.Id ?? "") == handId)
            {
                handPart = part.Id;
                break;
            }
        }

        if (handPart == null)
            return;

        if (!_woundableQuery.TryGetComponent(handPart.Value, out var handWoundable)
            || handWoundable.ParentWoundable == null)
            return;

        var armId = handWoundable.ParentWoundable.Value;

        if (!_woundableQuery.TryGetComponent(armId, out var armWoundable)
            || armWoundable.ParentWoundable == null)
            return;

        _jetpack.SetEnabled(jetpack, jetpackComp, false, user);
        // Is still needs to drop though, if we amputate with an unremovable item - it just gets deleted
        RemComp<UnremoveableComponent>(jetpack);
        _hands.TryDrop(user, jetpack, checkActionBlocker: false);

        _popup.PopupEntity(Loc.GetString("jetpack-inhand-arm-torn"), user, user, PopupType.LargeCaution);

        _wound.AmputateWoundable(armWoundable.ParentWoundable.Value, armId, armWoundable);
    }
}
