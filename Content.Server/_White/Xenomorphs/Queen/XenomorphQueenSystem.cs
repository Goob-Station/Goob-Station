using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Queen;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Xenomorphs.Queen;

public sealed class XenomorphQueenSystem : EntitySystem
{
    [Dependency] private readonly PlasmaSystem _plasma = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphQueenComponent, PromotionActionEvent>(OnPromotionAction);
        SubscribeLocalEvent<XenomorphQueenComponent, MobStateChangedEvent>(OnQueenStateChanged);
    }

    private void OnQueenStateChanged(Entity<XenomorphQueenComponent> ent, ref MobStateChangedEvent args)
    {
        // Only proceed if the queen just died
        if (args.NewMobState != MobState.Dead || args.OldMobState == MobState.Dead)
            return;

        // Broadcast to all Xenomorphs
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<XenomorphComponent, ActorComponent, MobStateComponent>();
        while (query.MoveNext(out var xenoUid, out _, out var actor, out var mobState))
        {
            if (xenoUid == ent.Owner)
                continue; // Skip the queen

            if (mobState.CurrentState == MobState.Dead)
                continue; // Don't add dead xeno

            filter.AddPlayer(actor.PlayerSession);
        }

        // Only send if we have players to send to
        _chat.DispatchFilteredAnnouncement(
            filter,
            Loc.GetString(ent.Comp.QueenDeathMessage),
            ent.Owner,
            ent.Comp.QueenDeathAnnouncement,
            true,
            ent.Comp.QueenDeathSound,
            ent.Comp.AnnouncementColor);
    }

    private void OnPromotionAction(Entity<XenomorphQueenComponent> ent, ref PromotionActionEvent args)
    {
        // Goobstation start
        if (args.Target == EntityUid.Invalid || args.Target == args.Performer)
            return;

        var target = args.Target;

        if (!_proto.Resolve(ent.Comp.MobRoyalCaste, out var royalProto))
            return;

        if (!TryComp<XenomorphComponent>(args.Target, out var xenomorph))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-invalid-target"), args.Performer);
            return;
        }

        if (!_mind.TryGetMind(target, out var mindId, out var mind))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-no-mind"), target);
            return;
        }

        // Check if target is not in the whitelist
        if (!ent.Comp.CasteWhitelist.Contains(xenomorph.Caste))
        {
            if (xenomorph.Caste == ent.Comp.RoyalCaste)
                _popup.PopupEntity(Loc.GetString("xenomorphs-queen-already-evolved", ("CasteName", royalProto.Name)), args.Performer);
            else
                _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-didnt-pass-whitelist"), args.Performer);
            return;
        }

        // Try direct evolution with optional mind transfer
        var coordinates = Transform(target).Coordinates;
        var newXeno = Spawn(ent.Comp.MobRoyalCaste, coordinates);

        // Transfer mind if it exists
        _mind.TransferTo(mindId, newXeno, mind: mind);

        // Get the target's name before deleting the entity
        var targetName = Name(target);

        // Clean up the old entity
        Del(target);

        // Deduct plasma cost if applicable
        _plasma.ChangePlasmaAmount(ent.Owner, -ent.Comp.EvolveCost);
        _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-success", ("target", targetName)), ent.Owner, ent.Owner);
        args.Handled = true;
        // Goobstation end
    }
}
