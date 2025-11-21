using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Actions;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Content.Shared.Humanoid;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// System for the massacre action. Can be re-used for other similar weapons(Like the gang weapon if that ever gets made).
/// </summary>
public sealed class SlasherMassacreSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlasherMassacreMacheteComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<SlasherMassacreUserComponent, SlasherMassacreEvent>(OnMassacreAction);
        SubscribeLocalEvent<SlasherMassacreMacheteComponent, MeleeHitEvent>(OnMeleeHitWeapon);
    }

    private void OnGetItemActions(EntityUid uid, SlasherMassacreMacheteComponent comp, GetItemActionsEvent args)
    {
        EnsureComp<SlasherMassacreUserComponent>(args.User);

        if (_net.IsServer)
            args.AddAction(ref comp.MassacreActionEntity, comp.MassacreActionId);

        Dirty(uid, comp);
    }

    private void OnMassacreAction(Entity<SlasherMassacreUserComponent> ent, ref SlasherMassacreEvent args)
    {
        if (!_net.IsServer)
        {
            args.Handled = true;
            return;
        }

        if (!ent.Comp.Active)
        {
            ent.Comp.Active = true;
            ent.Comp.HitCount = 0;
            ent.Comp.CurrentVictim = null;

            _popup.PopupEntity(Loc.GetString("slasher-massacre-start"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            _audio.PlayPvs(ent.Comp.MassacreIntro, ent.Owner);

        } // better formatting :shrug:
        else
            EndChain(ent.Owner, ent.Comp, showPopup: true);

        args.Handled = true;
        Dirty(ent);
    }

    private void EndChain(EntityUid uid, SlasherMassacreUserComponent comp, bool showPopup = false)
    {
        if (comp.Active && showPopup && _net.IsServer)
            _popup.PopupEntity(Loc.GetString("slasher-massacre-end"), uid, uid, PopupType.MediumCaution);

        comp.Active = false;
        comp.HitCount = 0;
        comp.CurrentVictim = null;
        Dirty(uid, comp);
    }

    private void OnMeleeHitWeapon(Entity<SlasherMassacreMacheteComponent> weaponEnt, ref MeleeHitEvent args)
    {
        if (!TryComp<SlasherMassacreUserComponent>(args.User, out var userComp)
            || !userComp.Active) // don't activate when comp isn't active.
            return;

        // End the chain when you miss.
        if (args.HitEntities.Count == 0)
        {
            EndChain(args.User, userComp, true);
            return;
        }

        // Only consider humanoid's as targets.
        EntityUid? victim = null;
        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<HumanoidAppearanceComponent>(hit))
                continue;

            victim = hit;
            break;
        }

        // If no valid humanoid was hit, treat like a miss and end the chain.
        if (victim == null)
        {
            EndChain(args.User, userComp, true);
            return;
        }

        // When the target changes reset hitcount.
        if (userComp.CurrentVictim != null && userComp.CurrentVictim != victim.Value)
        {
            if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("slasher-massacre-target-change"), args.User, args.User, PopupType.MediumCaution);

            userComp.HitCount = 0;
        }

        userComp.CurrentVictim = victim.Value;
        userComp.HitCount++;



        // Calculate damage bonus/penalty.
        var totalBonus = -weaponEnt.Comp.BaseDamagePenalty + weaponEnt.Comp.PerHitBonus * (userComp.HitCount - 1);
        if (totalBonus != 0)
        {
            new DamageSpecifier().DamageDict.Add("Slash", totalBonus);
            args.BonusDamage += new DamageSpecifier();
        }

        // If the victim died end the chain silently.
        if (_mobState.IsDead(victim.Value))
        {
            EndChain(args.User, userComp);
            return;
        }

        var playedDelimb = false;

        // Limb severing phase.
        if (userComp.HitCount >= weaponEnt.Comp.LimbSeverHits)
        {
            if (TrySeverRandomLimb(victim.Value, chance: weaponEnt.Comp.LimbSeverChance))
                playedDelimb = true;
        }

        // Decapitation.
        if (userComp.HitCount == weaponEnt.Comp.DecapitateHit)
        {
            if (Decapitate(victim.Value))
            {
                playedDelimb = true;
                if (_net.IsServer)
                    _popup.PopupEntity(Loc.GetString("slasher-massacre-decap"), victim.Value, PopupType.Large);
            }
            EndChain(args.User, userComp);
        }

        // Audio handling
        if (_net.IsServer)
        {
            if (playedDelimb)
                _audio.PlayPvs(weaponEnt.Comp.MassacreDelimb, args.User);
            else
                _audio.PlayPvs(weaponEnt.Comp.MassacreSlash, args.User);
        }

        Dirty(args.User, userComp);
    }

    // Handles severing a random limb.
    private bool TrySeverRandomLimb(EntityUid victim, float chance)
    {
        if (!_random.Prob(chance))
            return false;

        var parts = _body.GetBodyChildren(victim);
        var severable = new List<EntityUid>();

        foreach (var part in parts)
        {
            if (part.Component.PartType is BodyPartType.Arm or BodyPartType.Leg)
                severable.Add(part.Id);
        }

        if (severable.Count == 0)
            return false;

        var pickedLimb = _random.Pick(severable);

        if (!TryComp<WoundableComponent>(pickedLimb, out var limbWoundable) || !limbWoundable.ParentWoundable.HasValue)
            return false;

        _wounds.AmputateWoundableSafely(limbWoundable.ParentWoundable.Value, pickedLimb, limbWoundable);

        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString("slasher-massacre-limb"), victim, PopupType.Medium);
        return true;
    }

    // Handles decapitation.
    private bool Decapitate(EntityUid victim)
    {
        var parts = _body.GetBodyChildren(victim);
        EntityUid? head = null;
        EntityUid? chest = null;
        foreach (var part in parts)
        {
            switch (part.Component.PartType)
            {
                case BodyPartType.Head:
                    head = part.Id;
                    break;
                case BodyPartType.Chest:
                    chest = part.Id;
                    break;
            }
        }
        if (head == null || chest == null)
            return false;
        _wounds.AmputateWoundable(chest.Value, head.Value);
        return true;
    }
}
