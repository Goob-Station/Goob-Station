using Content.Shared.Body.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Hands.Components;
using Content.Shared._Shitmed.Weapons.Melee.Events;
using Content.Shared._Shitmed.Weapons.Ranged.Events;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Wieldable.Components;

namespace Content.Shared._Shitmed.Weapons.Systems;

public sealed class FumbleOnDamageSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandsComponent, AttemptMeleeEvent>(OnAttemptMeleeEvent);
        SubscribeLocalEvent<HandsComponent, GunShotBodyEvent>(OnAttemptShootEvent);
    }

    private void OnAttemptMeleeEvent(EntityUid uid, HandsComponent hands, ref AttemptMeleeEvent ev)
    {
        bool raiseOnAll = false;
        // This might get messy with furry species that have more than two hands, but who cares.
        if (ev.WeaponComponent.MustBeEquippedToUse
            || TryComp(ev.Weapon, out WieldableComponent? wieldable)
            && wieldable.Wielded)
            raiseOnAll = true;

        var ev2 = new AttemptHandsMeleeEvent();
        if (raiseOnAll)
        {
            RaiseLocalEvent(uid, ev2);
        }
        else if (hands.ActiveHand != null) // I dont think its possible for it to be null???
        {
            foreach (var part in _body.GetBodyChildrenOfType(uid, BodyPartType.Hand))
            {
                // Holy shit I need to add slotId assignment to each part this is so ass :wilted_rose:
                if (SharedBodySystem.GetPartSlotContainerId(part.Component.ParentSlot?.Id ?? "") == hands.ActiveHand.Name)
                {
                    ev2 = new AttemptHandsMeleeEvent(part.Component.Symmetry);
                    RaiseLocalEvent(part.Id, ev2);
                }
            }
        }

        if (ev2.Cancelled)
        {
            ev.Cancelled = true;
            return;
        }
    }

    private void OnAttemptShootEvent(EntityUid uid, HandsComponent hands, GunShotBodyEvent ev)
    {
        if (ev.GunUid == uid) // If the gun is the same user with a component e.g. laser eyes, dont bother.
            return;

        bool raiseOnAll = false;

        if (TryComp(ev.GunUid, out WieldableComponent? wieldable)
            && wieldable.Wielded)
            raiseOnAll = true;

        var ev2 = new AttemptHandsShootEvent();
        if (raiseOnAll)
        {
            RaiseLocalEvent(uid, ev2);
        }
        else if (hands.ActiveHand != null)
        {
            foreach (var part in _body.GetBodyChildrenOfType(uid, BodyPartType.Hand))
            {
                if (SharedBodySystem.GetPartSlotContainerId(part.Component.ParentSlot?.Id ?? "") == hands.ActiveHand.Name)
                {
                    ev2 = new AttemptHandsShootEvent(part.Component.Symmetry);
                    RaiseLocalEvent(part.Id, ev2);
                }
            }
        }
    }
}

