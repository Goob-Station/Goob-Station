using Content.Server._White.Xenomorphs.Evolution;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.Actions;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared._White.Actions;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Queen;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Interaction;

namespace Content.Server._White.Xenomorphs.Queen;

public sealed class XenomorphQueenSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly PlasmaSystem _plasma = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly XenomorphEvolutionSystem _xenomorphEvolution = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphQueenComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<XenomorphQueenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphQueenComponent, PromotionActionEvent>(OnPromotionAction);

        SubscribeLocalEvent<XenomorphPromotionComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnMapInit(EntityUid uid, XenomorphQueenComponent component, MapInitEvent args) =>
        _actions.AddAction(uid, ref component.PromotionAction, component.PromotionActionId);

    private void OnShutdown(EntityUid uid, XenomorphQueenComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.PromotionAction);

    private void OnPromotionAction(EntityUid uid, XenomorphQueenComponent component, PromotionActionEvent args)
    {
        if (Exists(component.Promotion))
        {
            QueueDel(component.Promotion);
            component.Promotion = null;
            return;
        }

        component.Promotion = Spawn(component.PromotionId);
        var promotion = EnsureComp<XenomorphPromotionComponent>(component.Promotion.Value);
        promotion.CasteWhitelist = component.CasteWhitelist;
        promotion.PromoteTo = component.PromoteTo;
        promotion.EvolutionDelay = component.EvolutionDelay;

        if (TryComp<PlasmaCostActionComponent>(component.PromotionAction, out var plasmaCostAction))
            promotion.PlasmaCost = plasmaCostAction.PlasmaCost;

        if (!_hands.TryForcePickupAnyHand(uid, component.Promotion.Value))
        {
            QueueDel(component.Promotion);
            component.Promotion = null;
            return;
        }

        args.Handled = true;
    }

    private void OnAfterInteract(EntityUid uid, XenomorphPromotionComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Target is not { } target
            || target == args.User
            || !TryComp<XenomorphComponent>(target, out var xenomorph))
            return;

        if (!component.CasteWhitelist.Contains(xenomorph.Caste))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-didnt-pass-whitelist"), args.User);
            return;
        }

        if (!_xenomorphEvolution.Evolve(target, component.PromoteTo, component.EvolutionDelay))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-no-mind"), args.User);
            return;
        }

        if (component.PlasmaCost != 0)
            _plasma.ChangePlasmaAmount(args.User, component.PlasmaCost);

        QueueDel(uid);
        args.Handled = true;
    }
}
