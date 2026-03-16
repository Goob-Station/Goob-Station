using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Content.Shared.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Goobstation.Common.Medical;
using Robust.Shared.Audio;


namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;

public sealed partial class WoundSystem
{
    /// <summary>
    /// Destroys an entity's body part if conditions are met.
    /// </summary>
    /// <param name="parentWoundableEntity">Parent of the woundable entity.</param>
    /// <param name="woundableEntity">The entity containing the vulnerable body part</param>
    /// <param name="woundableComp">Woundable component of woundableEntity.</param>
    public void DestroyWoundable(EntityUid parentWoundableEntity, EntityUid woundableEntity, WoundableComponent woundableComp)
    {
        if (!TryComp<BodyPartComponent>(woundableEntity, out var bodyPart))
            return;

        if (bodyPart.Body == null)
        {
            DropWoundableOrgans(woundableEntity, woundableComp);
            PredictedQueueDel(woundableEntity);
        }
        else
        {
            var body = bodyPart.Body.Value;
            var key = bodyPart.ToHumanoidLayers();
            if (key == null)
                return;

            // if wounds amount somehow changes it triggers an enumeration error. owch
            woundableComp.WoundableSeverity = WoundableSeverity.Severed;

            if (TryComp<TargetingComponent>(body, out var targeting))
            {
                targeting.BodyStatus = GetWoundableStatesOnBodyPainFeels(body);
                Dirty(body, targeting);

                if (_net.IsServer)
                    RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(body)), body);
            }

            _audio.PlayPvs(woundableComp.WoundableDestroyedSound, body);
            UpdateWoundableAppearance(woundableEntity);

            if (TryInduceWound(parentWoundableEntity, "Blunt", 0f, out var woundInduced))
            {
                var traumaInflicter = EnsureComp<TraumaInflicterComponent>(woundInduced.Value.Owner);

                _trauma.AddTrauma(
                    parentWoundableEntity,
                    (parentWoundableEntity, Comp<WoundableComponent>(parentWoundableEntity)),
                    (woundInduced.Value.Owner, traumaInflicter),
                    TraumaType.Dismemberment,
                    15f,
                    (bodyPart.PartType, bodyPart.Symmetry));

                ApplyDismembermentBleeding(woundInduced.Value.Owner);
            }

            Dirty(woundableEntity, woundableComp);

            if (IsWoundableRoot(woundableEntity, woundableComp))
                return;

            if (!_container.TryGetContainingContainer(parentWoundableEntity, woundableEntity, out var container))
                return;

            if (TryComp<InventoryComponent>(body, out var inventory) // Prevent error for non-humanoids
                && _body.GetBodyPartCount(body, bodyPart.PartType) == 1
                && _body.TryGetPartSlotContainerName(bodyPart.PartType, out var containerNames))
            {
                foreach (var containerName in containerNames)
                    _inventory.DropSlotContents(body, containerName, inventory);
            }
            var bodyPartId = container.ID;

            // Prevent anomalous behaviour
            if (bodyPart.PartType is BodyPartType.Hand or BodyPartType.Arm)
                _hands.TryDrop(body, woundableEntity);

            DropWoundableOrgans(woundableEntity, woundableComp);

            ApplyDismembermentBleeding(parentWoundableEntity);
            _body.DetachPart(parentWoundableEntity, bodyPartId.Remove(0, 15), woundableEntity);
            DestroyWoundableChildren(woundableEntity, woundableComp);

