using System.Linq;
using Content.Goobstation.Shared.Xenomorph;
using Content.Shared._White.Actions;
using Content.Shared._White.Actions.Events;
using Content.Shared._White.RadialSelector;
using Content.Shared._White.Xenomorphs.Construction;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Utility;

namespace Content.Server._White.Xenomorphs.Construction;

public sealed class XenomorphConstructionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphConstructionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<XenomorphConstructionComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<XenomorphConstructionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphConstructionComponent, XenomorphChooseStructureEvent>(OnChoose);
        SubscribeLocalEvent<XenomorphConstructionComponent, RadialSelectorSelectedMessage>(OnSelected);
        SubscribeLocalEvent<XenomorphConstructionComponent, XenomorphSecreteEvent>(OnSecrete);
    }

    private void OnMapInit(EntityUid uid, XenomorphConstructionComponent component, MapInitEvent args) =>
        EnsureActions(uid, component);

    private void OnStartup(EntityUid uid, XenomorphConstructionComponent component, ComponentStartup args) =>
        EnsureActions(uid, component);

    private void OnShutdown(EntityUid uid, XenomorphConstructionComponent component, ComponentShutdown args)
    {
        _ui.CloseUi(uid, RadialSelectorUiKey.Key);
        _actions.RemoveAction(uid, component.ChooseAction);
        _actions.RemoveAction(uid, component.SecreteAction);
        component.ChooseAction = null;
        component.SecreteAction = null;
    }

    private bool HasResinSpinner(EntityUid uid)
    {
        // Avoid Resolve(Body) ERROR logs during entity spawn/delete tests (no body yet).
        if (!TryComp<BodyComponent>(uid, out var body))
            return false;

        return _body.TryGetBodyOrganEntityComps<ResinSpinnerComponent>((uid, body), out _);
    }

    private void EnsureActions(EntityUid uid, XenomorphConstructionComponent component)
    {
        if (!HasResinSpinner(uid))
        {
            _actions.RemoveAction(uid, component.ChooseAction);
            _actions.RemoveAction(uid, component.SecreteAction);
            component.ChooseAction = null;
            component.SecreteAction = null;
            return;
        }

        _actions.AddAction(uid, ref component.ChooseAction, component.ChooseActionId);
        _actions.AddAction(uid, ref component.SecreteAction, component.SecreteActionId);

        if (component.SelectedId != null && TryGetOption(component, component.SelectedId, out var selected))
            ApplyChoice(uid, component, selected);
        else if (component.Options.Count > 0)
            ApplyChoice(uid, component, component.Options[0]);
    }

    private void OnChoose(EntityUid uid, XenomorphConstructionComponent component, XenomorphChooseStructureEvent args)
    {
        if (args.Handled)
            return;

        if (!HasResinSpinner(uid))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-construction-no-spinner"), uid, uid);
            _ui.CloseUi(uid, RadialSelectorUiKey.Key);
            return;
        }

        var entries = component.Options.Select(o => new RadialSelectorEntry
        {
            Prototype = o.Id,
            Icon = o.Icon,
        }).ToList();

        if (entries.Count == 0)
            return;

        _ui.TryToggleUi(uid, RadialSelectorUiKey.Key, uid);
        _ui.SetUiState(uid, RadialSelectorUiKey.Key, new TrackedRadialSelectorState(entries));
        args.Handled = true;
    }

    private void OnSelected(EntityUid uid, XenomorphConstructionComponent component, RadialSelectorSelectedMessage args)
    {
        if (!HasResinSpinner(uid))
        {
            _ui.CloseUi(uid, RadialSelectorUiKey.Key, args.Actor);
            return;
        }

        if (!TryGetOption(component, args.SelectedItem, out var option))
            return;

        ApplyChoice(uid, component, option);
        _popup.PopupEntity(Loc.GetString("xenomorphs-construction-selected", ("name", Loc.GetString(option.Name))), uid, uid);
        _ui.CloseUi(uid, RadialSelectorUiKey.Key, args.Actor);
    }

    private void OnSecrete(EntityUid uid, XenomorphConstructionComponent component, XenomorphSecreteEvent args)
    {
        if (args.Handled)
            return;

        if (!HasResinSpinner(uid))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-construction-no-spinner"), uid, uid);
            return;
        }

        if (component.SelectedId == null || !TryGetOption(component, component.SelectedId, out var option))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-construction-none-selected"), uid, uid);
            return;
        }

        var place = new PlaceTileEntityEvent
        {
            Entity = option.Entity,
            TileId = option.TileId,
            Length = option.BuildLength,
            Audio = component.BuildAudio,
            BlockedCollisionMask = option.BlockedCollisionMask,
            BlockedCollisionLayer = option.BlockedCollisionLayer,
            Performer = args.Performer,
            Action = args.Action,
            Target = args.Target,
        };

        RaiseLocalEvent(args.Performer, place, broadcast: true);
        // Do-after builds set Handled only after success; treat started channel as handled.
        args.Handled = place.Handled || option.BuildLength > 0;
    }

    private void ApplyChoice(EntityUid uid, XenomorphConstructionComponent component, XenomorphConstructionOption option)
    {
        component.SelectedId = option.Id;
        Dirty(uid, component);

        if (component.SecreteAction is not { } action)
            return;

        if (option.Icon != null)
            _actions.SetIcon(action, option.Icon);

        if (TryComp(action, out PlasmaCostActionComponent? plasma))
        {
            plasma.PlasmaCost = option.PlasmaCost;
            Dirty(action, plasma);
        }

        _meta.SetEntityName(action, Loc.GetString("xenomorphs-construction-secrete-name", ("name", Loc.GetString(option.Name))));
        _meta.SetEntityDescription(action, Loc.GetString("xenomorphs-construction-secrete-desc",
            ("name", Loc.GetString(option.Name)),
            ("cost", option.PlasmaCost)));
    }

    private static bool TryGetOption(XenomorphConstructionComponent component, string? id, out XenomorphConstructionOption option)
    {
        option = null!;
        if (id == null)
            return false;

        foreach (var entry in component.Options)
        {
            if (entry.Id != id)
                continue;

            option = entry;
            return true;
        }

        return false;
    }
}
