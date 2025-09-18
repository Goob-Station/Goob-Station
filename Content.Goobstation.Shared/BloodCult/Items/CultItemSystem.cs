using Content.Goobstation.Common.BloodCult;
using Content.Shared.Blocking;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.BloodCult.Items;

public sealed class CultItemSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CultItemComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<CultItemComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<CultItemComponent, BeforeGettingThrownEvent>(OnBeforeGettingThrown);
        SubscribeLocalEvent<CultItemComponent, BeingEquippedAttemptEvent>(OnEquipAttempt);
        SubscribeLocalEvent<CultItemComponent, AttemptMeleeEvent>(OnMeleeAttempt);
        SubscribeLocalEvent<CultItemComponent, BeforeBlockingEvent>(OnBeforeBlocking);
    }

    private void OnActivate(Entity<CultItemComponent> item, ref ActivateInWorldEvent args)
    {
        if (CanUse(args.User))
            return;

        args.Handled = true;
        KnockdownAndDropItem(item, args.User, Loc.GetString("cult-item-component-generic"));
    }

    private void OnUseInHand(Entity<CultItemComponent> item, ref UseInHandEvent args)
    {
        if (CanUse(args.User) ||
            // Allow non-cultists to remove embedded cultist weapons and getting knocked down afterwards on pickup
            TryComp<EmbeddableProjectileComponent>(item.Owner, out var embeddable) && embeddable.EmbeddedIntoUid != null)
            return;

        args.Handled = true;
        KnockdownAndDropItem(item, args.User, Loc.GetString("cult-item-component-generic"));
    }

    private void OnBeforeGettingThrown(Entity<CultItemComponent> item, ref BeforeGettingThrownEvent args)
    {
        if (CanUse(args.PlayerUid))
            return;

        args.Cancelled = true;
        KnockdownAndDropItem(item, args.PlayerUid, Loc.GetString("cult-item-component-throw-fail"));
    }

    private void OnEquipAttempt(Entity<CultItemComponent> item, ref BeingEquippedAttemptEvent args)
    {
        if (CanUse(args.EquipTarget))
            return;

        args.Cancel();
        KnockdownAndDropItem(item, args.Equipee, Loc.GetString("cult-item-component-equip-fail"));
    }

    private void OnMeleeAttempt(Entity<CultItemComponent> item, ref AttemptMeleeEvent args)
    {
        if (CanUse(args.User))
            return;

        args.Cancelled = true;
        KnockdownAndDropItem(item, args.User, Loc.GetString("cult-item-component-attack-fail"));
    }

    private void OnBeforeBlocking(Entity<CultItemComponent> item, ref BeforeBlockingEvent args)
    {
        if (CanUse(args.User))
            return;

        args.Cancel();
        KnockdownAndDropItem(item, args.User, Loc.GetString("cult-item-component-block-fail"));
    }

    private void KnockdownAndDropItem(Entity<CultItemComponent> item, EntityUid user, string message)
    {
        _popup.PopupPredicted(message, item, user);
        _stun.TryKnockdown(user, item.Comp.KnockdownDuration, true);
        _hands.TryDrop(user);
    }

    private bool CanUse(EntityUid? uid) => HasComp<BloodCultist.BloodCultistComponent>(uid) || HasComp<GhostComponent>(uid);
}