            PredictedQueueDel(woundableEntity);
        }
    }

    /// <summary>
    /// Amputates (not destroys) an entity's body part if conditions are met.
    /// </summary>
    /// <param name="parentWoundableEntity">Parent of the woundable entity. Yes.</param>
    /// <param name="woundableEntity">The entity containing the vulnerable body part</param>
    /// <param name="woundableComp">Woundable component of woundableEntity.</param>
    public void AmputateWoundable(EntityUid parentWoundableEntity, EntityUid woundableEntity, WoundableComponent? woundableComp = null)
    {
        if (!Resolve(woundableEntity, ref woundableComp)
            || _timing.ApplyingState)
            return;


        var bodyPart = Comp<BodyPartComponent>(parentWoundableEntity);
        if (bodyPart.Body is not { } body
            || !woundableComp.CanRemove)
            return;

        _audio.PlayPvs(woundableComp.WoundableDelimbedSound, bodyPart.Body.Value);

        var ampEv = new BeforeAmputationDamageEvent();
        RaiseLocalEvent(bodyPart.Body.Value, ref ampEv);

        if (woundableComp.DamageOnAmputate != null
            && _body.TryGetRootPart(bodyPart.Body.Value, out var rootPart)
            && !ampEv.Cancelled) // goob edit
            _damageable.TryChangeDamage(bodyPart.Body.Value,
                woundableComp.DamageOnAmputate,
                targetPart: _body.GetTargetBodyPart(rootPart));

        AmputateWoundableSafely(parentWoundableEntity, woundableEntity);

        if (TryComp<WoundableComponent>(parentWoundableEntity, out var parentWoundable)
            && parentWoundable.CanBleed)
        {
            foreach (var wound in GetWoundableWounds(parentWoundableEntity))
            {
                if (!TryComp<BleedInflicterComponent>(wound, out var bleeds))
                    continue;

                bleeds.BleedingAmountRaw += 20f;
                bleeds.Scaling = 1f;
                bleeds.ScalingLimit = 1f;
                bleeds.IsBleeding = true;
            }
        }


        if (!_net.IsServer)
            return;

        _throwing.TryThrow(
            woundableEntity,
            _random.NextAngle().ToWorldVec() * _random.NextFloat(0.8f, 5f),
            _random.NextFloat(0.5f, 1f),
            pushbackRatio: 0.3f
        );
    }

    /// <summary>
    /// Does whatever AmputateWoundable does, but does it without pain and the other mess.
    /// </summary>
    /// <param name="parentWoundableEntity">Parent of the woundable entity. Yes.</param>
    /// <param name="woundableEntity">The entity containing the vulnerable body part</param>
    /// <param name="woundableComp">Woundable component of woundableEntity.</param>
    public void AmputateWoundableSafely(EntityUid parentWoundableEntity,
        EntityUid woundableEntity,
        WoundableComponent? woundableComp = null,
        bool amputateChildrenSafely = false)
    {
        if (!Resolve(woundableEntity, ref woundableComp)
            || !woundableComp.CanRemove)
            return;

        var bodyPart = Comp<BodyPartComponent>(parentWoundableEntity);

        if (!bodyPart.Body.HasValue
            || !_container.TryGetContainingContainer(parentWoundableEntity, woundableEntity, out var container))
            return;

        var body = bodyPart.Body.Value;
        var bodyPartId = container.ID;
        woundableComp.WoundableSeverity = WoundableSeverity.Severed;

        if (TryComp<TargetingComponent>(body, out var targeting))
        {
            targeting.BodyStatus = GetWoundableStatesOnBodyPainFeels(body);
            Dirty(body, targeting);

            if (_net.IsServer)
                RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(body)), body);
        }

        var childBodyPart = Comp<BodyPartComponent>(woundableEntity);
        if (TryComp<InventoryComponent>(bodyPart.Body, out var inventory)
            && _body.GetBodyPartCount(body, bodyPart.PartType) == 1
            && _body.TryGetPartSlotContainerName(childBodyPart.PartType, out var containerNames))
        {
            foreach (var containerName in containerNames)
                _inventory.DropSlotContents(body, containerName, inventory);
        }

        if (childBodyPart.PartType is BodyPartType.Hand or BodyPartType.Arm)
            _hands.TryDrop(body, woundableEntity);

        Dirty(woundableEntity, woundableComp);
        UpdateWoundableAppearance(woundableEntity);

        // Still does the funny popping, if the children are critted. for the funny :3
        DestroyWoundableChildren(woundableEntity, woundableComp, amputateChildrenSafely);
        _body.DetachPart(parentWoundableEntity, bodyPartId.Remove(0, 15), woundableEntity);
    }

    private void DestroyWoundableChildren(EntityUid woundableEntity,
        WoundableComponent? woundableComp = null,
        bool amputateChildrenSafely = false)
    {
        if (!Resolve(woundableEntity, ref woundableComp, false))
            return;

        foreach (var child in woundableComp.ChildWoundables)
        {
            var childWoundable = Comp<WoundableComponent>(child);
            if (childWoundable.WoundableSeverity is WoundableSeverity.Mangled)
            {
                DestroyWoundable(woundableEntity, child, childWoundable);
                continue;
            }

            if (amputateChildrenSafely)
                AmputateWoundableSafely(woundableEntity, child, childWoundable, amputateChildrenSafely);
            else
                AmputateWoundable(woundableEntity, child, childWoundable);
        }
    }

    private void DropWoundableOrgans(EntityUid woundable, WoundableComponent? woundableComp)
    {
        if (!Resolve(woundable, ref woundableComp, false))
            return;

        foreach (var organ in _body.GetPartOrgans(woundable))
        {
            if (organ.Component.OrganSeverity == OrganSeverity.Normal)
            {
                // TODO: SFX for organs getting not destroyed, but thrown out
                _body.RemoveOrgan(organ.Id, organ.Component);
                var direction = _random.NextAngle().ToWorldVec();
                var dropAngle = _random.NextFloat(0.8f, 1.2f);
                var worldRotation = _transform.GetWorldRotation(organ.Id).ToVec();

                _throwing.TryThrow(
                    organ.Id,
                    _random.NextAngle().RotateVec(direction / dropAngle + worldRotation / 50),
                    0.5f * dropAngle * _random.NextFloat(-0.9f, 1.1f),
                    doSpin: false,
                    pushbackRatio: 0
                );
            }
            else
            {
                // Destroy it
                _trauma.TrySetOrganDamageModifier(
                    organ.Id,
                    organ.Component.OrganIntegrity * 100,
                    woundable,
                    "LETMETELLYOUHOWMUCHIVECOMETOHATEYOUSINCEIBEGANTOLIVE",
                    organ.Component);
            }
        }
    }

    private void ApplyDismembermentBleeding(EntityUid target, float amount = 20f)
    {
        var bleedInflicter = EnsureComp<BleedInflicterComponent>(target);
        bleedInflicter.BleedingAmountRaw += amount;
        bleedInflicter.Scaling = 1f;
        bleedInflicter.ScalingLimit = 1f;
        bleedInflicter.IsBleeding = true;
    }

    private bool TryFumble(string message, SoundPathSpecifier sound, EntityUid body, float odds)
    {
        if (_random.NextFloat() < odds)
        {
            _popup.PopupClient(Loc.GetString(message), body, PopupType.Medium);
            var ev = new DropHandItemsEvent();
            RaiseLocalEvent(body, ref ev, false);
            _audio.PlayPredicted(sound, body, body);
            return true;
        }
        return false;
    }
}
